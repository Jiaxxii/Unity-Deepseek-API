#if DEEPSEEK_PAST_CODE
namespace Xiyu.DeepSeekResult.DeepSeekApi.Response
{
    /// <summary>
    /// 发送 ai 请求时发生异常
    /// </summary>
    public class ChatException : System.Exception
    {
        /// <summary>
        /// 错误码
        /// </summary>
        public int StatusCode { get; }

        public ChatException(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }

        public ChatException(string message, System.Exception innerException) : base(message, innerException)
        {
            StatusCode = 500;
        }
    }
}
#endif