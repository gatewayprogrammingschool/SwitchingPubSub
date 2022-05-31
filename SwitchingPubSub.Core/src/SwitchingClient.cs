namespace SwitchingPubSub;

public class SwitchingClient<TTarget> : ISwitcingClient<TTarget>
    where TTarget : IMessageReceiver
{
#if !NET6_0_OR_GREATER
    public Type TargetType
        => typeof(TTarget);
#endif

    public IPubTarget<TTarget> PubTarget {get;}
    public IServiceProvider Services { get; }

    public SwitchingClient(IServiceProvider services)
    {
        Services = services;
        PubTarget = services.GetRequiredService<PubTarget<TTarget>>();
    }

    public Task InvokeAsync<TRequest>(TRequest parameters)
        where TRequest : IInvocationParameters
    {
        return PubTarget.InvokeAsync<TRequest>(parameters);
    }

    public Task<TResult> InvokeAsync<TRequest, TResult>(TRequest parameters)
        where TRequest : IInvocationParameters
        where TResult : IInvocationResult
    {
        return PubTarget.InvokeAsync<TRequest, TResult>(parameters);
    }

    public AsyncResult BeginInvoke<TRequest>(TRequest parameters)
        where TRequest : IInvocationParameters
    {
        return PubTarget.BeginInvoke<TRequest>(parameters);
    }

    public void EndInvoke(AsyncResult result)
    {
        PubTarget.EndInvoke(result);
    }

    public AsyncResult BeginInvoke<TRequest, TResult>(TRequest parameters)
        where TRequest : IInvocationParameters
        where TResult : IInvocationResult
    {
        return PubTarget.BeginInvoke<TRequest, TResult>(parameters);
    }

    public TResult? EndInvoke<TResult>(AsyncResult result) where TResult : IInvocationResult
    {
        return PubTarget.EndInvoke<TResult>(result);
    }

    public void Invoke<TRequest>(TRequest parameters)
        where TRequest : IInvocationParameters
    {
        PubTarget.Invoke<TRequest>(parameters);
    }

    public TResult? Invoke<TRequest, TResult>(TRequest parameters)
        where TRequest : IInvocationParameters
        where TResult : IInvocationResult
    {
        return PubTarget.Invoke<TRequest, TResult>(parameters);
    }
}
