using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Xiyu.DeepSeek.Messages
{
    public interface IMessagesCollector
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        IList<IMessage> Messages { get; }

        void Append(IMessage message);

        string SerializeJson(Formatting formatting = Formatting.None);

        JArray MessageCombination();

        void CheckAndThrow();
    }
}