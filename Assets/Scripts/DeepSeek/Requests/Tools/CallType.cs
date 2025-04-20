namespace Xiyu.DeepSeek.Requests.Tools
{
    public enum CallType
    {
        /// <summary>
        /// 意味着模型可以选择生成一条消息或调用一个或多个 tool。
        /// </summary>
        Auto,

        /// <summary>
        /// 意味着模型不会调用任何 tool，而是生成一条消息。
        /// </summary>
        None,

        /// <summary>
        /// 意味着模型必须调用一个或多个 tool。
        /// </summary>
        Required
    }
}