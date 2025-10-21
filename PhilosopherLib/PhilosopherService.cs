using System.Diagnostics;
using ForkLib;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimulationSettingsLib;
using StrategyContractLib;


namespace PhilosopherLib;

public class PhilosopherService : IHostedService
{
    private ILogger<PhilosopherService> _logger;
    private readonly string _name;
    private IHostApplicationLifetime _appLifetime;
    private Philosopher _philosopher;

    public PhilosopherService(string name, ITableManager tableManager, IOptions<SimulationSettings> settings, ILogger<PhilosopherService> logger, IHostApplicationLifetime hostApplicationLifetime, ITakingForksStrategy takingForksStrategy, IStatistics statistics)
    {
        _name = name;
        _logger = logger;
        _appLifetime = hostApplicationLifetime;

        int seatIndex = tableManager.TakeSeat(_name);

        _philosopher = new Philosopher(name, tableManager.GetLeftFork(seatIndex), tableManager.GetRightFork(seatIndex), takingForksStrategy, settings);

        statistics.AddPhilosopher(_philosopher);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug($"Philosopher {_name} started");
        _appLifetime.ApplicationStarted.Register(() =>
        {
            Task.Run(async () =>
            {
                try
                {
                    await _philosopher.Live(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogDebug($"philosopher {_name} was stopped");
                }
            });
        });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug($"Philosopher {_name} exited");
        return Task.CompletedTask;
    }
}


