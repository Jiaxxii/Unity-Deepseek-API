using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Xiyu.DeepSeekApi.Request
{
    /// <summary>
    /// 请求体接口 （用于将实体类转换为 JSON 对象）
    /// </summary>
    public interface IRequestBody
    {
        /// <summary>
        /// 消息列表
        /// </summary>
        IMessageUnits Messages { get; set; }

        /// <summary>
        /// 将实体类转换为 JSON 对象
        /// </summary>
        /// <param name="instance">允许传入一个 <see cref="JObject"/> 实例</param>
        /// <param name="formatting">JSON 格式（默认为：工整的）</param>
        /// <returns></returns>
        string ToJson(JObject instance = null, Formatting formatting = Formatting.None);
    }
}