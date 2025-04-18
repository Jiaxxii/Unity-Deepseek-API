using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using Xiyu.DeepSeek.Responses;

namespace Xiyu.DeepSeek.Requests
{
    [Serializable]
    public class StreamOptions
    {
        public static JsonSerializer JsonSerializer { get; } = new()
        {
            ContractResolver = ChatCompletion.DeSerializerSettings.ContractResolver ?? new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        public StreamOptions(bool includeUsage = false)
        {
            this.includeUsage = includeUsage;
        }

        [Tooltip("如果设置为 true，在流式消息最后的 data: [DONE\n此块上的 usage 字段显示整个请求的 token 使用统计\n所有其他块也将包含一个 usage 字段，但其值为 null。")] [SerializeField]
        private bool includeUsage;

        /// <summary>
        /// 如果设置为 true，在流式消息最后的 data: [DONE] 之前将会传输一个额外的块。
        /// 此块上的 usage 字段显示整个请求的 token 使用统计信息，而 choices 字段将始终是一个空数组。
        /// 所有其他块也将包含一个 usage 字段，但其值为 null。
        /// </summary>
        public bool IncludeUsage
        {
            get => includeUsage;
            set => includeUsage = value;
        }
    }
}