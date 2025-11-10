using EatingPhilosophers;
using ForkLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PhilosopherLib;
using SimulationSettingsLib;
using StatisticsLib;
using StrategyContractLib;
using StrategyImplementationLib;

namespace SystemUnitTests;

public class DeadlockTests
{
    private SimulationSettings _settingsFirst;
    private IOptions<SimulationSettings> _mockOptionsFirst;
    private SimulationSettings _settingsSecond;
    private IOptions<SimulationSettings> _mockOptionsSecond;
    private SimulationSettings _settingsThird;
    private IOptions<SimulationSettings> _mockOptionsThird;
    private SimulationSettings _settingsFourth;
    private IOptions<SimulationSettings> _mockOptionsFourth;
    private SimulationSettings _settingsFivth;
    private IOptions<SimulationSettings> _mockOptionsFivth;

    private IStatistics _mockStatistics;

    [SetUp]
    public void Setup()
    {
        _settingsFirst = new SimulationSettings
        {
            DurationSec = 20,
            DisplayIntervalMs = 200,
            ThinkingTimeMinMs = 10,
            ThinkingTimeMaxMs = 50,
            EatingTimeMinMs = 40,
            EatingTimeMaxMs = 50,
            ForkAcquisitionTimeMs = 5000
        };

        _settingsSecond = new SimulationSettings
        {
            DurationSec = 20,
            DisplayIntervalMs = 200,
            ThinkingTimeMinMs = 200,
            ThinkingTimeMaxMs = 1010,
            EatingTimeMinMs = 40,
            EatingTimeMaxMs = 50,
            ForkAcquisitionTimeMs = 5000
        };

        _settingsThird = new SimulationSettings
        {
            DurationSec = 20,
            DisplayIntervalMs = 200,
            ThinkingTimeMinMs = 2000,
            ThinkingTimeMaxMs = 2010,
            EatingTimeMinMs = 40,
            EatingTimeMaxMs = 50,
            ForkAcquisitionTimeMs = 3000
        };

        _settingsFourth = new SimulationSettings
        {
            DurationSec = 20,
            DisplayIntervalMs = 200,
            ThinkingTimeMinMs = 3000,
            ThinkingTimeMaxMs = 3010,
            EatingTimeMinMs = 40,
            EatingTimeMaxMs = 50,
            ForkAcquisitionTimeMs = 3000
        };

        _settingsFivth = new SimulationSettings
        {
            DurationSec = 20,
            DisplayIntervalMs = 200,
            ThinkingTimeMinMs = 4000,
            ThinkingTimeMaxMs = 4010,
            EatingTimeMinMs = 40,
            EatingTimeMaxMs = 50,
            ForkAcquisitionTimeMs = 3000
        };

        _mockOptionsFirst = Mock.Of<IOptions<SimulationSettings>>(
            x => x.Value == _settingsFirst
        );
        _mockOptionsSecond = Mock.Of<IOptions<SimulationSettings>>(
            x => x.Value == _settingsSecond
        );
        _mockOptionsThird = Mock.Of<IOptions<SimulationSettings>>(
            x => x.Value == _settingsThird
        );
        _mockOptionsFourth = Mock.Of<IOptions<SimulationSettings>>(
            x => x.Value == _settingsFourth
        );
        _mockOptionsFivth = Mock.Of<IOptions<SimulationSettings>>(
            x => x.Value == _settingsFivth
        );

    }

    [Test]
    public async Task TestDeadlock()
    {
        bool isDeadlock = false;


        var philosopherSettings = new Dictionary<Type, IOptions<SimulationSettings>>
        {
            [typeof(PlatooTest)] = Options.Create(_settingsFirst),
            [typeof(AristotleTest)] = Options.Create(_settingsSecond),
            [typeof(SocratesTest)] = Options.Create(_settingsThird),
            [typeof(DecartesTest)] = Options.Create(_settingsFourth),
            [typeof(KantTest)] = Options.Create(_settingsFivth)
        };

        var mockStrategy = new SimpleTakingForksStrategy();

        var tableManager = new TableManager(_mockOptionsFirst);


        var statisticsMockBuilder = new StatisticsMockBuilder()
        .WithDeadlockFlag(ref isDeadlock, tableManager.GetForks());

        IStatistics mockStatistics = statisticsMockBuilder.Build();

        var logger = new DeadlockDetectionLoggerMock<StatisticsService>();

        using var host = new TestHostBuilder(philosopherSettings, mockStatistics, mockStrategy, tableManager, logger)
        .BuildTestHost();

        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(11));

        var hostTask = host.RunAsync(cts.Token);

        hostTask.Wait();
        Assert.That(logger.DeadlockDetected, Is.True);
    }
}


