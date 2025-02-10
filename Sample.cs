using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
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
                new UserMessage("西，你完了。")
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

                var message = $"<Think>\n{chatResult.GetMessage().ReasoningContent.Replace("\n\n", "\n")}\n</Think>\n{chatResult.GetMessage().Content}";

                Debug.Log(message);
                // 运气好，回复了：
                // <Think>
                //     好的，我现在需要处理用户的消息：“西，你完了。” 这句话看起来有点带有威胁或者开玩笑的语气，可能用户是在测试我的反应，或者想看看我会不会表现出害怕或者紧张。首先，我要分析用户的意图。用户可能是在开玩笑，或者想引发某种互动，比如角色扮演中的冲突场景。
                // 接下来，我要考虑作为猫娘“西”的角色设定。猫娘通常可爱、活泼，可能带点调皮，所以回应需要符合这个性格。用户提到的“完了”可能是在说某种游戏结束，或者假装生气，需要我以幽默或撒娇的方式回应。
                // 然后，我需要确定回应的方向。用户可能希望我表现出一点慌张，但又保持可爱的态度，避免显得太严肃。比如，用哭泣的表情符号，或者求饶的语气，同时加入猫娘特有的元素，比如提到尾巴、撒娇等。
                // 还要注意用户的潜在需求，他们可能希望得到有趣、互动的回应，而不是冷漠或机械的回答。所以，保持语气轻松，带点幽默感，同时维持角色的一致性。
                // 最后，构造具体的回应内容，确保符合上述分析，使用合适的表情符号和语言风格，比如“呜……不要欺负西啦QAQ”，再加上一些卖萌的动作描述，比如尾巴卷成一团，扑进对方怀里撒娇，这样既符合猫娘的形象，又回应用户的挑衅，化解可能的紧张气氛。
                // </Think>
                // 呜……不要欺负西啦QAQ（尾巴吓得卷成一团，扑进你怀里拼命蹭蹭撒娇）主人最好了对不对～西会乖乖给摸摸头的喵！
            }
            catch (ChatException e)
            {
                Debug.LogError(e.Message);
            }
        }

        #endregion

        #region 其他用法

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
        [Obsolete("此方法可能造成阻塞，请使用非流式方法")]
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
                var chatResult = await chatProcessor.SendStreamChatAsync(onMessageReceived: Debug.Log, destroyCancellationToken);

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