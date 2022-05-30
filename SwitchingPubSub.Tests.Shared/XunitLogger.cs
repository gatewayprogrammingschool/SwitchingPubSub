namespace SwitchingPubSub.Tests;

internal class XunitLogger<TState> : ILogger, IDisposable
{
    private static readonly ConcurrentDictionary<object, XunitLogger<object>> _scopes = new();
    private bool disposedValue;

    public XunitLogger(ITestOutputHelper? outputHelper, TState state)
    {
        OutputHelper = outputHelper;
        State = state;
        LogLevel = LogLevel.Information;
    }

    public XunitLogger(ITestOutputHelper? outputHelper, TState state, LogLevel logLevel)
        : this(outputHelper, state)
    {
        LogLevel = logLevel;
    }

    public ITestOutputHelper? OutputHelper { get; protected set; }
    public TState State { get; } = default!;
    public LogLevel LogLevel { get; }

    public IDisposable BeginScope<TSubState>(TSubState state)
    {
        Func<object, XunitLogger<object>> value =
            _ => new XunitLogger<object>(OutputHelper, state!);

        return _scopes.GetOrAdd(state!, value);
    }

    public bool IsEnabled(LogLevel logLevel)
        => logLevel >= LogLevel;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "<Pending>")]
    public void Log<TSubState>(LogLevel logLevel,
                            EventId eventId,
                            TSubState subState,
                            Exception? exception,
                            Func<TSubState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        string message = $"[{State}:{logLevel}:{eventId}]: {formatter(subState, exception)}";

        if (!State!.Equals(subState)
            && _scopes.TryGetValue(subState!, out var logger))
        {
            logger.Log(logLevel, message: message);
        }
        else
        {
            OutputHelper?.WriteLine(message);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                OutputHelper = default;
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}