using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Xiyu.DeepSeekApi.Request
{
    /// <summary>
    /// 请求体接口 （用于将实体类转换为 JSON 对象）
    /// </summary>
    public interface IRequestBody
    {
        /// <summary>
        /// 消息列表
        /// </summary>
        public IMessageUnits Messages { get; }


        /// <summary>
        /// 使用的模型
        /// <para>(这里设置为只读，我不知道发送后还能不能修改，应该没问题)</para>
        /// </summary>
        public ModelType Model { get; }


        /// <summary>
        /// 介于 1 到 8192 间的整数，限制一次请求中模型生成 completion 的最大 token 数。
        /// 输入 token 和输出 token 的总长度受模型的上下文长度的限制。如未指定 max_tokens参数，默认使用 4096。
        /// </summary>
        public int MaxTokens { get; set; }

        /// <summary>
        /// 一个 object，指定模型必须输出的格式。设置为 { "type": "json_object" } 以启用 JSON 模式，该模式保证模型生成的消息是有效的 JSON。
        /// <para>***注意***: 使用 JSON 模式时，你还必须通过系统或用户消息指示模型生成 JSON。
        /// 否则，模型可能会生成不断的空白字符，直到生成达到令牌限制，从而导致请求长时间运行并显得“卡住”。
        /// 此外，如果 finish_reason="length"，这表示生成超过了 max_tokens 或对话超过了最大上下文长度，消息内容可能会被部分截断。
        /// </para>
        /// </summary>
        ResponseFormatType ResponseFormat { get; }


        /// <summary>
        /// 停止词，AI在生成时遇到这些词会停止生成更多的 token。
        /// </summary>
        StopOptions StopOptions { get; set; }


        /// <summary>
        /// 如果不为空 ，将会以 SSE（server-sent events）的形式以流式发送消息增量。消息流以 data: [DONE] 结尾。
        /// </summary>
        StreamOptions? StreamOptions { get; set; }


        /// <summary>
        /// 将实体类转换为 JSON 对象
        /// </summary>
        /// <param name="instance">允许传入一个 <see cref="JObject"/> 实例</param>
        /// <param name="formatting">JSON 格式（默认为：工整的）</param>
        /// <param name="stream">开启流式调用</param>
        /// <returns></returns>
        string ToJson(bool stream, JObject instance = null, Formatting formatting = Formatting.None);
    }
}