#if DEEPSEEK_PAST_CODE
using Newtonsoft.Json;

namespace Xiyu.DeepSeekResult.DeepSeekApi.Response
{
    /// <summary>
    /// 请求的用量信息。
    /// </summary>
    public readonly struct Usage
    {
        /// <summary>
        /// 一般来说你不用关心这个构造函数，因为它是用于反序列化的
        /// </summary>
        /// <param name="completionTokens"></param>
        /// <param name="promptTokens"></param>
        /// <param name="promptCacheHitTokens"></param>
        /// <param name="promptCacheMissTokens"></param>
        /// <param name="totalTokens"></param>
        /// <param name="completionTokensDetails"></param>
        [JsonConstructor]
        public Usage(int completionTokens, int promptTokens, int promptCacheHitTokens, int promptCacheMissTokens, int totalTokens, CompletionTokensDetails completionTokensDetails)
        {
            CompletionTokens = completionTokens;
            PromptTokens = promptTokens;
            PromptCacheHitTokens = promptCacheHitTokens;
            PromptCacheMissTokens = promptCacheMissTokens;
            TotalTokens = totalTokens;
            CompletionTokensDetails = completionTokensDetails;
        }

        /// <summary>
        /// 模型 completion 产生的 token 数。
        /// </summary>
        [JsonProperty(PropertyName = "completion_tokens")]
        public int CompletionTokens { get; }

        /// <summary>
        /// 用户 prompt 所包含的 token 数。该值等于 <see cref="PromptCacheHitTokens"/> +<see cref="PromptCacheMissTokens"/>
        /// </summary>

        [JsonProperty(PropertyName = "prompt_tokens")]
        public int PromptTokens { get; }

        /// <summary>
        /// 用户 prompt 中，命中上下文缓存的 token 数。
        /// </summary>

        [JsonProperty(PropertyName = "prompt_cache_hit_tokens")]
        public int PromptCacheHitTokens { get; }

        /// <summary>
        /// 用户 prompt 中，未命中上下文缓存的 token 数。
        /// </summary>

        [JsonProperty(PropertyName = "prompt_cache_miss_tokens")]
        public int PromptCacheMissTokens { get; }

        /// <summary>
        /// 请求中，所有 token 的数量（prompt + completion）。这个值等于 <see cref="PromptTokens"/> + <see cref="CompletionTokens"/>
        /// </summary>

        [JsonProperty(PropertyName = "total_tokens")]
        public int TotalTokens { get; }


        /// <summary>
        /// completion tokens 的详细信息。
        /// </summary>
        [JsonProperty(PropertyName = "completion_tokens_details")]
        public CompletionTokensDetails CompletionTokensDetails { get; }

        public bool IsEmpty() => CompletionTokens == 0 && PromptTokens == 0 && PromptCacheHitTokens == 0 && PromptCacheMissTokens == 0 && TotalTokens == 0 &&
                                 CompletionTokensDetails.ReasoningTokens == 0;
    }

    /// <summary>
    /// completion tokens 的详细信息。
    /// </summary>
    public readonly struct CompletionTokensDetails
    {
        [JsonConstructor]
        public CompletionTokensDetails(int reasoningTokens)
        {
            ReasoningTokens = reasoningTokens;
        }

        /// <summary>
        /// 推理模型所产生的思维链 token 数量
        /// </summary>
        [JsonProperty(PropertyName = "reasoning_tokens")]
        public int ReasoningTokens { get; }
    }
}
#endif