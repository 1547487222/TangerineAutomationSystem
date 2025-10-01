using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{
    [AttributeUsage(AttributeTargets.Method)]
    public class LockAttribute : Attribute { }

    public class AsyncLockInterceptor : IAsyncInterceptor
    {
        private readonly object _syncLock = new();
        private readonly SemaphoreSlim _asyncLock = new(1, 1);
        public void InterceptSynchronous(IInvocation invocation)
        {
            if (invocation.Method.IsDefined(typeof(LockAttribute), false))
            {
                lock (_syncLock)
                {
                    invocation.Proceed();
                }
            }
            else
            {
                invocation.Proceed();
            }
        }

        public void InterceptAsynchronous(IInvocation invocation)
        {
            invocation.ReturnValue = InternalInterceptAsync(invocation);
        }

        public void InterceptAsynchronous<TResult>(IInvocation invocation)
        {
            invocation.ReturnValue = InternalInterceptAsync<TResult>(invocation);
        }

        private async Task InternalInterceptAsync(IInvocation invocation)
        {
            if (invocation.Method.IsDefined(typeof(LockAttribute), false))
            {
                await _asyncLock.WaitAsync();
                try
                {
                    invocation.Proceed();
                    if (invocation.ReturnValue is Task task)
                    {
                        await task;
                    }
                }
                finally
                {
                    _asyncLock.Release();
                }
            }
            else
            {
                invocation.Proceed();
                if (invocation.ReturnValue is Task task)
                {
                    await task;
                }
            }
        }

        private async Task<TResult> InternalInterceptAsync<TResult>(IInvocation invocation)
        {
            if (invocation.Method.IsDefined(typeof(LockAttribute), false))
            {
                await _asyncLock.WaitAsync();
                try
                {
                    invocation.Proceed();
                    if (invocation.ReturnValue is Task<TResult> task)
                    {
                        return await task;
                    }
                    return default;
                }
                finally
                {
                    _asyncLock.Release();
                }
            }
            else
            {
                invocation.Proceed();
                if (invocation.ReturnValue is Task<TResult> task)
                {
                    return await task;
                }
                return default;
            }
        }
    }
}
