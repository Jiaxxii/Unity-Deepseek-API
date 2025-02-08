using Newtonsoft.Json;

namespace Xiyu.DeepSeekApi.Response.FIM
{
    /* 下面这些参数官方真的没有写啊，不是我懒！ https://api-docs.deepseek.com/zh-cn/api/create-completion */
    public readonly struct Logprobs
    {
        [JsonConstructor]
        public Logprobs(int[] textOffset, float[] tokenLogprobs, string[] tokens)
        {
            TextOffset = textOffset;
            TokenLogprobs = tokenLogprobs;
            Tokens = tokens;
        }

        [JsonProperty("text_offset")] public int[] TextOffset { get; }
        [JsonProperty("token_logprobs")] public float[] TokenLogprobs { get; }
        [JsonProperty("tokens")] public string[] Tokens { get; }
    }
}