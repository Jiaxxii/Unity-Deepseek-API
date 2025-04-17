using System.Diagnostics;

namespace Xiyu.DeepSeek.Messages
{
    [DebuggerDisplay("Role:{Role} Content:{Content} 思考:{ReasonerContent}")]
    public class AssistantReasonerMessage : AssistantMessage
    {
        public AssistantReasonerMessage(string reasonerContent, string content, string name = null) : base(content, name)
        {
            ReasonerContent = reasonerContent;
        }

        public string ReasonerContent { get; }
    }
}