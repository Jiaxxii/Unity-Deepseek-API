using System;
using Newtonsoft.Json;
using Xiyu.DeepSeekApi.Request;

namespace Xiyu.DeepSeekApi.Response
{
    /// <summary>
    /// 消息
    /// </summary>
    public readonly struct Message
    {
        /// <summary>
        /// 一般来说你不用关心这个构造函数，因为它是用于反序列化的
        /// </summary>
        /// <param name="content"></param>
        /// <param name="reasoningContent"></param>
        /// <param name="role"></param>
        [JsonConstructor]
        public Message(string content, string reasoningContent, RoleType role)
        {
            Content = content;
            ReasoningContent = reasoningContent;
            Role = role;
        }


        /// <summary>
        /// 该 completion 的内容
        /// </summary>
        [JsonProperty("content")]
        public string Content { get; }

        /// <summary>
        /// 仅适用于 deepseek-reasoner 模型。内容为 assistant 消息中在最终答案之前的推理内容。
        /// </summary>
        [JsonProperty("reasoning_content")]
        public string ReasoningContent { get; }

        /// <summary>
        /// 生成消息的角色
        /// </summary>
        [JsonProperty("role")]
        public RoleType Role { get; }

        public static Message None { get; } = new(string.Empty, string.Empty, RoleType.None);
    }
}