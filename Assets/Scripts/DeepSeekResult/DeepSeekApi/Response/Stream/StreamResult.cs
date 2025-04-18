#if DEEPSEEK_PAST_CODE
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Xiyu.DeepSeekResult.DeepSeekApi.Request;

namespace Xiyu.DeepSeekResult.DeepSeekApi.Response.Stream
{
    public abstract class StreamResult<T>
    {
        /// <summary>
        /// 多数情况下，这个构造函数不会被调用，因为这个类是用于反序列化的
        /// </summary>
        /// <param name="id"></param>
        /// <param name="choices"></param>
        /// <param name="created"></param>
        /// <param name="model"></param>
        /// <param name="systemFingerprint"></param>
        /// <param name="chatObject"></param>
        /// <param name="usage"></param>
        [JsonConstructor]
        protected StreamResult(string id, List<T> choices, int created, ModelType model, string systemFingerprint, string chatObject, Usage? usage)
        {
            Id = id;
            Choices = choices;
            Created = created;
            Model = model;
            SystemFingerprint = systemFingerprint;
            ChatObject = chatObject;
            Usage = usage;
        }

        /// <summary>
        /// 该对话的唯一标识符。
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; }


        /// <summary>
        /// 选择列表 (ai 的回复)
        /// </summary>
        [JsonProperty(PropertyName = "choices")]

        public List<T> Choices { get; }

        [JsonIgnore]
        public bool IsDone { get; protected set; }


        /// <summary>
        /// 创建聊天完成时的 Unix 时间戳（以秒为单位）。
        /// <para>如果求可读性请使用 <see cref="CreatedTime"/></para>
        /// </summary>
        [JsonProperty(PropertyName = "created")]
        public int Created { get; }

        /// <summary>
        /// 创建聊天完成时的时间。
        /// </summary>
        public DateTime CreatedTime => DateTimeOffset.FromUnixTimeSeconds(Created).DateTime;

        /// <summary>
        /// 使用的模型
        /// </summary>
        [JsonProperty(PropertyName = "model")]
        public ModelType Model { get; }


        /// <summary>
        /// 此指纹表示模型运行时使用的后端配置。
        /// <para>官方文档原话：This fingerprint represents the backend configuration that the model runs with.</para>
        /// </summary>
        [JsonProperty(PropertyName = "system_fingerprint")]
        public string SystemFingerprint { get; }

        /// <summary>
        /// 其值一般为 chat.completion
        /// </summary>
        [JsonProperty(PropertyName = "object")]
        public string ChatObject { get; }

        [JsonProperty(PropertyName = "usage")] public Usage? Usage { get; }
    }
}
#endif