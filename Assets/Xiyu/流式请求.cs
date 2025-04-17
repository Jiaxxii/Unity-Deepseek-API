using TMPro;
using UnityEngine;
using Xiyu.DeepSeek;
using Xiyu.DeepSeek.Messages;
using Xiyu.DeepSeek.Requests;
using ChatProcessor = Xiyu.DeepSeek.ChatProcessor;

namespace Xiyu
{
    public class 流式请求 : MonoBehaviour
    {
        [SerializeField] private string apiKey;

        [SerializeField] private TextMeshProUGUI text;

        // [SerializeField] ChatRequest requestBodyB;


        private async void Start()
        {
            var messageCollector = new MessagesCollector(
                new SystemMessage("你的全名叫：DeepSeek，将扮演ai助手回答用户的问题。"),
                new UserMessage("很高兴认识你，你叫什么？")
            );

            var messageRequest = new MessageRequest(ChatModel.Chat, messageCollector);

            var deepseekChat = new DeepseekChat(apiKey, messageRequest);

            var chatCompletionAsync = await deepseekChat.ChatCompletionAsync();
        }

        private void OnDestroy()
        {
            ChatProcessor.Dispose();
        }
    }
}