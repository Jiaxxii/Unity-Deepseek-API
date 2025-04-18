using Xiyu.DeepSeek.Requests;

namespace Xiyu.DeepSeek
{
    public class DeepseekReasoner : ChatProcessor
    {
        public DeepseekReasoner(string apiKey, ReasonerMessageRequest messageRequest) : base(apiKey, messageRequest)
        {
        }

        protected DeepseekReasoner(string apiKey, MessageRequest messageRequest) : base(apiKey, messageRequest)
        {
        }
    }
}