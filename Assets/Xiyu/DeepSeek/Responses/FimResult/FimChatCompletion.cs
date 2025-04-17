using System.Collections.Generic;
using Newtonsoft.Json;

namespace Xiyu.DeepSeek.Responses.FimResult
{
    public readonly struct FimChatCompletion : IValid
    {
        [JsonConstructor]
        public FimChatCompletion(string id, int created, ChatModel model, string systemFingerprint, string @object, Usage usage, IList<FimChoices> choices, Error? error)
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
        public Usage Usage { get; }


        public IList<FimChoices> Choices { get; }
        public Error? Error { get; }

        public bool IsValid()
        {
            return Error == null && !string.IsNullOrEmpty(Id) && Created != 0;
        }


        
    }
}