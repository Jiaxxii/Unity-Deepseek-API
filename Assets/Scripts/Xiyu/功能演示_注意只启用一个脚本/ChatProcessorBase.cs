using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Xiyu.DeepSeek;
using Xiyu.DeepSeek.Requests;
using Xiyu.DeepSeek.Responses.Expand;
// 注释由AI生成

namespace Xiyu.功能演示_注意只启用一个脚本
{
    public abstract class ChatProcessorBase : MonoBehaviour
    {
        protected string _apiKey;
        

        /// <summary>
        /// Represents the core processor instance responsible for handling chat-based interactions and completions.
        /// This field is central to the functionality of the chat system, managing operations such as message processing,
        /// function calls, and streaming responses. It is initialized with specific configurations and interacts
        /// with the underlying AI model to generate responses based on user inputs and predefined behaviors.
        /// </summary>
        protected ChatProcessor _processor;

        private ScrollRect _scrollRect;
        private TextMeshProUGUI _output;

        /// <summary>
        /// Represents a request object for chat message processing, encapsulating various parameters and configurations required for interacting with the chat system.
        /// This includes settings such as frequency penalty, presence penalty, temperature, top-p sampling, log probabilities, and tools that define callable functions during chat interactions.
        /// The object is used within the context of a chat processor to manage message data and guide AI behavior during conversations.
        /// </summary>
        [SerializeField] protected ChatMessageRequest chatMessageRequest;

        /// <summary>
        /// Represents a collector for managing and organizing messages within the chat processing system.
        /// This object is responsible for appending user and system messages, maintaining message history, and preparing message data for processing by the chat processor.
        /// It is utilized internally by the chat processor to ensure proper handling of message sequences during interactions with the AI model.
        /// </summary>
        protected readonly MessagesCollector _messagesCollector = new();

        /// <summary>
        /// Starts the initialization process for the component, setting up necessary configurations and initiating the first interaction.
        /// Logs the game object's name for debugging purposes and assigns the message collector to the chat processor.
        /// Establishes a default system prompt to define the AI's persona and appends it to the message collector.
        /// This method serves as a base setup for derived classes, which can extend its functionality by overriding it.
        /// </summary>
        protected virtual void Start()
        {
            Debug.Log($"<Color=#38cb90>{gameObject.name}</color>");
            chatMessageRequest.MessagesCollector = _messagesCollector;
            _processor = new DeepseekChat(_apiKey, chatMessageRequest);

            // 定义 ai 人设
            var defaultSystemPrompt = GetDefaultSystemPrompt();
            _messagesCollector.AppendSystemMessage(defaultSystemPrompt);
        }

        /// <summary>
        /// Initializes the component and performs essential setup tasks before the game starts.
        /// Loads the API key from resources, ensures no conflicting scripts are active, and initializes necessary UI components.
        /// If multiple instances of ChatProcessorBase-derived scripts are active, a warning is logged to prevent unintended behavior.
        /// Additionally, if an instance of the "使用示例" script is found, it is disabled to avoid conflicts.
        /// The method also locates and assigns the scroll view and output text field for later use in displaying messages.
        /// </summary>
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

        /// <summary>
        /// Prints the given text to the output field, optionally overriding the existing content.
        /// If the content height of the scroll view is smaller than the output field's height,
        /// it adjusts the scroll view's content size to ensure proper display.
        /// </summary>
        /// <param name="text">The text to be printed to the output field.</param>
        /// <param name="overrider">Indicates whether to override the existing content in the output field.
        /// If true, the existing content will be replaced; otherwise, the text will be appended.</param>
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

        /// <summary>
        /// Clears the text in the output field, optionally replacing it with a specified starting string.
        /// If no starting string is provided, the output field is cleared completely.
        /// </summary>
        /// <param name="start">An optional string to initialize the output field after clearing.
        /// If null or not provided, the output field will be set to an empty string.</param>
        protected void ClearText([CanBeNull] string start = null) => _output.text = start ?? string.Empty;


        /// <summary>
        /// Prints the token count and calculated price based on the provided usage data and optional chat model.
        /// The output includes the total number of tokens and an approximate price, formatted for display.
        /// </summary>
        /// <param name="usage">The usage data containing information about token consumption.</param>
        /// <param name="chatModel">The optional chat model used to calculate the price. If not provided, the model from the chat message request is used.</param>
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

        /// <summary>
        /// Retrieves the default system prompt from the resources.
        /// This prompt is used to define the initial behavior or persona of the AI system.
        /// The method expects a resource file named "SystemPrompt" to be present in the Resources folder.
        /// </summary>
        /// <returns>A string containing the default system prompt loaded from the "SystemPrompt" resource file.</returns>
        protected virtual string GetDefaultSystemPrompt() => Resources.Load<TextAsset>("SystemPrompt").text;

        private void OnDestroy()
        {
            ChatProcessor.Dispose();
        }
    }
}