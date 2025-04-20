using System.Diagnostics;
using Newtonsoft.Json;

namespace Xiyu.DeepSeek.Messages
{
    [DebuggerDisplay("role:{Role} content:{Content} tool id:{ToolCallId}")]
    public class ToolMessage : IMessage
    {
        public ToolMessage(string content, string toolCallId, ISerializer serializer = null)
        {
            Content = content;
            ToolCallId = toolCallId;
            Serializer = serializer ?? new Message.DefaultSerializerContent();
        }

        public Role Role => Role.Tool;
        public string Content { get; set; }

        [JsonIgnore] public ISerializer Serializer { get; }
        public string ToolCallId { get; set; }
    }
}