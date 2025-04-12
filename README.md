# DeepSeek API Unity 集成文档

*本文档由 DeepSeek-R1 生成，最后更新于 2025年4月13日*  
*代码当前处于持续迭代阶段，具体实现可能随版本更新发生变化*

**更加详细的实现细节和上下文管理请参考项目中的 "Sample.cs" 脚本**

## 目录
- [核心类说明](#核心类说明)
- [快速开始](#快速开始)
- [功能详解](#功能详解)
  - [基础聊天功能](#基础聊天功能)
  - [流式响应处理](#流式响应处理)
  - [深度思考模型](#深度思考模型)
  - [前缀续写功能](#前缀续写功能)
- [注意事项](#注意事项)
- [最佳实践](#最佳实践)
- [其他](#其他)

---

## 核心类说明

### 1. 消息类型
```csharp
// 系统消息（设置AI角色）
new SystemMessage("请您扮演一个AI，你的名字叫DeepSeek")

// 用户消息（用户输入）
new UserMessage("你好")

// 助手消息（AI回复/前缀续写）
AssistantMessage.ContinueWithPrefix("已生成前缀")
```

### 2. 请求体类型
```csharp
// 普通聊天模型请求体
ChatRequest request = new ChatRequest {
    Messages = new MessageCollector(new SystemMessage("系统提示"))
};

// 深度思考模型请求体
ReasonerRequest request = new ReasonerRequest {
    Messages = new MessageCollector(new SystemMessage("系统提示"))
};
```

### 3. 处理器类型
```csharp
// 普通模型处理器
ChatProcessor processor = new Chat(apiKey, requestBody);

// 深度思考模型处理器
ChatProcessor processor = new DeepseekReasoner(apiKey, requestBody);
```

---

## 快速开始

### 初始化配置
```csharp
private IRequestBody _requestBody;
private ChatProcessor _chatProcessor;

void Awake()
{
    // 选择初始化方式
    InitReasonerChatModel(); // 深度思考模型
    // InitChatModel();       // 普通模型
}

private void InitReasonerChatModel()
{
    _requestBody = new ReasonerRequest {
        Messages = new MessageCollector(new SystemMessage("系统提示"))
    };
    _chatProcessor = new DeepseekReasoner(apiKey, _requestBody);
}
```

---

## 功能详解

### 基础聊天功能

#### 同步请求示例
```csharp
async UniTask SendChatAsync(string message)
{
    _requestBody.Messages.AddMessage(new UserMessage(message));
    _requestBody.Messages.CheckAndThrow();

    try {
        var result = await _chatProcessor.SendChatAsync(destroyCancellationToken);
        Debug.Log(result.GetMessage().Content);
    }
    catch (OperationCanceledException) {
        Debug.LogWarning("请求已取消");
    }
}
```

### 流式响应处理

#### 风格一：回调式流处理
```csharp
async UniTask StreamWithCallback(string message)
{
    Action<IEnumerable<StreamChatResult>> callback = data => {
        foreach(var chunk in data.Where(d => !d.IsDone)){
            Debug.Log(chunk.GetMessage().Content);
        }
    };

    _requestBody.Messages.AddMessage(new UserMessage(message));
    var finalResult = await _chatProcessor.SendStreamChatAsync(callback, destroyCancellationToken);
    Debug.Log($"最终结果：{finalResult.GetMessage().Content}");
}
```

#### 风格二：迭代器式流处理
```csharp
async UniTask StreamWithIterator(string message)
{
    StreamChatResult lastChunk = null;
    
    await foreach (var chunk in _chatProcessor.SendStreamChatAsync(destroyCancellationToken)) 
    {
        if(!chunk.IsDone){
            Debug.Log(chunk.GetMessage().Content);
            lastChunk = chunk;
        }
    }
    
    Debug.Log($"总消耗Token：{lastChunk.Usage.Value.TotalTokens}");
}
```

### 深度思考模型

#### 特殊处理说明
```csharp
// 处理流式响应时的推荐方法
void HandleReasonerChunk(StreamChatResult chunk)
{
    var (msgType, content) = chunk.GetReasonerMessage();
    
    switch(msgType){
        case ModelType.DeepseekReasoner:
            Debug.Log($"<思考过程> {content}");
            break;
        case ModelType.Chat:
            Debug.Log($"<正式回复> {content}");
            break;
    }
}
```

### 前缀续写功能

#### 基础使用示例
```csharp
async UniTask ContinueWithPrefix(string message, string prefix)
{
    var continueOption = AssistantMessage.ContinueWithPrefix(prefix);
    
    // 同步方式
    var result = await _chatProcessor.SendChatAsync(continueOption, destroyCancellationToken);
    
    // 流式方式
    var streamResult = await _chatProcessor.SendStreamChatAsync(
        onReceiveData: chunks => {/* 处理片段 */}, 
        continueOption, 
        destroyCancellationToken
    );
}
```

---

## 注意事项

### 1. 并发请求限制
```csharp
// 错误示例（禁止并行请求）
await UniTask.WhenAll(
    SendChatAsync("问题1"),
    SendChatAsync("问题2")
);

// 正确做法（顺序执行）
await SendChatAsync("问题1");
await SendChatAsync("问题2");
```

### 2. 深度思考模型限制
- **2025年4月13日已知问题**：使用前缀续写功能时，思考内容（ReasoningContent）可能返回空值
- 推荐处理方式：
  ```csharp
  var result = await GetResponse();
  if(string.IsNullOrEmpty(result.ReasoningContent)){
      Debug.Log("官方API当前限制，思考内容不可用");
  }
  ```

### 3. 消息队列管理
```csharp
// 每次请求前检查消息结构
_requestBody.Messages.CheckAndThrow();

// 典型消息序列
Messages = [
    SystemMessage,
    UserMessage,
    AssistantMessage,
    UserMessage // 最后一条必须是用户消息
]
```

---

## 最佳实践

### Token 使用监控
```csharp
void LogUsage(StreamChatResult result)
{
    var usage = result.Usage.Value;
    Debug.Log($"本次消耗：" +
        $"\n- 输入Token: {usage.PromptTokens}" +
        $"\n- 输出Token: {usage.CompletionTokens}" +
        $"\n- 总数: {usage.TotalTokens}");
}
```

### 错误处理增强
```csharp
try {
    // ... API调用 ...
}
catch (ApiException ex) when(ex.StatusCode == 429){
    Debug.LogError("请求频率过高，请稍后重试");
}
catch (ApiException ex){
    Debug.LogError($"API错误：{ex.Message}");
}
catch (OperationCanceledException){
    Debug.Log("用户取消请求");
}
```

### 历史记录管理
```csharp
// 保留最近3轮对话
void TrimHistory()
{
    if(_requestBody.Messages.Count > 6){
        // 保留系统消息 + 最近3组对话
        var newMessages = new MessageCollector(_requestBody.Messages[0]);
        newMessages.AddRange(_requestBody.Messages.Skip(_requestBody.Messages.Count-5));
        _requestBody.Messages = newMessages;
    }
}
```

### 其他
```txt
好的，我现在需要帮用户分析他们提供的API示例文档，并按照他们的要求生成一个详细的中文Markdown格式文档。首先，我需要仔细阅读用户提供的代码和具体要求，确保理解每个部分的功能和使用方法。

用户的要求有几个关键点：
1. **代码示例需要详细注释**，不能访问未定义的属性。这意味着我要确保每个代码块都有清晰的解释，特别是涉及API调用的部分。
2. **代码还在迭代中**，所以文档需要说明这一点，提醒读者未来可能会有变化。
3. **声明深度思考模型在使用前缀续写时思考返回为空的问题**，这可能是官方的问题，需要特别标注。
4. **声明文档作者是DeepSeek-R1生成**，这点要放在合适的位置，比如开头或结尾。
5. **没有字数限制，越详细越好**，所以需要覆盖所有功能点，尽可能详细地解释每个方法和参数。
6. **在合适的位置提示参考“Sample.cs”脚本**，可能在每个主要部分或示例代码附近添加这个提示。

接下来，我需要分析提供的C#代码。代码是一个Unity的示例脚本，主要功能是使用DeepSeek的API进行聊天和深度思考模型的请求。包括同步和异步流式处理的不同方。

首先，结构上，代码分为初始化、普通聊天模型和深度思考模型的不同方法，以及各种请求方式（同步、流式风格一、风格二）。每个方法都有详细的注释，但需要转为文档中的说明。

我需要将每个方法的功能、参数、使用场景详细描述，特别是代码中的关键点如`SendChatAsync`、`SendStreamChatFirstAsync`等。同时，注意用户提到的三种请求 方式：异步等待、异步流式回调版、异步迭代器版，这些都需要分别说明。

在处理深度思考模型时，需要强调前缀续写功能的问题，即思考内容可能为空，这点要在相关章节突出显示。另外，代码中的异常处理（如OperationCanceledException）也需要在文档中提及，说明如何取消请求和错误处理。

关于代码迭代的声明，可能在文档的开头或每个章节末尾加入提示，说明当前版本和可能的更新。作者声明放在文档末尾比较合适。

最后，确保所有代码示例的注释详细，没有引用未定义的属性。例如，检查每个示例中的变量和方法是否都在代码中有定义，避免出现混淆或错误的信息。

完成这些分析后，组织文档结构，分为概述、快速开始、核心类说明、功能详解、注意事项等部分，确保逻辑清晰，方便开发者快速上手和查阅。
```
---
