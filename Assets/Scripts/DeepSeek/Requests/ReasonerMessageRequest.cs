using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Xiyu.DeepSeek.Messages;
using Xiyu.DeepSeek.Requests.Tools;

namespace Xiyu.DeepSeek.Requests
{
    [Serializable]
    public class ReasonerMessageRequest : MessageRequest
    {
        public ReasonerMessageRequest(ChatModel model, IMessagesCollector collector) : base(model, collector)
        {
        }

        private const string KeyResponseFormat = "response_format";



        [SerializeField] protected ResponseFormat responseFormat = ResponseFormat.Text;

        /// <summary>
        /// 指定模型必须输出的格式。
        /// </summary>
        public ResponseFormat ResponseFormat
        {
            get => responseFormat;
            set => responseFormat = value;
        }





        public override JObject SerializeParameter(JObject instance = null, bool overwrite = false)
        {
            var jObject = base.SerializeParameter(instance, overwrite);

            if (responseFormat == ResponseFormat.Text)
            {
                jObject.Remove(KeyResponseFormat);
            }
            else jObject.Add(KeyResponseFormat, "json_object");
            

            return jObject;
        }

        public ChatMessageRequest ToChatMessageRequest()
        {
            Model = ChatModel.Chat;
            return (ChatMessageRequest)this;
        }
    }
}