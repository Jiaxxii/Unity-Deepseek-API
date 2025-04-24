using System.Diagnostics;

namespace Xiyu.DeepSeek.Messages
{
    [System.Serializable]
    [DebuggerDisplay("Role:{Role} Content:{Content}")]
    public class AssistantMessage : Message
    {
        public AssistantMessage(string content, string name = null) : base(Role.Assistant, content, name)
        {
        }

        public override Role Role => Role.Assistant;
    }
}