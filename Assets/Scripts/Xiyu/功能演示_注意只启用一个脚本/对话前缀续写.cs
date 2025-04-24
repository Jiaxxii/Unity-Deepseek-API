using Cysharp.Threading.Tasks;
using Xiyu.DeepSeek.Messages;
using Xiyu.DeepSeek.Responses.Expand;

namespace Xiyu.功能演示_注意只启用一个脚本
{
    public class 对话前缀续写 : ChatProcessorBase
    {
        protected override async void Start()
        {
            base.Start();

            _messagesCollector.AppendUserMessage("你是叫璃雨吧？（打量）");


            // 我给的人设大概是 “高冷、不苟言笑、讨厌人类的” （人设定义见：Resources文件夹SystemPrompt.txt）
            // ai 回复大概率 会以 “（眼神厌恶）”、“（一下打开手）”、“（炸毛）”之类的话

            // 我们可以使用对话前缀续写来强制 ai 以我们提供的前缀开头
            var prefixMessage = new AssistantPrefixMessage("（无动于衷）呼……");
            var chatCompletion = await _processor.ChatCompletionStreamAsync(prefixMessage, onReceiveData: data =>
            {
                if (data.HasCompleteMsg())
                {
                    PrintText(data.GetMessage().Content);
                }
            });
            
            //////////////////////////////////////////////////////////////////////

            await UniTask.WaitForSeconds(3);
            ClearText();

            // 我们可以更加极端，我们发送 “是我救了你，我就是你的主人（生气）！” ai 绝对不会回复这种：
            // “（锋利的爪子抓向）恶心的人类，去死吧！”
            _messagesCollector.AppendUserMessage("是我救了你，我就是你的主人（生气）！");

            // 但是最好不要提到“死”这类话题，ai可能会直接拒绝回答
            var secondPrefixMessage = new AssistantPrefixMessage("（锋利的爪子抓向）恶心的人类，");
            var secondChatCompletion = await _processor.ChatCompletionStreamAsync(secondPrefixMessage, onReceiveData: data =>
            {
                if (data.HasCompleteMsg())
                {
                    PrintText(data.GetMessage().Content);
                }
            });

            PrintCount(chatCompletion.Usage + secondChatCompletion.Usage);
        }
    }
}