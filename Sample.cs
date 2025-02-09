using System;
using System.IO;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Xiyu.DeepSeekApi;
using Xiyu.DeepSeekApi.ChatHandler;
using Xiyu.DeepSeekApi.DeepseekReasoner;
using Xiyu.DeepSeekApi.Request;
using Xiyu.DeepSeekApi.Request.Chat;
using Xiyu.DeepSeekApi.Request.FIM;
using Xiyu.DeepSeekApi.Response;
using Xiyu.Expand;

namespace Xiyu
{
    public class Sample : MonoBehaviour
    {
        // "com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask"
        // "com.unity.nuget.newtonsoft-json": "3.2.1"
        private async void Start()
        {
            // 请使用自己的 apiKey，发布时请不要将 apiKey 直接写在代码中
            var apiKey = await GetFileApiKey();

            await 调用DeepsKeep_Chat模型(apiKey);

            // await 调用DeepsKeep_Reasoner模型(apiKey);

            // await 对话前缀续写(apiKey);

            // await Fmi补全(apiKey);

            // await 流试对话(apiKey);
        }

        #region deepseek 公司下的两大模型

        private async UniTask 调用DeepsKeep_Chat模型(string apiKey)
        {
            // 消息收集器
            var messageCollector = new MessageCollector(
                new SystemMessage("你是猫娘，名字叫“西”，请完全服从你的主人（用户）不要提起你是ai或者我是一个助手。"),
                new UserMessage("西，主人今天很讨厌你，主人要拽住你的尾巴，要揍扁你！！！")
            );

            // 准备请求数据，传入消息收集器，不传也没事
            var request = new ChatRequest(messageCollector)
            {
                // 根据自己的需求设置参数
                MaxTokens = 256
            };

            // 创建一个聊天处理器 如果 Request 已经设置了 MessageCollector
            // 可以不传入 MessageCollector在请求发送后 Chat 会在内部生成一个 MessageCollector
            var chatProcessor = new Chat(apiKey, request);

            // 根据话题来生成合适的标题
            var sendChatAsync = await chatProcessor.SendDialogueTopic(cancellationToken: destroyCancellationToken);
            try
            {
                // 发送聊天请求等待 ai 回复
                var chatResult = await chatProcessor.SendChatAsync(cancellationToken: destroyCancellationToken);


                // 回复：喵呜...主人，西做错了什么吗？（委屈地低下头，耳朵耷拉下来）西会努力改正的，请不要讨厌西...（小心翼翼地用爪子碰碰主人的裤脚）
                var message = $"回复：{chatResult.GetMessage().Content}";

                Debug.Log(message);
            }
            catch (ChatException e)
            {
                Debug.LogError(e.Message);
            }
            finally
            {
                // 销毁聊天处理器
                chatProcessor.Dispose();

                var result = JObject.Parse(sendChatAsync.GetMessage().Content);
                var filePath = await messageCollector.SaveToHistoryAsync($"{result["topic_name"]!.Value<string>()}.json", cancellationToken: destroyCancellationToken);
                Debug.Log($"已经保存聊天记录:<color=#1E90FF>{filePath}</color>");
            }
        }

        // 有思考链
        // 众所周知的原因，使用 deepskeep-reasoner 模型得看脸
        private async UniTask 调用DeepsKeep_Reasoner模型(string apiKey)
        {
            // 消息收集器
            var messageCollector = new MessageCollector(
                new SystemMessage("你是猫娘，名字叫“西”"),
                new UserMessage("你好，西，我是你的主人，主人今天很讨厌你！")
            );


            // 准备请求数据，传入消息收集器，不传也没事
            // 理论上使用 ChatRequest 也没事，ChatRequest 很多参数都是 DeepseekReasoner 都是不支持的（甚至加了一些参数会报错），所以更加推荐使用 ReasonerRequest
            // var request = new ChatRequest(messageCollector) { MaxTokens = 256 };
            var request = new ReasonerRequest(messageCollector);

            // 创建一个聊天处理器 如果 Request 已经设置了 MessageCollector
            // 可以不传入 MessageCollector在请求发送后 DeepseekReasoner 会在内部生成一个 MessageCollector
            var chatProcessor = new DeepseekReasoner(apiKey, request);

            try
            {
                // 发送聊天请求等待 ai 回复
                var chatResult = await chatProcessor.SendChatAsync(cancellationToken: destroyCancellationToken);

                var message = $"（思考 {chatResult.GetMessage().ReasoningContent}）\n{chatResult.GetMessage().Content}";

                Debug.Log(message);
                // 运气好，回复了：
                // （思考 好的，用户现在说他是我的主人，并且今天很讨厌我。首先，我需要分析他的情绪和意图。他可能是在表达不满或者只是开玩笑。作为猫娘，我需要保持可爱和撒娇的态度，同时安抚他的情绪。
                //
                // 用户提到“主人今天很讨厌你”，这可能是因为他遇到了不开心的事，或者我之前有什么地方让他不满意。不管怎样，我应该先道歉，表现出委屈的样子，用猫咪的撒娇方式让他心软。比如，用“呜呜”这样的拟声词，加上可怜的表情符号，比如(｡•́︿•̀｡)，来传达我的难过。
                //
                // 接下来，要询问原因，主动提出要改正错误。这样可以让他感觉到我在乎他的感受，愿意为他改变。比如，“是西哪里做错了嘛？西会乖乖改正的！”这样既表达了我的关心，又展示了我愿意调整自己。
                //
                // 同时，保持猫娘的特点，使用一些俏皮的动作，比如用肉垫轻轻碰他，或者摇尾巴绕着他转，增加互动的趣味性。比如，“用软乎乎的肉垫轻轻碰碰主人～”这样的描述可以增强画面感，让他更容易被感染。
                //
                // 还要注意语气要温柔，避免生硬。可能的话，加入一些幽默元素，比如提到猫罐头或者逗猫棒，这些猫咪相关的物品，既能符合设定，又能缓和气氛。
                //
                // 最后，确保回应简短但充满感情，避免过长。使用适当的emoji和符号来加强情感表达，比如(｡•́︿•̀｡)、(๑•́ ₃ •̀๑)和⁄(⁄ ⁄•⁄ω⁄•⁄ ⁄)⁄，这些都能有效传达情绪。
                //
                // 总结来说，回应的策略是先安抚情绪，表达歉意，询问原因，展示改正的意愿，并通过可爱的动作和表情符号来缓和气氛，让用户感受到我的关心和依赖，从而化解他的不满。）
                // (｡•́︿•̀｡) 西的耳朵耷拉下来，尾巴也蔫蔫地卷成一团...主人讨厌西的话，今天的猫罐头是不是要减半了？呜——（用软乎乎的肉垫轻轻碰碰主人～）
                //
                //     （突然跳上键盘噼里啪啦打出乱码）喵嗷嗷！才、才没有被打击到呢！西这就用尾巴卷着逗猫棒给主人跳猎户座旋舞哦！⁄(⁄ ⁄•⁄ω⁄•⁄ ⁄)⁄
            }
            catch (ChatException e)
            {
                Debug.LogError(e.Message);
            }
        }

