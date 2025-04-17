using Newtonsoft.Json;

namespace Xiyu.DeepSeek.Responses
{
    public readonly struct Choices
    {
        [JsonConstructor]
        public Choices(FinishReason? finishReason, int index, Message message, Logprobs? logprobs)
        {
            FinishReason = finishReason;
            Index = index;
            Message = message;
            Logprobs = logprobs;
        }

        /// <summary>
        /// 模型停止生成 token 的原因。
        /// <para>如果该属性为空则可能开启了流式接收</para>
        /// </summary>
        public FinishReason? FinishReason { get; }

        /// <summary>
        /// 该 completion 在模型生成的 completion 的选择列表中的索引。
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// 模型生成的 completion 消息。
        /// </summary>
        public Message Message { get; }

        /// <summary>
        /// 该 choice 的对数概率信息。
        /// </summary>
        public Logprobs? Logprobs { get; }
    }
}