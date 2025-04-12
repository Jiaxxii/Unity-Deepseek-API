using Newtonsoft.Json;

namespace Xiyu.DeepSeekApi.Response.FIM
{
    public readonly struct FimChoice
    {
        [JsonConstructor]
        public FimChoice(FinishReason? finishReason, int index, string text, Logprobs? logprobs)
        {
            FinishReason = finishReason;
            Index = index;
            Text = text;
            Logprobs = logprobs ?? new Logprobs();
        }

        /// <summary>
        /// 模型完成的原因。
        /// </summary>
        [JsonProperty("finish_reason")]
        public FinishReason? FinishReason { get; }


        /* 下面这些参数官方真的没有写啊，不是我懒！ https://api-docs.deepseek.com/zh-cn/api/create-completion */
        [JsonProperty("index")] public int Index { get; }

        /// <summary>
        /// 请求的文本
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; }

        [JsonProperty("logprobs")] public Logprobs? Logprobs { get; }
    }
}