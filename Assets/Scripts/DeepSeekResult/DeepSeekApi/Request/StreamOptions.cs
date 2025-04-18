#if DEEPSEEK_PAST_CODE
using Newtonsoft.Json;

namespace Xiyu.DeepSeekResult.DeepSeekApi.Request
{
    /// <summary>
    /// <para>我问AI怎么实时接收流式消息，给的代码和非流式的等待时间一样</para>
    /// </summary>
    public readonly struct StreamOptions
    {
        /// <summary>
        /// 如果设置为 true，在流式消息最后的 data: [DONE] 之前将会传输一个额外的块。
        /// 此块上的 usage 字段显示整个请求的 token 使用统计信息，而 choices 字段将始终是一个空数组。
        /// 所有其他块也将包含一个 usage 字段，但其值为 null。
        /// </summary>
        [JsonProperty(PropertyName = "include_usage")]

        public bool IncludeUsage { get; }

        /// <summary>
        /// </summary>
        /// <param name="includeUsage">
        /// 此块上的 usage 字段显示整个请求的 token 使用统计信息，而 choices 字段将始终是一个空数组。
        /// 所有其他块也将包含一个 usage 字段，但其值为 null。
        /// </param>
        public StreamOptions(bool includeUsage) => IncludeUsage = includeUsage;
    }
}
#endif