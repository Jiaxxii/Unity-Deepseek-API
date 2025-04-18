using Newtonsoft.Json;

namespace Xiyu.DeepSeek.Responses
{
    public readonly struct StreamChoices
    {
        [JsonConstructor]
        public StreamChoices(FinishReason? finishReason, int index, Delta delta)
        {
            FinishReason = finishReason;
            Index = index;
            Delta = delta;
        }

        /// <summary>
        /// 模型停止生成 token 的原因。
        /// </summary>
        public FinishReason? FinishReason { get; }

        /// <summary>
        /// 该 completion 在模型生成的 completion 的选择列表中的索引。
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// 流式返回的一个 completion 增量。
        /// </summary>
        public Delta Delta { get; }
    }
}