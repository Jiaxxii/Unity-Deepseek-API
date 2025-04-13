using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using Xiyu.DeepSeekApi;
using Xiyu.DeepSeekApi.Request;
using Xiyu.DeepSeekApi.Response;
using Xiyu.DeepSeekApi.Response.Stream;

namespace Xiyu
{
    public class ChatSample : MonoBehaviour
    {
        [SerializeField] private TMP_InputField chatInput;
        [SerializeField] private TextMeshProUGUI output;
        [SerializeField] private RectTransform content;

        [Space] [SerializeField] private string apiKey;

        [Space] [SerializeField] [TextArea(3, 8)]
        private string prompt;

        [Space] [SerializeField] private bool stream;
        [Space] [SerializeField] private ModelType modelType = ModelType.DeepseekChat;


        private readonly Dictionary<ModelType, IRequestBody> _buffer = new();

        private IRequestBody _requestBody;

        private ChatProcessor _processor;

        private MessageCollector _messageCollector;

        private void Awake()
        {
            _messageCollector = new MessageCollector(
                new SystemMessage(prompt)
            );

            Init();

            chatInput.onSubmit.AddListener(msg => SendMessageForget(msg).Forget());
        }

        private void Init()
        {
            if (_requestBody != null)
                throw new Exception("RequestBody is already set");

            if (modelType == ModelType.DeepseekChat)
            {
                var request = new ChatRequest(_messageCollector);
                _buffer.Add(modelType, request);
            }
            else
            {
                var request = new ReasonerRequest(_messageCollector);
                _buffer.Add(modelType, request);
            }

            _requestBody = _buffer[modelType];
            _processor = new Chat(apiKey, _requestBody);
        }

        private void TryConvert()
        {
            if (_requestBody.Model == modelType)
                return;

            if (!_buffer.TryGetValue(modelType, out _requestBody))
            {
                if (modelType == ModelType.DeepseekChat)
                {
                    var request = new ChatRequest(_messageCollector);
                    _buffer.Add(modelType, request);
                }
                else
                {
                    var request = new ReasonerRequest(_messageCollector);
                    _buffer.Add(modelType, request);
                }

                _requestBody = _buffer[modelType];
            }

            _processor.RequestBody = _requestBody;
        }

        private async UniTaskVoid SendMessageForget(string input)
        {
            chatInput.text = output.text = string.Empty;
            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            chatInput.interactable = false;

            TryConvert();


            var userMessage = new UserMessage(input);
            _requestBody.Messages.AddMessage(userMessage);

            _requestBody.Messages.CheckAndThrow();

            try
            {
                if (stream)
                {
                    var first = true;
                    var chatFirst = false;
                    StreamChatResult lastStreamChatResult = null;
                    await foreach (var data in _processor.SendStreamChatAsync(destroyCancellationToken))
                    {
                        var (msgType, msg) = data.GetReasonerMessage();

                        lastStreamChatResult = data;

                        if (string.IsNullOrEmpty(msg))
                        {
                            continue;
                        }

                        if (msgType == ModelType.DeepseekReasoner && first)
                        {
                            first = false;
                            chatFirst = true;
                            output.text += $"<color=#393939>{msg}";
                        }
                        else if (msgType == ModelType.DeepseekChat && chatFirst)
                        {
                            output.text += "</color>\n";
                            chatFirst = false;
                        }
                        else
                        {
                            output.text += msg;
                        }

                        content.sizeDelta = new Vector2(content.sizeDelta.x, output.rectTransform.sizeDelta.y);
                    }

                    output.text += TokenToString(lastStreamChatResult!.Usage!.Value);
                }
                else
                {
                    output.text = "等待响应……";
                    var chatResult = await _processor.SendChatAsync(destroyCancellationToken);
                    output.text = string.Empty;

                    var message = chatResult.GetMessage();

                    if (!string.IsNullOrWhiteSpace(message.ReasoningContent))
                    {
                        output.text += $"<color=#393939>{message.ReasoningContent}</color>\n";
                    }

                    output.text = $"{message.Content}{TokenToString(chatResult.Usage)}";
                }

                await UniTask.WaitForEndOfFrame(this);
                content.sizeDelta = new Vector2(content.sizeDelta.x, output.rectTransform.sizeDelta.y);
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("请求已终止！");
            }
            finally
            {
                chatInput.interactable = true;
                foreach (var messagesMessage in _requestBody.Messages.Messages)
                {
                    if (messagesMessage.Role == RoleType.User)
                    {
                        Debug.Log($"用户：{messagesMessage.Content}");
                    }
                    else if (messagesMessage.Role == RoleType.Assistant)
                    {
                        Debug.Log($"AI：{messagesMessage.Content}");
                    }
                    else if (messagesMessage.Role == RoleType.System)
                    {
                        Debug.LogWarning($"系统：{messagesMessage.Content}");
                    }
                }
            }
        }

        private static string TokenToString(Usage usage, bool newLine = true)
        {
            return $"{(newLine ? "\r\n" : string.Empty)}<color=#383838>消耗Token:</color><color=#b5cea8>{usage.TotalTokens}</color>";
        }
    }
}