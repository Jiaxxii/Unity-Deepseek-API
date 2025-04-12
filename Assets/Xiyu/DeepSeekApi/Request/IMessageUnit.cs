using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Xiyu.DeepSeekApi.Request
{
    /// <summary>
    /// 消息单元接口
    /// </summary>
    public interface IMessageUnit
    {
        /// <summary>
        /// 喵~这是一条消息
        /// </summary>
        [JsonProperty(PropertyName = "content")]
        string Content { get; }

        [JsonProperty(PropertyName = "role")] RoleType Role { get; }

        /// <summary>
        /// 可以选填的参与者的名称，为模型提供信息以区分相同角色的参与者。
        /// </summary>
        [CanBeNull]
        [JsonProperty(PropertyName = "name")]
        string Name { get; }

        /// <summary>
        /// 将对象转换为 JSON 字符串，不要直接使用 JsonConvert.SerializeObject 方法，此方法做了一些特殊处理
        /// </summary>
        /// <param name="formatting">指定JSON格式-默认为容易阅读的格式</param>
        /// <returns></returns>
        string ToJson(Formatting formatting = Formatting.Indented);
    }
}