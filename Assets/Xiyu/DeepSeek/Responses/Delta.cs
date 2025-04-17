using System.Diagnostics;
using Newtonsoft.Json;

namespace Xiyu.DeepSeek.Responses
{
    [DebuggerDisplay("Role：{Role} Content：{Content} 思考：{ReasoningContent}")]
    public readonly struct Delta
    {
        [JsonConstructor]
        public Delta(string content, string reasoningContent, RoleType role)
        {
            Content = content;
            ReasoningContent = reasoningContent;
            Role = role;
        }

        /// <summary>
        /// 该 completion 的内容。
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// 仅适用于 deepseek-reasoner 模型。内容为 assistant 消息中在最终答案之前的推理内容。
        /// </summary>
        public string ReasoningContent { get; }

        /// <summary>
        /// 生成这条消息的角色。
        /// </summary>
        public RoleType Role { get; }
    }
}