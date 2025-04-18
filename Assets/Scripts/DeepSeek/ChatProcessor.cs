using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xiyu.DeepSeek.Messages;
using Xiyu.DeepSeek.Requests;
using Xiyu.DeepSeek.Responses;
using Xiyu.DeepSeek.Responses.Expand;

namespace Xiyu.DeepSeek
{
    public class ChatProcessor
    {
        private static readonly HttpClient MainClient = new()
        {
            BaseAddress = new Uri(DeepSeekBaseUrl),
            Timeout = TimeSpan.FromMinutes(5)
        };

        public static void Dispose() => MainClient.Dispose();


        private readonly MediaTypeWithQualityHeaderValue _mediaTypeByJson = new("application/json");
        private readonly MediaTypeWithQualityHeaderValue _mediaTypeByStream = new("text/event-stream");

        private const string DeepSeekBaseUrl = "https://api.deepseek.com";
        private const string RequestUrlByChat = "/chat/completions";


        private readonly AuthenticationHeaderValue _authHeader;

        private readonly StringBuilder _contentBuilder = new();
        private readonly StringBuilder _reasoningContentBuilder = new();

        public ChatProcessor(string apiKey, MessageRequest messageRequest)
        {
            _authHeader = new AuthenticationHeaderValue("Bearer", apiKey ?? throw new ArgumentNullException(nameof(apiKey)));
            // MessagesCollector = collector ?? throw new ArgumentNullException(nameof(collector));
            MessageRequest = messageRequest ?? throw new ArgumentNullException(nameof(messageRequest));
        }

        protected ChatProcessor(string apiKey)
        {
            _authHeader = new AuthenticationHeaderValue("Bearer", apiKey ?? throw new ArgumentNullException(nameof(apiKey)));
            MessageRequest = null;
        }

        public MessageRequest MessageRequest { get; set; }

        #region 对话前缀续写

        /// <summary>
        /// 发起一次聊天请求 【前缀续写】【自动记录结果】 【强制关闭流式接收】
        /// </summary>
        /// <param name="prefixMessage">前缀内容，强制助手以此消息作为前缀并生成续写内容</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>对话补全内容</returns>
        public virtual async UniTask<ChatCompletion> ChatCompletionAsync(AssistantPrefixMessage prefixMessage, CancellationToken cancellationToken = default)
        {
            MessageRequest.SetStreamOptions(false);

            var requestJson = GetRequestBody(prefixMessage).ToString(Formatting.None);

            var requestUrl = GetBetaUrlPath(RequestUrlByChat);

            var report = await ChatCompletionAsync(requestUrl, requestJson, cancellationToken);

            RecordReport(report, prefixMessage.Content);

            return prefixMessage.IsJointPrefix ? report.Completion(prefixMessage.Content) : report;
        }

        /// <summary>
        /// 发起一次聊天请求 【前缀续写】【即发即收】【异步迭代器】【自动记录结果】 【强制开启流式接收】
        /// </summary>
        /// <param name="prefixMessage">前缀内容，强制助手以此消息作为前缀并生成续写内容</param>
        /// <param name="onReport">统计，拼接了所有消息</param>
        /// <returns>流式数据段</returns>
        public virtual IUniTaskAsyncEnumerable<StreamChatCompletion> ChatCompletionStreamAsync(AssistantPrefixMessage prefixMessage, Action<ChatCompletion> onReport = null)
        {
            MessageRequest.SetStreamOptions(true);

            var requestJson = GetRequestBody(prefixMessage).ToString(Formatting.None);

            var requestUrl = GetBetaUrlPath(RequestUrlByChat);

            return ChatCompletionStreamNotRecordAsync(requestUrl, requestJson, report =>
            {
                if (!prefixMessage.IsJointPrefix)
                {
                    // 不需要补全前缀 （即便不需要补全前缀，记录的消息也得是完整的内容，返回的最后统计不需要）
                    // 触发回调 （没有前缀信息）
                    onReport?.Invoke(report);
                }

                // 需要补全前缀
                RecordReport(report, prefixMessage.Content);


                if (prefixMessage.IsJointPrefix)
                    onReport?.Invoke(report.Completion(prefixMessage.Content));
            });
        }

