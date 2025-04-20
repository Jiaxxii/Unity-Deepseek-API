using System;
using System.Diagnostics;

namespace Xiyu.DeepSeek.Messages
{
    [Serializable]
    [DebuggerDisplay("role:{Role} content:{Content}")]
    public class SystemMessage : Message
    {
        public SystemMessage(string content, string name = null) : base(Role.System, content, name)
        {
        }

        public override Role Role => Role.System;
    }
}