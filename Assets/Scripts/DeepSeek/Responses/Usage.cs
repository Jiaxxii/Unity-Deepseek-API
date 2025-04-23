using System.Diagnostics;
using Newtonsoft.Json;
using Xiyu.DeepSeek.Responses.Expand;

namespace Xiyu.DeepSeek.Responses
{
    [DebuggerDisplay("{TotalTokens} tokens")]
    public readonly struct Usage
    {
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
        public int CompletionTokens { get; }

        /// <summary>
        /// 用户 prompt 所包含的 token 数。该值等于 <see cref="PromptCacheHitTokens"/> + <see cref="PromptCacheMissTokens"/>
        /// </summary>
        public int PromptTokens { get; }

        /// <summary>
        /// 用户 prompt 中，命中上下文缓存的 token 数。
        /// </summary>
        public int PromptCacheHitTokens { get; }

        /// <summary>
        /// 用户 prompt 中，未命中上下文缓存的 token 数。
        /// </summary>
        public int PromptCacheMissTokens { get; }

        /// <summary>
        /// 该请求中，所有 token 的数量（prompt + completion）。
        /// </summary>
        public int TotalTokens { get; }

        public CompletionTokensDetails CompletionTokensDetails { get; }

        public bool IsValid() =>
            CompletionTokens + PromptTokens + PromptCacheHitTokens + PromptCacheMissTokens + TotalTokens + CompletionTokensDetails.ReasoningTokens > 0;

        public static Usage operator +(Usage first, Usage second)
        {
            return new Usage(
                completionTokens: first.CompletionTokens + second.CompletionTokens,
                promptTokens: first.PromptTokens + second.PromptTokens,
                promptCacheHitTokens: first.PromptCacheHitTokens + second.PromptCacheHitTokens,
                promptCacheMissTokens: first.PromptCacheMissTokens + second.PromptCacheMissTokens,
                totalTokens: first.TotalTokens + second.TotalTokens,
                completionTokensDetails: first.CompletionTokensDetails + second.CompletionTokensDetails
            );
        }
    }

    [DebuggerDisplay("reasoning<UNK>{ReasoningTokens} tokens")]
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
        public int ReasoningTokens { get; }

        public static CompletionTokensDetails operator +(CompletionTokensDetails first, CompletionTokensDetails second) => new(first.ReasoningTokens + second.ReasoningTokens);
    }
}