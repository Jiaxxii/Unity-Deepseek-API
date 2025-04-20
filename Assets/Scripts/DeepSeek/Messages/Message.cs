using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Xiyu.DeepSeek.Messages
{
    // public class Ser
    // {
    //     public string SerializeJson()
    // }
    [System.Serializable]
    [DebuggerDisplay("role:{Role} content:{Content}")]
    public class Message : IMessage
    {
        protected Message(string content, string name = null)
        {
            this.content = content;
            this.name = name;
        }

        public Message(Role role, string content, string name = null)
        {
            this.content = content;
            this.name = name;
            this.role = role;
        }

        private Message()
        {
            Serializer = new DefaultSerializerContent();
        }

        public static readonly JsonSerializer SerializerOption = new()
        {
            Converters = { new StringEnumConverter() },
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            },
            NullValueHandling = NullValueHandling.Ignore
        };


        [SerializeField] private Role role;

        [SerializeField] [TextArea(3, 5)] private string content;

        [SerializeField] private string name;

        public virtual Role Role => role;


        [JsonIgnore] public ISerializer Serializer { get; protected set; } = new DefaultSerializerContent();

        public string Content
        {
            get => content;
            set => content = value;
        }

        /// <summary>
        /// 可以选填的参与者的名称，为模型提供信息以区分相同角色的参与者。
        /// </summary>
        /// <returns></returns>
        public string Name
        {
            get => name;
            set => name = value;
        }


        internal class DefaultSerializerContent : ISerializer
        {
            public JObject SerializeJson(IMessage message)
            {
                return JObject.FromObject(message, SerializerOption);
            }
        }
    }
}