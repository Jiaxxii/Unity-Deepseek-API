#if DEEPSEEK_PAST_CODE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Xiyu.Old.DeepSeekApi.Request;
using Xiyu.Old.DeepSeekApi.Response.FIM;
using Xiyu.Old.DeepSeekApi.Response.Stream;

namespace Xiyu.Old.DeepSeekApi
{
    /// <summary>
    /// deepseek-chat 模型 请求器
    /// </summary>
    public sealed class Chat : ChatProcessor
    {
        /// <summary>
        /// 初始化 Chat 处理器
        /// </summary>
        /// <param name="apiKey">你的 api key <para>（这里注册账号：https://platform.deepseek.com/usage）</para></param>
        /// <param name="requestBody">请求体</param>
        public Chat(string apiKey, IRequestBody requestBody) : base(apiKey, requestBody)
        {
        }

        private readonly StringBuilder _fimRequestStringBuilder = new();


        public string GetFimResponseMessage() => _fimRequestStringBuilder.ToString();

        /// <summary>
        /// <para>FIM 补全（Beta）</para>
        /// 用户可以提供前缀和后缀（可选），模型来补全中间的内容。FIM 常用于内容续写、代码补全等场景。
        /// </summary>
        /// <param name="fimFimRequest">FIM 请求体</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>请求结果</returns>
        public async UniTask<FimResult> SendChatAsync(FimRequest fimFimRequest, CancellationToken? cancellationToken = null)
        {
            var result = await base.SendChatAsync<FimResult>("/beta/completions", cancellationToken, requestJson: fimFimRequest.ToJson());

            TryRecordMessage(new AssistantMessage($"{fimFimRequest.Prompt}{result.Choices[0].Text}{fimFimRequest.Suffix}", string.Empty, string.Empty, true));

            return result;
        }


        public async IAsyncEnumerable<StreamFImResult> SendChatStreamAsync(FimRequest fimFimRequest, CancellationToken? cancellationToken = null)
        {
            _fimRequestStringBuilder.Clear();

            if (!fimFimRequest.Echo)
                _fimRequestStringBuilder.Append(fimFimRequest.Prompt);

            await foreach (var data in SendStreamChatAsync("/beta/completions", fimFimRequest.ToJson(), AnalysisSseData, cancellationToken))
            {
                yield return (StreamFImResult)data;
            }
        }


        public async UniTask<StreamFImResult> SendChatStreamAsync(FimRequest fimFimRequest, Action<IEnumerable<StreamFImResult>> onReceiveData,
            CancellationToken? cancellationToken = null)
        {
            _fimRequestStringBuilder.Clear();

            if (!fimFimRequest.Echo)
                _fimRequestStringBuilder.Append(fimFimRequest.Prompt);

            var streamFImResults = new List<StreamFImResult>();

            await SendStreamChatAsync("/beta/completions", ReceiveData, jsonContent: fimFimRequest.ToJson(), cancellationToken: cancellationToken);

            _fimRequestStringBuilder.Append(fimFimRequest.Suffix);


            var message = _fimRequestStringBuilder.ToString();

            var summarize = streamFImResults[^2];
            var fimChoice = new FimChoice(summarize.Choices[0].FinishReason, summarize.Choices[0].Index, message, summarize.Choices[0].Logprobs);
            var streamFImResult = new StreamFImResult(summarize.Id, new List<FimChoice> { fimChoice }, summarize.Created, summarize.Model, summarize.SystemFingerprint,
                summarize.ChatObject, summarize.Usage);

            TryRecordMessage(new AssistantMessage(message, string.Empty, string.Empty, true));

            return streamFImResult;

            void ReceiveData(byte[] data)
            {
                streamFImResults.Clear();
                streamFImResults.AddRange(Encoding.UTF8.GetString(data)
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(AnalysisSseData));

                onReceiveData.Invoke(streamFImResults);
            }
        }


        private StreamFImResult AnalysisSseData(string line)
        {
            var chatResult = StreamDeserializeObject(line, () => StreamFImResult.StreamCompleteResult);

            if (!chatResult.IsDone)
                _fimRequestStringBuilder.Append(chatResult.Choices[0].Text);

            return chatResult;
        }
    }
}
#endif