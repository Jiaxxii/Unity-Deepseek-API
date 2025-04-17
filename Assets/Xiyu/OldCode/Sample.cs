#if DEEPSEEK_PAST_CODE
// using System;
// using System.Collections.Generic;
// using Cysharp.Threading.Tasks;
// using UnityEngine;
// using Xiyu.DeepSeekApi;
// using Xiyu.DeepSeekApi.Request;
// using Xiyu.DeepSeekApi.Response.Stream;
//
// namespace Xiyu
// {
//     public class Sample : MonoBehaviour
//     {
//         [SerializeField] private string apiKey;
//
//         // 推荐声明法，然后在 start 或者 awake 中初始化
//         private IRequestBody _requestBody;
//         private ChatProcessor _chatProcessor;
//         
//         private IMessageUnits _messageCollector;
//
//         #region 初始化
//
//         /// <summary>
//         /// 初始化
//         /// </summary>
//         private void Awake()
//         {
//             _messageCollector = new MessageCollector();
//             // 初始化请求体 //
//             // 目前有两种： ChatRequest（最适合普通模型-Chat） 和 ReasonerRequest（适合深度思考模型-DeepseekReasoner）
//
//             // InitChatModel(); // 不深度思考模型
//             InitReasonerChatModel(); // 深度思考模型
//         }
//
//         private void InitChatModel()
//         {
//             _requestBody = new ChatRequest
//             {
//                 // 初始化消息收集器并且定义ai人设
//                 Messages = new MessageCollector(new SystemMessage("请您扮演一个AI，你的名字叫DeepSeek")),
//             };
//
//             // 消息处理器 //
//             // "requestBody"的实例是 ChatRequest 还是 ReasonerRequest 混用它们理论上不会有问题（因为本质上都是创建差不多的请求体）
//             // 但是为了不混淆还是用最适合它们的
//             _chatProcessor = new Chat(apiKey, _requestBody);
//
//             // 普通模型使用： new Chat()
//             // 需要深度思考使用： new DeepseekReasoner()
//         }
//
//         private void InitReasonerChatModel()
//         {
//             _requestBody = new ReasonerRequest
//             {
//                 // 初始化消息收集器并且定义ai人设
//                 Messages = new MessageCollector(new SystemMessage("请您扮演一个AI，你的名字叫DeepSeek")),
//             };
//
//             // 消息处理器 //
//             // "requestBody"的实例是 ChatRequest 还是 ReasonerRequest 混用它们理论上不会有问题（因为本质上都是创建差不多的请求体）
//             // 但是为了不混淆还是用最适合它们的
//             _chatProcessor = new DeepseekReasoner(apiKey, _requestBody);
//
//             // 普通模型使用： new Chat()
//             // 需要深度思考使用： new DeepseekReasoner()
//         }
//
//         #endregion
//
//         private void Start()
//         {
//             // 可以发送请求并且等待后再调用其他请求方法
//             // 必须再一个方法 await 之后再调用另一个方法
//             // 不能同时调用多个方法，比如：
//             // await UniTask.WhenAll(
//             //     SendChatAsync("你好呀，我是西雨与雨，你能直接告诉我 2186014465 + 5644106821 等于多少吗？"),
//             //     SendStreamChatFirstAsync("你好呀，我是西雨与雨，你能告诉我什么是Token吗（ai领域）？"),
//             //     SendStreamChatSecondAsync("你好呀，我是西雨与雨，你能告诉我 C# 的 ValueTask 有什么用吗？")
//             // );
//
//             #region Chat 模型
//
//             // await SendChatAsync("你好呀，我是西雨与雨，你能直接告诉我 2186014465 + 5644106821 等于多少吗？");
//             // Debug.Log($"<color=yellow>{new string('-', 30)}</color>");
//             //
//             // await SendStreamChatFirstAsync("你好呀，我是西雨与雨，你能告诉我什么是Token吗（ai领域）？");
//             // Debug.Log($"<color=yellow>{new string('-', 30)}</color>");
//             //
//             // await SendStreamChatSecondAsync("你好呀，我是西雨与雨，你能告诉我 C# 的 ValueTask 有什么用吗？");
//             // Debug.Log($"<color=yellow>{new string('-', 30)}</color>");
//
//             // await SendChatAsync("你好呀，我是西，你知道1+1=几吗？", "你好西，1+");
//             // Debug.Log($"<color=yellow>{new string('-', 30)}</color>");
//
//             // await SendStreamChatFirstAsync("你好呀，我是西，你知道1+1=几吗？", "你好西，1+");
//             // Debug.Log($"<color=yellow>{new string('-', 30)}</color>");
//
//             // await SendStreamChatSecondAsync("你好呀，我是西，你知道1+1=几吗？", "你好西，1+");
//             // Debug.Log($"<color=yellow>{new string('-', 50)}</color>");
//
//             // 
//
//             #endregion
//
//             #region 深度思考模型
//
//             // await SendChatReasonerAsync("你好呀，我是西雨与雨，你能告诉我C#中如何快速简洁的计算 2186014465 + 5644106821 等于多少吗？");
//             // Debug.Log($"<color=yellow>{new string('-', 30)}</color>");
//
//
//             // await SendStreamChatReasonerFirstAsync("你好呀，我是西雨与雨，你能告诉我什么是Token吗（ai领域）？");
//             // Debug.Log($"<color=yellow>{new string('-', 30)}</color>");
//
//
//             // await SendStreamChatReasonerSecondAsync("你好呀，我是西雨与雨，你能告诉我 C# 的 ValueTask 有什么用吗？");
//             // Debug.Log($"<color=yellow>{new string('-', 30)}</color>");
//
//
//             // await SendChatReasonerAsync("你好呀，我是西，我想知道大前天是今天的哪一天？", "你好呀西，大前天");
//             // Debug.Log($"<color=yellow>{new string('-', 30)}</color>");
//
//             // await SendStreamChatReasonerFirstAsync("你好呀，我是西，我想知道大前天是今天的哪一天？", "你好呀西，大前天");
//             // Debug.Log($"<color=yellow>{new string('-', 30)}</color>");
//
//             // await SendStreamChatReasonerSecondAsync("你好呀，我是西，我想知道大前天是今天的哪一天？", "你好呀西，大前天");
//             // Debug.Log($"<color=yellow>{new string('-', 30)}</color>");
//
//             #endregion
//         }
//
//
//         #region Chat
//
//         /// <summary>
//         /// 发起一次聊天请求
//         /// </summary>
//         private async UniTask SendChatAsync(string message)
//         {
//             Debug.Log("<color=#11d8ff>Model</color>：聊天请求");
//             Debug.Log($"<color=#11d8ff>Q：</color>：{message}");
//             // 消息
//             _requestBody.Messages.AddMessage(new UserMessage(message));
//             // 检查消息是否规范 （多数情况下请求前的最后一条消息必须是 User）
//             _requestBody.Messages.CheckAndThrow();
//
//             try
//             {
//                 // destroyCancellationToken ：建议添加，不然程序停止方法可能还在请求
//                 var chatResult = await _chatProcessor.SendChatAsync(destroyCancellationToken);
//
//                 // 等效：chatResult.Choices[0].Message.Content
//                 var content = chatResult.GetMessage().Content;
//
//                 Debug.Log($"回复：{content}");
//             }
//             catch (OperationCanceledException)
//             {
//                 Debug.LogWarning("请求已经取消");
//             }
//         }
//
//         /// <summary>
//         /// 发起一次聊天请求 【流式返回-即发即收】【风格一】
//         /// </summary>
//         private async UniTask SendStreamChatFirstAsync(string content)
//         {
//             Debug.Log("<color=#11d8ff>Model</color>：【流式返回-即发即收】【风格一】");
//             Debug.Log($"<color=#11d8ff>Q：</color>：{content}");
//             // （可以直接声明到onReceiveData参数中）
//             // 获取到数据片段时会调用此方法
//             Action<IEnumerable<StreamChatResult>> onReceiveData = data =>
//             {
//                 // 获取消息有 3 种方法，根据需求选择其中一种
//
//                 // 1.通过扩展方法直接获取这个片段的消息 （推荐）
//                 var message = data.GetMessage();
//
//                 // 2.通过 System.Linq 的 Select 方法 选择每个数据判断子项并且调用 GetMessage() 方法来获取消息，最后通过string.Concat 来拼接
//                 // message = string.Concat(data.Select(d => d.GetMessage().Content));
//
//                 // 3.通过 System.Linq 的 Where 方法先排除掉 DONE（结束块没有任何数据），然后选择每个数据片段子项通过传统的调用
//                 // Choices[0].DeltaMessage.Content 来获取消息最后通过string.Concat 来拼接
//                 // message = string.Concat(
//                 //     data.Where(d => !d.IsDone).Select(d => d.Choices[0].DeltaMessage.Content)
//                 // );
//
//                 Debug.Log(message);
//             };
//
//             // 消息
//             _requestBody.Messages.AddMessage(new UserMessage(content));
//             // 检查消息是否规范 （多数情况下请求前的最后一条消息必须是 User）
//             _requestBody.Messages.CheckAndThrow();
//
//             try
//             {
//                 // 调用流式方法，并且传递接收到消息时的回调 
//                 // destroyCancellationToken ：建议添加，不然程序停止方法可能还在请求
//                 var result = await _chatProcessor.SendStreamChatAsync(onReceiveData: onReceiveData, destroyCancellationToken);
//
//                 // result 会统计请求消耗的 Token，并且result中有完整的消息
//                 var fullMessage = result.GetMessage().Content;
//                 Debug.Log($"完整消息：{fullMessage}");
//
//                 // 这是 ToKen 统计
//                 var usage = result.Usage!.Value;
//             }
//             catch (OperationCanceledException)
//             {
//                 Debug.LogWarning("请求已经取消！");
//             }
//         }
//
//         /// <summary>
//         /// 发起一次聊天请求 【流式返回-即发即收】【风格二】
//         /// </summary>
//         /// <param name="content"></param>
//         private async UniTask SendStreamChatSecondAsync(string content)
//         {
//             Debug.Log("<color=#11d8ff>Model</color>：【流式返回-即发即收】【风格二】");
//             Debug.Log($"<color=#11d8ff>Q：</color>：{content}");
//             // 消息
//             _requestBody.Messages.AddMessage(new UserMessage(content));
//             // 检查消息是否规范 （多数情况下请求前的最后一条消息必须是 User）
//             _requestBody.Messages.CheckAndThrow();
//
//             // 调用流式方法，通过异步迭代器获取数据
//             // destroyCancellationToken ：建议添加，不然程序停止方法可能还在请求
//             StreamChatResult lastStreamChatResult = null;
//             try
//             {
//                 await foreach (var data in _chatProcessor.SendStreamChatAsync(destroyCancellationToken))
//                 {
//                     // 获取消息
//                     var message = data.GetMessage().Content;
//                     // 或者 string.IsNullOrEmpty(data.GetMessage().Content)
//
//                     // 最后一个块包含了Token消耗数量
//                     lastStreamChatResult = data;
//
//                     // 空消息不输出 （可选）
//                     if (string.IsNullOrEmpty(message))
//                     {
//                         continue;
//                     }
//
//                     Debug.Log(message);
//                 }
//             }
//             catch (OperationCanceledException)
//             {
//                 Debug.LogWarning("请求已经取消！");
//                 return;
//             }
//
//
//             if (lastStreamChatResult != null)
//             {
//                 // 这是 ToKen 统计
//                 var usage = lastStreamChatResult.Usage!.Value;
//             }
//         }
//
//         /// <summary>
//         /// 发起一次聊天请求 （对话前缀续写）
//         /// </summary>
//         private async UniTask SendChatAsync(string message, string prefix)
//         {
//             Debug.Log("<color=#11d8ff>Model</color>：对话前缀续写");
//             Debug.Log($"<color=#11d8ff>Q：</color>：{message}");
//             // 消息
//             _requestBody.Messages.AddMessage(new UserMessage(message));
//             // 检查消息是否规范 （多数情况下请求前的最后一条消息必须是 User）
//             _requestBody.Messages.CheckAndThrow();
//
//             try
//             {
//                 // destroyCancellationToken ：建议添加，不然程序停止方法可能还在请求
//                 // 建议使用静态方法创建
//                 var continueWithPrefix = AssistantMessage.ContinueWithPrefix(prefix);
//                 var chatResult = await _chatProcessor.SendChatAsync(continueWithPrefix, destroyCancellationToken);
//
//                 // 等效：chatResult.Choices[0].Message.Content
//                 var content = chatResult.GetMessage().Content;
//
//                 Debug.Log($"回复：{content}");
//             }
//             catch (OperationCanceledException)
//             {
//                 Debug.LogWarning("请求已经取消");
//             }
//         }
//
//         /// <summary>
//         /// 发起一次聊天请求 （对话前缀续写）【流式返回-即发即收】【风格一】
//         /// </summary>
//         private async UniTask SendStreamChatFirstAsync(string content, string prefix)
//         {
//             Debug.Log("<color=#11d8ff>Model</color>：【对话前缀续写】【流式返回-即发即收】【风格一】");
//             Debug.Log($"<color=#11d8ff>Q：</color>：{content}");
//             // （可以直接声明到onReceiveData参数中）
//             // 获取到数据片段时会调用此方法
//             Action<IEnumerable<StreamChatResult>> onReceiveData = data =>
//             {
//                 // 获取消息有 3 种方法，根据需求选择其中一种
//
//                 // 1.通过扩展方法直接获取这个片段的消息 （推荐）
//                 var message = data.GetMessage();
//
//                 // 2.通过 System.Linq 的 Select 方法 选择每个数据判断子项并且调用 GetMessage() 方法来获取消息，最后通过string.Concat 来拼接
//                 // message = string.Concat(data.Select(d => d.GetMessage().Content));
//
//                 // 3.通过 System.Linq 的 Where 方法先排除掉 DONE（结束块没有任何数据），然后选择每个数据片段子项通过传统的调用
//                 // Choices[0].DeltaMessage.Content 来获取消息最后通过string.Concat 来拼接
//                 // message = string.Concat(
//                 //     data.Where(d => !d.IsDone).Select(d => d.Choices[0].DeltaMessage.Content)
//                 // );
//
//                 Debug.Log(message);
//             };
//
//             // 消息
//             _requestBody.Messages.AddMessage(new UserMessage(content));
//             // 检查消息是否规范 （多数情况下请求前的最后一条消息必须是 User）
//             _requestBody.Messages.CheckAndThrow();
//
//             try
//             {
//                 // 调用流式方法，并且传递接收到消息时的回调 
//                 // destroyCancellationToken ：建议添加，不然程序停止方法可能还在请求
//                 // 建议使用静态方法创建
//                 var continueWithPrefix = AssistantMessage.ContinueWithPrefix(prefix);
//                 var result = await _chatProcessor.SendStreamChatAsync(onReceiveData, continueWithPrefix, destroyCancellationToken);
//
//                 // result 会统计请求消耗的 Token，并且result中有完整的消息
//                 var fullMessage = result.GetMessage().Content;
//                 Debug.Log($"完整消息：{fullMessage}");
//
//                 // 这是 ToKen 统计
//                 var usage = result.Usage!.Value;
//             }
//             catch (OperationCanceledException)
//             {
//                 Debug.LogWarning("请求已经取消！");
//             }
//         }
//
//         /// <summary>
//         /// 发起一次聊天请求 【流式返回-即发即收】【风格二】
//         /// </summary>
//         private async UniTask SendStreamChatSecondAsync(string content, string prefix)
//         {
//             Debug.Log("<color=#11d8ff>Model</color>：【对话前缀续写】【流式返回-即发即收】【风格二】");
//             Debug.Log($"<color=#11d8ff>Q：</color>：{content}");
//             // 消息
//             _requestBody.Messages.AddMessage(new UserMessage(content));
//             // 检查消息是否规范 （多数情况下请求前的最后一条消息必须是 User）
//             _requestBody.Messages.CheckAndThrow();
//
//             // 调用流式方法，通过异步迭代器获取数据
//             // destroyCancellationToken ：建议添加，不然程序停止方法可能还在请求
//             StreamChatResult lastStreamChatResult = null;
//             var continueWithPrefix = AssistantMessage.ContinueWithPrefix(prefix);
//             try
//             {
//                 await foreach (var data in _chatProcessor.SendStreamChatAsync(continueWithPrefix, destroyCancellationToken))
//                 {
//                     // 获取消息
//                     var message = data.GetMessage().Content;
//                     // 或者 string.IsNullOrEmpty(data.GetMessage().Content)
//
//                     // 最后一个块包含了Token消耗数量
//                     lastStreamChatResult = data;
//
//                     // 空消息不输出 （可选）
//                     if (string.IsNullOrEmpty(message))
//                     {
//                         continue;
//                     }
//
//                     Debug.Log(message);
//                 }
//             }
//             catch (OperationCanceledException)
//             {
//                 Debug.LogWarning("请求已经取消！");
//                 return;
//             }
//
//
//             if (lastStreamChatResult != null)
//             {
//                 // 这是 ToKen 统计
//                 var usage = lastStreamChatResult.Usage!.Value;
//             }
//         }
//
//         #endregion
//
//
//         #region Reasoner
//
//         /// <summary>
//         /// 发起一次聊天请求 【思考】
//         /// </summary>
//         private async UniTask SendChatReasonerAsync(string content)
//         {
//             Debug.Log("<color=#11d8ff>Model</color>：【思考】");
//             Debug.Log($"<color=#11d8ff>Q：</color>：{content}");
//             _requestBody.Messages.AddMessage(new UserMessage(content));
//             // 检查消息是否规范 （多数情况下请求前的最后一条消息必须是 User）
//             _requestBody.Messages.CheckAndThrow();
//
//             try
//             {
//                 // destroyCancellationToken ：建议添加，不然程序停止方法可能还在请求
//                 var chatResult = await _chatProcessor.SendChatAsync(destroyCancellationToken);
//
//                 // 等效：chatResult.Choices[0].Message
//                 var message = chatResult.GetMessage();
//
//                 Debug.Log($"<color=yellow>思考</color>：{message.ReasoningContent}");
//                 Debug.Log($"回复：{message.Content}");
//             }
//             catch (OperationCanceledException)
//             {
//                 Debug.LogWarning("请求已经取消");
//             }
//         }
//
//         /// <summary>
//         /// 发起一次聊天请求 【思考】【流式返回-即发即收】【风格一】
//         /// </summary>
//         private async UniTask SendStreamChatReasonerFirstAsync(string content)
//         {
//             Debug.Log("<color=#11d8ff>Model</color>：【思考】【流式返回-即发即收】【风格一】");
//             Debug.Log($"<color=#11d8ff>Q：</color>：{content}");
//             // （可以直接声明到onReceiveData参数中）
//             // 获取到数据片段时会调用此方法
//             Action<IEnumerable<StreamChatResult>> onReceiveData = data =>
//             {
//                 // 遍历每个数据流
//                 foreach (var streamChatResult in data)
//                 {
//                     if (streamChatResult.IsDone)
//                     {
//                         break;
//                     }
//
//                     // 读取方法1 （推荐）
//                     // 通过拓展方法来获取当前数据片段子项的消息
//                     var (msgType, msg) = streamChatResult.GetReasonerMessage();
//
//                     if (msgType == ModelType.None)
//                     {
//                         continue;
//                     }
//
//                     Debug.Log(msgType == ModelType.DeepseekReasoner ? $"<color=#a35835>{msg}</color>" : msg);
//
//
//                     // 读取方法2
//                     // // 获取消息 等价 streamChatResult.Choices[0] 
//                     // var message = streamChatResult.GetMessage();
//                     //
//                     // // 分别判断 内容 和 思考 是否为空
//                     // var isContentNull = string.IsNullOrEmpty(message.Content);
//                     // var isReasoningContentNull = string.IsNullOrEmpty(message.ReasoningContent);
//                     //
//                     // // 如果两个都为空 则 这个数据段可能没有生成任何 Token 或者 Token 是空格？（我也不确定）
//                     // if (isContentNull && isReasoningContentNull)
//                     // {
//                     //     // 跳过
//                     //     continue;
//                     // }
//                     //
//                     // // 谁是空的就 不 打印谁
//                     // // 一般来说 前面的都会是思考 （isContentNull == true） 后面都是内容 （isReasoningContentNull == false）
//                     // Debug.Log(isContentNull
//                     //     ? $"<color=#a35835>{message.ReasoningContent}</color>"
//                     //     : message.Content
//                     // );
//                 }
//             };
//
//             // 消息
//             _requestBody.Messages.AddMessage(new UserMessage(content));
//             // 检查消息是否规范 （多数情况下请求前的最后一条消息必须是 User）
//             _requestBody.Messages.CheckAndThrow();
//
//             try
//             {
//                 // 调用流式方法，并且传递接收到消息时的回调 
//                 // destroyCancellationToken ：建议添加，不然程序停止方法可能还在请求
//                 var result = await _chatProcessor.SendStreamChatAsync(onReceiveData: onReceiveData, destroyCancellationToken);
//
//                 // result 会统计请求消耗的 Token，并且result中有完整的消息
//                 var fullMessage = result.GetMessage();
//                 Debug.Log($"<color=yellow>思考</color>：<color=#51626f>{fullMessage.ReasoningContent}</color>");
//                 Debug.Log($"完整消息：{fullMessage.Content}");
//
//                 // 这是 ToKen 统计
//                 var usage = result.Usage!.Value;
//             }
//             catch (OperationCanceledException)
//             {
//                 Debug.LogWarning("请求已经取消！");
//             }
//         }
//
//         /// <summary>
//         /// 发起一次聊天请求 【思考】【流式返回-即发即收】【风格二】
//         /// </summary>
//         /// <param name="content"></param>
//         private async UniTask SendStreamChatReasonerSecondAsync(string content)
//         {
//             Debug.Log("<color=#11d8ff>Model</color>：【思考】【流式返回-即发即收】【风格二】");
//             Debug.Log($"<color=#11d8ff>Q：</color>：{content}");
//             // 消息
//             _requestBody.Messages.AddMessage(new UserMessage(content));
//             // 检查消息是否规范 （多数情况下请求前的最后一条消息必须是 User）
//             _requestBody.Messages.CheckAndThrow();
//
//             // 调用流式方法，通过异步迭代器获取数据
//             // destroyCancellationToken ：建议添加，不然程序停止方法可能还在请求
//             StreamChatResult lastStreamChatResult = null;
//             try
//             {
//                 await foreach (var data in _chatProcessor.SendStreamChatAsync(destroyCancellationToken))
//                 {
//                     if (data.IsDone)
//                     {
//                         break;
//                     }
//
//                     // 最后一个块包含了Token消耗数量
//                     lastStreamChatResult = data;
//
//                     // 读取方法1 （推荐）
//                     // 通过拓展方法来获取当前数据片段子项的消息
//                     var (msgType, msg) = data.GetReasonerMessage();
//
//                     if (msgType == ModelType.None)
//                     {
//                         continue;
//                     }
//
//                     Debug.Log(msgType == ModelType.DeepseekReasoner ? $"<color=#a35835>{msg}</color>" : msg);
//
//                     // // 读取方法2
//                     // // 获取消息 等价 data.Choices[0] 
//                     // var message = data.GetMessage();
//                     //
//                     // // 最后一个块包含了Token消耗数量
//                     // lastStreamChatResult = data;
//                     //
//                     // // 分别判断 内容 和 思考 是否为空
//                     // var isContentNull = string.IsNullOrEmpty(message.Content);
//                     // var isReasoningContentNull = string.IsNullOrEmpty(message.ReasoningContent);
//                     //
//                     // // 如果两个都为空 则 这个数据段可能没有生成任何 Token 或者 Token 是空格？（我也不确定）
//                     // if (isContentNull && isReasoningContentNull)
//                     // {
//                     //     // 跳过
//                     //     continue;
//                     // }
//                     //
//                     // // 谁是空的就 不 打印谁
//                     // // 一般来说 前面的都会是思考 （isContentNull == true） 后面都是内容 （isReasoningContentNull == false）
//                     // Debug.Log(isContentNull
//                     //     ? $"<color=#a35835>{message.ReasoningContent}</color>"
//                     //     : message.Content
//                     // );
//                 }
//             }
//             catch (OperationCanceledException)
//             {
//                 Debug.LogWarning("请求已经取消！");
//                 return;
//             }
//
//
//             if (lastStreamChatResult != null)
//             {
//                 // 这是 ToKen 统计
//                 var usage = lastStreamChatResult.Usage!.Value;
//             }
//         }
//
//         /// <summary>
//         /// 发起一次聊天请求 （对话前缀续写）【思考】 【流式返回-即收即发】 【深度思考好像不返回思考内容，也就是思考为空】
//         /// </summary>
//         private async UniTask SendChatReasonerAsync(string content, string prefix)
//         {
//             Debug.Log("<color=#11d8ff>Model</color>：对话前缀续写");
//             Debug.Log($"<color=#11d8ff>Q：</color>：{content}");
//             // 消息
//             _requestBody.Messages.AddMessage(new UserMessage(content));
//             // 检查消息是否规范 （多数情况下请求前的最后一条消息必须是 User）
//             _requestBody.Messages.CheckAndThrow();
//
//             try
//             {
//                 // destroyCancellationToken ：建议添加，不然程序停止方法可能还在请求
//                 // 建议使用静态方法创建
//                 var continueWithPrefix = AssistantMessage.ContinueWithPrefix(prefix);
//                 var chatResult = await _chatProcessor.SendChatAsync(continueWithPrefix, destroyCancellationToken);
//
//                 // 等效：chatResult.Choices[0].Message.Content
//                 var result = chatResult.GetMessage();
//
//                 Debug.Log($"<color=yellow>思考</color>：{result.ReasoningContent}");
//                 Debug.Log($"回复：{result.Content}");
//             }
//             catch (OperationCanceledException)
//             {
//                 Debug.LogWarning("请求已经取消");
//             }
//         }
//
//         /// <summary>
//         /// 发起一次聊天请求 （对话前缀续写）【思考】【流式返回-即发即收】【风格一】
//         /// </summary>
//         private async UniTask SendStreamChatReasonerFirstAsync(string content, string prefix)
//         {
//             Debug.Log("<color=#11d8ff>Model</color>：【思考】【对话前缀续写】【流式返回-即发即收】【风格一】");
//             Debug.Log($"<color=#11d8ff>Q：</color>：{content}");
//             // （可以直接声明到onReceiveData参数中）
//             // 获取到数据片段时会调用此方法
//             Action<IEnumerable<StreamChatResult>> onReceiveData = data =>
//             {
//                 // 获取消息有 3 种方法，根据需求选择其中一种
//
//                 // 1.通过扩展方法直接获取这个片段的消息 （推荐）
//                 var message = data.GetMessage();
//
//                 // 2.通过 System.Linq 的 Select 方法 选择每个数据判断子项并且调用 GetMessage() 方法来获取消息，最后通过string.Concat 来拼接
//                 // message = string.Concat(data.Select(d => d.GetMessage().Content));
//
//                 // 3.通过 System.Linq 的 Where 方法先排除掉 DONE（结束块没有任何数据），然后选择每个数据片段子项通过传统的调用
//                 // Choices[0].DeltaMessage.Content 来获取消息最后通过string.Concat 来拼接
//                 // message = string.Concat(
//                 //     data.Where(d => !d.IsDone).Select(d => d.Choices[0].DeltaMessage.Content)
//                 // );
//
//                 Debug.Log(message);
//             };
//
//             // 消息
//             _requestBody.Messages.AddMessage(new UserMessage(content));
//             // 检查消息是否规范 （多数情况下请求前的最后一条消息必须是 User）
//             _requestBody.Messages.CheckAndThrow();
//
//             try
//             {
//                 // 调用流式方法，并且传递接收到消息时的回调 
//                 // destroyCancellationToken ：建议添加，不然程序停止方法可能还在请求
//                 // 建议使用静态方法创建
//                 var continueWithPrefix = AssistantMessage.ContinueWithPrefix(prefix);
//                 var result = await _chatProcessor.SendStreamChatAsync(onReceiveData, continueWithPrefix, destroyCancellationToken);
//
//                 // result 会统计请求消耗的 Token，并且result中有完整的消息
//                 var fullMessage = result.GetMessage();
//
//                 Debug.Log($"<color=yellow>思考</color>：{fullMessage.ReasoningContent}");
//                 Debug.Log($"回复：{fullMessage.Content}");
//
//                 // 这是 ToKen 统计
//                 var usage = result.Usage!.Value;
//             }
//             catch (OperationCanceledException)
//             {
//                 Debug.LogWarning("请求已经取消！");
//             }
//         }
//
//         /// <summary>
//         /// 发起一次聊天请求 【思考】【流式返回-即发即收】【风格二】
//         /// </summary>
//         private async UniTask SendStreamChatReasonerSecondAsync(string content, string prefix)
//         {
//             Debug.Log("<color=#11d8ff>Model</color>：【思考】【对话前缀续写】【流式返回-即发即收】【风格二】");
//             Debug.Log($"<color=#11d8ff>Q：</color>：{content}");
//             // 消息
//             _requestBody.Messages.AddMessage(new UserMessage(content));
//             // 检查消息是否规范 （多数情况下请求前的最后一条消息必须是 User）
//             _requestBody.Messages.CheckAndThrow();
//
//             // 调用流式方法，通过异步迭代器获取数据
//             // destroyCancellationToken ：建议添加，不然程序停止方法可能还在请求
//             StreamChatResult lastStreamChatResult = null;
//             var continueWithPrefix = AssistantMessage.ContinueWithPrefix(prefix);
//             try
//             {
//                 await foreach (var data in _chatProcessor.SendStreamChatAsync(continueWithPrefix, destroyCancellationToken))
//                 {
//                     if (data.IsDone)
//                     {
//                         break;
//                     }
//
//                     // 最后一个块包含了Token消耗数量
//                     lastStreamChatResult = data;
//
//                     // 读取方法1 （推荐）
//                     // 通过拓展方法来获取当前数据片段子项的消息
//                     var (msgType, msg) = data.GetReasonerMessage();
//
//                     if (msgType == ModelType.None)
//                     {
//                         continue;
//                     }
//
//                     Debug.Log(msgType == ModelType.DeepseekReasoner ? $"<color=#a35835>{msg}</color>" : msg);
//
//                     // // 读取方法2
//                     // // 获取消息 等价 data.Choices[0] 
//                     // var message = data.GetMessage();
//                     //
//                     // // 最后一个块包含了Token消耗数量
//                     // lastStreamChatResult = data;
//                     //
//                     // // 分别判断 内容 和 思考 是否为空
//                     // var isContentNull = string.IsNullOrEmpty(message.Content);
//                     // var isReasoningContentNull = string.IsNullOrEmpty(message.ReasoningContent);
//                     //
//                     // // 如果两个都为空 则 这个数据段可能没有生成任何 Token 或者 Token 是空格？（我也不确定）
//                     // if (isContentNull && isReasoningContentNull)
//                     // {
//                     //     // 跳过
//                     //     continue;
//                     // }
//                     //
//                     // // 谁是空的就 不 打印谁
//                     // // 一般来说 前面的都会是思考 （isContentNull == true） 后面都是内容 （isReasoningContentNull == false）
//                     // Debug.Log(isContentNull
//                     //     ? $"<color=#a35835>{message.ReasoningContent}</color>"
//                     //     : message.Content
//                     // );
//                 }
//             }
//             catch (OperationCanceledException)
//             {
//                 Debug.LogWarning("请求已经取消！");
//                 return;
//             }
//
//
//             if (lastStreamChatResult != null)
//             {
//                 // 这是 ToKen 统计
//                 var usage = lastStreamChatResult.Usage!.Value;
//             }
//         }
//
//         #endregion
//     }
// }
#endif