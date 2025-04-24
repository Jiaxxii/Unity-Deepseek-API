using Xiyu.DeepSeek;
using Xiyu.DeepSeek.Requests;
using Xiyu.DeepSeek.Responses.Expand;

namespace Xiyu.功能演示_注意只启用一个脚本
{
    public class FIM补全 : ChatProcessorBase
    {
        protected override async void Start()
        {
            base.Start();

            // FIM 补全只有 deepseek-chat 模型可以用
            var deepseekChat = (DeepseekChat)_processor;

            // FIM补全 不需要拼接上下文
            // 提供开头（必填） 结尾（非必填），ai会补全中间的内容
            var fimRequest = new FimRequest("以下是C#中使用File.ReadAllText的示例```cs")
            {
                Suffix = "\r\n```\r\n\n希望能帮助到您，有什么问题请继续向我提问！",
                Echo = true
            };


            // 注意* FIM补全 不会把消息放到 messagesCollector 中，如果需要请指定参数 recordToMessageList 为 true
            var chatCompletion = await deepseekChat.FimChatCompletionStreamAsync(fimRequest, onReceiveData: data =>
            {
                if (data.HasCompleteMsg())
                {
                    PrintText(data.GetMessage().Text);
                }
            }, recordToMessageList: false);

            await deepseekChat.FimChatCompletionAsync(fimRequest, recordToMessageList: true);

            PrintCount(chatCompletion.Usage);
        }
    }
}