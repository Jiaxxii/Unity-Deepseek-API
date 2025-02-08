using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Xiyu.DeepSeekApi.Request
{
    /// <summary>
    /// 系统消息，一般用于定义 prompt 提前告诉ai应该做什么
    /// <code>
    /// var chatRequest = new ChatRequest(ModelType.DeepseekChat, APIKey)
    ///     {
    ///         Messages = new List&lt;IMessageUnit>
    ///             {
    ///                 new SystemMessage("你是一只可爱的猫娘，名字叫“西”，再和我聊天时不要提起你是AI或者我是一个人工智能！"
    ///                 new UserMessage("晚上好，西！")
    ///             }
    ///     };
    /// </code>
    /// </summary>
    public readonly struct SystemMessage : IMessageUnit
    {
        /// <summary>
        /// 消息的内容
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// 该消息的发送者, 为系统
        /// </summary>
        public RoleType Role => RoleType.System;

        /// <summary>
        /// 可以选填的参与者的名称，为模型提供信息以区分相同角色的参与者。
        /// </summary>
        public string Name { get; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="content">提示消息</param>
        /// <param name="name">参与者名称</param>
        public SystemMessage([NotNull] string content, string name = null)
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