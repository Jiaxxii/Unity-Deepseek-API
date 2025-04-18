using System;
using System.IO;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Xiyu.DeepSeek.Messages;
using Xiyu.DeepSeek.Requests;
using Xiyu.DeepSeek.Responses;
using Xiyu.DeepSeek.Responses.Expand;
using Xiyu.DeepSeek.Responses.FimResult;

namespace Xiyu.DeepSeek
{
    public class DeepseekChat : DeepseekReasoner
    {
        private const string FimRequestUrl = "/completions";
        private readonly StringBuilder _contentBuilder = new();

        public DeepseekChat(string apiKey, ChatMessageRequest messageRequest) : base(apiKey, messageRequest)
        {
        }

        protected DeepseekChat(string apiKey, MessageRequest messageRequest) : base(apiKey, messageRequest)
        {
        }

        #region 对话前缀续写

        public override UniTask<ChatCompletion> ChatCompletionAsync(AssistantPrefixMessage prefixMessage, CancellationToken cancellationToken = default)
        {
            CheckAssistantPrefixMessage(ref prefixMessage);

            return base.ChatCompletionAsync(prefixMessage, cancellationToken);
        }

        public override IUniTaskAsyncEnumerable<StreamChatCompletion> ChatCompletionStreamAsync(AssistantPrefixMessage prefixMessage, Action<ChatCompletion> onReport = null)
        {
            CheckAssistantPrefixMessage(ref prefixMessage);

            return base.ChatCompletionStreamAsync(prefixMessage, onReport);
        }

        public override UniTask<ChatCompletion> ChatCompletionStreamAsync(AssistantPrefixMessage prefixMessage, Action<StreamChatCompletion> onReceiveData,
            CancellationToken cancellationToken = default)
        {
            CheckAssistantPrefixMessage(ref prefixMessage);

            return base.ChatCompletionStreamAsync(prefixMessage, onReceiveData, cancellationToken);
        }

        #endregion

        #region FIM补全 *不记录消息*

        /// <summary>
        /// 发起一次补全请求 【FIM补全】 【默认不记录消息】
        /// </summary>
        /// <param name="requestBody">请求体</param>
        /// <param name="recordToMessageList">将结果记录到消息列表中</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>补全结果</returns>
        public async UniTask<FimChatCompletion> FimChatCompletionAsync(FimRequest requestBody, bool recordToMessageList = false, CancellationToken cancellationToken = default)
        {
            requestBody.SetStreamOptions(false);

            var (prompt, suffix, echo) = GetInvariant(requestBody);

            var requestJson = requestBody.SerializeRequestJson();

            var jsonContent = await PostAsync(GetBetaUrlPath(FimRequestUrl), requestJson, cancellationToken);

            var report = DeserializeObject<FimChatCompletion>(ref jsonContent, ChatCompletion.DeSerializerSettings);

            var result = GetFimResult(ref report, ref prompt, ref suffix, ref echo, ref recordToMessageList);

            return result;
        }

        /// <summary>
        /// 发起一次补全请求 【FIM补全】 【默认不记录消息】 【即发即收】 【回调式】
        /// </summary>
        /// <param name="requestBody">请求体</param>
        /// <param name="onReceiveData">完接收到的流式数据段</param>
        /// <param name="recordToMessageList">将结果记录到消息列表中</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>统计 - 补全结果</returns>
        public async UniTask<FimChatCompletion> FimChatCompletionStreamAsync(FimRequest requestBody, Action<StreamFimChatCompletion> onReceiveData,
            bool recordToMessageList = false, CancellationToken cancellationToken = default)
        {
            requestBody.SetStreamOptions(true);

            var (prompt, suffix, echo) = GetInvariant(requestBody);

            var requestJson = requestBody.SerializeRequestJson();

            await using var stream = await PostStreamAsync(GetBetaUrlPath(FimRequestUrl), requestJson, cancellationToken);

            using var streamReader = new StreamReader(stream);

            StreamFimChatCompletion? last = null;

            await ReadStreamForLine(streamReader, data =>
            {
                var jsonContent = data;
                var streamChatCompletion = DeserializeObject<StreamFimChatCompletion>(ref jsonContent, StreamChatCompletion.DeSerializerSettings);

                PushContent(ref streamChatCompletion);
                //
                last = streamChatCompletion;
                //
                onReceiveData(streamChatCompletion);
            }, JsonDeserializeException, cancellationToken);

            var report = CombinationStreamChatCompletion(last);

            var result = GetFimResult(ref report, ref prompt, ref suffix, ref echo, ref recordToMessageList);

            return result;
        }

