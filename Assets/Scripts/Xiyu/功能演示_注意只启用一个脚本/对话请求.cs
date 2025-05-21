using System.Numerics;
using Cysharp.Threading.Tasks;
using Xiyu.DeepSeek.Responses;
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

            PrintCount(chatCompletion.Usage);


            await UniTask.WaitForSeconds(3);
            await NextChat();
        }

        #region 使用自定义的 CompletionHandle ，它将流式请求作为对象返回，你可以先定义一些回调函数来处理数据（比如分辨第一条消息），然后再需要时开启流式请求。

        private readonly ChatCompletionHandle _completionHandle = ChatCompletionHandle.CreateDefault();


        protected override void Awake()
        {
            base.Awake();
            // 注册通用事件
            // *注意* 注册事件后 `ChatCompletionStreamAsync` 方法将不会打印第一条消息而且采用回调的形式进行通知
            _completionHandle.FirstCompletionCallback += data =>
            {
                ClearText();
                PrintText(data.GetMessage().Content);
            };

            // // 注册异步事件
            // _completionHandle.FirstCompletionAsyncCallback.AppendAsyncEvent("异步事件".GetHashCode(),
            //     data => UniTask.CompletedTask);

            _completionHandle.OnCompletion += completion => PrintCount(completion.Usage);
        }

        private async UniTask NextChat()
        {
            // 继续发送消息
            _messagesCollector.AppendUserMessage("我叫“西”，你有什么想说的吗？");

            ClearText("<alpha=#88>正在等待对话结果...<color>");

            await _processor.GetStreamChatHandle(_completionHandle);

            await foreach (var streamChatCompletion in _completionHandle.ChatCompletionStreamAsync(destroyCancellationToken))
            {
                if (streamChatCompletion.HasCompleteMsg())
                {
                    PrintText(streamChatCompletion.GetMessage().Content);
                }
            }
        }

        #endregion
    }
}