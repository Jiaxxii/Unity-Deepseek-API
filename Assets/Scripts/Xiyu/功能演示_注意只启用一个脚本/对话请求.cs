using System.Numerics;
using Xiyu.DeepSeek.Responses.Expand;

namespace Xiyu.功能演示_注意只启用一个脚本
{
    public class 对话请求 : ChatProcessorBase
    {
        protected override async void Start()
        {
            base.Start();

            // 发送消息 =》 早上好（扫了一眼昨天给她的晚饭）嗯……（完全没动）
            _messagesCollector.AppendUserMessage("你是…璃雨？");

            // 不需要实时显示推荐使用此方法
            // var chatCompletion = await _processor.ChatCompletionAsync();
            // PrintText(chatCompletion.GetMessage().Content);

            var chatCompletion = await _processor.ChatCompletionStreamAsync(onReceiveData: data =>
            {
                if (data.HasCompleteMsg())
                {
                    PrintText(data.GetMessage().Content);
                }
            });

            PrintText(
                $"\n\n</b><color=#65c2ca>{chatCompletion.Usage.TotalTokens}</color> <i>tokens</i> (<color=#c481cf><b>≈ {chatCompletion.CalculatePrice()}</b></color><color=red>￥</color>)");
        }
    }
}