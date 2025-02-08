using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Xiyu.DeepSeekApi.Request;
using Xiyu.DeepSeekApi.Response.FIM;

namespace Xiyu.DeepSeekApi.ChatHandler
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
        /// <param name="messageCollector">消息处理器</param>
        public Chat(string apiKey, IRequestBody requestBody, MessageCollector messageCollector = null) : base(apiKey, requestBody, messageCollector)
        {
        }

        /// <summary>
        /// 初始化 Chat 处理器
        /// </summary>
        /// <param name="apiKey">你的 api key <para>（这里注册账号：https://platform.deepseek.com/usage）</para></param>
        /// <param name="requestBody">请求体</param>
        public Chat(string apiKey, IRequestBody requestBody) : base(apiKey, requestBody)
        {
        }


        /// <summary>
        /// <para>FIM 补全（Beta）</para>
        /// 用户可以提供前缀和后缀（可选），模型来补全中间的内容。FIM 常用于内容续写、代码补全等场景。
        /// </summary>
        /// <param name="fimFimRequest">FIM 请求体</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>请求结果</returns>
        public async UniTask<FimResult> SendChatAsync(Request.FIM.FimRequest fimFimRequest, CancellationToken? cancellationToken = null)
        {
            return await base.SendChatAsync<FimResult>("/beta/completions", cancellationToken, requestJson: fimFimRequest.ToJson());
        }

        [Obsolete("偷懒了，还没实现！！！")]
        public /*async*/ UniTask SendChatStreamAsync(Request.FIM.FimRequest fimFimRequest, CancellationToken? cancellationToken = null)
        {
            throw new NotImplementedException("Stream is not implemented yet.");
            // var reader = await base.SendStreamChatAsync("/beta/completions", cancellationToken);
        }
    }
}