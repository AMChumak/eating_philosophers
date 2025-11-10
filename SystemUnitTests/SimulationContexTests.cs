using ForkLib;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PhilosopherLib;
using SimulationContextLib;
using SimulationSettingsLib;
using StatisticsLib;
using StrategyContractLib;
using StrategyImplementationLib;
using View;

namespace SystemUnitTests;


public class SimulationContextTests
{
    private SimulationSettings _settings;
    private IOptions<SimulationSettings> _mockOptions;
    private ServiceCollection _services;

    [SetUp]
    public void Setup()
    {
        _settings = new SimulationSettings
        {
            DurationSec = 20,
            DisplayIntervalMs = 200,
            ThinkingTimeMinMs = 10,
            ThinkingTimeMaxMs = 50,
            EatingTimeMinMs = 40,
            EatingTimeMaxMs = 50,
            ForkAcquisitionTimeMs = 5000
        };

        _mockOptions = Mock.Of<IOptions<SimulationSettings>>(
            x => x.Value == _settings
        );

        _services = new ServiceCollection();


        _services.AddDbContextFactory<SimulationContext>(options =>
            options.UseSqlite($"Data Source=test_{Guid.NewGuid()}.db"));

        _services.AddSingleton<ITakingForksStrategy,SimpleTakingForksStrategy>();
        _services.AddSingleton<ITableManager, TableManager>();
        _services.AddSingleton<IStatistics, Statistics>();
        _services.AddLogging(builder =>
        {
            builder.AddConsole();
        });
    }


    [Test]
    public void TestStatisticsConstructor()
    {
        var serviceProvider = _services.BuildServiceProvider();
        var contextFactory = serviceProvider.GetService<IDbContextFactory<SimulationContext>>();

        var statistics = new Statistics(serviceProvider.GetService<ITableManager>(), serviceProvider.GetService<ILogger<Statistics>>(), _mockOptions, contextFactory);

        using (var context = contextFactory.CreateDbContext())
        {
            context.Database.EnsureCreated();
            for (int i = 0; i < 5; i++)
            {
                var lastRecord = context.ForkUpdates
                    .Where(f => f.ForkId == i && f.UpdateTime <= TimeSpan.FromSeconds(1))
                    .OrderByDescending(p => p.UpdateTime)
                    .FirstOrDefault();

                Assert.That(lastRecord, Is.Not.Null);
                Assert.That(lastRecord.ForkOwner, Is.EqualTo(""));
                Assert.That(lastRecord.UpdateTime, Is.EqualTo(TimeSpan.Zero));
            }
            context.SaveChanges();
        }
    }

    [Test]
    public void TestStatiscsAddPhilosopher()
    {
        var serviceProvider = _services.BuildServiceProvider();
        var contextFactory = serviceProvider.GetService<IDbContextFactory<SimulationContext>>();
        var tableManager = serviceProvider.GetService<ITableManager>();
        var statistics = new Statistics(tableManager, serviceProvider.GetService<ILogger<Statistics>>(), _mockOptions, contextFactory);

        int seatNum = tableManager.TakeSeat("Alex Pashkov");
        var leftFork = tableManager.GetLeftFork(seatNum);
        var rightFork = tableManager.GetRightFork(seatNum);
        var strategy = new SimpleTakingForksStrategy();
        var philosopher = new Philosopher("Alex Pashkov", leftFork, rightFork, strategy, _mockOptions);

        statistics.AddPhilosopher(philosopher);

        using (var context = contextFactory.CreateDbContext())
        {
            context.Database.EnsureCreated();

            var lastRecord = context.PhilosopherUpdates
                .Where(p => p.Name == "Alex Pashkov" && p.UpdateTime <= TimeSpan.FromSeconds(1))
                .OrderByDescending(p => p.UpdateTime)
                .FirstOrDefault();

            Assert.That(lastRecord, Is.Not.Null);
            Assert.That(lastRecord.Name, Is.EqualTo("Alex Pashkov"));
            Assert.That(lastRecord.State, Is.EqualTo(PhilosopherState.Thinking));
            Assert.That(lastRecord.UpdateTime, Is.EqualTo(TimeSpan.Zero));

            context.SaveChanges();
        }
    }

