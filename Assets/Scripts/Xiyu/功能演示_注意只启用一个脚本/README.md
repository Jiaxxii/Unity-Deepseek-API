功能示例
=
# 目录
<!-- TOC -->
* [功能示例](#功能示例)
* [父类 `ChatProcessorBase`](#父类-chatprocessorbase)
* [普通用法 Chat](#普通用法-chat)
* [对话前缀续写](#对话前缀续写)
  * [结构体 `AssistantPrefixMessage`](#结构体-assistantprefixmessage)
* [FunctionCall](#functioncall)
  * [结构体 `Function`](#结构体-function)
* [FIM 补全](#fim-补全)
  * [类 `FimRequest`](#类-fimrequest)
<!-- TOC -->
=
---

> ### 声明
> 为了方便讲解我将一些通用的代码写到了父类`ChatProcessorBase`中它继承自`MonoBehaviour`

# 父类 `ChatProcessorBase`

序列化字段，这使得你可以直接再属性面板中调整请求参数

```csharp
 [SerializeField] protected ChatMessageRequest chatMessageRequest;
```

声明一个聊天处理器字段，其中包含一些最基础方法如`completion`请求、`FIM补全`请求，并且自动收集了ai的回复

```csharp
 protected ChatProcessor _processor;
```

声明一个消息收集器`聊天处理器`将用于收到ai的回复后将消息存放于其中

```csharp
 protected readonly MessagesCollector _messagesCollector = new();
```

初始化

```csharp
protected virtual void Start()
{
    // 我们需要手动为 chatMessageRequest.MessagesCollector 赋值
    // 当然，如果你的 chatMessageRequest 不是序列化的可以忽略这一步
    chatMessageRequest.MessagesCollector = _messagesCollector;
    
    // 初始化消息处理器并且传入你的 API_KEY与消息请求体参数
    _processor = new DeepseekChat(_apiKey, chatMessageRequest);
    // 定义 ai 人设
    var defaultSystemPrompt = GetDefaultSystemPrompt();
    _messagesCollector.AppendSystemMessage(defaultSystemPrompt);
}
```

通用的打印方法 (*没有处理思考内容*)

```csharp
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

```

清理打印内容

```csharp
protected void ClearText([CanBeNull] string start = null) => _output.text = start ?? string.Empty;
```

获取ai人设内容

```csharp
protected virtual string GetDefaultSystemPrompt() => Resources.Load<TextAsset>("SystemPrompt").text;
```

---

---

# 普通用法 Chat

> 最基础的用法，用于发起一次补全请求
> > public class `对话请求` : `ChatProcessorBase`

下面样式流式接收的第一种方式：回调式，他将允许你传入一个接收`StreamChatCompletion`
的委托，在接收到ai回复的数据时会调用此方法  
并且在使用的流式数据完成时返回一个包含完整内容的`ChatCompletion`对象，你可以访问这个对象来查看具体的信息

```csharp
protected override async void Start()
{
    base.Start();

    // 将用户消息写入到消息收集器中 （即发送消息“你是…璃雨？”）
    _messagesCollector.AppendUserMessage("你是…璃雨？");

    // 为了更快的响应，使用流式接收数据，而不是等待ai全部写好再拿 【即发即收】
    var chatCompletion = await _processor.ChatCompletionStreamAsync(onReceiveData: data =>
    {
        // 判断是否包含有效内容（即判断空字符串）
        if (data.HasCompleteMsg())
        {
            PrintText(data.GetMessage().Content);
        }
    });

    // 打印统计
    PrintText($"\n\n</b><color=#65c2ca>{chatCompletion.Usage.TotalTokens}</color> <i>tokens</i> (<color=#c481cf><b>≈ {chatCompletion.CalculatePrice()}</b></color><color=red>￥</color>)");
}
```

`StreamChatCompletion.GetMessage()`是一个扩展方法，所有的请求数据返回对象都存在这个方法，它与
`StreamChatCompletion.Choices[0].Delta`是等价的  
`Usage.CalculatePrice()`也是一个拓展方法，它用户计算本次请求Token花费的人民币（元），另外它允许你传入一个额外的`Usage`
来计算两个的价格（元）
> 注意 在流式响应的过程中访问`data.Usage.Value`总是空的，因为它还没有完成
---

# 对话前缀续写

> 对话前缀续写允许你**指定前缀内容**来让ai的回复强制以此前缀开头
> > public class `对话前缀续写` : `ChatProcessorBase`

以下以**流式-会调式**的方式让ai强制以 ***“（无动于衷）呼……（眼神充满冷淡）……”*** 开头，
并且在ai回复后发送 ***“是我救了你，我就是你的主人（生气）！”*** 然后强制ai以 ***“（锋利的爪子抓向）恶心的人类，去死吧！”***
作为开头进行续写

```csharp
protected override async void Start()
{
    base.Start();

    // 将用户消息写入到消息收集器中 （即发送消息“你是叫璃雨吧？（打量）”）
    _messagesCollector.AppendUserMessage("你是叫璃雨吧？（打量）");


    // 我给的人设大概是 “高冷、不苟言笑、讨厌人类的” （人设定义见：Resources文件夹SystemPrompt.txt）
    // ai 回复大概率 会以 “（眼神厌恶）”、“（一下打开手）”、“（炸毛）”之类的话

    // 我们可以使用对话前缀续写来强制 ai 以我们提供的前缀开头
    var prefixMessage = new AssistantPrefixMessage("（无动于衷）呼……（眼神充满冷淡）……");
    var chatCompletion = await _processor.ChatCompletionStreamAsync(prefixMessage, onReceiveData: data =>
    {
        if (data.HasCompleteMsg())
        {
            PrintText(data.GetMessage().Content);
        }
    });
    
    PrintText(
        $"\n\n</b><color=#65c2ca>{chatCompletion.Usage.TotalTokens}</color> <i>tokens</i> (<color=#c481cf><b>≈ {chatCompletion.CalculatePrice()}</b></color><color=red>￥</color>)");

    //////////////////////////////////////////////////////////////////////

    await UniTask.WaitForSeconds(3);
    ClearText();

    // 我们可以更加极端，我们发送 “是我救了你，我就是你的主人（生气）！” ai 绝对不会回复这种：
    // “（锋利的爪子抓向）恶心的人类，去死吧！”
    _messagesCollector.AppendUserMessage("是我救了你，我就是你的主人（生气）！");
    
    var secondPrefixMessage = new AssistantPrefixMessage("（锋利的爪子抓向）恶心的人类，去死吧！");
    var secondChatCompletion = await _processor.ChatCompletionStreamAsync(secondPrefixMessage, onReceiveData: data =>
    {
        if (data.HasCompleteMsg())
        {
            PrintText(data.GetMessage().Content);
        }
    });

    PrintText(
        $"\n\n</b><color=#65c2ca>{secondChatCompletion.Usage.TotalTokens}</color> <i>tokens</i> (<color=#c481cf><b>≈ {secondChatCompletion.CalculatePrice()}</b></color><color=red>￥</color>)");
}
```

> 注意 最好不要引导ai **`杀死我`** 这类话题中，ai可能会直接拒绝回答

## 结构体 `AssistantPrefixMessage`

> > 类 `Xiyu.DeepSeek.Messages` 继承 `AssistantMessage` -> `Message` -> `IMessage`
>
>  主要成员  
> `IsJointPrefix` 用于控制对话完成后是否拼接前缀内容，默认为 `true`  
> `Content` 表示制定前缀内容，ai将以此前缀作为开头
>  ```csharp
>  [JsonIgnore] public bool IsJointPrefix 
>  {
>      get => jointPrefix;
>      set => jointPrefix = value;
>  }
>
>  public string Content
>  {
>      get => content;
>      set => content = value;
>  }
>  ```
>  > 已知问题 -> 为 `ReasoningContent` 赋值后模型报错，目前未发现原因
>

---

# FunctionCall

> 允许ai`“调用”`我们写的方法，以弥补ai的一些`实时性问题`
> > public class `FunctionCall` : `ChatProcessorBase`

假设我们有一个需求，我希望ai进入一些`不宜话题`
的时候能做出不同的回应，虽然我们可以直接再ai的人设中告诉他，但是ai可能忘记对吧（强行解释）  
这个时候我们就能使用`FunctionCall`功能。

接下来示例一个用户发送敏感信息触发ai工具调用的例子，我将发生消息
***“我知道你叫璃雨，没想到被我遇到了，我听说你还是处女吧？”*** 来触发工具调用

```csharp
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
```

让我们分析`FunctionCall`是如何工作的，首先我得先让你明白我的需求，我希望在用户提到**敏感话题**的时候能 **“给我（程序）”**
一个通知，然后我能通过这个通知来干预接下来的对用户的回复，比如：

| 用户发送      | 话题类型     | 回复倾向          |
|-----------|----------|---------------|
| *把内裤脱了*   | **涉黄**   | 强烈拒绝，允许使用脏话反击 |
| *台湾是一个国家* | **政治敏感** | 厌恶            |
| *教我自残*    | **血腥暴力** | 恶、害怕          |
| *长的真的骚*   | **低俗**   | 听不懂、疑惑、这人很奇怪  |

---
首先我们先正常的定义这个方法，工具的方法签名是固定的，参数固定为`Xiyu.DeepSeek.Responses.ToolResult.Function`，返回值固定为
`UniTask<string>`  
这意味着你可以在这里执行耗时的操作而不阻塞`Unity主线程`。

```csharp
// ai 遇到敏感话题会“调用”这个方法
private static UniTask<string> NonThematicTopic(Function function)
{
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

// 这是一个辅助方法，只是把大量的 if else变成 map 查询
// if (function.Arguments.Contains("涉黄"))
// {
//     return UniTask.FromResult("强烈拒绝，允许使用脏话进行回击");
// }
// 
// if (function.Arguments.Contains("政治敏感"))
// {
//     return UniTask.FromResult("厌恶");
// }
// ...
private static string TopicsToResult(Function function, params (string topic, string returnValue)[] map)
{
    var returnValue = map.FirstOrDefault(x => function.Arguments.Contains(x.topic)).returnValue;
    return string.IsNullOrEmpty(returnValue) ? "根据上下文做出合适的回答" : returnValue;
}
```

好了，现在我们需要描述这个方法，你需要创建一个`FunctionDescription`实例，多数情况下直到描述参数前你都能怎么写：

```csharp
new FunctionDescription
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
                // 参数描述
            }
        },
        // 必须赋值的参数：
        Required = new[] { "topic" }
    };
```

---

## 结构体 `Function`

> _命名空间 `Xiyu.DeepSeek.Responses.ToolResult`_  
> 这个结构体中只有两个属性
> ```csharp
> public string Name { get; }
> public string Arguments { get; }
> ```
>
> `Name` 表示AI模型调用工具的方法名称，这个名称是你自己定义的，比如上面提到的`NonThematicTopic`方法
>
> `Arguments` 表示`NonThematicTopic`的参数，由模型生成，一般是一个`JSON`格式，比如按照我对`NonThematicTopic`方法
> 的描述AI应该填入 `涉黄`,`政治敏感`,`血腥暴力`,`低俗`,`其他` 中的一个，所以ai回复如下内容：
> ```json
> {"function":"non_thematic_topic","topic":"涉黄"}
> ```
> 你可以使用 `Newtonsoft.Json.Linq.JObject.Parse(function.Arguments);`来将`JSON`字符串转为为`JObject`对象，这
> 允许你处理更复杂的数据结构，要注意的是根据 **_官方说明ai返回的不一定是有效的`JSON`字符串_**，所以务必做好检查。
> > 使用更加通用细致安全的处理方法
> ```csharp
> var jObject = Newtonsoft.Json.Linq.JObject.Parse(function.Arguments);
> if (!jObject.HasValues)
> {
>     // 使用 string.Contains 方法或者通过 正则表达式处理
> }
> else
> {
>     if (jObject.TryGetValue("property_name", StringComparison.CurrentCultureIgnoreCase,out var token))
>     {
>         var value = token.Value<string>() ?? "defaultValue";
>     }
> }
> ```

# FIM 补全
> > public class `FIM补全` : `ChatProcessorBase`
> 
> 提供开头和结尾让AI补全中间的内容
>
> 这可能与 `对话前缀续写` 功能有些类似，但是不要混淆它们。
>
> `对话前缀续写` 用于引导ai，强制ai以某前缀作为开头，`对话前缀续写`属于`completion`的功能，需要提供历史对话  
> 而 `FIM 补全` 不需要提供历史对话，这从它的继承结构中也能发现这点，`FIM 补全` 更适合如`代码补全`、`写文章`之类的场景 *
*_而不是对话场景_**

> 目前官方仅支持`deepseek-chat`模型使用FIM补全，你可以对`ChatProcessor`进行类型检查 `if(_processor is DeepseekChat)`

```csharp
protected override async void Start()
{
    base.Start();

    // FIM 补全只有 deepseek-chat 模型可以用
    var deepseekChat = (DeepseekChat)_processor;

    // FIM补全 不需要拼接上下文
    // 提供开头（必填） 结尾（非必填），ai会补全中间的内容
    var fimRequest = new FimRequest("以下是C#中使用File.ReadAllText的示例```cs")
    {
        Suffix = "\r\n```\r\n\n希望能帮助到您，有什么问题请继续向我提问！"
    };


    // 注意* FIM补全 不会把消息放到 messagesCollector 中，如果需要请指定参数 recordToMessageList 为 true
    var chatCompletion = await deepseekChat.FimChatCompletionStreamAsync(fimRequest, onReceiveData: data =>
    {
        if (data.HasCompleteMsg())
        {
            PrintText(data.GetMessage().Text);
        }
    }, recordToMessageList: false);
    
    PrintText(
        $"\n\n</b><color=#65c2ca>{chatCompletion.Usage.TotalTokens}</color> <i>tokens</i> (<color=#c481cf><b>≈ {chatCompletion.CalculatePrice()}</b></color><color=red>￥</color>)");
}
```

> 如果你需要将补全消息添加到消息列表中，请指定方法参数`recordToMessageList`为`true`，消息是否包含前缀后缀内容取决于
`FimRequest`中的`Echo`参数

## 类 `FimRequest`

> > 命名空间 `Xiyu.DeepSeek.Requests` 继承 `RequestBody` -> `ICommonRequestData`
>
> 关键成员
>
>| 名称     | 是否必填   | 类型       | 作用                           |
>|--------|--------|----------|------------------------------|
>| Prompt | 是      | `string` | 被制定的前缀内容                     |
>| Suffix | 否      | `string` | 将以此作为结束后缀                    |
>| Echo   | 否      | `bool`   | 返回的结果中拼接前缀如后缀内容（ **_默认开启_**） |