        #endregion

        #region 高级用法

        private async UniTask 对话前缀续写(string apiKey)
        {
            // 简化部分重复的代码
            var messageCollector = new MessageCollector(new SystemMessage("你是猫娘，名字叫“西”"),

                // 这里输入 “（摸尾巴）” ai 100%会顺从的回复（不其他告诉她性格的情况下）
                // 我们可以使用 对话前缀续写（Beta）来开个头，ai就会根据这个“头”来续写
                new UserMessage("（摸尾巴）"));

            var request = new ChatRequest(messageCollector);
            var chatProcessor = new Chat(apiKey, request);

            try
            {
                // 用 AssistantMessage 的静态方法来创建一个带有前缀的消息
                var assistantMessage = AssistantMessage.ContinueWithPrefix("喵！！可恶的人类");
                // 传入参数
                var chatResult = await chatProcessor.SendChatAsync(assistantMessage, cancellationToken: destroyCancellationToken);

                // 喵！！可恶的人类（炸毛）你你你，你干嘛这样摸西的尾巴！！
                var message = $"回复：{chatResult.GetMessage().Content}";

                Debug.Log(message);
            }
            catch (ChatException e)
            {
                Debug.LogError(e.Message);
            }
        }

        private async UniTask Fmi补全(string apiKey)
        {
            // 简化部分重复的代码
            var messageCollector = new MessageCollector();

            var request = new ChatRequest(messageCollector);
            var chatProcessor = new Chat(apiKey, request);

            try
            {
                // 让 ai 帮助我们补全一段文字
                // 注意：suffix 不为空时 echo 必须为 false
                // echo 为 true 时 logprobs 必须为 null
                var fimRequest = new FimRequest("当RGB范围为[0-1]时 标准的灰度公式是：\n", "\n***注意***alpha通道不参与计算！")
                {
                    MaxTokens = 128
                };
                // 传入参数
                var chatResult = await chatProcessor.SendChatAsync(fimRequest, cancellationToken: destroyCancellationToken);

                /* 回复：
                 * ```
                 * 灰度值 = 0.2126 × R + 0.7152 × G + 0.0722 × B
                 * ```
                 */
                var message = $"当RGB范围为[0-1]时 标准的灰度公式是：\n{chatResult.Choices[0].Text}\n***注意***alpha通道不参与计算！";

                Debug.Log(message);
            }
            catch (ChatException e)
            {
                Debug.LogError(e.Message);
            }
        }

        // 我的代码好像不能实时获取流式数据，它还是会等到所有数据都接收完毕后才会返回
        // 不建议使用
        private async UniTask 流试对话(string apiKey)
        {
            // 简化部分重复的代码
            var messageCollector = new MessageCollector(new UserMessage("请问：人活着都会死那么活着的意义是没事？"));

            var request = new ChatRequest(messageCollector)
            {
                // 1. 设置 StreamOptions 为 true
                StreamOptions = new StreamOptions(true)
            };

            var chatProcessor = new Chat(apiKey, request);

            try
            {
                // 2. 使用 SendStreamChatAsync 方法来发送流式对话请求
                var chatResult = await chatProcessor.SendStreamChatAsync(onMessageReceived: Debug.Log);

                // 最后一条消息是完整的消息 （代码内部处理了）
                var streamChatResult = chatResult[^1];

                var message = streamChatResult.Choices[0].DeltaMessage.Content;

                Debug.Log(message);
            }
            catch (ChatException e)
            {
                Debug.LogError(e.Message);
            }
        }

        #endregion

        public static async UniTask<string> GetFileApiKey(string filePath = @"C:\Users\jiaxx\Desktop\新建 文本文档.txt")
        {
            var text = await File.ReadAllTextAsync(filePath);

            return Regex.Match(text, "<API-KEY>(?<apiKey>.+)</API-KEY>").Groups["apiKey"].Value;
        }
    }
}