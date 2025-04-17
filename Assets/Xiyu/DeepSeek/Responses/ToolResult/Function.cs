using Newtonsoft.Json;

namespace Xiyu.DeepSeek.Responses.ToolResult
{
    public readonly struct Function
    {
        [JsonConstructor]
        public Function(string name, string arguments)
        {
            Name = name;
            Arguments = arguments;
        }

        /// <summary>
        /// 模型调用的 function 名。
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 要调用的 function 的参数，由模型生成，格式为 JSON。
        /// 请注意，模型并不总是生成有效的 JSON，并且可能会臆造出你函数模式中未定义的参数。
        /// 在调用函数之前，请在代码中验证这些参数。
        /// </summary>
        public string Arguments { get; }
    }
}