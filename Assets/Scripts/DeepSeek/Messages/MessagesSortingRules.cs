using System;
using Newtonsoft.Json;

namespace Xiyu.DeepSeek.Messages
{
    public class MessagesSortingRulesException : Exception
    {
        public MessagesSortingRulesException(string message, IMessage exceptionMessage = null) : base(message)
        {
            ExceptionMessage = exceptionMessage;
        }

        public IMessage ExceptionMessage { get; }

        public override string ToString()
        {
            return $"聊天对话排序不符合规则：{Message} \n异常元素：{ExceptionMessage.Serializer.SerializeJson(ExceptionMessage).ToString(Formatting.None)}";
        }
    }
}