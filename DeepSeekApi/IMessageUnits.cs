using System.Collections.Generic;
using Xiyu.DeepSeekApi.Request;

namespace Xiyu.DeepSeekApi
{
    /// <summary>
    /// 消息列表单元接口
    /// </summary>
    public interface IMessageUnits
    {
        /// <summary>
        /// 消息列表
        /// </summary>
        public List<IMessageUnit> Messages { get; set; }
    }
}