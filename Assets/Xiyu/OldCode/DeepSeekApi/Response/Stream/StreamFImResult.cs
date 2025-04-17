#if DEEPSEEK_PAST_CODE
using System.Collections.Generic;
using Newtonsoft.Json;
using Xiyu.Old.DeepSeekApi.Request;
using Xiyu.Old.DeepSeekApi.Response.FIM;

namespace Xiyu.Old.DeepSeekApi.Response.Stream
{
    public class StreamFImResult : StreamResult<FimChoice>
    {
        [JsonConstructor]
        public StreamFImResult(string id, List<FimChoice> choices, int created, ModelType model, string systemFingerprint, string chatObject, Usage? usage) : base(id, choices,
            created, model, systemFingerprint, chatObject, usage)
        {
        }

        public static StreamFImResult StreamCompleteResult { get; } =
            new(string.Empty, new List<FimChoice>(), 0, ModelType.None, string.Empty, string.Empty, null)
            {
                IsDone = true
            };


        public string GetMessage() => IsDone ? string.Empty : Choices[0].Text;
    }
}
#endif