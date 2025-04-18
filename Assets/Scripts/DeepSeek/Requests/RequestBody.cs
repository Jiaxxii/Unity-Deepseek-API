using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using Xiyu.DeepSeek.Requests.CommonRequestDataInterface;

namespace Xiyu.DeepSeek.Requests
{
    [Serializable]
    public class RequestBody : ICommonRequestData
    {
        public static JsonSerializer JsonSerializer { get; } = new()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        protected RequestBody(ChatModel model)
        {
            this.model = model;
        }

        private RequestBody()
        {
        }


        public const string KeyMessages = "messages";
        private const string KeyModel = "model";
        private const string KeyMaxToken = "max_token";
        private const string KeyStop = "stop";
        private const string KeyStream = "stream";
        private const string KeyStreamOptions = "stream_options";


        [SerializeField] private ChatModel model;

        /// <summary>
        ///  [deepseek-chat, deepseek-reasoner]
        /// </summary>
        public ChatModel Model
        {
            get => model;
            set => model = value;
        }


        [SerializeField] private int maxToken = 4096;

        /// <summary>
        /// 介于 1 到 8192 间的整数，限制一次请求中模型生成 completion 的最大 token 数。
        /// 输入 token 和输出 token 的总长度受模型的上下文长度的限制。
        /// 如未指定 max_tokens参数，默认使用 4096。
        /// </summary>
        public int MaxToken
        {
            get => maxToken;
            set => maxToken = value;
        }

        [SerializeField] private string[] stop;

        /// <summary>
        /// 一个 string 或最多包含 16 个 string 的 list，在遇到这些词时，API 将停止生成更多的 token。
        /// </summary>
        public HashSet<string> Stop
        {
            get => stop.ToHashSet();
            set => stop = value.ToArray();
        }

        [SerializeField] private bool stream;

        // /// <summary>
        // /// 如果设置为 True，将会以 SSE（server-sent events）的形式以流式发送消息增量。消息流以 data: [DONE] 结尾。
        // /// </summary>
        // public bool Stream
        // {
        //     get => stream;
        //     set => stream = value;
        // }

        [SerializeField] private StreamOptions streamOptions;

        /// <summary>
        /// 流式输出相关选项。只有在 stream 参数为 true 时，才可设置此参数。
        /// </summary>
        public StreamOptions StreamOptions
        {
            get => streamOptions;
            set
            {
                if (value == null)
                {
                    stream = false;
                    streamOptions = null;
                }
                else
                {
                    stream = true;
                    streamOptions = value;
                }
            }
        }


        public virtual string SerializeRequestJson(JObject instance = null, Formatting formatting = Formatting.None, bool overwrite = false)
        {
            var parameter = SerializeParameter(instance, overwrite);

            return parameter.ToString(formatting);
        }

        public virtual JObject SerializeParameter(JObject instance = null, bool overwrite = false)
        {
            if ((instance == null || instance.Count == 0) && overwrite)
            {
                overwrite = false;
            }

            instance ??= new JObject();

            AppendModelInJObject(instance, overwrite);

            AppendMaxTokenInJObject(instance, overwrite);

            AppendStopInJObject(instance, overwrite);

            AppendStreamInJObject(instance, overwrite);

            return instance;
        }

        protected void AppendModelInJObject(JObject instance, bool overwrite)
        {
            if (model == ChatModel.None)
                throw new ArgumentException("Model cannot be empty", nameof(model));


            AddToken(instance, KeyModel, overwrite,
                () => JToken.Parse(JsonConvert.SerializeObject(model, new StringEnumConverter()))
            );
        }

        protected void AppendMaxTokenInJObject(JObject instance, bool overwrite)
        {
            if (maxToken != 4096)
            {
                AddToken(instance, KeyMaxToken, overwrite,
                    () => Mathf.Clamp(maxToken, 2, 8192)
                );
            }
            else instance.Remove(KeyMaxToken);
        }

        protected void AppendStopInJObject(JObject instance, bool overwrite)
        {
            var hashSet = stop == null ? new HashSet<string>() : stop.ToHashSet();

            if (hashSet.Count == 1)
            {
                AddToken(instance, KeyStop, overwrite,
                    () => hashSet.First()
                );
            }
            else if (hashSet.Count > 1)
            {
                AddToken(instance, KeyStop, overwrite,
                    () => JArray.FromObject(hashSet)
                );
            }
            else instance.Remove(KeyStop);
        }

        protected void AppendStreamInJObject(JObject instance, bool overwrite)
        {
            if (stream)
            {
                AddToken(instance, KeyStream, overwrite,
                    () => true
                );

                if (streamOptions.IncludeUsage)
                {
                    AddToken(instance, KeyStreamOptions, overwrite,
                        () => JObject.FromObject(streamOptions, JsonSerializer)
                    );
                }
                else instance.Remove(KeyStreamOptions);
            }
            else
            {
                instance.Remove(KeyStream);
                instance.Remove(KeyStreamOptions);
            }
        }


        protected void AddToken(JObject instance, string key, bool overwrite, Func<JToken> value)
        {
            if (instance.ContainsKey(key))
            {
                if (overwrite)
                    instance[key] = value();
            }
            else instance.Add(key, value());
        }

        public void SetStreamOptions(bool openStream)
        {
            if (!openStream)
            {
                stream = false;
                streamOptions = null;
                return;
            }

            if (stream) return;

            stream = true;
            streamOptions = new StreamOptions();
        }
    }
}