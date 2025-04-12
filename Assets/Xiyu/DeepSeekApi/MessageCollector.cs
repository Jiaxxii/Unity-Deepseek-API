using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xiyu.DeepSeekApi.Request;

namespace Xiyu.DeepSeekApi
{
    /// <summary>
    /// 消息收集器-
    /// </summary>
    [JetBrains.Annotations.PublicAPI]
    public sealed class MessageCollector : IEnumerable<IMessageUnit>, IMessageUnits
    {
        /// <summary>
        /// 初始化消息收集器，可以传入多个消息
        /// </summary>
        /// <param name="messages">可添加的类型有：<see cref="SystemMessage"/> <see cref="UserMessage"/> <see cref="AssistantMessage"/></param>
        public MessageCollector(params IMessageUnit[] messages)
        {
            Messages.AddRange(messages);
        }

        /// <summary>
        /// 初始化消息收集器
        /// </summary>
        /// <param name="messages">可添加的类型有：<see cref="SystemMessage"/> <see cref="UserMessage"/> <see cref="AssistantMessage"/></param>
        public MessageCollector(IMessageUnit messages)
        {
            Messages.Add(messages);
        }


        /// <summary>
        /// 所有消息的集合
        /// <para>（虽然可以，但是还是不太建议赋新值）</para>
        /// </summary>
        public List<IMessageUnit> Messages { get; set; } = new();

        /// <summary>
        /// 添加一条消息
        /// <para>目前可添加的类型有：<see cref="SystemMessage"/> <see cref="UserMessage"/> <see cref="AssistantMessage"/></para>
        /// </summary>
        /// <param name="message">消息</param>
        /// <exception cref="ArgumentException"><see cref="message"/> 为 null</exception>
        public void AddMessage(IMessageUnit message)
        {
            if (message is null)
            {
                throw new ArgumentException("message is null");
            }

            Messages.Add(message);
        }

        public void AddMessageRange(params IMessageUnit[] messageUnits)
        {
            if (messageUnits.Any(m => m is null))
            {
                throw new ArgumentException("messageUnits is null");
            }

            Messages.AddRange(messageUnits);
        }

        public bool Check()
        {
            if (Messages == null || Messages.Count == 0)
                return false;

            if (Messages[^1].Role == RoleType.User)
                return true;

            return Messages[^1].Role == RoleType.Assistant && ((AssistantMessage)Messages[^1]).Prefix;
        }

        public void CheckAndThrow()
        {
            if (Messages == null || Messages.Count == 0)
                throw new NullReferenceException("没有任何消息！");

            var last = Messages.Last();
            if (last.Role == RoleType.User || (last.Role == RoleType.Assistant && ((AssistantMessage)last).Prefix))
                return;

            throw new ArgumentException("当最后一条消息是助时，Prefix必须为True", nameof(last));
        }

        /// <summary>
        /// 将消息收集器转换为 JObject
        /// </summary>
        /// <returns></returns>
        public JObject ToJObject() => new()
        {
            { "messages", new JArray(Messages.Select(x => JObject.Parse(x.ToJson()))) }
        };

        public override string ToString() => JsonConvert.SerializeObject(this);

        public IEnumerator<IMessageUnit> GetEnumerator() => Messages.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}