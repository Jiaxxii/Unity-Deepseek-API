#if DEEPSEEK_PAST_CODE
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Xiyu.Old.DeepSeekApi.Request
{
    /// <summary>
    /// 响应格式类型
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ResponseFormatType
    {
        /// <summary>
        /// 文本类型
        /// </summary>
        [EnumMember(Value = "text")] Text,
        
        /// <summary>
        /// JSON 类型，根据官网说，就算使用 JSON 模式也可能返回空白字符 （正在修复）
        /// </summary>

        [EnumMember(Value = "json_object")] JsonObject
    }
}
#endif