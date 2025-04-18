using System.Collections.Generic;
using Newtonsoft.Json;

namespace Xiyu.DeepSeek.Responses.FimResult
{
    public readonly struct StreamFimChatCompletion : IValid
    {
        [JsonConstructor]
        public StreamFimChatCompletion(string id, int created, ChatModel model, string systemFingerprint, string @object, Usage? usage, IList<StreamFimChoices> choices,
            Error? error)
        {
            Id = id;
            Created = created;
            Model = model;
            SystemFingerprint = systemFingerprint;
            Object = @object;
            Usage = usage;
            Choices = choices;
            Error = error;
        }

        public string Id { get; }
        public int Created { get; }
        public ChatModel Model { get; }
        public string SystemFingerprint { get; }
        public string Object { get; }
        public Usage? Usage { get; }


        public IList<StreamFimChoices> Choices { get; }
        public Error? Error { get; }

        public bool IsValid()
        {
            return Error == null && !string.IsNullOrEmpty(Id) && Created != 0;
        }

        public static FimChatCompletion CountChatCompletion(StreamFimChatCompletion last, string fullContent, Usage usage)
        {
            var choicesList = new List<FimChoices> { new(last.Choices[0].FinishReason!.Value, last.Choices[0].Index, last.Choices[0].Logprobs, fullContent) };
            return new FimChatCompletion(last.Id, last.Created, last.Model, last.SystemFingerprint, last.Object, usage, choicesList, last.Error);
        }
    }
}