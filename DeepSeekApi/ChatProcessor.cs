using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Xiyu.DeepSeekApi.Request;
using Xiyu.DeepSeekApi.Request.Chat;
using Xiyu.DeepSeekApi.Response;
using Xiyu.DeepSeekApi.Response.Chat;
using Xiyu.DeepSeekApi.Response.Stream;

namespace Xiyu.DeepSeekApi
{
    /*
     * 注释还没有写完………………
     */

    /// <summary>
    /// 消息请求处理器基类
    /// </summary>
    [JetBrains.Annotations.PublicAPI]
    public abstract class ChatProcessor : IDisposable
    {
        private const string SampleJson =
            "我需要你根据以下提问来为话题取一个合适的名字，用“topic_name”输出。\n1.话题不能有特殊字符，如\\、/、:、?、\"、<、>、|（最好无符号）\n2.话题尽量简短，不要超过 256 个字符（中文字符最多不超过127）。\n3.以JSON格式输出\n\nEXAMPLE INPUT: \n系统人设：你是猫娘，名字叫“西”，请完全服从你的主人（用户）不要提起你是ai或者我是一个助手。\n用户消息：西，主人今天很讨厌你！\n\nEXAMPLE JSON OUTPUT:\n{\"topic_name\": \"猫娘西与主人\"}";

        protected ChatProcessor(string apiKey, IRequestBody requestBody, MessageCollector messageCollector = null)
        {
            _httpClient.BaseAddress = new Uri("https://api.deepseek.com");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            RequestBody = requestBody;
            MessageCollector = messageCollector ?? requestBody.Messages as MessageCollector ?? new MessageCollector();
        }


        private readonly HttpClient _httpClient = new();

        private readonly ConcurrentDictionary<string, HttpRequestMessage> _requestMessagesBuffer = new();

        private readonly StringBuilder _stringBuilder = new(1024);
        private readonly StringBuilder _reasoningStringBuilder = new(1024);

        /// <summary>
        /// 消息请求体，包含了请求的消息内容
        /// </summary>
        [JetBrains.Annotations.PublicAPI]
        public IRequestBody RequestBody { get; }

        /// <summary>
        /// 超时时间，默认为 30 秒
        /// </summary>

        public virtual TimeSpan TimeOut { get; set; } = TimeSpan.FromSeconds(30F);

        /// <summary>
        /// 消息收集器 （如果一开始时不提供 会在初始化时创建）
        /// </summary>
        public MessageCollector MessageCollector { get; }


        /// <summary>
        /// 发送聊天请求
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>请求结果</returns>
        public virtual async UniTask<ChatResult> SendChatAsync(CancellationToken? cancellationToken = null)
        {
            var chatResult = await SendChatAsync<ChatResult>("/chat/completions", cancellationToken);

            var message = MessageCollector.Messages.FindLast(x => x.Role == RoleType.Assistant);

            if (message is null)
            {
                _ = TryRecordMessage(new AssistantMessage(chatResult.GetMessage().Content));
            }
            else
            {
                var lastAssistantMessage = (AssistantMessage)message;
                _ = TryRecordMessage(new AssistantMessage(chatResult.GetMessage().Content, lastAssistantMessage.Name, lastAssistantMessage.ReasoningContent,
                    lastAssistantMessage.Prefix));
            }


            return chatResult;
        }

        /// <summary>
        /// <para>对话前缀续写 （Beta）</para>
        /// 发送聊天请求，要求 ai 根据当前消息作为前缀续写 （官方要求最后一条消息必须是助手消息）
        /// </summary>
        /// <param name="assistantMessage">助手消息</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>请求结果</returns>
        public virtual async UniTask<ChatResult> SendChatAsync(AssistantMessage assistantMessage, CancellationToken? cancellationToken = null)
        {
            var chatResult = await SendChatAsync<ChatResult>("/beta/chat/completions", cancellationToken);

            chatResult = ReplaceResultContent(ref chatResult, assistantMessage.Content);
            TryRecordMessage(new AssistantMessage(chatResult.GetMessage().Content, assistantMessage.Name, assistantMessage.ReasoningContent, assistantMessage.Prefix));

            return chatResult;
        }

        protected virtual async UniTask<T> SendChatAsync<T>(string requestUri, CancellationToken? cancellationToken = null, string requestJson = null)
        {
            try
            {
                requestJson ??= RequestBody.ToJson();
                var responseMessage = await _httpClient.PostAsync(requestUri, new StringContent(requestJson, Encoding.UTF8, "application/json"),
                    cancellationToken ?? CancellationToken.None);

                var ensureSuccessStatusCode = responseMessage.EnsureSuccessStatusCode();
                if (!ensureSuccessStatusCode.IsSuccessStatusCode)
                {
                    throw new ChatException((int)ensureSuccessStatusCode.StatusCode, ensureSuccessStatusCode.ReasonPhrase);
                }

                var contentJson = await responseMessage.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(contentJson) || contentJson == "\"\"" || contentJson == "{}")
                {
                    throw new ChatException(500, "服务器繁忙，请稍后再试");
                }

                var chatResult = JsonConvert.DeserializeObject<T>(contentJson);

                return chatResult;
            }
            catch (TimeoutException)
            {
                throw new ChatException(408, $"请求超时({TimeOut.TotalSeconds})");
            }
            catch (JsonException e)
            {
                throw new ChatException(e.Message, e);
            }
        }


