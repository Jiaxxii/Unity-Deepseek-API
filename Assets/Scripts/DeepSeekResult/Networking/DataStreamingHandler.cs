#if DEEPSEEK_PAST_CODE
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Xiyu.DeepSeekResult.Networking
{
    public class DataStreamingHandler : UnityEngine.Networking.DownloadHandlerScript
    {
        private event Action<byte[]> OnReceiveData;
        private readonly ChannelWriter<byte[]> _writer;

        private int _lastLength;

        public DataStreamingHandler(Action<byte[]> onReceiveData, ChannelWriter<byte[]> writer)
        {
            OnReceiveData = onReceiveData;
            _writer = writer;
        }

        public DataStreamingHandler(Action<byte[]> onReceiveData)
        {
            OnReceiveData = onReceiveData;
            _writer = null;
        }

        public DataStreamingHandler(ChannelWriter<byte[]> writer)
        {
            OnReceiveData = null;
            _writer = writer;
        }


        protected override byte[] GetData()
        {
            return null; // 不需要完整数据
        }

        protected override void CompleteContent()
        {
            base.CompleteContent();
            _writer?.Complete();
        }

        protected override bool ReceiveData(byte[] receiveData, int receiveLength)
        {
            // 1. 检查数据有效性（防御性编程）
            if (receiveData == null || receiveLength <= 0)
            {
                Debug.LogWarning("Received invalid data");
                return false;
            }

            OnReceiveData?.Invoke(receiveData); // 传递当前数据块

            _writer?.TryWrite(receiveData);

            // 5. 返回成功状态
            return true;
        }
    }
}
#endif