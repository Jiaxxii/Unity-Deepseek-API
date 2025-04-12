using System.Collections.Generic;
using System.Linq;
using Xiyu.DeepSeekApi.Request.FIM;

namespace Xiyu.DeepSeekApi.Response.Stream
{
    public static class Expand
    {
        public static string GetMessage(this IEnumerable<StreamChatResult> results)
        {
            return string.Concat(results.Select(v => v.GetMessage().Content));
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