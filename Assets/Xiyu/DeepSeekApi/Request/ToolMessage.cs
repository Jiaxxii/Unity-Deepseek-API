using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Xiyu.DeepSeekApi.Request
{
    /// <summary>
    /// ***未知***
    /// </summary>
    public readonly struct ToolMessage : IMessageUnit
    {
        /// <summary>
        /// 工具的消息的内容
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// 该消息的发送者, 为工具
        /// </summary>
        public RoleType Role => RoleType.Tool;


        /// <summary>
        /// 此消息所响应的 tool call 的 ID。
        /// </summary>
        [JsonProperty(PropertyName = "tool_call_id")]
        public string Name { get; }


        public ToolMessage([NotNull] string content, [NotNull] string name)
        {
            Content = content;
            Name = name;
        }


        public string ToJson(Formatting formatting = Formatting.Indented) => JsonConvert.SerializeObject(this, formatting, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        });
    }
}