        /// <summary>
        /// 发起一次聊天请求 【前缀续写】【即发即收】【回调式】【自动记录结果】 【强制开启流式接收】
        /// </summary>
        /// <param name="prefixMessage">前缀内容，强制助手以此消息作为前缀并生成续写内容</param>
        /// <param name="onReceiveData">接收到的流式数据段</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>统计，拼接了所有消息</returns>
        public virtual UniTask<ChatCompletion> ChatCompletionStreamAsync(AssistantPrefixMessage prefixMessage, Action<StreamChatCompletion> onReceiveData,
            CancellationToken cancellationToken = default)
        {
            MessageRequest.SetStreamOptions(true);

            var requestJson = GetRequestBody(prefixMessage).ToString(Formatting.None);

            var requestUrl = GetBetaUrlPath(RequestUrlByChat);

            return ChatCompletionStreamNotRecordAsync(requestUrl, requestJson, onReceiveData, report =>
            {
                RecordReport(report, prefixMessage.Content);
                return prefixMessage.IsJointPrefix ? report.Completion(prefixMessage.Content) : report;
            }, cancellationToken);
        }

        #endregion

        #region 对话补全

        /// <summary>
        /// 发起一次聊天请求 【自动记录结果】
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>对话补全内容</returns>
        public UniTask<ChatCompletion> ChatCompletionAsync(CancellationToken cancellationToken = default)
        {
            MessageRequest.SetStreamOptions(false);
            var requestJson = MessageRequest.SerializeRequestJson();

            return ChatCompletionAsync(RequestUrlByChat, requestJson, cancellationToken);
        }


        /// <summary>
        /// 发起一次聊天请求 【即发即收】 【回调式】 【自动记录结果】
        /// </summary>
        /// <param name="onReceiveData">解析数据后反序列化的结构体对象</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <exception cref="NullReferenceException"><see cref="onReceiveData"/> 为空</exception>
        /// <returns>返回统计，包含Token以及完整的响应内容</returns>
        public UniTask<ChatCompletion> ChatCompletionStreamAsync(Action<StreamChatCompletion> onReceiveData,
            CancellationToken cancellationToken = default)
        {
            MessageRequest.SetStreamOptions(true);
            var requestJson = MessageRequest.SerializeRequestJson();

            return ChatCompletionStreamAsync(RequestUrlByChat, requestJson, onReceiveData, cancellationToken);
        }

        /// <summary>
        /// 发起一次聊天请求 【即发即收】 【异步迭代器式】 【自动记录结果】
        /// </summary>
        /// <param name="onReport">当迭代完成时返回统计，包含Token以及完整的响应内容</param>
        /// <returns>解析数据后反序列化的结构体对象</returns>
        public IUniTaskAsyncEnumerable<StreamChatCompletion> ChatCompletionStreamAsync(Action<ChatCompletion> onReport)
        {
            MessageRequest.SetStreamOptions(true);
            var requestJson = MessageRequest.SerializeRequestJson();

            return ChatCompletionStreamAsync(RequestUrlByChat, requestJson, onReport);
        }

        #endregion


        /// <summary>
        /// 发起一次聊天请求 【自动记录结果】
        /// </summary>
        /// <param name="requestJson">请求体</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>对话补全内容</returns>
        /// <exception cref="ArgumentNullException"><see cref="requestJson"/> 为空</exception>
        protected UniTask<ChatCompletion> ChatCompletionAsync(string requestJson, CancellationToken cancellationToken = default)
        {
            return ChatCompletionAsync(RequestUrlByChat, requestJson, cancellationToken);
        }


        /// <summary>
        /// 发起一次聊天请求 【即发即收】 【回调式】 【自动记录结果】
        /// </summary>
        /// <param name="requestJson">请求体</param>
        /// <param name="onReceiveData">解析数据后反序列化的结构体对象</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <exception cref="ArgumentNullException"><see cref="requestJson"/> 为空</exception>
        /// <exception cref="NullReferenceException"><see cref="onReceiveData"/> 为空</exception>
        /// <returns>返回统计，包含Token以及完整的响应内容</returns>
        protected UniTask<ChatCompletion> ChatCompletionStreamAsync(string requestJson, Action<StreamChatCompletion> onReceiveData,
            CancellationToken cancellationToken = default)
        {
            return ChatCompletionStreamAsync(RequestUrlByChat, requestJson, onReceiveData, cancellationToken);
        }

