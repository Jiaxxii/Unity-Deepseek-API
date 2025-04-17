using System;
using UnityEngine;

namespace Xiyu.DeepSeek.Requests.Tools
{
    [Serializable]
    public class ChatCompletionNameToolChoice
    {
        public ChatCompletionNameToolChoice(string functionName)
        {
            this.functionName = functionName;
        }

        [SerializeField] private ToolType toolType = ToolType.Function;
        [SerializeField] private string functionName;

        public ToolType ToolType
        {
            get => toolType;
            set => toolType = value;
        }

        public string FunctionName
        {
            get => functionName;
            set => functionName = value;
        }

        public static ChatCompletionNameToolChoice Create(string functionName) => new(functionName);
    }
}