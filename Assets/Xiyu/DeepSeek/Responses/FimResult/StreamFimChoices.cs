using Newtonsoft.Json;

namespace Xiyu.DeepSeek.Responses.FimResult
{
    public class StreamFimChoices
    {
        [JsonConstructor]
        public StreamFimChoices(FinishReason? finishReason, int index, Logprobs? logprobs, string text)
        {
            FinishReason = finishReason;
            Index = index;
            Logprobs = logprobs;
            Text = text;
        }

        public FinishReason? FinishReason { get; }
        public int Index { get; }
        public Logprobs? Logprobs { get; }
        public string Text { get; }
    }
}