        /// <summary>
        /// 发起一次聊天请求 【即发即收】 【异步迭代器式】 【自动记录结果】
        /// </summary>
        /// <param name="requestJson">请求体</param>
        /// <param name="onReport">当迭代完成时返回统计，包含Token以及完整的响应内容</param>
        /// <returns>解析数据后反序列化的结构体对象</returns>
        /// <exception cref="ArgumentNullException"><see cref="requestJson"/> 为空</exception>
        protected IUniTaskAsyncEnumerable<StreamChatCompletion> ChatCompletionStreamAsync(string requestJson, Action<ChatCompletion> onReport)
        {
            return ChatCompletionStreamAsync(RequestUrlByChat, requestJson, onReport);
        }


        /// <summary>
        /// 发起一次聊天请求 【自动记录结果】
        /// </summary>
        /// <param name="requestUrl">请求URL（相对）</param>
        /// <param name="requestJson">请求体</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>对话不全结果</returns>
        /// <exception cref="ArgumentNullException"><see cref="requestJson"/> 为空</exception>
        protected async UniTask<ChatCompletion> ChatCompletionAsync(string requestUrl, string requestJson, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(requestJson))
                throw new ArgumentNullException(nameof(requestJson));

            var jsonContent = await PostAsync(requestUrl, requestJson, cancellationToken);

            var deserializeObject = DeserializeObject<ChatCompletion>(ref jsonContent, ChatCompletion.DeSerializerSettings);

            RecordReport(deserializeObject);

            return deserializeObject;
        }

        /// <summary>
        /// 发起一次聊天请求 【即发即收】 【回调式】 【自动记录结果】
        /// </summary>
        /// <param name="requestUrl">请求URL（相对）</param>
        /// <param name="requestJson">请求体</param>
        /// <param name="onReceiveData">解析数据后反序列化的结构体对象</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <exception cref="ArgumentNullException"><see cref="requestJson"/> 为空</exception>
        /// <exception cref="NullReferenceException"><see cref="onReceiveData"/> 为空</exception>
        /// <returns>返回统计，包含Token以及完整的响应内容</returns>
        protected UniTask<ChatCompletion> ChatCompletionStreamAsync(string requestUrl, string requestJson, Action<StreamChatCompletion> onReceiveData,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(requestJson))
                throw new ArgumentNullException(nameof(requestJson));

