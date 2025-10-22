using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PhilosopherLib;

namespace StatisticsLib;

public class StatisticsService : IHostedService
{
    private ILogger<StatisticsService> _logger;
    private IHostApplicationLifetime _applicationLifetime;
    private IStatistics _statistics;

    public StatisticsService(ILogger<StatisticsService> logger, IHostApplicationLifetime applicationLifetime, IStatistics statistics)
    {
        _logger = logger;
        _applicationLifetime = applicationLifetime;
        _statistics = statistics;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _applicationLifetime.ApplicationStarted.Register(() =>
        {
            Task.Run(async () =>
            {
                try
                {
                    await _statistics.Overwatch(cancellationToken);
                    _statistics.PrintStatistics();
                }
                catch (OperationCanceledException)
                {
                    _logger.LogDebug("Statistics service was stopped");
                }
                catch (Exception)
                {
                    _logger.LogInformation("THERE IS DEADLOCK!\nEND SIMULATION");
                }
                finally
                {
                    _applicationLifetime.StopApplication();
                }
            });
        });

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Statistics service exited");
        return Task.CompletedTask;
    }
}