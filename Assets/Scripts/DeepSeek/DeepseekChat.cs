using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using UnityEngine;
using Xiyu.DeepSeek.Messages;
using Xiyu.DeepSeek.Requests;
using Xiyu.DeepSeek.Responses;
using Xiyu.DeepSeek.Responses.Expand;
using Xiyu.DeepSeek.Responses.FimResult;
using Xiyu.DeepSeek.Responses.ToolResult;

namespace Xiyu.DeepSeek
{
    public class DeepseekChat : DeepseekReasoner
    {
        private const string FimRequestUrl = "/completions";
        private readonly StringBuilder _contentBuilder = new();

        private readonly Dictionary<string, Func<Function, UniTask<string>>> _tools = new();

        public DeepseekChat(string apiKey, ChatMessageRequest messageRequest) : base(apiKey,
            messageRequest)
        {
        }

        public DeepseekChat(string apiKey, ChatMessageRequest messageRequest, KeyValuePair<string, Func<Function, UniTask<string>>> toolFunction) : base(apiKey,
            messageRequest)
        {
            _tools.Add(toolFunction.Key, toolFunction.Value);
        }

        public DeepseekChat(string apiKey, ChatMessageRequest messageRequest, params KeyValuePair<string, Func<Function, UniTask<string>>>[] toolFunctions) : base(apiKey,
            messageRequest)
        {
            foreach (var (functionName, function) in toolFunctions)
            {
                _tools.Add(functionName, function);
            }
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


        #region CHAT

        public override async UniTask<ChatCompletion> ChatCompletionAsync(CancellationToken cancellationToken = default)
        {
            var firstChatCompletion = await base.ChatCompletionAsync(cancellationToken);
            if (!TryGetTools(firstChatCompletion, out var tools))
            {
                return firstChatCompletion;
            }

            await CallToolFunctionAsync(tools);

            return (await base.ChatCompletionAsync(cancellationToken)).Completion(string.Empty, firstChatCompletion.Usage);
        }

        public override async UniTask<ChatCompletion> ChatCompletionStreamAsync(Action<StreamChatCompletion> onReceiveData, CancellationToken cancellationToken = default)
        {
            var firstChatCompletion = await base.ChatCompletionStreamAsync(onReceiveData, cancellationToken);

            if (!TryGetTools(firstChatCompletion, out var tools))
            {
                return firstChatCompletion;
            }

            await CallToolFunctionAsync(tools);

            var secondChatCompletion = await base.ChatCompletionStreamAsync(onReceiveData, cancellationToken);

            return secondChatCompletion.Completion(string.Empty, firstChatCompletion.Usage);
        }

        public override UniTaskCancelableAsyncEnumerable<StreamChatCompletion> ChatCompletionStreamAsync(Action<ChatCompletion> onReport,
            CancellationToken cancellationToken = default)
        {
            return UniTaskAsyncEnumerable.Create<StreamChatCompletion>(async (writer, token) =>
            {
                MessageRequest.SetStreamOptions(true);
                var requestJson = MessageRequest.SerializeRequestJson();


                ChatCompletion? fistChatCompletion = null;
                await foreach (var data in ChatCompletionStreamAsync(RequestUrlByChat, requestJson, null, report => fistChatCompletion = report))
                {
                    await writer.YieldAsync(data);
                }

                if (!fistChatCompletion.HasValue)
                    throw new HttpResponseErrorException("流式读取异常，未按照预期获取到统计结果！");


                if (!TryGetTools(fistChatCompletion.Value, out var tools))
                {
                    onReport?.Invoke(fistChatCompletion.Value);
                    return;
                }

                await CallToolFunctionAsync(tools);

                MessageRequest.SetStreamOptions(true);
                requestJson = MessageRequest.SerializeRequestJson();

                ChatCompletion? secondChatCompletion = null;
                await foreach (var data in ChatCompletionStreamAsync(RequestUrlByChat, requestJson, null, report => secondChatCompletion = report))
                {
                    await writer.YieldAsync(data);
                }

                if (!secondChatCompletion.HasValue)
                    throw new HttpResponseErrorException("流式读取异常，未按照预期获取到统计结果！");

                onReport?.Invoke(secondChatCompletion.Value.Completion(string.Empty, fistChatCompletion.Value.Usage));
            }).WithCancellation(cancellationToken);
        }

        #endregion


        #region 添加定义的 FunctonCall

        public void AddFunction(KeyValuePair<string, Func<Function, UniTask<string>>> function)
        {
            _tools.Add(function.Key, function.Value);
        }

        public bool TryAddFunction(KeyValuePair<string, Func<Function, UniTask<string>>> function)
        {
            return _tools.TryAdd(function.Key, function.Value);
        }

        #endregion


        private async UniTask CallToolFunctionAsync(IList<Tool> tools)
        {
            foreach (var tool in tools)
            {
                if (!_tools.TryGetValue(tool.Function.Name, out var toolFunc))
                {
                    Debug.LogWarning($"模型请求方法调用，但是您未定义方法\"{tool.Function.Name}\"");
                    continue;
                }


                var result = await toolFunc(tool.Function);
                var toolMessage = new ToolMessage(result, tool.ID);

                MessageRequest.MessagesCollector.Append(toolMessage);
            }
        }


        private static bool TryGetTools(ChatCompletion chatCompletion, out IList<Tool> tools)
        {
            if (chatCompletion.Choices[0].FinishReason is not FinishReason.ToolCalls)
            {
                tools = null;
                return false;
            }


            tools = chatCompletion.Choices[0].Message.ToolCalls;
            return true;
        }

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