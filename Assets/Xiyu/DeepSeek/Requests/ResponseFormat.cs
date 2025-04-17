using System.Runtime.Serialization;

namespace Xiyu.DeepSeek.Requests
{
    public enum ResponseFormat
    {
        [EnumMember(Value = "text")]Text,
        /// <summary>
        /// 使用 JSON 模式时，你还必须通过系统或用户消息指示模型生成 JSON。否则，模型可能会生成不断的空白字符，直到生成达到令牌限制，从而导致请求长时间运行并显得“卡住”。
        /// 此外，如果 <see cref="Xiyu.DeepSeek.Responses.FinishReason"/> == <see cref="Xiyu.DeepSeek.Responses.FinishReason.Length"/>，这表示生成超过了 max_tokens 或对话超过了最大上下文长度，消息内容可能会被部分截断。
        /// </summary>
        [EnumMember(Value = "json_object")]  Json
    }
}