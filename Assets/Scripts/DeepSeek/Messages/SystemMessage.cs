using System;

namespace Xiyu.DeepSeek.Messages
{
    [Serializable]
    public class SystemMessage : Message
    {
        public SystemMessage(string content, string name = null) : base(Role.System, content, name)
        {
        }

        public override Role Role => Role.System;
    }
}