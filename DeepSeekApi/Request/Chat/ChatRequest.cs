using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Xiyu.DeepSeekApi.Request.Chat
{
    /// <summary>
    /// 专门用于 Deepseek-chat 的请求
    /// </summary>
    public class ChatRequest : IRequestBody
    {
        /// <summary>
        /// 初始化请求体，可以传入消息收集器
        /// </summary>
        /// <param name="messages">消息收集器</param>
        public ChatRequest(IMessageUnits messages = null)
        {
            Messages = messages;
        }


        #region 属性

        /// <summary>
        /// 消息收集器
        /// </summary>
        public IMessageUnits Messages { get; set; }

        /// <summary>
        /// 使用的模型
        /// <para>(这里设置为只读，我不知道发送后还能不能修改，应该没问题)</para>
        /// </summary>
        [JetBrains.Annotations.UsedImplicitly]
        public ModelType Model => ModelType.DeepseekChat;

        /// <summary>
        /// 介于 -2.0 和 2.0 之间的数字。如果该值为正，那么新 token 会根据其在已有文本中的出现频率受到相应的惩罚，降低模型重复相同内容的可能性。
        /// </summary>
        public float Frequency { get; set; } = 0F;


        /// <summary>
        /// 介于 1 到 8192 间的整数，限制一次请求中模型生成 completion 的最大 token 数。
        /// 输入 token 和输出 token 的总长度受模型的上下文长度的限制。如未指定 max_tokens参数，默认使用 4096。
        /// </summary>
        public int MaxTokens { get; set; } = 4096;


        /// <summary>
        /// 介于 -2.0 和 2.0 之间的数字。如果该值为正，那么新 token 会根据其是否已在已有文本中出现受到相应的惩罚，从而增加模型谈论新主题的可能性。
        /// </summary>
        public float PresencePenalty { get; set; } = 0F;


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

        /// <summary>
        /// 将请求体转换为 JSON 字符串，作用于 Body
        /// </summary>
        /// <param name="instance">允许传入 <see cref="JObject"/> 实例</param>
        /// <param name="formatting">JSON格式 （默认为：工整的）</param>
        /// <returns></returns>
        public string ToJson(JObject instance = null, Formatting formatting = Formatting.None)
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

            if (StreamOptions is not null)
            {
                instance.Add("stream", true);
                instance.Add("stream_options", JObject.FromObject(StreamOptions.Value));
            }

            instance.Add("temperature", Temperature is >= 0 and <= 2F ? Temperature : 1F);

            instance.Add("top_p", TopP >= 1 ? TopP : 1);

            return instance.ToString(formatting);
        }
    }
}