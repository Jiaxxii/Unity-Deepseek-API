#if DEEPSEEK_PAST_CODE
using System.Collections.Generic;
using System.Linq;

namespace Xiyu.DeepSeekResult.DeepSeekApi.Response.Stream
{
    public static class Expand
    {
        public static string GetMessage(this IEnumerable<StreamChatResult> results)
        {
            return string.Concat(results.Select(v => v.GetMessage().Content));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result">
        /// 如果思考与回答都是空字符串则返回 <see cref="Request.ModelType.None"/> 
        /// 否则返回 <see cref="Request.ModelType.DeepseekReasoner"/>  表示思考内容
        ///  <see cref="Request.ModelType.DeepseekChat"/>  表示回答内容
        /// </param>
        /// <returns></returns>
        public static (Request.ModelType msgType, string msg) GetReasonerMessage(this StreamChatResult result)
        {
            // 获取当前数据流的 Message 等价 streamChatResult.Choices[0] 
            var message = result.GetMessage();

            // 分别判断 内容 和 思考 是否为空
            var isContentNull = string.IsNullOrEmpty(message.Content);
            var isReasoningContentNull = string.IsNullOrEmpty(message.ReasoningContent);

            // 如果两个都为空 则 这个数据段可能没有生成任何 Token 或者 Token 是空格？（我也不确定）
            if (isContentNull && isReasoningContentNull)
            {
                // 跳过
                return (Request.ModelType.None, string.Empty);
            }

            // 谁是空的就 不 打印谁
            // 一般来说 前面的都会是思考 （isContentNull == true） 后面都是内容 （isReasoningContentNull == false）
            return isContentNull
                ? (Request.ModelType.DeepseekReasoner, message.ReasoningContent)
                : (Request.ModelType.DeepseekChat, message.Content);
        }

        public static (string message, string reasoning) GetReasonerMessage(this IEnumerable<StreamChatResult> results)
        {
            var contentTotal = string.Empty;
            var reasoningContentTotal = string.Empty;
            foreach (var (content, reasoningContent) in results.Select(v => (v.GetMessage().Content, v.GetMessage().ReasoningContent)))
            {
                contentTotal += content;
                reasoningContentTotal += reasoningContent;
            }

            return (contentTotal, reasoningContentTotal);
        }

        public static string GetReasoning(this IEnumerable<StreamChatResult> results)
        {
            return string.Concat(results.Select(v => v.GetMessage().ReasoningContent));
        }

        public static string GetMessage(this IEnumerable<StreamFImResult> results)
        {
            return string.Concat(results.Select(v => v.GetMessage()));
        }
    }
}
#endif