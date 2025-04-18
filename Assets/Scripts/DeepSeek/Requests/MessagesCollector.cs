using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.Collections;
using UnityEngine;
using Xiyu.DeepSeek.Messages;

namespace Xiyu.DeepSeek.Requests
{
#if UNITY_EDITOR
    [Serializable]
#endif
    public class MessagesCollector : IMessagesCollector
    {
#if UNITY_EDITOR
        [SerializeField] [ReadOnly] private List<Message> readOnlyMessage = new();
#endif

        public MessagesCollector(params IMessage[] messages)
        {
            Messages = new List<IMessage>(messages);
        }

        public MessagesCollector(params Message[] messages)
        {
            Messages = new List<IMessage>(messages);
        }

        public MessagesCollector(UserMessage userMessage)
        {
            Messages = new List<IMessage> { userMessage };
        }

        public MessagesCollector(SystemMessage userMessage)
        {
            Messages = new List<IMessage> { userMessage };
        }

        public MessagesCollector()
        {
            Messages = new List<IMessage>();
        }


        public IList<IMessage> Messages { get; }

        public void CheckAndThrow()
        {
            if (Messages == null || Messages.Count == 0)
                throw new NullReferenceException("没有任何消息！");

            var last = Messages.Last();
            if (last.Role == Role.Assistant && last is not AssistantPrefixMessage)
            {
                throw new MessagesSortingRulesException("当模式非对话前缀续写时最后一条必须是用户消息！", Messages[0]);
            }
        }

        public void Append(IMessage message)
        {
            if (string.IsNullOrEmpty(message.Content))
            {
                throw new ArgumentException("The message content is empty.", nameof(message));
            }

#if UNITY_EDITOR
            switch (message)
            {
                case ToolMessage toolMessage:
                    readOnlyMessage.Add(new Message(Role.Tool, toolMessage.ToolCallId));
                    break;
                case AssistantMessage assistantMessage:
                    readOnlyMessage.Add(new AssistantMessage(assistantMessage.Content));
                    break;
                default:
                    readOnlyMessage.Add((Message)message);
                    break;
            }
#endif

            Messages.Add(message);
        }


        public void AppendSystemMessage(string message, string name = null)
        {
            Append(new Message(Role.System, message, name));
        }

        public void AppendUserMessage(string message, string name = null)
        {
            Append(new Message(Role.User, message, name));
        }

        public void AppendAssistantMessage(string message, string name = null)
        {
            Append(new Message(Role.Assistant, message, name));
        }

        public void AppendAssistantPrefixMessage(string message, bool jointPrefix, string name = null, string reasoningContent = null)
        {
            Messages.Add(new AssistantPrefixMessage(reasoningContent, jointPrefix, message, name)
            {
                Name = name
            });
        }

        public void AppendToolMessage(string message, string toolCallId, ISerializer serializer = null)
        {
            if (string.IsNullOrWhiteSpace(toolCallId))
                throw new ArgumentException("The tool call id is empty.", nameof(toolCallId));

            Append(new ToolMessage(message, toolCallId, serializer));
        }


        public string SerializeJson(Formatting formatting = Formatting.None)
        {
            return MessageCombination().ToString(formatting);
        }

        public JArray MessageCombination()
        {
            return new JArray(Messages.Select(m => m.Serializer.SerializeJson(m)));
        }
    }
}