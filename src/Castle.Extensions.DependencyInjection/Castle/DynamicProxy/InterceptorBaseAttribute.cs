using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Castle.DynamicProxy
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class InterceptorBaseAttribute : Attribute, IInterceptor
    {
        void IInterceptor.Intercept(IInvocation invocation)
        {
            var returnType = invocation.Method.ReturnType;
            var builder = AsyncMethodBuilder.TryCreate(returnType);
            if (builder != null)
            {
                var asyncInvocation = new AsyncInvocation(invocation);
                var stateMachine = new AsyncStateMachine(asyncInvocation, builder, task: InterceptAsync(asyncInvocation));
                builder.Start(stateMachine);
                invocation.ReturnValue = builder.Task();
            }
            else
            {
                Intercept(invocation);
            }
        }

        protected virtual void Intercept(IInvocation invocation) { }

        protected abstract ValueTask InterceptAsync(IAsyncInvocation invocation);
    }


    #region internal.

    internal static class AsyncMethodBuilder
    {
        public static object TryCreate(Type returnType)
        {
            var builderType = GetAsyncMethodBuilderType(returnType);
            if (builderType != null)
            {
                var createMethod = builderType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
                var builder = createMethod.Invoke(null, null);
                return builder;
            }
            else
            {
                return null;
            }
        }

        private static Type GetAsyncMethodBuilderType(Type returnType)
        {
            var asyncMethodBuilderAttribute = (AsyncMethodBuilderAttribute)Attribute.GetCustomAttribute(returnType, typeof(AsyncMethodBuilderAttribute), inherit: false);
            if (asyncMethodBuilderAttribute != null)
            {
                var builderType = asyncMethodBuilderAttribute.BuilderType;
                if (builderType.IsGenericTypeDefinition)
                {
                    Debug.Assert(returnType.IsConstructedGenericType);
                    return builderType.MakeGenericType(returnType.GetGenericArguments());
                }
                else
                {
                    return builderType;
                }
            }
            else if (returnType == typeof(ValueTask))
            {
                return typeof(AsyncValueTaskMethodBuilder);
            }
            else if (returnType == typeof(Task))
            {
                return typeof(AsyncTaskMethodBuilder);
            }
            else if (returnType.IsGenericType)
            {
                var returnTypeDefinition = returnType.GetGenericTypeDefinition();
                if (returnTypeDefinition == typeof(ValueTask<>))
                {
                    return typeof(AsyncValueTaskMethodBuilder<>).MakeGenericType(returnType.GetGenericArguments()[0]);
                }
                else if (returnTypeDefinition == typeof(Task<>))
                {
                    return typeof(AsyncTaskMethodBuilder<>).MakeGenericType(returnType.GetGenericArguments()[0]);
                }
            }
            // NOTE: `AsyncVoidMethodBuilder` is intentionally excluded here because we want to end up in a synchronous
            // `Intercept` callback for non-awaitable methods.
            return null;
        }

        public static void AwaitOnCompleted(this object builder, object awaiter, object stateMachine)
        {
            var awaitOnCompletedMethod = builder.GetType().GetMethod("AwaitOnCompleted", BindingFlags.Public | BindingFlags.Instance).MakeGenericMethod(awaiter.GetType(), stateMachine.GetType());
            awaitOnCompletedMethod.Invoke(builder, new object[] { awaiter, stateMachine });
        }

        public static void SetException(this object builder, Exception exception)
        {
            var setExceptionMethod = builder.GetType().GetMethod("SetException", BindingFlags.Public | BindingFlags.Instance);
            setExceptionMethod.Invoke(builder, new object[] { exception });
        }

        public static void SetResult(this object builder, object result)
        {
            var setResultMethod = builder.GetType().GetMethod("SetResult", BindingFlags.Public | BindingFlags.Instance);
            if (setResultMethod.GetParameters().Length == 0)
            {
                setResultMethod.Invoke(builder, null);
            }
            else
            {
                setResultMethod.Invoke(builder, new object[] { result });
            }
        }

        public static void Start(this object builder, object stateMachine)
        {
            var startMethod = builder.GetType().GetMethod("Start", BindingFlags.Public | BindingFlags.Instance).MakeGenericMethod(stateMachine.GetType());
            startMethod.Invoke(builder, new object[] { stateMachine });
        }

        public static object Task(this object builder)
        {
            var taskProperty = builder.GetType().GetProperty("Task", BindingFlags.Public | BindingFlags.Instance);
            return taskProperty.GetValue(builder);
        }
    }

    public interface IAsyncInvocation
    {
        IReadOnlyList<object> Arguments { get; }
        MethodInfo Method { get; }
        object Result { get; set; }
        ValueTask ProceedAsync();
    }

    internal sealed class AsyncInvocation : IAsyncInvocation
    {
        private readonly IInvocation _invocation;
        private readonly IInvocationProceedInfo _proceed;

        public AsyncInvocation(IInvocation invocation)
        {
            _invocation = invocation;
            _proceed = invocation.CaptureProceedInfo();
        }

        public IReadOnlyList<object> Arguments => _invocation.Arguments;

        public MethodInfo Method => _invocation.Method;

        public object Result { get; set; }

        public ValueTask ProceedAsync()
        {
            var previousReturnValue = _invocation.ReturnValue;
            try
            {
                _proceed.Invoke();
                var returnValue = _invocation.ReturnValue;
                if (returnValue != previousReturnValue)
                {
                    var awaiter = returnValue.GetAwaiter();
                    if (awaiter.IsCompleted())
                    {
                        try
                        {
                            Result = awaiter.GetResult();
                            return default;
                        }
                        catch (Exception exception)
                        {
                            return new ValueTask(Task.FromException(exception));
                        }
                    }
                    else
                    {
                        var tcs = new TaskCompletionSource<bool>();
                        awaiter.OnCompleted(() =>
                        {
                            try
                            {
                                Result = awaiter.GetResult();
                                tcs.SetResult(true);
                            }
                            catch (Exception exception)
                            {
                                tcs.SetException(exception);
                            }
                        });
                        return new ValueTask(tcs.Task);
                    }
                }
                else
                {
                    return default;
                }
            }
            finally
            {
                _invocation.ReturnValue = previousReturnValue;
            }
        }
    }

    internal sealed class AsyncStateMachine : IAsyncStateMachine
    {
        private readonly IAsyncInvocation asyncInvocation;
        private readonly object builder;
        private readonly ValueTask task;

        public AsyncStateMachine(IAsyncInvocation asyncInvocation, object builder, ValueTask task)
        {
            this.asyncInvocation = asyncInvocation;
            this.builder = builder;
            this.task = task;
        }

        public void MoveNext()
        {
            try
            {
                var awaiter = task.GetAwaiter();

                if (awaiter.IsCompleted)
                {
                    awaiter.GetResult();
                    // TODO: validate `asyncInvocation.Result` against `asyncInvocation.Method.ReturnType`!
                    builder.SetResult(asyncInvocation.Result);
                }
                else
                {
                    builder.AwaitOnCompleted(awaiter, this);
                }
            }
            catch (Exception exception)
            {
                builder.SetException(exception);
            }
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
        }
    }

    internal static class Awaiter
    {
        public static object GetAwaiter(this object awaitable)
        {
            // TODO: `.GetAwaiter()` extension methods are not yet supported!
            var getAwaiterMethod = awaitable.GetType().GetMethod("GetAwaiter", BindingFlags.Public | BindingFlags.Instance);
            return getAwaiterMethod.Invoke(awaitable, null);
        }

        public static bool IsCompleted(this object awaiter)
        {
            var isCompletedProperty = awaiter.GetType().GetProperty("IsCompleted", BindingFlags.Public | BindingFlags.Instance);
            return (bool)isCompletedProperty.GetValue(awaiter);
        }

        public static void OnCompleted(this object awaiter, Action continuation)
        {
            var onCompletedMethod = awaiter.GetType().GetMethod("OnCompleted", BindingFlags.Public | BindingFlags.Instance);
            onCompletedMethod.Invoke(awaiter, new object[] { continuation });
        }

        public static object GetResult(this object awaiter)
        {
            var getResultMethod = awaiter.GetType().GetMethod("GetResult", BindingFlags.Public | BindingFlags.Instance);
            return getResultMethod.Invoke(awaiter, null);
        }
    }
    #endregion
}
