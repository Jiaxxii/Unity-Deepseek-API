#if DEEPSEEK_PAST_CODE
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Xiyu.Old.DeepSeekApi.Request
{
    /// <summary>
    /// 最多包含 16 个 string 的 list，在遇到这些词时，API 将停止生成更多的 token。
    /// <para>我目前看到的应用场合是输出为 JSON 模式时，加上Stop["'''"] 来防止AI输出多于内容</para>
    /// </summary>
    [PublicAPI]
    public class StopOptions : IEnumerable<string>
    {
        /// <summary>
        /// 最多允许 16 个词，超过会被忽略
        /// </summary>
        /// <param name="stop"></param>
        public StopOptions(params string[] stop) => _stopValues = new HashSet<string>(stop.Take(16));

        private readonly HashSet<string> _stopValues;

        /// <summary>
        /// 当前停止词的数量
        /// </summary>
        public int Count => _stopValues.Count;

        /// <summary>
        /// 尝试添加一个停止词，如果已经有 16 个停止词或存在，则返回 false
        /// </summary>
        /// <param name="value">停止词</param>
        /// <returns></returns>
        public bool TryAdd(string value) => _stopValues.Count <= 16 && _stopValues.Add(value);

        /// <summary>
        /// 将一个停止词从列表中移除然后加入新的停止词，如果原停止词不存在则返回 false
        /// </summary>
        /// <param name="rawValue">要替换的词</param>
        /// <param name="newValue">新词</param>
        /// <returns></returns>
        public bool Replace(string rawValue, string newValue) => _stopValues.Remove(rawValue) && _stopValues.Add(newValue);


        public IEnumerator<string> GetEnumerator() => _stopValues.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
#endif