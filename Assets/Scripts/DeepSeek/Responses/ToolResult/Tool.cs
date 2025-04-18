using Newtonsoft.Json;

namespace Xiyu.DeepSeek.Responses.ToolResult
{
    public readonly struct Tool
    {
        [JsonConstructor]
        public Tool(string id, string type, Function function)
        {
            ID = id;
            Type = type;
            Function = function;
        }

        /// <summary>
        /// tool 调用的 ID。
        /// </summary>
        public string ID { get; }

        /// <summary>
        /// tool 的类型。目前仅支持 function。
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// 模型调用的 function。
        /// </summary>
        public Function Function { get; }
    }
}