#if DEEPSEEK_PAST_CODE
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Xiyu.DeepSeekResult.DeepSeekApi.Request
{
    /// <summary>
    /// AI 助手消息
    /// </summary>
    public readonly struct AssistantMessage : IMessageUnit
    {
        /// <summary>
        /// 多数情况下，您不需要直接创建 AssistantMessage 对象，如果需要也可以使用 <see cref="ContinueWithPrefix"/> 方法
        /// </summary>
        /// <param name="content">ai 回复的消息（可以“造假”）</param>
        /// <param name="name"></param>
        /// <param name="reasoningContent"></param>
        /// <param name="prefix"></param>
        public AssistantMessage([NotNull] string content, string name = null, string reasoningContent = null, bool prefix = false)
        {
            Content = content;
            Name = name;
            ReasoningContent = reasoningContent;
            Prefix = prefix;
        }

        #region 属性

        /// <summary>
        /// 助手消息的内容（deepSeek的回答）
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// 该消息的发送者, 为助手
        /// </summary>
        public RoleType Role => RoleType.Assistant;

        public string Name { get; }


        /// <summary>
        /// (Beta)
        /// 强制模型在其回答中以此 assistant 消息中提供的前缀内容开始。您必须设置 base_url="https://api.deepseek.com/beta" 来使用此功能。
        /// 用于 deepseek-reasoner 模型在对话前缀续写功能下，作为最后一条 assistant 思维链内容的输入。
        /// <para>不要传入JSON</para>
        /// </summary>
        [CanBeNull]
        [JsonIgnore]
        [JsonProperty(PropertyName = "reasoning_content")]
        public string ReasoningContent { get; }


        /// <summary>
        /// 如果为 true，则强制模型在其回答中以此 assistant 消息中提供的前缀内容开始。
        /// 此时，消息的最后类型必须是 AssistantMessage。
        /// </summary>
        [JsonProperty(PropertyName = "prefix")]
        public bool Prefix { get; }

        #endregion

        #region 方法

        public string ToJson(Formatting formatting = Formatting.Indented)
        {
            // 官方说 不要将思维链内容传入 JSON
            // if (!string.IsNullOrEmpty(ReasoningContent) && Prefix)
            // {
            //     var fromObject = JObject.FromObject(this, new JsonSerializer { NullValueHandling = NullValueHandling.Ignore });
            //     fromObject.Add("reasoning_content", ReasoningContent);
            // }

            return JsonConvert.SerializeObject(this, formatting, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        /// <summary>
        /// 创建一个带有前缀的助手消息
        /// <code>
        /// // 通过此方法达到的效果相同：
        /// new AssistantMessage("我了解，1+1=", prefix: true);
        /// </code>
        /// </summary>
        /// <param name="prefix">前缀内容（ai 会以这个内容开头）</param>
        /// <returns></returns>
        public static AssistantMessage ContinueWithPrefix(string prefix) => new(prefix, prefix: true);

        #endregion
    }
}
#endif