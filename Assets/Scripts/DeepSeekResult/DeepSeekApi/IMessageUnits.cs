#if DEEPSEEK_PAST_CODE
using System.Collections.Generic;
using Xiyu.DeepSeekResult.DeepSeekApi.Request;

namespace Xiyu.DeepSeekResult.DeepSeekApi
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


        public void AddMessage(IMessageUnit messageUnit);

        public void AddMessageRange(params IMessageUnit[] messageUnits);


        public bool Check();

        public void CheckAndThrow();
    }
}
#endif