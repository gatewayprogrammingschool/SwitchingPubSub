namespace SwitchingPubSub.Tests;

internal class TestResultMessage : IInvocationResult
{
    public TestResultMessage(IDictionary<string, object> parameters)
    {
        ReturnValues = parameters;
        Value = string.Join(", ", parameters.Select(p => p.ToString()));
    }

    public string Value { get; }
    public string Name => GetType().Name;
    public IDictionary<string, object>? ReturnValues { get; private set; }

    public void SetResult(object result)
        => ReturnValues = result as IDictionary<string, object>;
}
