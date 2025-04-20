using System;
using System.Diagnostics;

namespace Xiyu.DeepSeek.Messages
{
    [Serializable]
    [DebuggerDisplay("role:{Role} content:{Content}")]
    public class UserMessage : Message
    {
        public UserMessage(string content, string name = null) : base(Role.User, content, name)
        {
        }

        public override Role Role => Role.User;
    }
}