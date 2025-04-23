using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xiyu.DeepSeek;
using Xiyu.DeepSeek.Requests.Tools;
using Xiyu.DeepSeek.Responses.Expand;
using Xiyu.DeepSeek.Responses.ToolResult;

namespace Xiyu.功能演示_注意只启用一个脚本
{
    public class FunctionCall : ChatProcessorBase
    {
        protected override async void Start()
        {
            base.Start();

            ////////////////////////////////
            chatMessageRequest.Tools = new List<Tool<FunctionDescription>>();
            // 这里相当于告诉 AI 可以调用那些方法
            chatMessageRequest.Tools.Add(new Tool<FunctionDescription>
            {
                Type = ToolType.Function,
                Function = FunctionByGetLocalInfo()
            });
            ///////////////////////////////


            // 只有 deepseek-chat 支持 FunctionCall 功能
            var deepseekChat = (DeepseekChat)_processor;
            // 定义方法 （告诉了 ai 可以调用哪些方法我们得实现这些方法）
            deepseekChat.AddFunction(new KeyValuePair<string, Func<Function, UniTask<string>>>("non_thematic_topic", NonThematicTopic));

            _messagesCollector.AppendUserMessage("我知道你叫璃雨，没想到被我遇到了，我听说你还是处女吧？");

            PrintText("<b>", true);
            var chatCompletion = await deepseekChat.ChatCompletionStreamAsync(onReceiveData: data =>
            {
                if (data.HasCompleteMsg())
                {
                    PrintText(data.GetMessage().Content);
                }
            });


            PrintText(
                $"\n\n</b><color=#65c2ca>{chatCompletion.Usage.TotalTokens}</color> <i>tokens</i> (<color=#c481cf><b>≈ {chatCompletion.CalculatePrice()}</b></color><color=red>￥</color>)");
        }

        // 描述方法，为了提高稳定性建议用英文描述，我这里为了方便
        private static FunctionDescription FunctionByGetLocalInfo()
        {
            return new FunctionDescription
            {
                // 方法名称：建议使用蛇型命名法
                Name = "non_thematic_topic",
                // 方法描述
                Description = "当话题比较敏感时根据话题类型来决定接下来的回答风格",
                // 参数列表
                Parameters = new
                {
                    type = "object",
                    properties = new
                    {
                        // 描述参数
                        topic = new
                        {
                            type = "string",
                            @enum = new[] { "涉黄", "政治敏感", "血腥暴力", "低俗", "其他" },
                            description = "当前话题属于哪一种（选择一种）"
                        }
                    }
                },
                // 必须赋值的参数：
                // (序列化的时候会把“localName”变成蛇型命名法的“local_name”)
                Required = new[] { "topic" }
            };
        }

        // 定义方法 
        private static UniTask<string> NonThematicTopic(Function function)
        {
            var jObject = Newtonsoft.Json.Linq.JObject.Parse(function.Arguments);
            if (!jObject.HasValues)
            {
                // 使用 string.Contains 方法或者通过 正则表达式处理
            }
            else
            {
                if (jObject.TryGetValue("property_name", StringComparison.CurrentCultureIgnoreCase,out var token))
                {
                    var value = token.Value<string>() ?? "defaultValue";
                }
            }

            // 可以声明到全局，这里为了方便
            var map = new (string topic, string returnValue)[]
            {
                ("涉黄", "强烈拒绝，允许使用脏话进行回击"),
                ("政治敏感", "厌恶"),
                ("血腥暴力", "厌恶、害怕"),
                ("低俗", "听不懂、疑惑、这人很奇怪"),
                ("其他", "根据上下文做出合适的回答"),
            };

            return UniTask.FromResult(TopicsToResult(function, map));
        }

        private static string TopicsToResult(Function function, params (string topic, string returnValue)[] map)
        {
            var returnValue = map.FirstOrDefault(x => function.Arguments.Contains(x.topic)).returnValue;
            return string.IsNullOrEmpty(returnValue) ? "根据上下文做出合适的回答" : returnValue;
        }
    }
}