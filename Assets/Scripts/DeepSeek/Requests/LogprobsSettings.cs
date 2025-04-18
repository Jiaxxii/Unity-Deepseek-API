using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Xiyu.DeepSeek.Requests
{
    [Serializable]
    public class LogprobsSettings
    {
        /// <summary>
        /// 是否返回所输出 token 的对数概率。在 message 的 content 中返回每个输出 token 的对数概率。
        /// </summary>
        /// <param name="topLogprobs"></param>
        public LogprobsSettings(int topLogprobs)
        {
            this.topLogprobs = topLogprobs;
        }

        [SerializeField] private bool ignore;
        [SerializeField] [Range(0, 20)] private int topLogprobs;


        public bool Ignore => ignore;

        /// <summary>
        /// 一个介于 0 到 20 之间的整数 N，指定每个输出位置返回输出概率 top N 的 token，且返回这些 token 的对数概率。
        /// </summary>
        public int Logprobs
        {
            get => topLogprobs;
            set => topLogprobs = value;
        }
    }
}