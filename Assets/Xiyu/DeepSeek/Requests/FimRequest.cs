using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Xiyu.DeepSeek.Requests.CommonRequestDataInterface;

namespace Xiyu.DeepSeek.Requests
{
    [Serializable]
    public sealed class FimRequest : RequestBody, IChatFimCommonRequestData
    {
        public FimRequest(string prompt, int maxToken = 512) : base(ChatModel.Chat)
        {
            this.prompt = prompt;

            // 如果只是简单的补全建议调小 Token
            MaxToken = maxToken;
        }

        public const string KeyEcho = "echo";
        public const string KeyPrompt = "prompt";
        public const string KeySuffix = "suffix";
        public const string KeyFrequencyPenalty = "frequency_penalty";
        public const string KeyPresencePenalty = "presence_penalty";
        public const string KeyTemperature = "temperature";
        public const string KeyTopP = "top_p";
        public const string KeyLogprobs = "logprobs";

        [SerializeField] private bool echo;

        /// <summary>
        /// 完整内容 = (<see cref="Prompt"/> + 生成内容 + <see cref="Suffix"/>)
        /// </summary>
        public bool Echo
        {
            get => echo;
            set => echo = value;
        }

        [SerializeField] [TextArea(3, 5)] private string prompt;

        /// <summary>
        /// 用于生成完成内容的提示
        /// </summary>
        public string Prompt
        {
            get => prompt;
            set => prompt = value;
        }

        [SerializeField] [TextArea(3, 5)] private string suffix;

        /// <summary>
        /// 制定被补全内容的后缀。
        /// </summary>
        public string Suffix
        {
            get => suffix;
            set => suffix = value;
        }


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


        [SerializeField] [Range(0F, 1F)] private float topP = 1;

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
            get => logprobs;
            set => logprobs = value;
        }


        [SerializeField] [Range(0, 20)] private int logprobs;

        /// <summary>
        /// 制定输出中包含 logprobs 最可能输出 token 的对数概率，包含采样的 token。
        /// 例如，如果 logprobs 是 20，API 将返回一个包含 20 个最可能的 token 的列表。
        /// API 将始终返回采样 token 的对数概率，因此响应中可能会有最多 logprobs+1 个元素。
        /// logprobs 的最大值是 20。
        /// </summary>
        public int Logprobs
        {
            get => logprobs;
            set => logprobs = value;
        }

        public override string SerializeRequestJson(JObject instance = null, Formatting formatting = Formatting.None, bool overwrite = false)
        {
            var parameter = SerializeParameter(instance, overwrite);

            // 不序列化消息列表

            return parameter.ToString(formatting);
        }

        public override JObject SerializeParameter(JObject instance = null, bool overwrite = false)
        {
            var jObject = base.SerializeParameter(instance, overwrite);

            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("Prompt is empty", nameof(prompt));

            jObject.Add(KeyPrompt, prompt);

            jObject.Add(KeyEcho, false);

            if (string.IsNullOrWhiteSpace(suffix))
            {
                jObject.Remove(KeySuffix);
            }
            else jObject.Add(KeySuffix, suffix);


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

            if (Mathf.Approximately(topP, 1))
            {
                jObject.Remove(KeyTopP);
            }
            else jObject.Add(KeyTopP, Mathf.Clamp(topP, 0, 1));


            if (Mathf.Approximately(temperature, 1))
            {
                jObject.Remove(KeyTemperature);
            }
            else jObject.Add(KeyTemperature, Mathf.Clamp(presencePenalty, 0, 2));

            if (logprobs == 0)
            {
                jObject.Remove(KeyLogprobs);
            }
            else jObject.Add(KeyLogprobs, logprobs);


            return jObject;
        }
    }
}