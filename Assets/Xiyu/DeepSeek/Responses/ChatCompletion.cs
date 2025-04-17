using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Xiyu.DeepSeek.Responses
{
    public interface IValid
    {
        /// <summary>
        /// 错误信息
        /// </summary>
        Error? Error { get; }

        /// <summary>
        /// 当 <see cref="Error"/> 为 null 时该方法固定返回 False
        /// 否则判断当前结构体是否包含关键值
        /// </summary>
        /// <returns>返回结构体是否具备有效值</returns>
        bool IsValid();
    }

    [DebuggerDisplay("Role：{Choices[0].Message.Role} Content：{Choices[0].Message.Content} 思考：{Choices[0].Message.ReasoningContent}")]
    [DebuggerDisplay("Usage：{Usage}")]
    public readonly struct ChatCompletion : IValid
    {
        [JsonConstructor]
        public ChatCompletion(string id, IList<Choices> choices, ChatModel model, int created, string systemFingerprint, string @object, Usage usage, Error? error)
        {
            ID = id;
            Choices = choices;
            Model = model;
            Created = created;
            SystemFingerprint = systemFingerprint;
            Object = @object;
            Usage = usage;
            Error = error;
        }

        public static JsonSerializerSettings DeSerializerSettings { get; } = new()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };


        /// <summary>
        /// 该对话的唯一标识符。
        /// </summary>
        public string ID { get; }


        public IList<Choices> Choices { get; }


        /// <summary>
        /// 使用的模型
        /// </summary>
        public ChatModel Model { get; }

        /// <summary>
        /// 创建聊天完成时的 Unix 时间戳（秒）
        /// </summary>
        public int Created { get; }


        /// <summary>
        /// 此指纹表示模型运行时使用的后端配置。
        /// </summary>
        public string SystemFingerprint { get; }

        public string Object { get; }

        /// <summary>
        /// 该对话补全请求的用量信息。
        /// </summary>
        public Usage Usage { get; }


        public Error? Error { get; }

        public bool IsValid() => Error == null && !string.IsNullOrWhiteSpace(ID);
    }
}