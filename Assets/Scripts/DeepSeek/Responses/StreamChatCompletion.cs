using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xiyu.DeepSeek.Messages;
using Xiyu.DeepSeek.Responses.ToolResult;

namespace Xiyu.DeepSeek.Responses
{
    [DebuggerDisplay("[{CurrentDelta.Role}] -token:{Usage.Value.TotalTokens}- {CurrentDelta.Content} （{CurrentDelta.Content}）")]
    public readonly struct StreamChatCompletion : IValid
    {
        [JsonConstructor]
        public StreamChatCompletion(string id, IList<StreamChoices> choices, ChatModel model, int created, string systemFingerprint, string @object, Usage? usage, Error? error)
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

        public static JsonSerializerSettings DeSerializerSettings => ChatCompletion.DeSerializerSettings;


#if UNITY_EDITOR
        public Delta CurrentDelta => Choices[0].Delta;
#endif

        /// <summary>
        /// 该对话的唯一标识符。
        /// </summary>
        public string ID { get; }


        public IList<StreamChoices> Choices { get; }

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
        /// 如果开启了流式并且 new StreamOptions(includeUsage:true) 在流式消息最后的 data: [DONE] 之前将会传输一个额外的块。
        /// 此块上的 usage 字段显示整个请求的 token 使用统计信息，而 choices 字段将始终是一个空数组。
        /// 所有其他块也将包含一个 usage 字段，但其值为 null。
        /// </summary>
        public Usage? Usage { get; }


        public Error? Error { get; }

        public bool IsValid() => Error == null && !string.IsNullOrWhiteSpace(ID);

        /// <summary>
        ///  使用于 includeUsage == false
        /// </summary>
        /// <param name="lastCompletion">最后一个数据块</param>
        /// <param name="roleType">不能是最后一个数据块中的类型</param>
        /// <param name="content">完整的回答</param>
        /// <param name="reasoningContent">完整的思考内容</param>
        /// <param name="usage">最后一个数据的块</param>
        /// <param name="tools">最后一个数据的块</param>
        /// <returns></returns>
        public static ChatCompletion CountChatCompletion(StreamChatCompletion lastCompletion, Role roleType, string content, string reasoningContent, Usage usage,
            IList<Tool> tools)
        {
            var choices = new List<Choices>(1)
            {
                new(lastCompletion.Choices[0].FinishReason, lastCompletion.Choices[0].Index, new Message(content, reasoningContent, roleType, tools), null)
            };
            return new ChatCompletion(lastCompletion.ID, choices, lastCompletion.Model, lastCompletion.Created, lastCompletion.SystemFingerprint, lastCompletion.Object,
                usage, null);
        }
    }
}