using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

namespace Xiyu.DeepSeek.Responses
{
    public class ChatCompletionHandle
    {
        private ChatCompletionHandle(Func<Action<ChatCompletion>, IUniTaskAsyncEnumerable<StreamChatCompletion>> functionDefinition)
        {
            _function = functionDefinition;
        }

        private Func<Action<ChatCompletion>, IUniTaskAsyncEnumerable<StreamChatCompletion>> _function;
        private readonly SemaphoreSlim _invokeLock = new(1, 1);

        private ChatCompletionHandle()
        {
        }

        public Action<StreamChatCompletion> FirstCompletionCallback { get; set; }
        public AsyncFunc<StreamChatCompletion> FirstCompletionAsyncCallback { get; } = new();
        public bool IsFirstCompletion { get; private set; }

        public Action<ChatCompletion> OnCompletion { get; set; }


        private bool _concurrently;

        public UniTaskCancelableAsyncEnumerable<StreamChatCompletion> ChatCompletionStreamAsync(CancellationToken cancellationToken)
        {
            return UniTaskAsyncEnumerable.Create<StreamChatCompletion>(async (writer, token) =>
            {
                await _invokeLock.WaitAsync(token).AsUniTask();
                try
                {
                    await foreach (var streamChatCompletion in _function(OnCompletion).WithCancellation(token))
                    {
                        if (FirstCompletionCallback == null && FirstCompletionAsyncCallback.HasAnyEvent())
                        {
                            await writer.YieldAsync(streamChatCompletion);
                        }
                        else if (IsFirstCompletion)
                        {
                            await writer.YieldAsync(streamChatCompletion);
                            continue;
                        }

                        IsFirstCompletion = true;
                        FirstCompletionCallback?.Invoke(streamChatCompletion);

                        if (FirstCompletionAsyncCallback.HasAnyEvent())
                            await FirstCompletionAsyncCallback.InvokeAsync(streamChatCompletion, _concurrently);
                    }
                }
                finally
                {
                    _invokeLock.Release();
                }
            }).WithCancellation(cancellationToken);
        }

        public async UniTask SetConcurrently(bool concurrently)
        {
            await _invokeLock.WaitAsync().AsUniTask();
            _concurrently = concurrently;
            _invokeLock.Release();
        }

        public async UniTask<ChatCompletionHandle> Reset(Func<Action<ChatCompletion>, IUniTaskAsyncEnumerable<StreamChatCompletion>> functionDefinition)
        {
            await _invokeLock.WaitAsync().AsUniTask();
            _function = functionDefinition;
            IsFirstCompletion = false;
            _concurrently = false;
            _invokeLock.Release();
            return this;
        }

        internal static ChatCompletionHandle Create(Func<Action<ChatCompletion>, IUniTaskAsyncEnumerable<StreamChatCompletion>> functionDefinition)
        {
            return new ChatCompletionHandle(functionDefinition);
        }

        internal static UniTask<ChatCompletionHandle> Build(ChatCompletionHandle handle,
            Func<Action<ChatCompletion>, IUniTaskAsyncEnumerable<StreamChatCompletion>> functionDefinition)
        {
            return handle?.Reset(functionDefinition) ?? UniTask.FromResult(Create(functionDefinition));
        }

        public static ChatCompletionHandle CreateDefault() => new();
    }
}