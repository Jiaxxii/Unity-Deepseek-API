#if DEEPSEEK_PAST_CODE
using System.Collections.Generic;
using Newtonsoft.Json;
using Xiyu.DeepSeekResult.DeepSeekApi.Request;

namespace Xiyu.DeepSeekResult.DeepSeekApi.Response.Stream
{
    public class StreamChatResult : StreamResult<StreamChoice>
    {
        [JsonConstructor]
        public StreamChatResult(string id, List<StreamChoice> choices, int created, ModelType model, string systemFingerprint, string chatObject, Usage? usage) : base(id, choices,
            created, model, systemFingerprint, chatObject, usage)
        {
        }

        public static StreamChatResult StreamCompleteResult { get; } = new(string.Empty, new List<StreamChoice>(), 0, ModelType.None, string.Empty, string.Empty, new Usage())
        {
            IsDone = true
        };

        public Message GetMessage() => IsDone ? Message.None : Choices[0].DeltaMessage;
        
    }
}
#endif