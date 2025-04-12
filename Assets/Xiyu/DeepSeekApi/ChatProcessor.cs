using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.Networking;
using Xiyu.DeepSeekApi.Request;
using Xiyu.DeepSeekApi.Response;
using Xiyu.DeepSeekApi.Response.Chat;
using Xiyu.DeepSeekApi.Response.Stream;
using Xiyu.Networking;

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
        protected ChatProcessor(string apiKey, IRequestBody requestBody)
        {
            _httpClient.BaseAddress = new Uri("https://api.deepseek.com");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            RequestBody = requestBody;
            MessageCollector = (MessageCollector)requestBody.Messages;
        }

        protected const string DeepSeekChatUrl = "/chat/completions";
        protected const string DeepSeekChatContinuationPrefix = "/beta/chat/completions";


        private readonly HttpClient _httpClient = new();
        private UnityWebRequest _webRequest;

        private readonly ConcurrentDictionary<string, HttpRequestMessage> _requestMessagesBuffer = new();

        private readonly StringBuilder _stringBuilder = new(1024);
        private readonly StringBuilder _reasoningStringBuilder = new(1024);
        private List<StreamChatResult> _streamChatResults = new();


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


        public (string message, string reasoning) GetCurrentChatMessage()
        {
            var message = _stringBuilder.ToString();
            var reasoning = _reasoningStringBuilder.Length == 0 ? string.Empty : _reasoningStringBuilder.ToString();
            return (message, reasoning);
        }

        public (string message, string reasoning) GetCurrentStreamChatMessage()
        {
            var resultMsg = string.Empty;
            var resultReasoning = string.Empty;
            foreach (var (message, reasoning) in _streamChatResults.Select(v
                         => (message: v.GetMessage().Content, reasoning: v.GetMessage().ReasoningContent)))
            {
                resultMsg += message;
                resultReasoning += reasoning;
            }

            return (resultMsg, resultReasoning);
        }


        /// <summary>
        /// 发送聊天请求
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>请求结果</returns>
        public async UniTask<ChatResult> SendChatAsync(CancellationToken? cancellationToken = null)
        {
            var chatResult = await SendChatAsync<ChatResult>(DeepSeekChatUrl, cancellationToken);

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
            TryRecordMessage(assistantMessage);
            var chatResult = await SendChatAsync<ChatResult>(DeepSeekChatContinuationPrefix, cancellationToken);

            var prefix = assistantMessage.Content; // RequestBody.Model == ModelType.DeepseekChat ? assistantMessage.Content : string.Empty;
            chatResult = ReplaceResultContent(ref chatResult, prefix);
            TryRecordMessage(new AssistantMessage(chatResult.GetMessage().Content, assistantMessage.Name, assistantMessage.ReasoningContent, assistantMessage.Prefix));

            return chatResult;
        }

        protected async UniTask<T> SendChatAsync<T>(string requestUri, CancellationToken? cancellationToken = null, string requestJson = null)
        {
            try
            {
                requestJson ??= RequestBody.ToJson(false);
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
        /// <param name="onReceiveData">收到回复段落时</param>
        /// <param name="assistantMessage">助手消息</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>请求结果</returns>
        public async UniTask<StreamChatResult> SendStreamChatAsync(Action<IEnumerable<StreamChatResult>> onReceiveData, AssistantMessage assistantMessage,
            CancellationToken? cancellationToken = null)
        {
            TryRecordMessage(assistantMessage);

            var result = await SendStreamChatAsync(DeepSeekChatContinuationPrefix, onReceiveData, cancellationToken);

            var prefix = assistantMessage.Content; // RequestBody.Model == ModelType.DeepseekChat ? assistantMessage.Content : string.Empty;

            result = ReplaceResultContent(ref result, prefix);
            MessageCollector.Messages[^1] =
                new AssistantMessage(result.GetMessage().Content, assistantMessage.Name, assistantMessage.ReasoningContent, assistantMessage.Prefix);
            return result;
        }

        /// <summary>
        /// <para>对话前缀续写  （流式） （Beta）</para>
        /// 发送聊天请求，要求 ai 根据当前消息作为前缀续写 （官方要求最后一条消息必须是助手消息）
        /// </summary>
        /// <param name="assistantMessage">助手消息</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>请求结果</returns>
        public async IAsyncEnumerable<StreamChatResult> SendStreamChatAsync(AssistantMessage assistantMessage, CancellationToken? cancellationToken = null)
        {
            TryRecordMessage(assistantMessage);
            StreamChatResult lastStreamResult = null;
            await foreach (var data in SendStreamChatAsync(DeepSeekChatContinuationPrefix, null, AnalysisSseData, cancellationToken))
            {
                var streamChatResult = lastStreamResult = (StreamChatResult)data;
                yield return streamChatResult;
            }


            var (message, reasoning) = GetCurrentStreamChatMessage();

            var prefix = assistantMessage.Content; // RequestBody.Model == ModelType.DeepseekChat ? assistantMessage.Content : string.Empty;

            lastStreamResult = ReplaceResultContent(ref lastStreamResult, prefix, message);
            MessageCollector.Messages[^1] =
                new AssistantMessage(lastStreamResult.GetMessage().Content, assistantMessage.Name, reasoning, assistantMessage.Prefix);

            yield return lastStreamResult;
        }

        /// <summary>
        /// <para>发送聊天请求  （流式）</para>
        /// </summary>
        /// <param name="onReceiveData">收到回复段落时</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>请求结果</returns>
        public async UniTask<StreamChatResult> SendStreamChatAsync(Action<IEnumerable<StreamChatResult>> onReceiveData, CancellationToken? cancellationToken = null)
        {
            return await SendStreamChatAsync(DeepSeekChatUrl, onReceiveData, cancellationToken);
        }


        public async IAsyncEnumerable<StreamChatResult> SendStreamChatAsync(CancellationToken? cancellationToken = null)
        {
            await foreach (var data in SendStreamChatAsync(DeepSeekChatUrl, null, AnalysisSseData, cancellationToken))
            {
                var streamChatResult = (StreamChatResult)data;
                yield return streamChatResult;
            }
        }

        protected async IAsyncEnumerable<StreamResult<T>> SendStreamChatAsync<T>(
            string requestUri,
            string jsonContent,
            Func<string, StreamResult<T>> analysisSseData,
            CancellationToken? cancellationToken = null)
        {
            _streamChatResults.Clear();
            _stringBuilder.Clear();
            _reasoningStringBuilder.Clear();

            var channel = Channel.CreateSingleConsumerUnbounded<byte[]>();

            var request = SendStreamChatAsync(requestUri, null, channel.Writer, jsonContent, cancellationToken);

            while (await channel.Reader.WaitToReadAsync())
            {
                var line = Encoding.UTF8.GetString(await channel.Reader.ReadAsync());
                var dataStrings = line.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                foreach (var streamChatResult in dataStrings.Select(analysisSseData))
                {
                    if (streamChatResult.IsDone)
                    {
                        // 这个段是 [NODE]，没有任何数据
                        break;
                    }

                    yield return streamChatResult;
                }
            }

            await request;
        }

        protected async UniTask<StreamChatResult> SendStreamChatAsync(string requestUri, Action<IEnumerable<StreamChatResult>> onReceiveData,
            CancellationToken? cancellationToken = null)
        {
            _stringBuilder.Clear();
            _reasoningStringBuilder.Clear();
            _streamChatResults.Clear();

            var streamChatResults = new List<StreamChatResult>();
            await SendStreamChatAsync(requestUri, ReceiveData, cancellationToken: cancellationToken);

            var (message, reasoning) = GetCurrentStreamChatMessage();

            var summarize = streamChatResults[^2];
            var msg = new Message(message, reasoning, RoleType.Assistant);
            var streamChoice = new StreamChoice(summarize.Choices[0].FinishReason, summarize.Choices[0].Index, msg);
            var streamChatResult = new StreamChatResult(summarize.Id, new List<StreamChoice> { streamChoice }, summarize.Created, summarize.Model, summarize.SystemFingerprint,
                summarize.ChatObject, summarize.Usage);

            TryRecordMessage(new AssistantMessage(message, string.Empty, reasoning));


            return streamChatResult;

            void ReceiveData(byte[] data)
            {
                streamChatResults.Clear();
                streamChatResults.AddRange(Encoding.UTF8.GetString(data)
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(AnalysisSseData));

                onReceiveData?.Invoke(streamChatResults);
            }
        }

        protected T StreamDeserializeObject<T>(string line, Func<T> doneFunc)
        {
            if (line.StartsWith(": keep-alive"))
            {
                throw new ChatException(503, "服务器繁忙，请稍后再试");
            }

            if (string.IsNullOrEmpty(line) || !line.StartsWith("data:")) return default;

            var jsonData = line["data:".Length..].Trim();

            if (jsonData.StartsWith("[DONE]"))
            {
                return doneFunc.Invoke();
            }

            try
            {
                var chatResult = JsonConvert.DeserializeObject<T>(jsonData);
                return chatResult;
            }
            catch (JsonException ex)
            {
                throw new ChatException("JSON 解析错误", ex);
            }
        }

        private StreamChatResult AnalysisSseData(string line)
        {
            var chatResult = StreamDeserializeObject(line, () => StreamChatResult.StreamCompleteResult);

            if (chatResult.IsDone) return chatResult;


            var message = chatResult.GetMessage().Content;

            if (!string.IsNullOrEmpty(chatResult.GetMessage().ReasoningContent))
            {
                _reasoningStringBuilder.Append(chatResult.GetMessage().ReasoningContent);
            }

            _streamChatResults.Add(chatResult);

            _stringBuilder.Append(message);

            return chatResult;
        }


        protected async UniTask SendStreamChatAsync(string requestUri, Action<byte[]> onReceiveData, ChannelWriter<byte[]> writer = null,
            string jsonContent = null, CancellationToken? cancellationToken = null)
        {
            using var request = GetRequestMessage(true, requestUri);

            _webRequest = new UnityWebRequest(new Uri(_httpClient.BaseAddress, request.RequestUri), request.Method.Method);
            _webRequest.SetRequestHeader("Content-Type", "application/json");
            _webRequest.SetRequestHeader("Authorization", _httpClient.DefaultRequestHeaders.Authorization.ToString());
            // _webRequest.SetRequestHeader("Accept", "stream");

            jsonContent ??= RequestBody.ToJson(true);
            _webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonContent));

            _webRequest.downloadHandler = new DataStreamingHandler(onReceiveData, writer);

            await _webRequest.SendWebRequest().ToUniTask(cancellationToken: cancellationToken ?? CancellationToken.None);

            if (_webRequest.result == UnityWebRequest.Result.Success)
            {
                return;
            }

            throw new ChatException((int)_webRequest.result, _webRequest.error);
        }


        private HttpRequestMessage GetRequestMessage(bool stream, string requestUri)
        {
            if (_requestMessagesBuffer.TryGetValue(requestUri, out var requestMessage))
            {
                return requestMessage;
            }

            requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = new StringContent(RequestBody.ToJson(stream), Encoding.UTF8, "application/json")
            };

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

        protected static ChatResult ReplaceResultContent(ref ChatResult chatRequest, string prefix)
        {
            var combination = $"{prefix}{chatRequest.GetMessage().Content}";

            var chatRequestChoices = chatRequest.Choices;

            chatRequestChoices[^1] =
                new Choice(chatRequest.Choices[0].FinishReason, chatRequest.Choices[0].Index,
                    new Message(combination, chatRequest.GetMessage().ReasoningContent, chatRequest.GetMessage().Role));

            return new ChatResult(chatRequest.Id, chatRequestChoices, chatRequest.Created, chatRequest.Model, chatRequest.SystemFingerprint, chatRequest.ChatObject,
                chatRequest.Usage);
        }

        protected static StreamChatResult ReplaceResultContent(ref StreamChatResult chatRequest, string prefix, string content = null)
        {
            var combination = $"{prefix}{content ?? chatRequest.GetMessage().Content}";

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