using System;

namespace Xiyu.DeepSeek.Messages
{
    [Serializable]
    public class UserMessage : Message
    {
        public UserMessage(string content, string name = null) : base(Role.User, content, name)
        {
        }

        public override Role Role => Role.User;
    }
}