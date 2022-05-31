namespace SwitchingPubSub;

public interface IInvocationParameters //<TType>
    // where TType : Type
{
    string Name
#if NET6_0_OR_GREATER
        => typeof(IInvocationParameters/* <TType> */).FullName
        ?? typeof(IInvocationParameters/* <TType> */).Name;
#else
        { get; }
#endif

    IDictionary<string, object> Parameters { get; }
}
