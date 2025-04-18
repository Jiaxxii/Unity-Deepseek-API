using System;
using UnityEngine;

namespace Xiyu.DeepSeek.Requests.Tools
{
    [Serializable]
    public class ToolChoice
    {
        public ToolChoice(ChatCompletionToolChoice chatCompletionToolChoice)
        {
            this.chatCompletionToolChoice = chatCompletionToolChoice;
        }

        public ToolChoice(ChatCompletionNameToolChoice chatCompletionNameToolChoice)
        {
            this.chatCompletionNameToolChoice = chatCompletionNameToolChoice;
        }

        [SerializeField] private ChatCompletionToolChoice chatCompletionToolChoice;
        [SerializeField] private ChatCompletionNameToolChoice chatCompletionNameToolChoice;

        /// <summary>
        /// 当没有 tool 时，默认值为 none。如果有 tool 存在，默认值为 auto。
        /// </summary>
        public ChatCompletionToolChoice ChatCompletionToolChoice
        {
            get => chatCompletionToolChoice;
            set => chatCompletionToolChoice = value;
        }


        public ChatCompletionNameToolChoice ChatCompletionNameToolChoice
        {
            get => chatCompletionNameToolChoice;
            set => chatCompletionNameToolChoice = value;
        }
    }
}