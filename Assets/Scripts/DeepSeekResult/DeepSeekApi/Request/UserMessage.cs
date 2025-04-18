#if DEEPSEEK_PAST_CODE
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Xiyu.DeepSeekResult.DeepSeekApi.Request
{
    /// <summary>
    /// 用户消息
    /// </summary>
    public readonly struct UserMessage : IMessageUnit
    {
        /// <summary>
        /// 用户消息的内容
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// 该消息的发送者, 为用户
        /// </summary>
        public RoleType Role => RoleType.User;


        /// <summary>
        /// 供AI使用的名称，用于区分不同的用户
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="content">用户说的话</param>
        /// <param name="name">用户名称</param>
        public UserMessage([NotNull] string content, string name = null)
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
#endif