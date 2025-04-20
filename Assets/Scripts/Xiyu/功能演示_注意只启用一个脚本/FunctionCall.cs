using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
            deepseekChat.AddFunction(new KeyValuePair<string, Func<Function, UniTask<string>>>("get_local_info", GetLocalInfo));

            // 不接受工具的回应消息 （默认已经开启了）
            // deepseekChat.ReceiveToolData = false;

            _messagesCollector.AppendUserMessage("你知道\"九陆-自然\"吗，听说那是类猫人聚集最多的地方？");

            var chatCompletion = await deepseekChat.ChatCompletionStreamAsync(onReceiveData: data =>
            {
                if (data.HasCompleteMsg())
                {
                    PrintText(data.GetMessage().Content);
                }
            });


            PrintText($"\n\nToken统计：{chatCompletion.Usage.TotalTokens}");
        }

        // 描述方法 GetLocalInfo
        private static FunctionDescription FunctionByGetLocalInfo()
        {
            return new FunctionDescription
            {
                // 方法名称：建议使用蛇型命名法 GetLocalInfo => get_local_info
                Name = "get_local_info",
                // 方法描述
                Description = "获取\"九陆大区\"的信息，用户应该先提供一个具体的九陆名称",
                // 参数列表
                Parameters = new
                {
                    type = "object",
                    properties = new
                    {
                        // 参数名称
                        localName = new
                        {
                            // 参数类型
                            type = "string",
                            // 参数描述
                            description = "九陆大区的具体名称，如“九陆-古里”"
                        },
                    }
                },
                // 必须赋值的参数：
                // (序列化的时候会把“localName”变成蛇型命名法的“local_name”)
                Required = new[] { "local_name" }
            };
        }


        // 定义方法 
        private UniTask<string> GetLocalInfo(Function function)
        {
            var localName = function.Arguments;
            if (localName.Contains("九陆-自然"))
            {
                return UniTask.FromResult("占地面积27.55万平方公里，是除“九陆-山海樱”之后第二大的类人猫聚集地，" +
                                          "人类想进入“九陆-自然”是需要申请签证的，并且极少数人能拿到，这里的文明程度" +
                                          "虽然不亚于人类，但是被称为“九陆之最-宜居地带”、“类猫人的深圳”，烟烟小时候" +
                                          "生活在这里，回来搬迁到了“九陆-北海樱”");
            }

            if (localName.Contains("九陆-山海樱"))
            {
                return UniTask.FromResult("占地面积30.89万平方公里，虽然九陆中最大的，还是全世界热带雨林占地面积最大的" +
                                          "，但是环境多变，不宜依据，根据《2025年类人猫口统计调查》统计只有不到10万人猫口。");
            }

            if (localName.Contains("九陆-北海樱"))
            {
                return UniTask.FromResult("");
            }

            return UniTask.FromResult("错误-无法获取指定信息！");
        }
    }
}