using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Xiyu.DeepSeek
{
    public class AsyncFunc<T>
    {
        private readonly Dictionary<int, Func<T, UniTask>> _asyncEventsMap = new();
        private readonly Dictionary<int, Func<T, UniTask>> _appendBuffer = new();
        private readonly HashSet<int> _removeBuffer = new();
        private readonly SemaphoreSlim _invokeLock = new(1, 1);

        public bool IsRunning { get; private set; }

        public void AppendAsyncEvent(int key, Func<T, UniTask> func)
        {
            if (IsRunning)
                _appendBuffer[key] = func;
            else
                _asyncEventsMap[key] = func;
        }

        public void RemoveAsyncEvent(int key)
        {
            if (IsRunning)
                _removeBuffer.Add(key);
            else
                _asyncEventsMap.Remove(key);
        }

        public async UniTask InvokeAsync(T arg, bool concurrently = false)
        {
            await _invokeLock.WaitAsync().AsUniTask();
            try
            {
                IsRunning = true;

                if (concurrently)
                {
                    await UniTask.WhenAll(_asyncEventsMap.Select(kv => kv.Value(arg)));
                }
                else
                {
                    foreach (var kv in _asyncEventsMap.Where(kv => !_removeBuffer.Contains(kv.Key)))
                        await kv.Value(arg);
                }

                // Process appends after execution
                foreach (var kv in _appendBuffer.Where(kv =>
                             !_asyncEventsMap.ContainsKey(kv.Key) && !_removeBuffer.Contains(kv.Key)))
                {
                    _asyncEventsMap[kv.Key] = kv.Value;
                    if (!concurrently)
                        await kv.Value(arg);
                }

                // Apply removes
                foreach (var key in _removeBuffer)
                    _asyncEventsMap.Remove(key);

                _appendBuffer.Clear();
                _removeBuffer.Clear();
            }
            finally
            {
                IsRunning = false;
                _invokeLock.Release();
            }
        }

        public bool HasAnyEvent() => _asyncEventsMap.Count > 0;

        public async UniTask ClearAsyncEvents()
        {
            try
            {
                await _invokeLock.WaitAsync().AsUniTask();
            }
            finally
            {
                _asyncEventsMap.Clear();
                _appendBuffer.Clear();
                _removeBuffer.Clear();
                _invokeLock.Release();
            }
        }
    }
}