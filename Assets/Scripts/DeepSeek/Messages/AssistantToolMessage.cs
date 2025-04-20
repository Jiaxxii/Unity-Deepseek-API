using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Xiyu.DeepSeek.Responses.ToolResult;

namespace Xiyu.DeepSeek.Messages
{
    [DebuggerDisplay("Role：{Role} Tool：{ToolCalls.Count}")]
    public class AssistantToolMessage : IMessage
    {
        public AssistantToolMessage(IList<Tool> toolCalls, ISerializer serializer = null)
        {
            ToolCalls = toolCalls;
            Serializer = serializer ?? new Message.DefaultSerializerContent();
        }

        [JsonIgnore] public ISerializer Serializer { get; }

        /// <summary>
        /// 生成这条消息的角色。
        /// </summary>
        public Role Role => Role.Assistant;


        /// <summary>
        /// 固定为 ""
        /// </summary>
        [JsonIgnore]
        public string Content
        {
            get => string.Empty;
            set { }
        }

        /// <summary>
        /// 模型生成的 tool 调用，例如 function 调用。
        /// </summary>
        public IList<Tool> ToolCalls { get; }


        public static implicit operator AssistantToolMessage(List<Tool> tools)
        {
            return new AssistantToolMessage(tools);
        }
    }
}