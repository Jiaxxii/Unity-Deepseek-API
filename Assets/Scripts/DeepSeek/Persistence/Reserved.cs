namespace Xiyu.DeepSeek.Persistence
{
    /// <summary>
    ///定义一组保留标志，用于根据消息的角色和类型对其进行过滤或分类。
    ///此枚举旨在处理位操作，允许组合多个标志。
    /// </summary>
    [System.Flags]
    public enum Reserved
    {
        /// <summary>
        /// 无保留。
        /// </summary>
        None = 0,

        /// <summary>
        /// 保留助手消息（普通的）。
        /// </summary>
        Assistant = 1 << 0,

        /// <summary>
        /// 保留助手消息（带前缀的）。
        /// </summary>
        AssistantPrefix = 1 << 1,

        /// <summary>
        /// 保留助手消息（带思考的）。
        /// </summary>
        AssistantReasoner = 1 << 2,

        /// <summary>
        /// 保留助手请求工具调用消息。
        /// </summary>
        AssistantTool = 1 << 3,

        /// <summary>
        /// 保留工具调用消息。
        /// </summary>
        Tool = 1 << 4,

        /// <summary>
        /// 保留系统消息。
        /// </summary>
        System = 1 << 5,

        /// <summary>
        /// 表示用户角色的消息类型，用于标识需要保留的用户相关消息。
        /// </summary>
        User = 1 << 6,

        /// <summary>
        /// 表示需要保留的聊天消息类型，包括助手推理消息、普通助手消息以及用户消息。
        /// </summary>
        Chat = AssistantReasoner | Assistant | User,

        /// <summary>
        /// 表示需要保留的简单消息类型，包括助手推理消息、普通助手消息以及用户消息。
        /// </summary>
        Simple = AssistantReasoner | Assistant | User | System,

        /// <summary>
        /// 表示需要保留的消息类型组合，包括助手、助手前缀、助手推理器、用户和系统消息。
        /// </summary>
        Message = Assistant | AssistantPrefix | AssistantReasoner | User | System,

        /// <summary>
        /// 表示保留所有消息类型。
        /// </summary>
        All = Assistant | AssistantPrefix | AssistantReasoner | AssistantTool | Message | System | Tool | User
    }
}