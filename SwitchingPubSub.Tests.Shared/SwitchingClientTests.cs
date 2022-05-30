namespace SwitchingPubSub.Tests;

public class SwitchingClientTests : UnitTestBase
{
    static SwitchingClientTests()
    {
        UnitTestBase.ConfigureAdditionalLoggingEvent += ConfigureAdditionalLogging;
        UnitTestBase.ConfigureAdditionalServicesEvent += ConfigureAdditionalServices;
    }

    public SwitchingClientTests(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
    }

    private static ISwitcingClient<TestMessageReceiver>? Client => TestHost?
            .Services?
            .GetRequiredService<ISwitcingClient<TestMessageReceiver>>();

    private static ISwitcingClient<AlternateMessageReceiver>? AltClient => TestHost?
            .Services?
            .GetRequiredService<ISwitcingClient<AlternateMessageReceiver>>();

    private ISwitcingClient<IMessageReceiver>? GetClient((ISwitcingClient<IMessageReceiver> client, TestParameters parameters) p)
        => p.client;


    [Fact]
    public void SwitchingClientConstructors()
    {
        var client = TestHost?
            .Services
            .GetService<ISwitcingClient<TestMessageReceiver>>();

        client.Should().NotBeNull();
        client.Should().BeAssignableTo<ISwitcingClient<TestMessageReceiver>>();

        Logger!.LogInformation("Default Dependency Injection By Interface Passed.");

        client = TestHost?
            .Services
            .GetService<SwitchingClient<TestMessageReceiver>>();

        client.Should().NotBeNull();
        client.Should().BeAssignableTo<SwitchingClient<TestMessageReceiver>>();

        Logger!.LogInformation("Default Dependency Injection By Class Passed.");
    }

    public class TestParameters : IInvocationParameters
    {
        public string Name => GetType().Name;

        public IDictionary<string, object> Parameters { get; } =
            new ConcurrentDictionary<string, object>();

        public TestParameters(IDictionary<string, object> parameters)
        {
            parameters.ToList().ForEach(p => Parameters.Add(p.Key, p.Value));
        }

        public TestParameters() { }
    }

    public static object[] GetEmpty()
    {
        return new object[] {
            new ValueTuple<ISwitcingClient<IMessageReceiver>, TestParameters>((ISwitcingClient<IMessageReceiver>)Client, new TestParameters()),
            new ValueTuple<ISwitcingClient<IMessageReceiver>, TestParameters>((ISwitcingClient<IMessageReceiver>)AltClient, new TestParameters()),
        };
    }

    public static object[] GetOne()
    {
        Dictionary<string, object> dict = new() { { "p1", 0 } };
        return new object[] {
            new ValueTuple<ISwitcingClient<IMessageReceiver>, TestParameters>((ISwitcingClient<IMessageReceiver>)Client, new TestParameters(dict)),
            new ValueTuple<ISwitcingClient<IMessageReceiver>, TestParameters>((ISwitcingClient<IMessageReceiver>)AltClient, new TestParameters(dict)),
        };
    }

    public static IEnumerable<object[]> GetAllParameters()
    {
        foreach (var item in GetEmpty())
        {
            yield return new object[] { item };
        }

        foreach (var item in GetOne())
        {
            yield return new object[] { item };
        }
    }

    [Theory]
    [MemberData(nameof(GetAllParameters))]
    public async Task InvokeAsyncTest((ISwitcingClient<IMessageReceiver> client, TestParameters parameters) p)
    {
        ISwitcingClient<IMessageReceiver>? client = GetClient(p);

        await client!.InvokeAsync(p.parameters);

        Logger!.LogInformation($"InvokeAsync({p.parameters.GetType().Name}) completed successfully.");
    }

    [Theory]
    [MemberData(nameof(GetAllParameters))]
    public async Task InvokeAsync_WithResult_Test((ISwitcingClient<IMessageReceiver> client, TestParameters parameters) p)
    {
        ISwitcingClient<IMessageReceiver>? client = GetClient(p);

        var result = await client!.InvokeAsync<TestParameters, TestResultMessage>(p.parameters);

        result.Should().NotBeNull();
        result.ReturnValues.Should().BeEquivalentTo(p.parameters.Parameters);

        Logger!.LogInformation($"InvokeAsync<TestParameters, TestResultMessage>({p.parameters.GetType().Name}) completed successfully.");
    }

    [Theory]
    [MemberData(nameof(GetAllParameters))]
    public void BeginInvokeTest((ISwitcingClient<IMessageReceiver> client, TestParameters parameters) p)
    {
        ISwitcingClient<IMessageReceiver>? client = GetClient(p);

        AsyncResult asyncResult = client!.BeginInvoke(p.parameters);

        asyncResult.Should().NotBeNull();

        if(asyncResult.AsyncWaitHandle.WaitOne(1000))
        {
            Logger!.LogInformation($"BeginInvoke({p.parameters.GetType().Name}) completed successfully.");
        }
        else
        {
            false.Should().BeTrue("Timeout waiting for result.");
        }
    }

