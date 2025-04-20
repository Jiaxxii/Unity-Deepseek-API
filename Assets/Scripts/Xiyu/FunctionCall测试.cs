using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Xiyu.DeepSeek;
using Xiyu.DeepSeek.Messages;
using Xiyu.DeepSeek.Requests;
using Xiyu.DeepSeek.Requests.Tools;
using Xiyu.DeepSeek.Responses;
using Xiyu.DeepSeek.Responses.Expand;
using Random = UnityEngine.Random;

namespace Xiyu
{
    public class FunctionCall测试 : MonoBehaviour
    {
        private string _apiKey;

        private readonly Dictionary<string, Func<DeepSeek.Responses.ToolResult.Function, UniTask<string>>> _tools = new();

        private void Awake()
        {
            _apiKey = Resources.Load<TextAsset>("DEEPSEEK_API_KEY").text;
            _tools.Add("get_weather", func => UniTask.FromResult(Random.Range(24, 32).ToString()));
        }

        private async void Start()
        {
            var messagesCollector = new MessagesCollector(
                new UserMessage("What's the weather like in Hangzhou and Beijing?")
            );

            var chatMessageRequest = new ChatMessageRequest(ChatModel.Chat, messagesCollector)
            {
                Tools = new List<Tool<Function>>()
                {
                    new Tool<Function>()
                    {
                        Function = new Function
                        {
                            Name = "get_weather",
                            Description = "Get weather of an location, the user shoud supply a location first",
                            Parameters = new
                            {
                                type = "object",
                                properties = new
                                {
                                    location = new
                                    {
                                        type = "string",
                                        description = "The city and state, e.g. San Francisco, CA",
                                    }
                                }
                            },
                            Required = new[] { "location" }
                        }
                    }
                }
            };

            var deepseekChat = new DeepseekChat(_apiKey, chatMessageRequest);

            var chatCompletionAsync = await deepseekChat.ChatCompletionAsync();
        }
    }
}