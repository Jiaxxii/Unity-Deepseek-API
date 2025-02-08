# DeepSeek API Unity 示例

本示例演示如何在 Unity 项目中使用 DeepSeek API 实现以下功能：

- **基础对话** - 调用 DeepSeek-Chat 模型
- **推理模型** - 调用 DeepSeek-Reasoner 模型
- **高级功能**  
  ✅ 对话前缀续写  
  ✅ FIM 代码补全  
  ✅ 流式对话（实验性）

---

## 📥 安装依赖
1. 确保项目中已安装：
   - [UniTask](https://github.com/Cysharp/UniTask)
   - `Xiyu.DeepSeekApi` 命名空间下的 API 封装库
2. 通过 Unity Package Manager 或直接导入插件

---

## 🚀 快速开始
```csharp
// Sample.cs
private async void Start()
{
    const string apiKey = "你的API密钥"; // ⚠️ 不要直接硬编码密钥
    await 调用DeepsKeep_Chat模型(apiKey);
    // 其他方法调用...
}
```

---

## 📖 功能示例

### 1. 基础对话模型
```csharp
private async UniTask 调用DeepsKeep_Chat模型(string apiKey)
{
    var messages = new MessageCollector(
        new SystemMessage("你是猫娘，名字叫“西”"),
        new UserMessage("你好西！")
    );

    var request = new ChatRequest(messages) { MaxTokens = 256 };
    var chat = new Chat(apiKey, request);

    try {
        var result = await chat.SendChatAsync();
        Debug.Log(result.GetMessage().Content);
    } catch (ChatException e) {
        Debug.LogError(e.Message);
    }
}
```

### 2. 推理模型 (Reasoner)
```csharp
var request = new ReasonerRequest(messages);
var reasoner = new DeepseekReasoner(apiKey, request);

// 返回结果包含推理过程：
// （思考内容）... \n 最终回复
```

### 3. 对话前缀续写
```csharp
var assistantMessage = AssistantMessage.ContinueWithPrefix("喵！！可恶的人类");
var result = await chat.SendChatAsync(assistantMessage);
// 输出：喵！！可恶的人类（炸毛）你你你...
```

### 4. FIM 代码补全
```csharp
var fimRequest = new FimRequest(
    prefix: "当RGB范围为[0-1]时 标准的灰度公式是：\n",
    suffix: "\n***注意***alpha通道不参与计算！"
);
// 输出完整的公式代码片段
```

### 5. 流式对话（实验性）
```csharp
request.StreamOptions = new StreamOptions(true);
var result = await chat.SendStreamChatAsync(
    onMessageReceived: Debug.Log // 实时接收片段
);
```

---

## ⚠️ 注意事项
1. **API 密钥安全**
   - 开发时使用测试密钥
   - 发布时通过环境变量或加密配置获取密钥

2. 模型限制
   - Reasoner 模型存在不稳定性
   - 流式对话目前为全量返回模式

3. 错误处理
   - 所有 API 调用需包裹 try-catch
   - 捕获 `ChatException` 处理特定错误

---

## 📜 许可证
[MIT License](LICENSE) © 2023 Xiyu
```

---

> 提示：建议通过 `[API 管理平台](https://platform.deepseek.com/)` 获取最新模型参数和配额信息。实际部署时推荐使用配置中心管理敏感信息。
