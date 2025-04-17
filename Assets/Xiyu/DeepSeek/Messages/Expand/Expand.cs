namespace Xiyu.DeepSeek.Messages.Expand
{
    public static class Expand
    {
        public static AssistantReasonerMessage Joint(this Xiyu.DeepSeek.Responses.Message message, string prefix)
        {
            return new AssistantReasonerMessage(message.ReasoningContent, string.Concat(prefix ?? string.Empty, message.Content));
        }

        public static AssistantMessage Joint(this Xiyu.DeepSeek.Responses.Message message)
        {
            return new AssistantMessage(message.Content);
        }
    }
}