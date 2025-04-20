using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Xiyu.DeepSeek.Responses
{
    /// <summary>
    /// AI 完成生成（停止）的原因
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum FinishReason
    {
        None,
        /// <summary>
        /// 模型自然停止生成，或遇到 stop 序列中列出的字符串。
        /// </summary>
        [EnumMember(Value = "stop")] Stop,

        /// <summary>
        /// 输出长度达到了模型上下文长度限制，或达到了 max_tokens 的限制。
        /// </summary>
        [EnumMember(Value = "length")] Length,

        /// <summary>
        /// 输出内容因触发过滤策略而被过滤。
        /// </summary>
        [EnumMember(Value = "content_filter")] ContentFilter,

        /// <summary>
        /// 系统推理资源不足，生成被打断。
        /// </summary>
        [EnumMember(Value = "insufficient_system_resources")]
        InsufficientSystemResources,
        
        [EnumMember(Value = "tool_calls")]
        ToolCalls
    }
}