public class StatisticsMockBuilder
{
    private bool _deadlockOccurred = false;
    private readonly List<Philosopher> _philosophers = new();
    private List<Fork> _forks = [];

    public IStatistics Build()
    {
        var mock = new Mock<IStatistics>();

        mock.Setup(s => s.Overwatch(It.IsAny<CancellationToken>()))
            .Returns(async (CancellationToken token) =>
            {
                var start = DateTime.Now;
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(200, token);
                    PrintSystemState(mock.Object);
                }
            });

        mock.Setup(s => s.AddPhilosopher(It.IsAny<Philosopher>()))
            .Callback<Philosopher>(_philosophers.Add);

        mock.Setup(s => s.OnPhilosopherChangedStatus(It.IsAny<Philosopher>(), It.IsAny<PhilosopherState>()))
            .Callback<Philosopher, PhilosopherState>((philosopher, state) =>
            {
            });

        mock.Setup(s => s.OnForkOwnerChanged(It.IsAny<Fork>(), It.IsAny<IForkOwner>()))
            .Callback<Fork, IForkOwner?>((fork, owner) =>
            {
            });

        return mock.Object;
    }

    public StatisticsMockBuilder WithDeadlockFlag(ref bool deadlockFlag, List<Fork> forks)
    {
        _deadlockOccurred = deadlockFlag;
        _forks = forks;
        return this;
    }

    private void PrintSystemState(IStatistics statistics)
    {
        int countHalfOwners = 0;

        for (int i = 0; i < _philosophers.Count; ++i)
        {
            var philosopher = _philosophers[i];
            Console.WriteLine($"philosopher: {_philosophers.Count}, left: {philosopher.LeftFork.Owner}, right: {philosopher.RightFork.Owner}");
            if ((philosopher.LeftFork.Owner == philosopher.Name && philosopher.RightFork.Owner != philosopher.Name) ||
                (philosopher.LeftFork.Owner != philosopher.Name && philosopher.RightFork.Owner == philosopher.Name))
            {
                countHalfOwners++;
            }
        }
        Console.WriteLine($"philosophers: {_philosophers.Count}, halfs: {countHalfOwners}");
        if (countHalfOwners == _philosophers.Count && _philosophers.Count > 0)
        {
            _deadlockOccurred = true;
        }
    }

    public bool GetDeadlockFlag() => _deadlockOccurred;
}


public class DeadlockDetectionLoggerMock<T> : ILogger<T>
{
    public bool DeadlockDetected { get; private set; }
    private readonly List<string> _loggedMessages = new();

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return NullScope.Instance;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        _loggedMessages.Add(message);

        if (logLevel == LogLevel.Information && message.Contains("THERE IS DEADLOCK!\nEND SIMULATION"))
        {
            DeadlockDetected = true;
        }
    }

    public IReadOnlyList<string> GetLoggedMessages() => _loggedMessages.AsReadOnly();

    private class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new NullScope();
        public void Dispose() { }
    }
}

public class TestHostBuilder
{
    private readonly Dictionary<Type, IOptions<SimulationSettings>> _philosopherSettings;
    private readonly IStatistics _mockStatistics;
    private readonly ITakingForksStrategy _mockStrategy;

    private readonly ITableManager _tableManager;

    private readonly ILogger<StatisticsService> _logger;

    public TestHostBuilder(
        Dictionary<Type, IOptions<SimulationSettings>> philosopherSettings,
        IStatistics mockStatistics,
        ITakingForksStrategy mockStrategy, ITableManager tableManager, ILogger<StatisticsService> logger)
    {
        _philosopherSettings = philosopherSettings;
        _mockStatistics = mockStatistics;
        _mockStrategy = mockStrategy;
        _tableManager = tableManager;
        _logger = logger;
    }

    public IHost BuildTestHost()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<IOptions<SimulationSettings>>(_philosopherSettings[typeof(PlatooTest)]);

                services.AddSingleton<ITakingForksStrategy, SimpleTakingForksStrategy>();
                services.AddSingleton<ITableManager,TableManager>();
                services.AddSingleton<IStatistics, Statistics>();

                services.AddHostedService<PlatooTest>(provider =>
                {
                    var settings = _philosopherSettings[typeof(PlatooTest)];
                    var tableManager = provider.GetRequiredService<ITableManager>();
                    var logger = provider.GetRequiredService<ILogger<PlatooTest>>();
                    var hostLifetime = provider.GetRequiredService<IHostApplicationLifetime>();
                    var strategy = provider.GetRequiredService<ITakingForksStrategy>();
                    var statistics = provider.GetRequiredService<IStatistics>();

                    return new PlatooTest(
                        "Platoo", tableManager, settings, logger, hostLifetime, strategy, statistics);
                });

