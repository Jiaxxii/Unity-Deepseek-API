using System.Collections.Generic;
using System.Linq;
using Xiyu.DeepSeek.Responses.FimResult;

namespace Xiyu.DeepSeek.Responses.Expand
{
    public static class Expand
    {
        public static Message GetMessage(this ChatCompletion chatCompletion)
        {
            return chatCompletion.Choices.First().Message;
        }

        public static Delta GetMessage(this StreamChatCompletion streamChatCompletion)
        {
            return streamChatCompletion.Choices.First().Delta;
        }

        public static FimChoices GetMessage(this FimChatCompletion chatCompletion)
        {
            return chatCompletion.Choices.First();
        }

        public static StreamFimChoices GetMessage(this StreamFimChatCompletion streamChatCompletion)
        {
            return streamChatCompletion.Choices.First();
        }

        public static bool HasCompleteMsg(this StreamChatCompletion streamChatCompletion)
        {
            return streamChatCompletion.Choices.Any(v => !string.IsNullOrEmpty(v.Delta.Content) || !string.IsNullOrEmpty(v.Delta.ReasoningContent));
        }


        public static bool HasCompleteMsg(this StreamFimChatCompletion streamChatCompletion)
        {
            return streamChatCompletion.Choices.Any(v => !string.IsNullOrEmpty(v.Text));
        }

        public static ChatCompletion Completion(this ChatCompletion chatCompletion, string prefix)
        {
            var choice = chatCompletion.Choices[0];
            var message = new Message(string.Concat(prefix, choice.Message.Content), choice.Message.ReasoningContent, choice.Message.Role, choice.Message.ToolCalls);
            var choicesList = new List<Choices> { new(choice.FinishReason, choice.Index, message, choice.Logprobs) };
            return new ChatCompletion(chatCompletion.ID, choicesList, chatCompletion.Model, chatCompletion.Created, chatCompletion.SystemFingerprint, chatCompletion.Object,
                chatCompletion.Usage, chatCompletion.Error);
        }

        public static FimChatCompletion Completion(this FimChatCompletion chatCompletion, string prompt, string suffix)
        {
            var choice = chatCompletion.Choices[0];
            var text = string.Concat(prompt ?? string.Empty, choice.Text, suffix ?? string.Empty);
            var fimChoicesList = new List<FimChoices> { new(choice.FinishReason, choice.Index, choice.Logprobs, text) };
            return new FimChatCompletion(chatCompletion.Id, chatCompletion.Created, chatCompletion.Model, chatCompletion.SystemFingerprint, chatCompletion.Object,
                chatCompletion.Usage,
                fimChoicesList,
                chatCompletion.Error);
        }
    }
}