#if DEEPSEEK_PAST_CODE
using Newtonsoft.Json;

namespace Xiyu.Old.DeepSeekApi.Response.Chat
{
    /// <summary>
    /// 选择项
    /// </summary>
    public readonly struct Choice
    {
        /// <summary>
        /// 一般来说你不用关心这个构造函数，因为它是用于反序列化的
        /// </summary>
        /// <param name="finishReason"></param>
        /// <param name="index"></param>
        /// <param name="message"></param>
        [JsonConstructor]
        public Choice(FinishReason finishReason, int index, Message message)
        {
            FinishReason = finishReason;
            Index = index;
            Message = message;
        }

        /// <summary>
        /// 模型停止生成的原因
        /// </summary>
        [JsonProperty(PropertyName = "finish_reason")]
        public FinishReason FinishReason { get; }

        /// <summary>
        /// 该 completion 在模型生成的 completion 的选择列表中的索引。
        /// </summary>
        [JsonProperty(PropertyName = "index")]
        public int Index { get; }

        /// <summary>
        /// 模型生成的 completion 消息。
        /// </summary>

        [JsonProperty(PropertyName = "message", NullValueHandling = NullValueHandling.Ignore)]
        public Message Message { get; }
    }
}
#endif