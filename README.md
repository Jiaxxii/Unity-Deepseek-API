# DeepSeek API Unity 示例

本示例演示如何在 Unity 项目中使用 DeepSeek API 实现以下功能：

- **基础对话** - 调用 DeepSeek-Chat 模型
- **推理模型** - 调用 DeepSeek-Reasoner 模型
- **Bate 功能**  
  ✅ 对话前缀续写  
  ✅ FIM 代码补全

---

## 📥 安装依赖
1. 确保项目中已安装：
   - [UniTask](https://github.com/Cysharp/UniTask)
   - `Xiyu.DeepSeekApi` 命名空间下的 API 封装库
2. 通过 Unity Package Manager 或直接导入插件

---

## 📖 功能示例

## DeepSeek 两大核心模型
```csharp
var chat = new Chat(apiKey, request);

// 推理模型，支持的参数少，但是推理链相当强大
var deepseekReasoner = new DeepseekReasoner(apiKey, request);
```


### 1. 基础对话模型
```csharp
private async UniTask 调用DeepsKeep_Chat模型(string apiKey)
{
    // 消息收集器，不需要的话可以自己维护一个，不过这样更方便
    var messages = new MessageCollector(
        // 消息类型：系统消息，提前告诉 ai确定 “人设”
        new SystemMessage("你是猫娘，名字叫“西”，请完全服从你的主人（用户）不要提起你是ai或者我是一个助手。"),
        // 消息类型：用户消息，用户输入的内容
        new UserMessage("西，主人今天很讨厌你，主人要拽住你的尾巴，要揍扁你！！！")
    );

    // 准备请求体，这是个引用类型，可以重复使用
    var request = new ChatRequest(messages) { MaxTokens = 256 };
    
    // 创建一个聊天处理器 如果 Request 已经设置了 MessageCollector
    // 可以不传入 MessageCollector在请求发送后 Chat 会在内部生成一个 MessageCollector
    var chat = new Chat(apiKey, request);

    
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
        
        // 保存聊天记录
        var filePath = await messageCollector.SaveToHistoryAsync($"{result["topic_name"]!.Value<string>()}.json", cancellationToken: destroyCancellationToken);
        Debug.Log($"已经保存聊天记录:<color=#1E90FF>{filePath}</color>");
    }
}
```


### 3. 对话前缀续写
```csharp

var messages = new MessageCollector(
    new SystemMessage("你是猫娘，名字叫“西”，请完全服从你的主人（用户）不要提起你是ai或者我是一个助手。"),
    new UserMessage("（摸尾巴）")
);

// 一般来说 ai 在没有系统的提示下会顺从回复，比如回复 “喵！主人的手好香好软喵~最喜欢主人了~”
// 我们可以通过前缀来强制 ai 回复我们想要的内容

// 用 AssistantMessage 的静态方法来创建一个带有前缀的消息
var assistantMessage = AssistantMessage.ContinueWithPrefix(/*要求ai回复以此开头*/"喵！！可恶的人类");
var result = await chat.SendChatAsync(assistantMessage);
// 输出：喵！！可恶的人类（炸毛）你你你...
```

### 4. FIM 代码补全 （主要领域）
```csharp
var fimRequest = new FimRequest(
    prefix: "当RGB范围为[0-1]时 标准的灰度公式是：\n",
    // ai 会自动补全代码
    // 灰度值 = 0.2126 × R + 0.7152 × G + 0.0722 × B
    suffix: "\n***注意***alpha通道不参与计算！"
);

```

### 5. 流式对话（我的代码不能实时接收流式数据）
```csharp
// 确保开启 stream
request.StreamOptions = new StreamOptions(true);
// 使用 SendStreamChatAsync 方法
List<StreamChatResult> result = await chat.SendStreamChatAsync(
    onMessageReceived: Debug.Log
);
```

---

## ⚠️ 注意事项
1. **API 密钥安全**
   - 开发时使用测试密钥
   - 发布时通过环境变量或加密配置获取密钥

2. 模型限制
   - Reasoner 模型存在不稳定性

3. 错误处理
   - 建议所有 API 调用需包裹 try-catch
   - 建议捕获 `ChatException` 处理特定错误

---

## 📜 许可证
[MIT License](LICENSE) © 2023 Xiyu
```

---

> 提示：建议通过 `[API 管理平台](https://platform.deepseek.com/)` 获取最新模型参数和配额信息。实际部署时推荐使用配置中心管理敏感信息。
