using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Xiyu.DeepSeekApi.Request
{
    /// <summary>
    /// 专门用于 Deepseek-chat 的请求
    /// </summary>
    public class ChatRequest : IRequestBody
    {
        public ChatRequest(params IMessageUnit[] messageUnits)
        {
            Messages = new MessageCollector(messageUnits);
        }

        public ChatRequest(IMessageUnit messageUnit)
        {
            Messages = new MessageCollector(messageUnit);
        }

        public ChatRequest()
        {
            Messages = new MessageCollector();
        }


        #region 属性

        public IMessageUnits Messages { get; set; }


        [JetBrains.Annotations.UsedImplicitly] public ModelType Model => ModelType.DeepseekChat;


        /// <summary>
        /// 介于 -2.0 和 2.0 之间的数字。如果该值为正，那么新 token 会根据其在已有文本中的出现频率受到相应的惩罚，降低模型重复相同内容的可能性。
        /// </summary>
        public float Frequency { get; set; } = 0F;


        public int MaxTokens { get; set; } = 4096;

        public float PresencePenalty { get; set; } = 0F;

        public ResponseFormatType ResponseFormat { get; } = ResponseFormatType.Text;


        public StopOptions StopOptions { get; set; }


        public StreamOptions? StreamOptions { get; set; }


        /// <summary>
        /// 采样温度，介于 0 和 2 之间。更高的值，如 0.8，会使输出更随机，而更低的值，如 0.2，会使其更加集中和确定。
        /// 我们通常建议可以更改这个值或者更改 top_p，但不建议同时对两者进行修改。
        /// </summary>
        public float Temperature { get; set; } = 0F;


        /// <summary>
        /// 作为调节采样温度的替代方案，模型会考虑前 top_p 概率的 token 的结果。
        /// 所以 0.1 就意味着只有包括在最高 10% 概率中的 token 会被考虑。
        /// 我们通常建议修改这个值或者更改 temperature，但不建议同时对两者进行修改。
        /// </summary>
        public float TopP { get; set; } = 1F;

        #endregion


        public string ToJson(bool stream, JObject instance = null, Formatting formatting = Formatting.None)
        {
            instance ??= new JObject();

            if (Messages is not null)
            {
                instance.Add("messages", new JArray(Messages.Messages.Select(x => JObject.Parse(x.ToJson()))));
            }

            instance.Add("model", JToken.Parse(JsonConvert.SerializeObject(Model, new StringEnumConverter())));


            if (Frequency < 0)
            {
                instance.Add("frequency_penalty", Frequency);
            }

            instance.Add("frequency_penalty", Frequency is >= -2F and <= 2F ? Frequency : 0);

            instance.Add("max_tokens", MaxTokens is >= 1 and <= 8192 ? MaxTokens : 4096);

            instance.Add("presence_penalty", PresencePenalty is >= -2F and <= 2F ? PresencePenalty : 0F);

            instance.Add("response_format", new JObject { { "type", JToken.Parse(JsonConvert.SerializeObject(ResponseFormat, new StringEnumConverter())) } });

            if (StopOptions is not null)
            {
                instance.Add("stop", new JArray(StopOptions));
            }

            if (stream)
            {
                instance.Add("stream", true);

                if (StreamOptions is not null)
                {
                    instance.Add("stream_options", JObject.FromObject(StreamOptions.Value));
                }
            }


            instance.Add("temperature", Temperature is >= 0 and <= 2F ? Temperature : 1F);

            instance.Add("top_p", TopP >= 1 ? TopP : 1);

            return instance.ToString(formatting);
        }
    }
}