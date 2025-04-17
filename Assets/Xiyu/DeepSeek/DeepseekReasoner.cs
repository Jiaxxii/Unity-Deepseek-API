using Xiyu.DeepSeek.Requests;

namespace Xiyu.DeepSeek
{
    public class DeepseekReasoner : ChatProcessor
    {
        public DeepseekReasoner(string apiKey, MessageRequest messageRequest) : base(apiKey, messageRequest)
        {
        }
    }
}