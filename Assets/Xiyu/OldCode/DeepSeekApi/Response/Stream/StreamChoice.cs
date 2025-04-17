#if DEEPSEEK_PAST_CODE
using Newtonsoft.Json;

namespace Xiyu.Old.DeepSeekApi.Response.Stream
{
    public readonly struct StreamChoice
    {
        /// <summary>
        /// 一般来说你不用关心这个构造函数，因为它是用于反序列化的
        /// </summary>
        /// <param name="finishReason"></param>
        /// <param name="index"></param>
        /// <param name="deltaMessage"></param>
        [JsonConstructor]
        public StreamChoice(FinishReason? finishReason, int index, Message deltaMessage)
        {
            FinishReason = finishReason;
            Index = index;
            DeltaMessage = deltaMessage;
        }

        /// <summary>
        /// 模型停止生成的原因
        /// </summary>
        [JsonProperty(PropertyName = "finish_reason")]
        public FinishReason? FinishReason { get; }

        /// <summary>
        /// 该 completion 在模型生成的 completion 的选择列表中的索引。
        /// </summary>
        [JsonProperty(PropertyName = "index")]
        public int Index { get; }

        /// <summary>
        /// 模型生成的 completion 消息。
        /// </summary>

        [JsonProperty(PropertyName = "delta")]
        public Message DeltaMessage { get; }
    }
}
#endif