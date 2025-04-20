using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Xiyu.DeepSeek.Messages;
using Xiyu.DeepSeek.Responses.ToolResult;

namespace Xiyu.DeepSeek.Responses
{
    [DebuggerDisplay("[{Role}] msg-{Content} ({ReasoningContent})")]
    [DebuggerDisplay("tool({ToolCalls.Count})：[{ToolsFunctionNameDisplay()}]")]
    public readonly struct Delta
    {
        [JsonConstructor]
        public Delta(string content, string reasoningContent, Role role, IList<Tool> toolCalls)
        {
            Content = content;
            ReasoningContent = reasoningContent;
            Role = role;
            ToolCalls = toolCalls;
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
        public Role Role { get; }

        /// <summary>
        /// 模型生成的 tool 调用，例如 function 调用。
        /// </summary>
        public IList<Tool> ToolCalls { get; }

#if UNITY_EDITOR
        public string ToolsFunctionNameDisplay() => string.Join(',', ToolCalls.Select(t => t.Function.Name));
#endif
    }
}