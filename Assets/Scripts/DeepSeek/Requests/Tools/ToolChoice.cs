using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Xiyu.DeepSeek.Requests.Tools
{
    [Serializable]
    public class ToolChoice
    {
        public ToolChoice(CallType callType, string functionName = null)
        {
            this.callType = callType;
            this.functionName = functionName;
        }

        [SerializeField] private CallType callType;
        [SerializeField] private string functionName;

        /// <summary>
        /// 当没有 tool 时，默认值为 none。如果有 tool 存在，默认值为 auto。
        /// </summary>
        public CallType CallType
        {
            get => callType;
            set => callType = value;
        }


        public string FunctionName
        {
            get => functionName;
            set => functionName = value;
        }
    }
}