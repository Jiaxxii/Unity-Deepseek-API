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
        [SerializeField] private RectTransform contentRectTransform;

        [SerializeField] private MessagesCollector collector;

        [SerializeField] private ChatMessageRequest requestBody;

        private DeepseekChat _deepseekChat;

        private ChatCompletion _lastChatCompletion;

        private void Awake()
        {
            collector = new MessagesCollector(
                new SystemMessage("你叫“西”，是一个普普通通的人类，用户是你的朋友“雨”（不要输出unicode表情）。")
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

                    PrintText(
                        $"\n\n<color=#58616e>消耗Token：</color><color=#60AEEA>{_lastChatCompletion.Usage.TotalTokens}</color>");

                    var message = _lastChatCompletion.GetMessage();
                    if (!string.IsNullOrEmpty(message.ReasoningContent))
                    {
                        Debug.Log($"思考：<color=#df6b67>{message.ReasoningContent}</color>");
                    }

                    Debug.Log($"回复：<color=#61AFEF>{message.Content}</color>");
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

            Debug.Log($"<color=#ab77dc>ChatComplete</color>请求：<color=#61AFEF>{input}</color>");
            PrintText(string.Empty, true);

            var reasoningBegin = true;
            var reasoningEnd = false;
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
                    PrintText($"<color=#df6b67>{message.ReasoningContent}");
                }
                else if (!string.IsNullOrEmpty(message.ReasoningContent) && !reasoningBegin)
                {
                    PrintText(message.ReasoningContent);
                    continue;
                }

                if (string.IsNullOrEmpty(message.ReasoningContent) && !string.IsNullOrEmpty(message.Content) && !reasoningEnd)
                {
                    reasoningEnd = true;
                    PrintText($"</color>\n\n<b>{message.Content}");
                }
                else
                {
                    PrintText(message.Content);
                }
            }

            PrintText("<\b>");
        }

        private async UniTask PrefixChatCompletionAsync(string input, string prefix)
        {
            collector.AppendUserMessage(input);
            collector.CheckAndThrow();

            Debug.Log($"<color=#ab77dc>前缀续写</color>请求：<color=#61AFEF>{input}</color>");

            var assistantPrefixMessage = new AssistantPrefixMessage(prefix);
            PrintText(prefix, true);

            await foreach (var data in _deepseekChat.ChatCompletionStreamAsync(assistantPrefixMessage, onReport: report => _lastChatCompletion = report))
            {
                if (!data.HasCompleteMsg()) continue;

                // 前缀续写没有思考内容
                PrintText(data.GetMessage().Content);
            }
        }


        private void PrintText(string text, bool overrider = false)
        {
            if (overrider)
            {
                outputText.text = text;
            }
            else outputText.text += text;

            if (contentRectTransform.sizeDelta.y < outputText.rectTransform.sizeDelta.y)
            {
                const float additional = 300;
                contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, outputText.rectTransform.sizeDelta.y + additional);
            }
        }

        private void OnDestroy()
        {
            ChatProcessor.Dispose();
        }
    }
}