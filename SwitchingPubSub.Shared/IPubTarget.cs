using AsyncResult = SwitchingPubSub.AsyncResult;

public interface IPubTarget<out TTarget>
    where TTarget : IMessageReceiver
{
    Type TargetType
#if NET6_0_OR_GREATER
        => typeof(TTarget);
#else
        {get;}
#endif

    Task InvokeAsync<TRequest>(TRequest parameters)
        where TRequest : IInvocationParameters;

    Task<TResult> InvokeAsync<TRequest, TResult>(TRequest parameters)
        where TRequest : IInvocationParameters
        where TResult : IInvocationResult;

    AsyncResult BeginInvoke<TRequest>(TRequest parameters)
        where TRequest : IInvocationParameters;

    void EndInvoke(AsyncResult result);

    AsyncResult BeginInvoke<TRequest, TResult>(TRequest parameters)
        where TRequest : IInvocationParameters
        where TResult : IInvocationResult;

    TResult? EndInvoke<TResult>(AsyncResult result)
        where TResult : IInvocationResult;

    void Invoke<TRequest>(TRequest parameters)
        where TRequest : IInvocationParameters;

    TResult? Invoke<TRequest, TResult>(TRequest parameters)
        where TRequest : IInvocationParameters
        where TResult : IInvocationResult;
}
