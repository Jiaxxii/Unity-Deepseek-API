using System.Diagnostics;
using Newtonsoft.Json;

namespace Xiyu.DeepSeek
{
    public readonly struct ResponsesError
    {
        [JsonConstructor]
        public ResponsesError(Error? error)
        {
            Error = error;
        }

        public Error? Error { get; }
    }

    [DebuggerDisplay("{ToString()}")]
    public readonly struct Error
    {
        [JsonConstructor]
        public Error(string message, string type, string param, string code)
        {
            Message = message;
            Type = type;
            Param = param;
            Code = code;
        }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; }

        [JsonProperty(PropertyName = "type")] public string Type { get; }
        [JsonProperty(PropertyName = "param")] public string Param { get; }
        [JsonProperty(PropertyName = "code")] public string Code { get; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }
    }
}