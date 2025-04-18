#if DEEPSEEK_PAST_CODE
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Xiyu.DeepSeekResult.DeepSeekApi.Request
{
    /// <summary>
    /// deepseek 模型类型
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ModelType
    {
        None = 0,
        /// <summary>
        /// 如果没有特殊需求，一般使用此模型，因为这个响应快
        /// </summary>
        [EnumMember(Value = "deepseek-chat")] DeepseekChat,

        /// <summary>
        /// 会深度思考，但是响应速度会慢一些
        /// </summary>
        [EnumMember(Value = "deepseek-reasoner")]
        DeepseekReasoner
    }
}
#endif