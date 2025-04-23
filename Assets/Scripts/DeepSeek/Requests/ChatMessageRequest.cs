using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Xiyu.DeepSeek.Requests.CommonRequestDataInterface;
using Xiyu.DeepSeek.Requests.Tools;

namespace Xiyu.DeepSeek.Requests
{
    [Serializable]
    public sealed class ChatMessageRequest : ReasonerMessageRequest, IChatFimCommonRequestData
    {
        public ChatMessageRequest(ChatModel model, MessagesCollector collector) : base(model, collector)
        {
        }


        private const string KeyFrequencyPenalty = "frequency_penalty";
        private const string KeyPresencePenalty = "presence_penalty";
        private const string KeyTemperature = "temperature";
        private const string KeyTopP = "top_p";
        private const string KeyLogprobs = "logprobs";
        private const string KeyTopLogprobs = "top_logprobs";
        private const string KeyTools = "tools";
        private const string KeyToolChoice = "tool_choice";

        [SerializeField] [Range(-2F, 2F)] private float frequencyPenalty;

        /// <summary>
        /// 介于 -2.0 和 2.0 之间的数字。如果该值为正，那么新 token 会根据其在已有文本中的出现频率受到相应的惩罚，降低模型重复相同内容的可能性。
        /// </summary>
        public float FrequencyPenalty
        {
            get => frequencyPenalty;
            set => frequencyPenalty = value;
        }


        [SerializeField] [Range(-2F, 2F)] private float presencePenalty;

        /// <summary>
        /// 介于 -2.0 和 2.0 之间的数字。如果该值为正，那么新 token 会根据其是否已在已有文本中出现受到相应的惩罚，从而增加模型谈论新主题的可能性。
        /// </summary>
        public float PresencePenalty
        {
            get => presencePenalty;
            set => presencePenalty = value;
        }


        [SerializeField] [Range(0F, 2F)] private float temperature = 1;

        /// <summary>
        /// 采样温度，介于 0 和 2 之间。更高的值，如 0.8，会使输出更随机，而更低的值，如 0.2，会使其更加集中和确定。
        /// 我们通常建议可以更改这个值或者更改 top_p，但不建议同时对两者进行修改。
        /// </summary>
        public float Temperature
        {
            get => temperature;
            set => temperature = value;
        }


        [SerializeField] [Range(0.1F, 1F)] private float topP = 1;

        /// <summary>
        /// 作为调节采样温度的替代方案，模型会考虑前 top_p 概率的 token 的结果。
        /// 所以 0.1 就意味着只有包括在最高 10% 概率中的 token 会被考虑。
        /// 我们通常建议修改这个值或者更改 temperature，但不建议同时对两者进行修改。
        /// </summary>
        public float TopP
        {
            get => topP;
            set => topP = value;
        }


        int IChatFimCommonRequestData.LogprobsProperty
        {
            get => logprobs?.Logprobs ?? 0;
            set
            {
                logprobs ??= new LogprobsSettings(value);

                logprobs.Logprobs = value;
            }
        }


        [SerializeField] private LogprobsSettings logprobs;

        /// <summary>
        /// 输出 token 的对数概率。
        /// </summary>
        public LogprobsSettings Logprobs
        {
            get => logprobs;
            set => logprobs = value;
        }

        /// <summary>
        /// 模型可能会调用的 tool 的列表。目前，仅支持 function 作为工具。
        /// 使用此参数来提供以 JSON 作为输入参数的 function 列表。最多支持 128 个 function。
        /// </summary>
        public IList<Tool<FunctionDescription>> Tools { get; set; }


        [SerializeField] private ToolChoice toolChoice;

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

            if (frequencyPenalty == 0)
            {
                jObject.Remove(KeyFrequencyPenalty);
            }
            else jObject.Add(KeyFrequencyPenalty, Mathf.Clamp(frequencyPenalty, -2, 2));

            if (presencePenalty == 0)
            {
                jObject.Remove(KeyPresencePenalty);
            }
            else jObject.Add(KeyPresencePenalty, Mathf.Clamp(presencePenalty, -2, 2));


            if (Mathf.Approximately(temperature, 1))
            {
                jObject.Remove(KeyTemperature);
            }
            else jObject.Add(KeyTemperature, Mathf.Clamp(presencePenalty, 0, 2));

            if (Mathf.Approximately(topP, 1))
            {
                jObject.Remove(KeyTopP);
            }
            else jObject.Add(KeyTopP, Mathf.Clamp(topP, 0.1F, 1));


            if (logprobs == null || logprobs.Ignore || logprobs.Logprobs == 0)
            {
                jObject.Remove(KeyLogprobs);
                jObject.Remove(KeyTopLogprobs);
            }
            else
            {
                jObject.Add(KeyLogprobs, true);
                jObject.Add(KeyTopLogprobs, Mathf.Clamp(logprobs.Logprobs, 0, 20));
            }

            if (Tools == null)
            {
                jObject.Remove(KeyTools);
                jObject.Remove(KeyToolChoice);
                return jObject;
            }

            if (Tools.Count == 0 && toolChoice is { CallType: CallType.Required })
            {
                throw new ArgumentException("你指定了模型必须调用工具，但是你没有定义任何工具！");
            }

            if (toolChoice != null && !string.IsNullOrWhiteSpace(toolChoice.FunctionName))
            {
                var token = new JObject
                {
                    ["type"] = "function",
                    ["function"] = new JObject
                    {
                        ["name"] = toolChoice.FunctionName,
                    }
                };
                jObject.Add(KeyToolChoice, token);
            }
            else
            {
                if (toolChoice == null)
                {
                    jObject.Remove(KeyToolChoice);
                    jObject.Add(KeyTools, JArray.FromObject(Tools, JsonSerializer));
                    return jObject;
                }

                if (toolChoice.CallType == CallType.None)
                {
                    jObject.Remove(KeyToolChoice);
                    jObject.Remove(KeyTools);
                    return jObject;
                }

                if (toolChoice.CallType == CallType.Auto)
                {
                    jObject.Remove(KeyToolChoice);
                }
                else
                {
                    jObject.Add(KeyToolChoice, toolChoice.CallType.ToString().ToLower());
                }
            }

            jObject.Add(KeyTools, JArray.FromObject(Tools, JsonSerializer));


            return jObject;
        }

        public ReasonerMessageRequest ToDeepseekReasoner()
        {
            Model = ChatModel.Reasoner;
            return this;
        }
    }
}