        /// <summary>
        /// <para>对话前缀续写  （流式） （Beta）</para>
        /// 发送聊天请求，要求 ai 根据当前消息作为前缀续写 （官方要求最后一条消息必须是助手消息）
        /// </summary>
        /// <param name="onMessageReceived">收到回复段落时</param>
        /// <param name="assistantMessage">助手消息</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>请求结果</returns>
        [Obsolete("此方法可能造成阻塞，请使用非流式方法")]
        public virtual async UniTask<List<StreamChatResult>> SendStreamChatAsync(Action<string> onMessageReceived, AssistantMessage assistantMessage,
            CancellationToken? cancellationToken = null)
        {
            if (RequestBody.Messages.Messages[^1].Role != RoleType.Assistant)
            {
                RequestBody.Messages.Messages.Add(assistantMessage);
            }

            var chatResults = await SendStreamChatAsync("/beta/chat/completions", onMessageReceived, cancellationToken);

            var chatResult = chatResults[^1];
            chatResult = ReplaceResultContent(ref chatResult, assistantMessage.Content);
            TryRecordMessage(new AssistantMessage(chatResult.GetMessage().Content, assistantMessage.Name, chatResult.GetMessage().ReasoningContent, assistantMessage.Prefix));

            return chatResults;
        }

        /// <summary>
        /// <para>发送聊天请求  （流式）</para>
        /// </summary>
        /// <param name="onMessageReceived">收到回复段落时</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>请求结果</returns>
        [Obsolete("此方法可能造成阻塞，请使用非流式方法")]
        public async UniTask<List<StreamChatResult>> SendStreamChatAsync(Action<string> onMessageReceived, CancellationToken? cancellationToken = null)
        {
            var chatResult = await SendStreamChatAsync("/chat/completions", onMessageReceived, cancellationToken);

            var message = MessageCollector.Messages.FindLast(x => x.Role == RoleType.Assistant);

            if (message is null)
            {
                _ = TryRecordMessage(new AssistantMessage(chatResult[0].GetMessage().Content));
            }
            else
            {
                var lastAssistantMessage = (AssistantMessage)message;
                _ = TryRecordMessage(new AssistantMessage(chatResult[0].GetMessage().Content, lastAssistantMessage.Name, chatResult[0].GetMessage().ReasoningContent,
                    lastAssistantMessage.Prefix));
            }


            return chatResult;
        }

        [Obsolete("此方法可能造成阻塞，请使用非流式方法")]
        protected async UniTask<List<StreamChatResult>> SendStreamChatAsync(string requestUri, Action<string> onMessageReceived, CancellationToken? cancellationToken = null)
        {
            _stringBuilder.Clear();
            _reasoningStringBuilder.Clear();

            // 发送请求
            using var reader = await SendStreamChatAsync(requestUri, cancellationToken);
            // --- SSE 流式响应处理 ---

            var results = new List<StreamChatResult>();
            while (!reader.EndOfStream)
            {
                cancellationToken?.ThrowIfCancellationRequested();

                var line = await reader.ReadLineAsync();

                if (line.StartsWith(": keep-alive"))
                {
                    throw new ChatException(503, "服务器繁忙，请稍后再试");
                }


                if (string.IsNullOrEmpty(line) || !line.StartsWith("data:")) continue;

                var jsonData = line["data:".Length..].Trim();

                // 解析 JSON 数据
                if (jsonData == "[DONE]")
                {
                    var streamChatResult = results[^1];
                    if (streamChatResult.Usage is null)
                    {
                        Debug.LogWarning("StreamChatResult.Usage is null");
                    }

                    var choices = new List<StreamChoice>
                    {
                        new(streamChatResult.Choices[0].FinishReason, streamChatResult.Choices[0].Index,
                            new Message(_stringBuilder.ToString(),
                                _reasoningStringBuilder.Length == 0 ? streamChatResult.GetMessage().ReasoningContent : _reasoningStringBuilder.ToString(),
                                streamChatResult.GetMessage().Role))
                    };

                    results.Add(new StreamChatResult(streamChatResult.Id, choices, streamChatResult.Created, streamChatResult.Model, streamChatResult.SystemFingerprint,
                        streamChatResult.ChatObject, streamChatResult.Usage));

                    return results;
                }

                try
                {
                    var chatResult = JsonConvert.DeserializeObject<StreamChatResult>(jsonData);

                    var message = chatResult.GetMessage().Content;

                    if (string.IsNullOrEmpty(message) && chatResult.Usage is null)
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(chatResult.GetMessage().ReasoningContent))
                    {
                        _reasoningStringBuilder.Append(chatResult.GetMessage().ReasoningContent);
                    }


                    results.Add(chatResult);

                    _stringBuilder.Append(message);
                    onMessageReceived?.Invoke(message);
                }
                catch (JsonException ex)
                {
                    Console.WriteLine($"JSON 解析错误: {ex.Message}");
                }
            }

