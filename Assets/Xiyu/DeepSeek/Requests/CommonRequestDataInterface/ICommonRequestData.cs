using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Xiyu.DeepSeek.Requests.CommonRequestDataInterface
{
    /// <summary>
    /// 通用的请求数据数据
    /// </summary>
    public interface ICommonRequestData
    {
        /// <summary>
        ///  [deepseek-chat, deepseek-reasoner]
        /// </summary>
        public ChatModel Model { get; set; }


        /// <summary>
        /// 介于 1 到 8192 间的整数，限制一次请求中模型生成 completion 的最大 token 数。
        /// 输入 token 和输出 token 的总长度受模型的上下文长度的限制。
        /// 如未指定 max_tokens参数，默认使用 4096。
        /// </summary>
        public int MaxToken { get; set; }

        /// <summary>
        /// 一个 string 或最多包含 16 个 string 的 list，在遇到这些词时，API 将停止生成更多的 token。
        /// </summary>
        public HashSet<string> Stop { get; set; }


        /// <summary>
        /// 流式输出相关选项。只有在 stream 参数为 true 时，才可设置此参数。
        /// </summary>
        public StreamOptions StreamOptions { get; set; }


        string SerializeRequestJson(JObject instance = null, Formatting formatting = Formatting.None, bool overwrite = false);


        void SetStreamOptions(bool openStream);
    }
}