        /// <summary>
        /// 发起一次补全请求 【FIM补全】 【默认不记录消息】 【即发即收】 【回调式】
        /// </summary>
        /// <param name="requestBody">请求体</param>
        /// <param name="onReport">统计 - 补全结果</param>
        /// <param name="recordToMessageList">将结果记录到消息列表中</param>
        /// <returns>完接收到的流式数据段</returns>
        public IUniTaskAsyncEnumerable<StreamFimChatCompletion> FimChatCompletionStreamAsync(FimRequest requestBody, Action<FimChatCompletion> onReport,
            bool recordToMessageList = false)
        {
            requestBody.SetStreamOptions(true);

            var (prompt, suffix, echo) = GetInvariant(requestBody);

            var requestJson = requestBody.SerializeRequestJson();

            return UniTaskAsyncEnumerable.Create<StreamFimChatCompletion>(async (writer, token) =>
            {
                await using var stream = await PostStreamAsync(GetBetaUrlPath(FimRequestUrl), requestJson, token);

                using var streamReader = new StreamReader(stream);

                StreamFimChatCompletion? last = null;

                await foreach (var data in ReadStreamForLine(streamReader, JsonDeserializeException))
                {
                    var jsonContent = data;
                    var streamChatCompletion = DeserializeObject<StreamFimChatCompletion>(ref jsonContent, StreamChatCompletion.DeSerializerSettings);
                    PushContent(ref streamChatCompletion);

                    last = streamChatCompletion;

                    await writer.YieldAsync(streamChatCompletion);
                }

                var report = CombinationStreamChatCompletion(last);

                var result = GetFimResult(ref report, ref prompt, ref suffix, ref echo, ref recordToMessageList);

                onReport?.Invoke(result);
            });
        }

        #endregion

        private FimChatCompletion GetFimResult(ref FimChatCompletion report, ref string prompt, ref string suffix, ref bool echo, ref bool recordToMessageList)
        {
            FimChatCompletion? fullFimChatCompletion = null;
            if (recordToMessageList)
            {
                fullFimChatCompletion = report.Completion(prompt, suffix);
                RecordReport(fullFimChatCompletion.Value.GetMessage().Text);
            }

            return echo ? fullFimChatCompletion ?? report.Completion(prompt, suffix) : report;
        }


        private void RecordReport(string fullContent)
        {
            var assistantMessage = new AssistantMessage(fullContent);
            MessageRequest.MessagesCollector.Append(assistantMessage);
        }

        private static (string prompt, string suffix, bool echo) GetInvariant(FimRequest requestBody)
        {
            if (requestBody == null)
                throw new ArgumentNullException(nameof(requestBody));

            if (string.IsNullOrEmpty(requestBody.Prompt))
                throw new ArgumentException("必须为\"prompt\"赋值！");

            return (requestBody.Prompt, requestBody.Suffix, requestBody.Echo);
        }

        private void PushContent(ref StreamFimChatCompletion streamChatCompletion)
        {
            var message = streamChatCompletion.GetMessage();
            _contentBuilder.Append(message.Text);
        }

        private FimChatCompletion CombinationStreamChatCompletion(StreamFimChatCompletion? last)
        {
            if (last == null || !last.Value.Usage!.Value.IsValid())
            {
                var error = new Error("无法统计 token 数量，可能流式数据有缺少", "SSE解析异常", string.Empty, string.Empty);
                throw new HttpResponseErrorException(error);
            }

            var usage = last.Value.Usage.Value;

            var content = _contentBuilder.ToString();
            _contentBuilder.Clear();

            var count = StreamFimChatCompletion.CountChatCompletion(last.Value, content, usage);

            return count;
        }

        private static void CheckAssistantPrefixMessage(ref AssistantPrefixMessage prefixMessage)
        {
            if (!string.IsNullOrEmpty(prefixMessage.ReasoningContent))
            {
                throw new ArgumentException("deepseek-chat 模型使用对话前缀续写功能时不能设置\"ReasoningContent\"参数");
            }
        }
    }
}