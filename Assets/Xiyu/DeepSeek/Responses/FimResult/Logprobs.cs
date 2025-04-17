using System.Collections.Generic;
using Newtonsoft.Json;

namespace Xiyu.DeepSeek.Responses.FimResult
{
    public struct Logprobs
    {
        [JsonConstructor]
        public Logprobs(IEnumerable<int> textOffset, IEnumerable<float> tokenLogprobs, IEnumerable<string> tokens, IEnumerable<object> topLogprobs)
        {
            TextOffset = textOffset;
            TokenLogprobs = tokenLogprobs;
            Tokens = tokens;
            TopLogprobs = topLogprobs;
        }

        public IEnumerable<int> TextOffset { get; }
        public IEnumerable<float> TokenLogprobs { get; }
        public IEnumerable<string> Tokens { get; }

        [JsonIgnore] public IEnumerable<object> TopLogprobs { get; }
    }
}