            return ChatCompletionStreamNotRecordAsync(requestUrl, requestJson, onReceiveData, report =>
            {
                RecordReport(report);

                return report;
            }, cancellationToken);
        }

        /// <summary>
        /// 发起一次聊天请求 【即发即收】 【异步迭代器式】 【自动记录结果】
        /// </summary>
        /// <param name="requestUrl">请求URL （相对）</param>
        /// <param name="requestJson">请求体</param>
        /// <param name="onReport">当迭代完成时返回统计，包含Token以及完整的响应内容</param>
        /// <returns>解析数据后反序列化的结构体对象</returns>
        /// <exception cref="ArgumentNullException"><see cref="requestJson"/> 为空</exception>
        protected IUniTaskAsyncEnumerable<StreamChatCompletion> ChatCompletionStreamAsync(string requestUrl, string requestJson, Action<ChatCompletion> onReport)
        {
            if (string.IsNullOrWhiteSpace(requestJson))
                throw new ArgumentNullException(nameof(requestJson));

            return ChatCompletionStreamNotRecordAsync(requestUrl, requestJson, report =>
            {
                RecordReport(report);

                onReport?.Invoke(report);
            });
        }


        protected IUniTaskAsyncEnumerable<StreamChatCompletion> ChatCompletionStreamNotRecordAsync(
            string requestUrl, string requestJson, Action<ChatCompletion> onReport = null)
        {
            return UniTaskAsyncEnumerable.Create<StreamChatCompletion>(async (writer, token) =>
            {
                try
                {
                    await using var stream = await PostStreamAsync(requestUrl, requestJson, token);

                    using var streamReader = new StreamReader(stream);

                    RoleType? first = null;
                    StreamChatCompletion? last = null;

                    await foreach (var data in ReadStreamForLine(streamReader, JsonDeserializeException))
                    {
                        var jsonContent = data;
                        var streamChatCompletion = DeserializeObject<StreamChatCompletion>(ref jsonContent, StreamChatCompletion.DeSerializerSettings);

                        PushContent(ref streamChatCompletion);

                        AnalyzeBeingEnd(ref first, ref last, ref streamChatCompletion);

                        await writer.YieldAsync(streamChatCompletion);
                    }

                    // 统计 - 缺少前缀内容
                    var report = CombinationStreamChatCompletion(first, last);
                    onReport?.Invoke(report);
                }
                finally
                {
                    await UniTask.SwitchToMainThread();
                }
            });
        }


        protected async UniTask<ChatCompletion> ChatCompletionStreamNotRecordAsync(string requestUrl, string requestJson, Action<StreamChatCompletion> onReceiveData,
            Func<ChatCompletion, ChatCompletion> resultFunc,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await using var stream = await PostStreamAsync(requestUrl, requestJson, cancellationToken);

                using var streamReader = new StreamReader(stream);

                RoleType? first = null;
                StreamChatCompletion? last = null;

                await ReadStreamForLine(streamReader, data =>
                {
                    var jsonContent = data;
                    var streamChatCompletion = DeserializeObject<StreamChatCompletion>(ref jsonContent, StreamChatCompletion.DeSerializerSettings);

                    PushContent(ref streamChatCompletion);

                    AnalyzeBeingEnd(ref first, ref last, ref streamChatCompletion);

                    onReceiveData(streamChatCompletion);
                }, JsonDeserializeException, cancellationToken);

                var report = CombinationStreamChatCompletion(first, last);
                return resultFunc(report);
            }
            finally
            {
                await UniTask.SwitchToMainThread();
            }
        }


        /// <summary>
        /// 根据输入的上下文（<see cref="requestJson"/>），来让模型补全对话内容。
        /// </summary>
        /// <param name="requestUri">请求地址</param>
        /// <param name="requestJson">请求体</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>模型不全的内容</returns>
        protected async UniTask<string> PostAsync(string requestUri, string requestJson, CancellationToken cancellationToken = default)
        {
            using var requestMessage = CreateRequest(requestUri, requestJson, _mediaTypeByJson);

            using var response = await MainClient.SendAsync(requestMessage, cancellationToken)
                .AsUniTask();

            if (!response.IsSuccessStatusCode)
            {
                DeserializeObjectHttpErrorThrow(response, await response.Content.ReadAsStringAsync());
            }

            return await response.Content.ReadAsStringAsync();
        }

        private static void DeserializeObjectHttpErrorThrow(HttpResponseMessage response, string errorJson)
        {
            if (errorJson == null)
            {
                response.EnsureSuccessStatusCode();
                return;
            }

            try
            {
                var responsesError = JsonConvert.DeserializeObject<ResponsesError>(errorJson, ChatCompletion.DeSerializerSettings);

                throw new HttpResponseErrorException(responsesError.Error!.Value);
            }
            catch (JsonSerializationException)
            {
                response.EnsureSuccessStatusCode();
            }
        }


        /// <summary>
        /// 根据输入的上下文（requestJson），来让模型补全对话内容。【即发即收】
        /// <para>***操作 Unity UI 对象时需要返回主线程 <see cref="UniTask.SwitchToMainThread(CancellationToken)"/></para>
        /// <code>
        /// try
        /// {
        ///     await using (var postStreamAsync = await PostStreamAsync(requestUri, requestJson, cancellationToken))
        ///     {
        ///         // 执行操作
        ///     }
        /// }finally
        /// {
        ///     await UniTask.SwitchToMainThread();
        /// }
        /// </code>
        /// </summary>
        /// <param name="requestUri">请求地址</param>
        /// <param name="requestJson">请求体</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>模型不全的内容流</returns>
        protected async UniTask<Stream> PostStreamAsync(string requestUri, string requestJson, CancellationToken cancellationToken = default)
        {
            using var requestMessage = CreateRequest(requestUri, requestJson, _mediaTypeByStream);

            var response = await MainClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .AsUniTask(false);

            if (!response.IsSuccessStatusCode)
            {
                DeserializeObjectHttpErrorThrow(response, await response.Content.ReadAsStringAsync());
            }

            return await response.Content.ReadAsStreamAsync();
        }


        /// <summary>
        /// 创建请求消息
        /// </summary>
        /// <param name="requestUri">相对地址</param>
        /// <param name="requestJson">请求体</param>
        /// <param name="mediaType"></param>
        /// <returns></returns>
        protected HttpRequestMessage CreateRequest(string requestUri, string requestJson, MediaTypeWithQualityHeaderValue mediaType)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
            requestMessage.Content = new StringContent(requestJson, Encoding.UTF8, _mediaTypeByJson.MediaType);

            requestMessage.Headers.Accept.Add(mediaType);
            requestMessage.Headers.Authorization = _authHeader;

            return requestMessage;
        }


        /// <summary>
        /// 将 <see cref="jsonContent"/> 反序列化为 <see cref="T"/>
        /// </summary>
        /// <param name="jsonContent">模型返回的 Json</param>
        /// <param name="settings">反序列化配置</param>
        /// <typeparam name="T">目标类型 T 必须实现 <see cref="IValid"/></typeparam>
        /// <returns>反序列化实例</returns>
        /// <exception cref="HttpResponseErrorException">请求错误异常</exception>
        /// <exception cref="JsonDeserializeException">Json 序列化异常</exception>
        protected static T DeserializeObject<T>(ref string jsonContent, JsonSerializerSettings settings = null) where T : IValid
        {
            try
            {
                var deserializeObject = JsonConvert.DeserializeObject<T>(jsonContent, settings);

                if (deserializeObject.Error != null)
                {
                    throw new HttpResponseErrorException(deserializeObject.Error.Value);
                }

                if (!deserializeObject.IsValid())
                {
                    UnityEngine.Debug.LogWarning($"可能序列化了一个不包含有效值的结构体！json:\n{jsonContent}");
                }

                return deserializeObject;
            }
            catch (JsonSerializationException)
            {
                UnityEngine.Debug.LogError($"无法序列化对象{typeof(T)}-{jsonContent}");
                throw;
            }
        }


        /// <summary>
        /// 尝试将 Json 序列化为错误实例 <see cref="ResponsesError"/> 然后转为 <see cref="HttpResponseErrorException"/> 异常并返回
        /// </summary>
        /// <param name="jsonContent">JSON</param>
        /// <returns>异常</returns>
        protected static Exception JsonDeserializeException(string jsonContent)
        {
            try
            {
                var deserializeObject = JsonConvert.DeserializeObject<ResponsesError>(jsonContent, ChatCompletion.DeSerializerSettings);

                if (deserializeObject.Error == null)
                {
                    return new Exception($"流式接收不以\"data: \"开头的数据：{jsonContent}");
                }

                return new HttpResponseErrorException(deserializeObject.Error.Value);
            }
            catch (JsonSerializationException e)
            {
                UnityEngine.Debug.LogWarning($"流式接收不以\"data: \"开头的数据：{jsonContent}");
                return e;
            }
        }


        /// <summary>
        /// 读取数据流（SSE）并尝试进行截取字符串 【回调式】
        /// </summary>
        /// <code>
        /// await ReadStreamForLine(streamReader, data =>
        ///         {
        ///             // 处理你的数据
        ///             Debug.Log(data);
        ///         }, notDataException)
        /// </code>
        /// <param name="streamReader">数据流</param>
        /// <param name="onReceiveData">SSE 数据段回调</param>
        /// <param name="notDataException">当数据不是 SSE 标准时引发的错误</param>
        /// <param name="cancellationToken">取消令牌</param>
        protected static async UniTask ReadStreamForLine(StreamReader streamReader, Action<string> onReceiveData,
            Func<string, Exception> notDataException, CancellationToken cancellationToken = default)

        {
            try
            {
                while (!streamReader.EndOfStream)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    await UniTask.SwitchToThreadPool();
                    var line = await streamReader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    if (DataIsDone(line, notDataException, out var sseDate))
                    {
                        break;
                    }

                    await UniTask.SwitchToMainThread();
                    onReceiveData?.Invoke(sseDate);
                }
            }
            finally
            {
                await UniTask.SwitchToMainThread();
            }
        }

        /// <summary>
        /// 读取数据流（SSE）并尝试进行截取字符串 【await foreach】
        /// </summary>
        /// <code>
        /// await foreach(string data in ReadStreamForLine(streamReader, notDataException))
        /// {
        ///     // 处理你的数据
        ///     Debug.Log(data);
        /// }
        /// </code>
        /// <param name="streamReader">数据流</param>
        /// <param name="notDataException">当数据不是 SSE 标准时引发的错误</param>
        protected static IUniTaskAsyncEnumerable<string> ReadStreamForLine(StreamReader streamReader, Func<string, Exception> notDataException)
        {
            return UniTaskAsyncEnumerable.Create<string>(async (writer, token) =>
            {
                try
                {
                    while (!streamReader.EndOfStream)
                    {
                        if (token.IsCancellationRequested)
                        {
                            token.ThrowIfCancellationRequested();
                        }

                        await UniTask.SwitchToThreadPool();
                        var line = await streamReader.ReadLineAsync();
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }

                        if (DataIsDone(line, notDataException, out var sseDate))
                        {
                            break;
                        }


                        await UniTask.SwitchToMainThread();
                        await writer.YieldAsync(sseDate);
                    }
                }
                finally
                {
                    await UniTask.SwitchToMainThread();
                }
            });
        }


        /// <summary>
        /// 将 sse 的一行进行截取
        /// </summary>
        /// <param name="line">sse数据行 data: xxx</param>
        /// <param name="notDataException">如果此数据段不以data: 开头则抛出此异常</param>
        /// <param name="sseDate">截取后的数据</param>
        /// <returns>是否结束</returns>
        /// <exception cref="Exception">数据非sse抛出<see cref="notDataException"/></exception>
        protected static bool DataIsDone(string line, Func<string, Exception> notDataException, out string sseDate)
        {
            if (!line.StartsWith("data: "))
            {
                throw notDataException(line);
            }

            sseDate = line["data: ".Length..];

            return sseDate.StartsWith("[DONE]", StringComparison.OrdinalIgnoreCase);
        }

        protected JArray OneCombination(AssistantPrefixMessage assistantPrefixMessage)
        {
            var messageCombination = MessageRequest.MessagesCollector.MessageCombination();
            messageCombination.Add(assistantPrefixMessage.Serializer.SerializeJson(assistantPrefixMessage));

            return messageCombination;
        }

        private JObject GetRequestBody(AssistantPrefixMessage assistantPrefixMessage)
        {
            var message = OneCombination(assistantPrefixMessage);

            var instance = MessageRequest.SerializeParameter();
            instance.Add(RequestBody.KeyMessages, message);

            return instance;
        }


        protected static string GetBetaUrlPath(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            return url[0] == '/' ? $"beta{url}" : $"beta/{url}";
        }


        private static void AnalyzeBeingEnd(ref RoleType? first, ref StreamChatCompletion? last, ref StreamChatCompletion streamChatCompletion)
        {
            var message = streamChatCompletion.GetMessage();

            if (first == null)
            {
                first = message.Role;
            }
            else last = streamChatCompletion;
        }

        private void PushContent(ref StreamChatCompletion streamChatCompletion)
        {
            var message = streamChatCompletion.GetMessage();

            _contentBuilder.Append(message.Content);
            if (streamChatCompletion.Model == ChatModel.Reasoner)
            {
                _reasoningContentBuilder.Append(message.ReasoningContent);
            }
        }

        private void RecordReport(ChatCompletion report, string prefix = null)
        {
            var message = report.GetMessage();
            var fullContent = string.Concat(prefix ?? string.Empty, message.Content);

            IMessage fullMessage = string.IsNullOrEmpty(message.ReasoningContent)
                ? new AssistantMessage(fullContent) // 没有思考内容
                : new AssistantReasonerMessage(message.ReasoningContent, fullContent); // 有思考内容

            MessageRequest.MessagesCollector.Append(fullMessage);
        }


        private ChatCompletion CombinationStreamChatCompletion(RoleType? first, StreamChatCompletion? last)
        {
            if (!first.HasValue)
            {
                var error = new Error("流式数据异常，没有解析到任意内容！", "SSE解析异常", string.Empty, string.Empty);
                throw new HttpResponseErrorException(error);
            }

            if (last == null || !last.Value.Usage!.Value.IsValid())
            {
                var error = new Error("无法统计 token 数量，可能流式数据有缺少", "SSE解析异常", string.Empty, string.Empty);
                throw new HttpResponseErrorException(error);
            }

            var usage = last.Value.Usage.Value;

            var content = _contentBuilder.ToString();
            _contentBuilder.Clear();

            var reasoningContent = _reasoningContentBuilder.ToString();
            _reasoningContentBuilder.Clear();

            var count = StreamChatCompletion.CountChatCompletion(last.Value, first.Value, content, reasoningContent, usage);

            return count;
        }
    }
}