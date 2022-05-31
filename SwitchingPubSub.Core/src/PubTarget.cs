namespace SwitchingPubSub;

public class PubTarget<TTarget> : IPubTarget<TTarget>
    where TTarget : IMessageReceiver
{
#if !NET6_0_OR_GREATER
    public Type TargetType
        => typeof(TTarget);
#endif
    private static ConcurrentDictionary<AsyncResult, object> _asyncResults = new();

    public TTarget? Target { get; protected set; }
    public IServiceProvider ServiceProvider { get; }

    public PubTarget(IServiceProvider serviceProvider)
    {
        Target = serviceProvider!.GetService<TTarget>();
        ServiceProvider = serviceProvider;
    }

    private void EnsureTarget()
    {
        if (Target is null)
        {
            Target = ServiceProvider.GetRequiredService<TTarget>();
        }
    }

    public Task InvokeAsync<TRequest>(TRequest parameters)
        where TRequest : IInvocationParameters
    {
        EnsureTarget();

        return Target!.ReceiveMessageAsync(parameters.Parameters);
    }

    public Task<TResult> InvokeAsync<TRequest, TResult>(TRequest parameters)
        where TRequest : IInvocationParameters
        where TResult : IInvocationResult
    {
        EnsureTarget();

        return Target!.ReceiveMessageAsync<TResult>(parameters.Parameters);
    }

    public AsyncResult BeginInvoke<TRequest>(TRequest parameters)
        where TRequest : IInvocationParameters
    {
        var result = new AsyncResult()
        {
            AsyncState = parameters
        };

        Task.Run(async () =>
        {
            try
            {
                await InvokeAsync<TRequest>(parameters);

                result.IsCompleted = true;
            }
            catch (Exception ex)
            {
                result.IsFaulted = true;
                result.Fault = ex;
            }

            result.Semaphore.Release();
        });

        return result;
    }

    public void EndInvoke(AsyncResult result)
    {
        if(result.IsFaulted)
        {
            if (result.Fault is not null)
            {
                throw new AggregateException(result.Fault);
            }

            throw new ApplicationException("AsyncResult is faulted but no Fault was returned.");
        }

        if (!result.IsCompleted)
        {
            result.AsyncWaitHandle.WaitOne();
            //return _asyncResults.TryGetValue(result, out object? returnValue)
            //    ? returnValue
            //    : null;
        }
    }

    public AsyncResult BeginInvoke<TRequest, TResult>(TRequest parameters)
        where TRequest : IInvocationParameters
        where TResult : IInvocationResult
    {
        var result = new AsyncResult()
        {
            AsyncState = parameters
        };

        Task.Run(async () =>
        {
            try
            {
                TResult returned = await InvokeAsync<TRequest, TResult>(parameters);

                result.IsFaulted = !_asyncResults.TryAdd(result, returned);

                if (result.IsFaulted)
                {
                    result.Fault = new ApplicationException("Could not add results to dictionary.");
                }

                result.IsCompleted = true;
            }
            catch (Exception ex)
            {
                result.IsFaulted = true;
                result.Fault = ex;
            }

            result.Semaphore.Release();
        });

        return result;
    }

    public TResult? EndInvoke<TResult>(AsyncResult result) where TResult : IInvocationResult
    {
        if (result.IsFaulted)
        {
            if (result.Fault is not null)
            {
                throw new AggregateException(result.Fault);
            }

            throw new ApplicationException("AsyncResult is faulted but no Fault was returned.");
        }

        if (!result.IsCompleted)
        {
            result.AsyncWaitHandle.WaitOne();
        }

        if (_asyncResults.TryRemove(result, out object? returnValue))
        {
            return (TResult)returnValue;
        }

        return default(TResult);
    }

    public void Invoke<TRequest>(TRequest parameters)
        where TRequest : IInvocationParameters
    {
        ManualResetEventSlim mre = new (false);

        Task.Run(async () =>
        {
            await InvokeAsync<TRequest>(parameters);

            mre.Set();
        });

        mre.Wait();
    }

    public TResult? Invoke<TRequest, TResult>(TRequest parameters)
        where TRequest : IInvocationParameters
        where TResult : IInvocationResult
    {
        TResult? result = default;
        ManualResetEventSlim mre = new(false);

        Task.Run(async () =>
        {
            result = await InvokeAsync<TRequest, TResult>(parameters);

            mre.Set();
        });

        mre.Wait();

        return result;
    }
}

public interface IMessageReceiver
{
    Task ReceiveMessageAsync(IDictionary<string, object> parameters);
    Task<TResult?> ReceiveMessageAsync<TResult>(IDictionary<string, object> parameters)
                where TResult : IInvocationResult;
}