#if DEEPSEEK_PAST_CODE
using System;
using Xiyu.DeepSeekResult.DeepSeekApi.Request;

namespace Xiyu.DeepSeekResult.DeepSeekApi
{
    /// <summary>
    /// deepseek-reasoner 模型 请求器
    /// </summary>
    [JetBrains.Annotations.PublicAPI]
    public sealed class DeepseekReasoner : ChatProcessor
    {

        /// <summary>
        /// 初始化 DeepseekReasoner 处理器
        /// </summary>
        /// <param name="apiKey">你的 api key <para>（这里注册账号：https://platform.deepseek.com/usage）</para></param>
        /// <param name="requestBody">请求体</param>
        /// <param name="timeOut">超时时间</param>
        public DeepseekReasoner(string apiKey, IRequestBody requestBody, TimeSpan? timeOut = null) : base(apiKey, requestBody)
        {
            TimeOut = timeOut ?? TimeSpan.Zero;
        }

        /// <summary>
        /// Deepseek Reasoner 因为需要推理，所以超时时间需要设置的长一些或者不设置 （不重写默认是30秒超时）
        /// </summary>
        public override TimeSpan TimeOut { get; set; }


        // /// <summary>
        // /// <para>对话前缀续写 （Beta）</para>
        // /// 发送聊天请求，要求 ai 根据当前消息作为前缀续写 （官方要求最后一条消息必须是助手消息）
        // /// </summary>
        // /// <param name="assistantMessage">助手消息</param>
        // /// <param name="cancellationToken">取消令牌</param>
        // /// <returns>请求结果</returns>
        // public override async UniTask<ChatResult> SendChatAsync(AssistantMessage assistantMessage, CancellationToken? cancellationToken = null)
        // {
        //     var chatResult = await SendChatAsync<ChatResult>("/beta/chat/completions", cancellationToken);
        //
        //     chatResult = ReplaceResultContent(ref chatResult, assistantMessage.Content);
        //     TryRecordMessage(new AssistantMessage(chatResult.GetMessage().Content, assistantMessage.Name, prefix: assistantMessage.Prefix));
        //
        //     return chatResult;
        // }
    }
}
#endif