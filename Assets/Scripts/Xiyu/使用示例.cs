using System;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using Xiyu.DeepSeek;
using Xiyu.DeepSeek.Messages;
using Xiyu.DeepSeek.Requests;
using Xiyu.DeepSeek.Responses;
using Xiyu.DeepSeek.Responses.Expand;

namespace Xiyu
{
    public class 使用示例 : MonoBehaviour
    {
        [SerializeField] private string apiKey;
        [SerializeField] private TextMeshProUGUI outputText;
        [SerializeField] private TMP_InputField inputField;

        [SerializeField] private MessagesCollector collector;

        [SerializeField] private ChatMessageRequest requestBody;

        private DeepseekChat _deepseekChat;

        private ChatCompletion _lastChatCompletion;

        private void Awake()
        {
            collector = new MessagesCollector(
                new SystemMessage("你叫“西”，是一个普普通通的人类，用户是你的朋友“雨”。")
            );

            requestBody.MessagesCollector = collector;

            _deepseekChat = new DeepseekChat(apiKey, requestBody);
        }

        private async void Start()
        {
            try
            {
                var handler = inputField.GetAsyncSubmitEventHandler(destroyCancellationToken);
                while (true)
                {
                    var input = await handler.OnSubmitAsync();
                    inputField.interactable = false;

                    var parameters = input.Split('$', StringSplitOptions.RemoveEmptyEntries);

                    if (parameters.Length == 0)
                    {
                        continue;
                    }

                    FindObjectOfType<Loading>().IsRun = true;
                    if (parameters.Length == 1)
                    {
                        await ChatCompletionAsync(input);
                    }
                    else
                    {
                        // msg$p=prefix
                        var prefixGroup = Regex.Match(parameters[1], "^p=(?<prefix>.+)$").Groups["prefix"];
                        if (prefixGroup.Success)
                        {
                            await PrefixChatCompletionAsync(parameters[0], prefixGroup.Value);
                        }
                        else
                        {
                            await ChatCompletionAsync(input);
                        }
                    }

                    FindObjectOfType<Loading>().IsRun = false;
                    outputText.text += $"\r\n<color=#58616e>消耗Token：</color><color=#60AEEA>{_lastChatCompletion.Usage.TotalTokens}</color>";
                    inputField.interactable = true;
                }
            }
            catch (OperationCanceledException)
            {
                Debug.Log("请求已经取消！");
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
        }

        private async UniTask ChatCompletionAsync(string input)
        {
            collector.AppendUserMessage(input);
            collector.CheckAndThrow();

            Debug.Log($"<color=#ab77dc>ChatComplete</color>请求：{input}");
            outputText.text = string.Empty;

            var reasoningBegin = true;
            await foreach (var data in _deepseekChat.ChatCompletionStreamAsync(onReport: report => _lastChatCompletion = report))
            {
                if (!data.HasCompleteMsg())
                {
                    continue;
                }

                var message = data.GetMessage();

                if (!string.IsNullOrEmpty(message.ReasoningContent) && string.IsNullOrEmpty(message.Content) && reasoningBegin)
                {
                    reasoningBegin = false;
                    outputText.text += $"<color=#5f8090>{message.ReasoningContent}";
                }
                else if (string.IsNullOrEmpty(message.ReasoningContent) && !string.IsNullOrEmpty(message.Content) && !reasoningBegin)
                {
                    outputText.text += $"{message.ReasoningContent}</color>";
                }
                else
                {
                    outputText.text += message.Content;
                }
            }
        }

        private async UniTask PrefixChatCompletionAsync(string input, string prefix)
        {
            collector.AppendUserMessage(input);
            collector.CheckAndThrow();

            Debug.Log($"<color=#ab77dc>前缀续写</color>请求：{input}");

            var assistantPrefixMessage = new AssistantPrefixMessage(prefix);
            outputText.text = prefix;

            await foreach (var data in _deepseekChat.ChatCompletionStreamAsync(assistantPrefixMessage, onReport: report => _lastChatCompletion = report))
            {
                if (!data.HasCompleteMsg()) continue;

                // 前缀续写没有思考内容
                outputText.text += data.GetMessage().Content;
            }
        }

        private void OnDestroy()
        {
            ChatProcessor.Dispose();
        }
    }
}