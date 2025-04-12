using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Xiyu.DeepSeekApi.Request;
using Xiyu.DeepSeekApi.Response.Stream;

namespace Xiyu.DeepSeekApi.Response.FIM
{
    /*好多参数，就算复制粘贴也好累*/
    public readonly struct FimResult
    {
        [JsonConstructor]
        public FimResult(string id, List<FimChoice> choices, int created, ModelType model, string systemFingerprint, Usage usage)
        {
            ID = id;
            Choices = choices;
            Created = created;
            Model = model;
            SystemFingerprint = systemFingerprint;
            Usage = usage;
        }

        /// <summary>
        /// 补全响应的 ID。
        /// </summary>
        [JsonProperty("id")]
        public string ID { get; }


        /// <summary>
        /// 模型生成的补全内容的选择列表。
        /// </summary>
        [JsonProperty("choices")]
        public List<FimChoice> Choices { get; }

        /// <summary>
        /// 标志补全请求开始时间的 Unix 时间戳（以秒为单位）。
        /// </summary>
        [JsonProperty("created")]
        public int Created { get; }

        /// <summary>
        /// 标志补全请求开始时间的 Unix 时间
        /// </summary>
        public DateTime CreatedTime => DateTimeOffset.FromUnixTimeSeconds(Created).DateTime;

        /// <summary>
        /// 补全请求所用的模型。
        /// </summary>
        [JsonProperty("model")]
        public ModelType Model { get; }

        /// <summary>
        /// 模型运行时的后端配置的指纹。
        /// </summary>
        [JsonProperty("system_fingerprint")]
        public string SystemFingerprint { get; }

        /// <summary>
        /// 一定为"text_completion"
        /// </summary>
        public static string Text => "text_completion";

        /// <summary>
        /// 该对话补全请求的用量信息。
        /// </summary>
        [JsonProperty("usage")]
        public Usage Usage { get; }
    }
}