using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Xiyu.DeepSeekApi.Request;

namespace Xiyu.DeepSeekApi.DeepseekReasoner
{
    /// <summary>
    /// 注意*DeepseekReasoner*模型不支持的参数：temperature、top_p、presence_penalty、frequency_penalty、logprobs、top_logprobs。
    /// </summary>
    public class ReasonerRequest : IRequestBody
    {
        public ReasonerRequest(IMessageUnits messages = null)
        {
            Messages = messages;
        }

        public IMessageUnits Messages { get; set; }

        /// <summary>
        /// 使用的模型
        /// <para>(这里设置为只读，我不知道发送后还能不能修改，应该没问题)</para>
        /// </summary>
        [JetBrains.Annotations.UsedImplicitly]
        public ModelType Model => ModelType.DeepseekReasoner;


        /// <summary>
        /// 介于 1 到 8192 间的整数，限制一次请求中模型生成 completion 的最大 token 数。
        /// 输入 token 和输出 token 的总长度受模型的上下文长度的限制。如未指定 max_tokens参数，默认使用 4096。
        /// </summary>
        public int MaxTokens { get; set; } = 4096;


        /// <summary>
        /// 一个 object，指定模型必须输出的格式。设置为 { "type": "json_object" } 以启用 JSON 模式，该模式保证模型生成的消息是有效的 JSON。
        /// <para>***注意***: 使用 JSON 模式时，你还必须通过系统或用户消息指示模型生成 JSON。
        /// 否则，模型可能会生成不断的空白字符，直到生成达到令牌限制，从而导致请求长时间运行并显得“卡住”。
        /// 此外，如果 finish_reason="length"，这表示生成超过了 max_tokens 或对话超过了最大上下文长度，消息内容可能会被部分截断。
        /// </para>
        /// </summary>
        public ResponseFormatType ResponseFormat { get; } = ResponseFormatType.Text;


        /// <summary>
        /// 停止词，AI在生成时遇到这些词会停止生成更多的 token。
        /// </summary>
        public StopOptions StopOptions { get; set; }


        /// <summary>
        /// 如果不为空 ，将会以 SSE（server-sent events）的形式以流式发送消息增量。消息流以 data: [DONE] 结尾。
        /// </summary>
        public StreamOptions? StreamOptions { get; set; }


        public string ToJson(JObject instance = null, Formatting formatting = Formatting.None)
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

            if (StreamOptions is not null && StreamOptions.Value.IncludeUsage)
            {
                instance.Add("stream", true);
                instance.Add("stream_options", JObject.FromObject(StreamOptions.Value));
            }

            return instance.ToString(formatting);
        }
    }
}