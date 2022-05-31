namespace SwitchingPubSub;

public class AsyncResult : IAsyncResult, IDisposable
{
    public object? AsyncState { get; internal set; }
    public bool CompletedSynchronously { get; internal set; }
    public bool IsCompleted { get; internal set; }
    public bool IsFaulted { get; internal set; }
    public Exception? Fault { get; internal set; }
    public WaitHandle AsyncWaitHandle => Semaphore;
    public Semaphore Semaphore { get; } = new(0, 1);

    public void Dispose()
    {
        Semaphore.Dispose();
    }

    public override string ToString()
    {
        return $"{{ IsCompleted: {IsCompleted}, AsynceState: {AsyncState?.ToString() ?? "<<null>>"}, CompletedSynchronously: {CompletedSynchronously}, IsFaulted: {IsFaulted}, Fault: {Fault?.ToString() ?? "<<null>>"} }}";
    }
}
