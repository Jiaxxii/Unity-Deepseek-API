using System.Runtime.Serialization;

namespace Xiyu.DeepSeek.Messages
{
    public enum Role
    {
        [EnumMember(Value = "system")] System,
        [EnumMember(Value = "user")] User,
        [EnumMember(Value = "assistant")] Assistant,
        [EnumMember(Value = "tool")] Tool
    }
}