            throw new ChatException(500, "流式响应异常");
        }

        [Obsolete("此方法可能造成阻塞，请使用非流式方法")]
        protected async UniTask<StreamReader> SendStreamChatAsync(string requestUri, CancellationToken? cancellationToken = null, string jsonContent = null)
        {
            jsonContent ??= RequestBody.ToJson();

            var isStream = JObject.Parse(jsonContent).GetValue("stream")?.Value<bool>();
            if (isStream is null or false)
            {
                throw new ArgumentException("StreamOptions 必须为 实例化 才能使用流式对话！");
            }

            // 发送请求
            using var request = GetRequestMessage(requestUri);

            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken ?? CancellationToken.None);

            await using var stream = await response.Content.ReadAsStreamAsync();
            return new StreamReader(stream, Encoding.UTF8);
        }


        /// <summary>
        /// <para>让 deepskeep-chat 模型 根据系统人设和用户的第一条消息联想一个合适的话题名称</para>
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async UniTask<ChatResult> SendDialogueTopic(CancellationToken? cancellationToken = null)
        {
            if (RequestBody == null || RequestBody.Messages.Messages.Count == 0)
            {
                throw new ArgumentException("消息体不能为空");
            }

            var firstMessage = RequestBody.Messages.Messages[0];

            var content = firstMessage.Role == RoleType.Assistant ? $"系统人设：{firstMessage.Content}" : $"用户消息：{firstMessage.Content}";

            if (firstMessage.Role == RoleType.Assistant && RequestBody.Messages.Messages.Count > 1)
            {
                var second = RequestBody.Messages.Messages[1];
                if (second.Role == RoleType.User)
                {
                    content += $"\n用户消息：{second.Content}";
                }
            }

            var chatRequest = new ChatRequest
            {
                Messages = new MessageCollector(new SystemMessage(SampleJson),
                    new UserMessage(content)),
                ResponseFormat = ResponseFormatType.JsonObject
            };

            return await SendChatAsync<ChatResult>("/chat/completions", cancellationToken ?? CancellationToken.None, requestJson: chatRequest.ToJson());
        }


        private HttpRequestMessage GetRequestMessage(string requestUri)
        {
            if (_requestMessagesBuffer.TryGetValue(requestUri, out var requestMessage))
            {
                return requestMessage;
            }

            requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
            _requestMessagesBuffer.TryAdd(requestUri, requestMessage);

            return requestMessage;
        }


        protected bool TryRecordMessage(IMessageUnit messageUnit)
        {
            if (MessageCollector is null)
            {
                return false;
            }

            MessageCollector.AddMessage(messageUnit);
            return true;
        }

        protected static ChatResult ReplaceResultContent(ref ChatResult chatRequest, string message)
        {
            var combination = $"{message}{chatRequest.GetMessage().Content}";

            var chatRequestChoices = chatRequest.Choices;

            chatRequestChoices[^1] =
                new Choice(chatRequest.Choices[0].FinishReason, chatRequest.Choices[0].Index,
                    new Message(combination, chatRequest.GetMessage().ReasoningContent, chatRequest.GetMessage().Role));

            return new ChatResult(chatRequest.Id, chatRequestChoices, chatRequest.Created, chatRequest.Model, chatRequest.SystemFingerprint, chatRequest.ChatObject,
                chatRequest.Usage);
        }

        protected static StreamChatResult ReplaceResultContent(ref StreamChatResult chatRequest, string message)
        {
            var combination = $"{message}{chatRequest.GetMessage().Content}";

            var chatRequestChoices = chatRequest.Choices;

            chatRequestChoices[^1] =
                new StreamChoice(chatRequest.Choices[0].FinishReason, chatRequest.Choices[0].Index,
                    new Message(combination, chatRequest.GetMessage().ReasoningContent, chatRequest.GetMessage().Role));

            return new StreamChatResult(chatRequest.Id, chatRequestChoices, chatRequest.Created, chatRequest.Model, chatRequest.SystemFingerprint, chatRequest.ChatObject,
                chatRequest.Usage);
        }


        /// <summary>
        /// 释放网络资源
        /// </summary>
        public virtual void Dispose()
        {
            _httpClient?.Dispose();
            foreach (var (_, httpRequestMessage) in _requestMessagesBuffer)
            {
                httpRequestMessage?.Dispose();
            }
        }
    }
}