using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Xiyu.DeepSeekApi.Request
{
    /// <summary>
    /// 注意*DeepseekReasoner*模型不支持的参数：temperature、top_p、presence_penalty、frequency_penalty、logprobs、top_logprobs。
    /// </summary>
    public class ReasonerRequest : IRequestBody
    {
        public ReasonerRequest(IMessageUnits messageUnit)
        {
            Messages = messageUnit ?? throw new NullReferenceException("The message unit is null.");
        }


        public IMessageUnits Messages { get; set; }
        public ModelType Model => ModelType.DeepseekReasoner;
        public int MaxTokens { get; set; } = 4096;
        public ResponseFormatType ResponseFormat { get; } = ResponseFormatType.Text;
        public StopOptions StopOptions { get; set; }
        public StreamOptions? StreamOptions { get; set; }

        public string ToJson(bool stream, JObject instance = null, Formatting formatting = Formatting.None)
        {
            instance ??= new JObject();

            instance.Add("messages", new JArray(Messages.Messages.Select(x => JObject.Parse(x.ToJson()))));

            instance.Add("model", JToken.Parse(JsonConvert.SerializeObject(Model, new StringEnumConverter())));


            instance.Add("max_tokens", MaxTokens is >= 1 and <= 8192 ? MaxTokens : 4096);


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


            return instance.ToString(formatting);
        }
    }
}