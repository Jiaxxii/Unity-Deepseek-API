using System.Diagnostics;
using Newtonsoft.Json;
using UnityEngine;

namespace Xiyu.DeepSeek.Messages
{
    /// <summary>
    /// (Beta) 设置此参数为 true，来强制模型在其回答中以此 assistant 消息中提供的前缀内容开始。
    /// </summary>
    [System.Serializable]
    [DebuggerDisplay("Role:{Role} Content:{Content} Prefix:{Prefix}")]
    public class AssistantPrefixMessage : AssistantMessage
    {
        [SerializeField] private string reasoningContent;

        // ReSharper disable once MemberInitializerValueIgnored
        // UNITY Serializable

        [SerializeField] private bool jointPrefix = true;

        public AssistantPrefixMessage(string content, bool jointPrefix = true, string reasoningContent = null, string name = null) : base(content, name)
        {
            this.reasoningContent = reasoningContent;
            this.jointPrefix = jointPrefix;
        }

        public bool Prefix => true;

        [JsonIgnore] public bool IsJointPrefix => jointPrefix;

        /// <summary>
        /// (Beta) 用于 deepseek-reasoner 模型在对话前缀续写功能下，作为最后一条 assistant 思维链内容的输入。
        /// </summary>
        public string ReasoningContent
        {
            get => reasoningContent;
            set => reasoningContent = value;
        }
    }
}