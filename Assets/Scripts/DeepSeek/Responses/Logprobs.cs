using System.Collections.Generic;
using Newtonsoft.Json;

namespace Xiyu.DeepSeek.Responses
{
    public readonly struct Logprobs
    {
        [JsonConstructor]
        public Logprobs(IList<Content> content)
        {
            Content = content;
        }

        /// <summary>
        /// 一个包含输出 token 对数概率信息的列表。
        /// </summary>
        public IList<Content> Content { get; }

    }

    public readonly struct Content
    {
        [JsonConstructor]
        public Content(string token, float logprob, byte[] bytes, IList<TopLogprobs> topLogprobs)
        {
            Token = token;
            Logprob = logprob;
            Bytes = bytes;
            TopLogprobs = topLogprobs;
        }

        /// <summary>
        /// 输出的 token。
        /// </summary>
        public string Token { get; }
        
        /// <summary>
        /// 该 token 的对数概率。-9999.0 代表该 token 的输出概率极小，不在 top 20 最可能输出的 token 中。
        /// </summary>
        public float Logprob { get; }
        
        /// <summary>
        /// 一个包含该 token UTF-8 字节表示的整数列表。一般在一个 UTF-8 字符被拆分成多个 token 来表示时有用。如果 token 没有对应的字节表示，则该值为 null。
        /// </summary>
        public byte[] Bytes { get; } 
        
        /// <summary>
        /// 一个包含在该输出位置上，输出概率 top N 的 token 的列表，以及它们的对数概率。在罕见情况下，返回的 token 数量可能少于请求参数中指定的 top_logprobs 值。
        /// </summary>
        public IList<TopLogprobs> TopLogprobs { get; }
    }

    public readonly struct TopLogprobs
    {
        [JsonConstructor]
        public TopLogprobs(string token, float logprob, byte[] bytes)
        {
            Token = token;
            Logprob = logprob;
            Bytes = bytes;
        }

        /// <summary>
        /// 输出的 token。
        /// </summary>
        public string Token { get; }
        
        /// <summary>
        /// 该 token 的对数概率。-9999.0 代表该 token 的输出概率极小，不在 top 20 最可能输出的 token 中。
        /// </summary>
        public float Logprob { get; }
        
        /// <summary>
        /// 一个包含该 token UTF-8 字节表示的整数列表。一般在一个 UTF-8 字符被拆分成多个 token 来表示时有用。如果 token 没有对应的字节表示，则该值为 null。
        /// </summary>
        public byte[] Bytes { get; } 
    }


}