                services.AddHostedService<AristotleTest>(provider =>
                {
                    var settings = _philosopherSettings[typeof(AristotleTest)];
                    var tableManager = provider.GetRequiredService<ITableManager>();
                    var logger = provider.GetRequiredService<ILogger<AristotleTest>>();
                    var hostLifetime = provider.GetRequiredService<IHostApplicationLifetime>();
                    var strategy = provider.GetRequiredService<ITakingForksStrategy>();
                    var statistics = provider.GetRequiredService<IStatistics>();

                    return new AristotleTest(
                        "Aristotle", tableManager, settings, logger, hostLifetime, strategy, statistics);
                });

                services.AddHostedService<SocratesTest>(provider =>
                {
                    var settings = _philosopherSettings[typeof(SocratesTest)];
                    var tableManager = provider.GetRequiredService<ITableManager>();
                    var logger = provider.GetRequiredService<ILogger<SocratesTest>>();
                    var hostLifetime = provider.GetRequiredService<IHostApplicationLifetime>();
                    var strategy = provider.GetRequiredService<ITakingForksStrategy>();
                    var statistics = provider.GetRequiredService<IStatistics>();

                    return new SocratesTest(
                        "Socrates", tableManager, settings, logger, hostLifetime, strategy, statistics);
                });

                services.AddHostedService<DecartesTest>(provider =>
                {
                    var settings = _philosopherSettings[typeof(DecartesTest)];
                    var tableManager = provider.GetRequiredService<ITableManager>();
                    var logger = provider.GetRequiredService<ILogger<DecartesTest>>();
                    var hostLifetime = provider.GetRequiredService<IHostApplicationLifetime>();
                    var strategy = provider.GetRequiredService<ITakingForksStrategy>();
                    var statistics = provider.GetRequiredService<IStatistics>();

                    return new DecartesTest(
                        "Descartes", tableManager, settings, logger, hostLifetime, strategy, statistics);
                });

                services.AddHostedService<KantTest>(provider =>
                {
                    var settings = _philosopherSettings[typeof(KantTest)];
                    var tableManager = provider.GetRequiredService<ITableManager>();
                    var logger = provider.GetRequiredService<ILogger<KantTest>>();
                    var hostLifetime = provider.GetRequiredService<IHostApplicationLifetime>();
                    var strategy = provider.GetRequiredService<ITakingForksStrategy>();
                    var statistics = provider.GetRequiredService<IStatistics>();

                    return new KantTest(
                        "Kant", tableManager, settings, logger, hostLifetime, strategy, statistics);
                });

                services.AddHostedService<StatisticsService>(provider =>
                {
                    var applicationLifetime = provider.GetRequiredService<IHostApplicationLifetime>();
                    var statistics = provider.GetRequiredService<IStatistics>();

                    return new StatisticsService(_logger, applicationLifetime, statistics);
                });
            })
            .Build();
    }
}


public class PlatooTest : PhilosopherService
{
    public PlatooTest(string name, ITableManager tableMgr, IOptions<SimulationSettings> opts, ILogger<PhilosopherService> logger, IHostApplicationLifetime life, ITakingForksStrategy strategy, IStatistics statistics)
        : base(name, tableMgr, opts, logger, life, strategy, statistics)
    {
    }
}
public class AristotleTest : PhilosopherService
{
    public AristotleTest(string name, ITableManager tableMgr, IOptions<SimulationSettings> opts, ILogger<PhilosopherService> logger, IHostApplicationLifetime life, ITakingForksStrategy strategy, IStatistics statistics)
        : base(name, tableMgr, opts, logger, life, strategy, statistics)
    {
    }
}

public class SocratesTest : PhilosopherService
{
    public SocratesTest(string name, ITableManager tableMgr, IOptions<SimulationSettings> opts, ILogger<PhilosopherService> logger, IHostApplicationLifetime life, ITakingForksStrategy strategy, IStatistics statistics)
        : base(name, tableMgr, opts, logger, life, strategy, statistics)
    {
    }
}

public class DecartesTest : PhilosopherService
{
    public DecartesTest(string name, ITableManager tableMgr, IOptions<SimulationSettings> opts, ILogger<PhilosopherService> logger, IHostApplicationLifetime life, ITakingForksStrategy strategy, IStatistics statistics)
        : base(name, tableMgr, opts, logger, life, strategy, statistics)
    {
    }
}

public class KantTest : PhilosopherService
{
    public KantTest(string name, ITableManager tableMgr, IOptions<SimulationSettings> opts, ILogger<PhilosopherService> logger, IHostApplicationLifetime life, ITakingForksStrategy strategy, IStatistics statistics)
        : base(name, tableMgr, opts, logger, life, strategy, statistics)
    {
    }
}