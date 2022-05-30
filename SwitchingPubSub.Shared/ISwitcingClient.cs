public interface ISwitcingClient<out TTarget> : IPubTarget<TTarget>
    where TTarget : IMessageReceiver
{
    IPubTarget<TTarget> PubTarget { get; }
}
