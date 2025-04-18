using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xiyu.DeepSeek.Messages;

namespace Xiyu.DeepSeek.Requests
{
    [Serializable]
    public class MessageRequest : RequestBody
    {
        public MessageRequest(ChatModel model, IMessagesCollector messagesCollector) : base(model)
        {
            MessagesCollector = messagesCollector;
        }

        public IMessagesCollector MessagesCollector { get; set; }


        public override string SerializeRequestJson(JObject instance = null, Formatting formatting = Formatting.None, bool overwrite = false)
        {
            var parameter = SerializeParameter(instance, overwrite);

            parameter.Add(KeyMessages, SerializeMessages());

            return parameter.ToString(formatting);
        }

        public virtual JArray SerializeMessages()
        {
            return MessagesCollector.MessageCombination();
        }
    }
}