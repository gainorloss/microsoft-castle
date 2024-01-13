namespace Castle.DynamicProxy
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]//修改：允许多个标注 galoS@2024-1-12 15:43:14
    public class InterceptorAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        public InterceptorAttribute(Type type)
        {
            InterceptorType = type;
        }

        /// <summary>
        /// 
        /// </summary>
        public Type InterceptorType { get; protected set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class InterceptorBase
        : IInterceptor
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
}
