using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Xiyu.DeepSeekApi.Request
{
    /// <summary>
    /// 角色类型
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RoleType
    {
        None,

        /// <summary>
        /// 系统消息，一般用于定义 prompt 提前告诉ai应该做什么
        /// </summary>
        [EnumMember(Value = "system")] System,

        /// <summary>
        /// 用户消息
        /// </summary>
        [EnumMember(Value = "user")] User,

        /// <summary>
        /// ai 助手消息
        /// </summary>
        [EnumMember(Value = "assistant")] Assistant,

        /// <summary>
        /// 工具 （没用过）
        /// </summary>
        [EnumMember(Value = "tool")] Tool
    }
}