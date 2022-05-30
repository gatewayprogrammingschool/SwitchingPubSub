namespace SwitchingPubSub.Tests;

[SuppressMessage("Usage", "CA2254:Template should be a static expression", Justification = "Ignore")]
public abstract class UnitTestBase
{
    private ILogger<UnitTestBase>? _logger;
    private static IHost? _host = null;

    protected static IHost? TestHost => _host ??= Initialize();

    protected ILogger? Logger
        => _logger ??= TestHost?.Services.GetService<ILogger<UnitTestBase>>();

    protected static IHost Initialize()
    {
        var hostBuilder = Host.CreateDefaultBuilder();

        hostBuilder.ConfigureLogging(ConfigureLogging);
        hostBuilder.ConfigureServices(ConfigureServices);

        return hostBuilder.Build();
    }

    protected UnitTestBase(ITestOutputHelper outputHelper)
    {
        UnitTestBase.OutputHelper = outputHelper;

        UnitTestBase.Initialize();

        string message = $"Created {GetType().FullName}";
        Logger!.LogDebug(message);
    }

    protected delegate void ConfigureAdditionalServicesHandler(HostBuilderContext context, IServiceCollection collection);
    protected delegate void ConfigureAdditionalLoggingHandler(HostBuilderContext context, ILoggingBuilder loggingBuilder);

    protected static event ConfigureAdditionalServicesHandler? ConfigureAdditionalServicesEvent;
    protected static event ConfigureAdditionalLoggingHandler? ConfigureAdditionalLoggingEvent;

    private static void ConfigureLogging(HostBuilderContext context, ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.AddProvider(new XunitLoggingProvider(OutputHelper));

        ConfigureAdditionalLoggingEvent?.Invoke(context, loggingBuilder);
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection collection)
    {
        ConfigureAdditionalServicesEvent?.Invoke(context, collection);
    }

    protected static ITestOutputHelper? OutputHelper { get; private set; }
}