using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Xiyu.DeepSeek.Requests.Tools
{
    // [Serializable]
    // public class TargetFunction
    // {
    //     public TargetFunction(string functionName)
    //     {
    //         this.functionName = functionName;
    //     }
    //
    //     [SerializeField] private ToolType toolType = ToolType.Function;
    //
    //     [JsonProperty(PropertyName = "name")] [SerializeField]
    //     private string functionName;
    //
    //     public ToolType ToolType
    //     {
    //         get => toolType;
    //         set => toolType = value;
    //     }
    //
    //     public string FunctionName
    //     {
    //         get => functionName;
    //         set => functionName = value;
    //     }
    //
    //     public static TargetFunction Create(string functionName) => new(functionName);
    // }
}