    [Test]
    public void TestStateLoaderGetAllForkIds()
    {
        var serviceProvider = _services.BuildServiceProvider();
        var contextFactory = serviceProvider.GetService<IDbContextFactory<SimulationContext>>();
        var tableManager = serviceProvider.GetService<ITableManager>();

        using (var context = contextFactory.CreateDbContext())
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            foreach (var fork in tableManager!.GetForks())
            {
                var forkUpdate = new ForkUpdate { ForkId = fork.OrderNumber, ForkOwner = "", UpdateTime = TimeSpan.Zero };
                context.ForkUpdates.Add(forkUpdate);
            }
            context.SaveChanges();
        }

        var stateLoader = new StateLoader(contextFactory);

        var forkIds = stateLoader.GetAllForkIds();

        for (int i = 0; i < 5; i++)
        {
            Assert.That(forkIds[i], Is.EqualTo(i));
        }
        return;
    }

    [Test]
    public void TestStateLoaderGetAllPhilosopherNames()
    {
        var serviceProvider = _services.BuildServiceProvider();
        var contextFactory = serviceProvider.GetService<IDbContextFactory<SimulationContext>>();
        var tableManager = serviceProvider.GetService<ITableManager>();

        using (var context = contextFactory.CreateDbContext())
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var phAUpdate = new PhilosopherUpdate { Name = "A", State = PhilosopherState.Thinking, UpdateTime = TimeSpan.Zero };
            var phBUpdate = new PhilosopherUpdate { Name = "B", State = PhilosopherState.Thinking, UpdateTime = TimeSpan.Zero };
            var phCUpdate = new PhilosopherUpdate { Name = "C", State = PhilosopherState.Thinking, UpdateTime = TimeSpan.Zero };

            context.PhilosopherUpdates.Add(phAUpdate);
            context.PhilosopherUpdates.Add(phBUpdate);
            context.PhilosopherUpdates.Add(phCUpdate);

            context.SaveChanges();
        }

        var stateLoader = new StateLoader(contextFactory);

        var names = stateLoader.GetAllPhilosopherNames();


        Assert.That(names, Has.Count.EqualTo(3));
        Assert.That(names.Contains("A"), Is.EqualTo(true));
        Assert.That(names.Contains("B"), Is.EqualTo(true));
        Assert.That(names.Contains("C"), Is.EqualTo(true));

        return;
    }


    [Test]
    public void TestStateLoaderGetLastForkState()
    {
        var serviceProvider = _services.BuildServiceProvider();
        var contextFactory = serviceProvider.GetService<IDbContextFactory<SimulationContext>>();

        var statistics = new Statistics(serviceProvider.GetService<ITableManager>(), serviceProvider.GetService<ILogger<Statistics>>(), _mockOptions, contextFactory);

        using (var context = contextFactory.CreateDbContext())
        {
            context.Database.EnsureCreated();

            var forkUpdate = new ForkUpdate { ForkId = 0, ForkOwner = "Alex Pashkov", UpdateTime = TimeSpan.FromSeconds(0.5) };
            context.ForkUpdates.Add(forkUpdate);

            context.SaveChanges();
        }

        var stateLoader = new StateLoader(contextFactory);

        var lastForkState = stateLoader.GetLastForkState(0, TimeSpan.FromSeconds(1));

        Assert.That(lastForkState, Is.EqualTo("Alex Pashkov"));
    }


    [Test]
    public void TestStateLoaderGetLastPhilosopherState()
    {
        var serviceProvider = _services.BuildServiceProvider();
        var contextFactory = serviceProvider.GetService<IDbContextFactory<SimulationContext>>();

        var statistics = new Statistics(serviceProvider.GetService<ITableManager>(), serviceProvider.GetService<ILogger<Statistics>>(), _mockOptions, contextFactory);

        using (var context = contextFactory.CreateDbContext())
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var phAUpdate = new PhilosopherUpdate { Name = "A", State = PhilosopherState.Thinking, UpdateTime = TimeSpan.Zero };
            var phA2Update = new PhilosopherUpdate { Name = "A", State = PhilosopherState.Hungry, UpdateTime = TimeSpan.FromSeconds(0.5) };

            context.PhilosopherUpdates.Add(phAUpdate);
            context.PhilosopherUpdates.Add(phA2Update);

            context.SaveChanges();
        }

        var stateLoader = new StateLoader(contextFactory);

        var lastPhilosopherState = stateLoader.GetLastPhilosopherState("A", TimeSpan.FromSeconds(1));

        Assert.That(lastPhilosopherState, Is.EqualTo(PhilosopherState.Hungry));
    }
}