    [Theory]
    [MemberData(nameof(GetAllParameters))]
    public void EndInvokeTest((ISwitcingClient<IMessageReceiver> client, TestParameters parameters) p)
    {
        ISwitcingClient<IMessageReceiver>? client = GetClient(p);

        AsyncResult asyncResult = client!.BeginInvoke(p.parameters);

        asyncResult.Should().NotBeNull();

        if (asyncResult.AsyncWaitHandle.WaitOne(1000))
        {
            client!.EndInvoke(asyncResult);

            asyncResult.IsFaulted.Should().BeFalse(asyncResult.Fault?.ToString() ?? "Fault is null");
            asyncResult.IsCompleted.Should().BeTrue("Handle released but operation is not complete.");

            Logger!.LogInformation($"EndInvoke({asyncResult}) completed successfully.");
        }
        else
        {
            false.Should().BeTrue("Timeout waiting for result.");
        }
    }

    [Theory]
    [MemberData(nameof(GetAllParameters))]
    public void BeginInvoke_WithResult_Test((ISwitcingClient<IMessageReceiver> client, TestParameters parameters) p)
    {
        ISwitcingClient<IMessageReceiver>? client = GetClient(p);

        AsyncResult asyncResult = client!.BeginInvoke<TestParameters, TestResultMessage>(p.parameters);

        asyncResult.Should().NotBeNull();

        if (asyncResult.AsyncWaitHandle.WaitOne(1000))
        {
            Logger!.LogInformation($"BeginInvoke<TestParameters, TestResultMessage>({p.parameters.GetType().Name}) completed successfully.");
        }
        else
        {
            false.Should().BeTrue("Timeout waiting for result.");
        }
    }

    [Theory]
    [MemberData(nameof(GetAllParameters))]
    public void EndInvoke_WithResult_Test((ISwitcingClient<IMessageReceiver> client, TestParameters parameters) p)
    {
        ISwitcingClient<IMessageReceiver>? client = GetClient(p);

        AsyncResult asyncResult = client!.BeginInvoke<TestParameters, TestResultMessage>(p.parameters);

        asyncResult.Should().NotBeNull();

        if (asyncResult.AsyncWaitHandle.WaitOne(1000))
        {
            var result = client!.EndInvoke<TestResultMessage>(asyncResult);

            asyncResult.IsFaulted.Should().BeFalse(asyncResult.Fault?.ToString() ?? "Fault is null");
            asyncResult.IsCompleted.Should().BeTrue("Handle released but operation is not complete.");

            result.Should().NotBeNull();
            result!.ReturnValues.Should().BeEquivalentTo(p.parameters.Parameters);

            Logger!.LogInformation($"EndInvoke<TestParameters, TestResultMessage>({asyncResult}) completed successfully.");
        }
        else
        {
            false.Should().BeTrue("Timeout waiting for result.");
        }
    }

    [Theory]
    [MemberData(nameof(GetAllParameters))]
    public void InvokeTest((ISwitcingClient<IMessageReceiver> client, TestParameters parameters) p)
    {
        ISwitcingClient<IMessageReceiver>? client = GetClient(p);

        client!.Invoke(p.parameters);

        Logger!.LogInformation($"Invoke({p.parameters.GetType().Name}) completed successfully.");
    }

    [Theory]
    [MemberData(nameof(GetAllParameters))]
    public void Invoke_WithResult_Test((ISwitcingClient<IMessageReceiver> client, TestParameters parameters) p)
    {
        ISwitcingClient<IMessageReceiver>? client = GetClient(p);

        var result = client!.Invoke<TestParameters, TestResultMessage>(p.parameters);

        result.Should().NotBeNull();
        result!.ReturnValues.Should().BeEquivalentTo(p.parameters.Parameters);

        Logger!.LogInformation($"Invoke<TestParameters, TestResultMessage>({p.parameters.GetType().Name}) completed successfully.");
    }


    protected static void ConfigureAdditionalLogging(HostBuilderContext context, ILoggingBuilder loggingBuilder)
    {
        // Ignore
    }

    protected static void ConfigureAdditionalServices(HostBuilderContext context, IServiceCollection collection)
    {
        collection.AddSingleton<TestMessageReceiver>();
        collection.AddSingleton<AlternateMessageReceiver>();
        collection.AddSingleton<IMessageReceiver, TestMessageReceiver>();
        collection.AddTransient<PubTarget<TestMessageReceiver>>();
        collection.AddTransient<IPubTarget<TestMessageReceiver>, PubTarget<TestMessageReceiver>>();
        collection.AddTransient<PubTarget<AlternateMessageReceiver>>();
        collection.AddTransient<IPubTarget<AlternateMessageReceiver>, PubTarget<AlternateMessageReceiver>>();
        collection.AddTransient<SwitchingClient<TestMessageReceiver>>();
        collection.AddTransient<SwitchingClient<AlternateMessageReceiver>>();
        collection.AddTransient<ISwitcingClient<TestMessageReceiver>, SwitchingClient<TestMessageReceiver>>();
        collection.AddTransient<ISwitcingClient<AlternateMessageReceiver>, SwitchingClient<AlternateMessageReceiver>>();
        collection.AddTransient(_ => new TestResultMessage(new Dictionary<string, object>()));
        collection.AddTransient<IInvocationResult, TestResultMessage>();
    }
}
