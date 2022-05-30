namespace SwitchingPubSub.Tests;

internal class XunitLoggingProvider : ILoggerProvider
{
    public XunitLoggingProvider(ITestOutputHelper? outputHelper)
    {
        OutputHelper = outputHelper;
    }

    public ITestOutputHelper? OutputHelper { get; protected set; }

    public ILogger CreateLogger(string categoryName)
        => new XunitLogger<string>(OutputHelper, categoryName);

    public void Dispose()
    {
        OutputHelper = default;
    }
}
