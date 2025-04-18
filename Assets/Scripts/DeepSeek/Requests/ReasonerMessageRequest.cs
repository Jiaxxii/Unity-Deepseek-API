using System;
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
        private const string KeyTools = "tools";
        private const string KeyToolChoice = "tool_choice";


        [SerializeField] protected ResponseFormat responseFormat = ResponseFormat.Text;

        /// <summary>
        /// 指定模型必须输出的格式。
        /// </summary>
        public ResponseFormat ResponseFormat
        {
            get => responseFormat;
            set => responseFormat = value;
        }


        /// <summary>
        /// 模型可能会调用的 tool 的列表。目前，仅支持 function 作为工具。
        /// 使用此参数来提供以 JSON 作为输入参数的 function 列表。最多支持 128 个 function。
        /// </summary>
        public Tool<Function> Tools { get; set; }


        [SerializeField] protected ToolChoice toolChoice;

        /// <summary>
        /// 控制模型调用 tool 的行为。
        /// </summary>
        public ToolChoice ToolChoice
        {
            get => toolChoice;
            set => toolChoice = value;
        }


        public override JObject SerializeParameter(JObject instance = null, bool overwrite = false)
        {
            var jObject = base.SerializeParameter(instance, overwrite);

            if (responseFormat == ResponseFormat.Text)
            {
                jObject.Remove(KeyResponseFormat);
            }
            else jObject.Add(KeyResponseFormat, "json_object");


            if (toolChoice == null || toolChoice.ChatCompletionToolChoice == ChatCompletionToolChoice.None)
            {
                jObject.Remove(KeyToolChoice);
                return jObject;
            }

            if (toolChoice.ChatCompletionNameToolChoice == null)
            {
                jObject.Add(KeyTools, toolChoice.ChatCompletionToolChoice.ToString().ToLower());
            }
            else
            {
                jObject.Add(KeyTools, JObject.FromObject(toolChoice.ChatCompletionNameToolChoice, JsonSerializer));
            }


            if (Tools == null)
            {
                jObject.Remove(KeyTools);
            }
            else
            {
                jObject.Add(KeyTools, JObject.FromObject(Tools, JsonSerializer));
            }


            return jObject;
        }

        public ChatMessageRequest ToChatMessageRequest()
        {
            Model = ChatModel.Chat;
            return (ChatMessageRequest)this;
        }
    }
}