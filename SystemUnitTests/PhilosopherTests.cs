using ForkLib;
using Microsoft.Extensions.Options;
using Moq;
using PhilosopherLib;
using SimulationSettingsLib;
using StrategyContractLib;

namespace SystemUnitTests;

public class PhilosopherTests
{
    private ITakingForksStrategy _mockStrategy;
    private List<PhilosopherState> _statesQ;
    private CancellationTokenSource _cts;
    private Fork _leftFork;
    private Fork _rightFork;
    private Philosopher _philosopher;
    private IOptions<SimulationSettings> _mockOptions;
    private SimulationSettings _settings;

    [SetUp]
    public void Setup()
    {
        _settings = new SimulationSettings
        {
            DurationSec = 10,
            DisplayIntervalMs = 200,
            ThinkingTimeMinMs = 30,
            ThinkingTimeMaxMs = 100,
            EatingTimeMinMs = 40,
            EatingTimeMaxMs = 50,
            ForkAcquisitionTimeMs = 20
        };

        _mockOptions = Mock.Of<IOptions<SimulationSettings>>(
            x => x.Value == _settings
        );

        _mockStrategy = Mock.Of<ITakingForksStrategy>();
        _leftFork = new Fork(_settings.ForkAcquisitionTimeMs, 0);
        _rightFork = new Fork(_settings.ForkAcquisitionTimeMs, 1);
        _statesQ = [];

        _cts = new CancellationTokenSource();
        _philosopher = new Philosopher("Alex Pashkov", _leftFork, _rightFork, _mockStrategy, _mockOptions);
    }

    [TearDown]
    public void Teardown()
    {
        _cts.Dispose();
    }


    [Test]
    public async Task TestPhilosopherStateMachineAsync()
    {
        _philosopher.ChangedState += CheckNextState;

        Assert.That(_philosopher.State, Is.EqualTo(PhilosopherState.Thinking));

        await _philosopher.Live(_cts.Token);

        Assert.That(_statesQ[0], Is.EqualTo(PhilosopherState.Thinking));
        Assert.That(_statesQ[1], Is.EqualTo(PhilosopherState.Hungry));
        Assert.That(_statesQ[2], Is.EqualTo(PhilosopherState.Eating));
        Assert.That(_statesQ[3], Is.EqualTo(PhilosopherState.Thinking));
        Console.WriteLine($"states count {_statesQ.Count}");
    }

    private void CheckNextState(Philosopher philosopher, PhilosopherState state)
    {
        foreach (var phs in _statesQ)
        {
            if (phs == state)
            {
                _cts.Cancel();
                break;
            }
        }

        _statesQ.Add(state);
    }
}