public interface IInvocationResult
{
    string Name { get; }

    IDictionary<string, object>? ReturnValues { get; }

    void SetResult(object result);
}
