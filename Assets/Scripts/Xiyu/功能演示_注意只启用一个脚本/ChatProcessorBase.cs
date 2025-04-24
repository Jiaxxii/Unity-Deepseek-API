using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xiyu.DeepSeek;
using Xiyu.DeepSeek.Requests;
using Xiyu.DeepSeek.Responses.Expand;

namespace Xiyu.功能演示_注意只启用一个脚本
{
    public abstract class ChatProcessorBase : MonoBehaviour
    {
        protected string _apiKey;
        protected ChatProcessor _processor;

        private ScrollRect _scrollRect;
        private TextMeshProUGUI _output;

        [SerializeField] protected ChatMessageRequest chatMessageRequest;
        protected readonly MessagesCollector _messagesCollector = new();

        protected virtual void Start()
        {
            Debug.Log($"<Color=#38cb90>{gameObject.name}</color>");
            chatMessageRequest.MessagesCollector = _messagesCollector;
            _processor = new DeepseekChat(_apiKey, chatMessageRequest);

            // 定义 ai 人设
            var defaultSystemPrompt = GetDefaultSystemPrompt();
            _messagesCollector.AppendSystemMessage(defaultSystemPrompt);
        }

        protected virtual void Awake()
        {
            _apiKey = Resources.Load<TextAsset>("DEEPSEEK_API_KEY").text;

            var types = FindObjectsOfType<ChatProcessorBase>();
            if (types.Count(t => t.gameObject.activeSelf && t.enabled) > 1)
            {
                Debug.LogWarning(
                    $"示例脚本同时开启多个可能会造成非预期的结果，它们分别是：[{string.Join(',', types.Where(t => t.gameObject.activeSelf && t.enabled).Select(t => t.gameObject.name))}]");
            }

            var use = FindObjectOfType<使用示例>();
            if (use != null)
            {
                if (use.enabled)
                {
                    Debug.LogWarning("为了防止冲突，关闭了脚本\"使用示例\"");
                }

                use.enabled = false;
            }

            _scrollRect = GameObject.Find("OUTPUT").GetComponentInChildren<ScrollRect>();
            _output = _scrollRect.content.GetComponentInChildren<TextMeshProUGUI>();
        }

        protected void PrintText(string text, bool overrider = false)
        {
            if (overrider)
            {
                _output.text = text;
            }
            else _output.text += text;

            if (_scrollRect.content.sizeDelta.y < _output.rectTransform.sizeDelta.y)
            {
                const float additional = 100;
                _scrollRect.content.sizeDelta = new Vector2(_scrollRect.content.sizeDelta.x, _output.rectTransform.sizeDelta.y + additional);
            }
        }

        protected void ClearText([CanBeNull] string start = null) => _output.text = start ?? string.Empty;


        protected void PrintCount(DeepSeek.Responses.Usage usage, ChatModel? chatModel = null)
        {
            var printText = string.Concat(
                "\n\n</b><color=#65c2ca>",
                usage.TotalTokens.ToString(),
                "</color> <i>tokens</i>",
                "(<color=#c481cf><b>≈",
                usage.CalculatePrice(chatModel ?? chatMessageRequest.Model).ToString(CultureInfo.CurrentCulture),
                "</b></color><color=red>￥</color>)"
            );

            PrintText(printText);
        }

        protected virtual string GetDefaultSystemPrompt() => Resources.Load<TextAsset>("SystemPrompt").text;

        private void OnDestroy()
        {
            ChatProcessor.Dispose();
        }
    }
}