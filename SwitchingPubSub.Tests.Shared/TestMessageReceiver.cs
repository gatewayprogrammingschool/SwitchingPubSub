namespace SwitchingPubSub.Tests;

internal class TestMessageReceiver : IMessageReceiver
{
    public TestMessageReceiver(IServiceProvider provider, ILogger<TestMessageReceiver> logger)
    {
        Provider = provider;
        Logger = logger;
    }

    public IServiceProvider Provider { get; }
    public ILogger<TestMessageReceiver> Logger { get; }

    public Task ReceiveMessageAsync(IDictionary<string, object> parameters)
    {
        var result = new TestResultMessage(parameters);
        Logger.LogInformation(result.Value);

        return Task<TestResultMessage>.FromResult(result);
    }

    public Task<TResult?> ReceiveMessageAsync<TResult>(IDictionary<string, object> parameters)
        where TResult : IInvocationResult
    {
        Logger.LogInformation(string.Join("\n", parameters.Select(p => p.ToString())));

        TResult? result = Provider.GetRequiredService<TResult>();
        result.SetResult(parameters);

        return Task<TResult?>.FromResult(result);
    }
}
