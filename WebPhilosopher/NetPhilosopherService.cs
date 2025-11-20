using System.Diagnostics;
using ForkLib;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimulationSettingsLib;
using StrategyContractLib;
using PhilosopherLib;
using NetworkContracts;
using System.Net.Http.Json;

namespace WebPhilosopher;

public class NetPhilosopherService : IHostedService
{
    private HttpClient _httpClient;
    private string _tableServiceUrl;
    private ILogger<PhilosopherService> _logger;
    private readonly string _name;
    private IHostApplicationLifetime _appLifetime;
    public Philosopher Phiosopher;
    private CancellationTokenSource _cts;

    private int _duration;

    public NetPhilosopherService(IFork leftFork, IFork rightFork, IOptions<SimulationSettings> settings, ILogger<PhilosopherService> logger, IHostApplicationLifetime hostApplicationLifetime, ITakingForksStrategy takingForksStrategy, HttpClient httpClient, string uri)
    {
        _httpClient = httpClient;
        _tableServiceUrl = uri;
        _name = Environment.GetEnvironmentVariable("PHILOSOPHER_NAME") ?? "Unknown";
        _logger = logger;
        _appLifetime = hostApplicationLifetime;
        _cts = new CancellationTokenSource();
        _duration = settings.Value.DurationSec;

        Phiosopher = new Philosopher(_name, leftFork, rightFork, takingForksStrategy, settings);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts.CancelAfter(_duration * 1000);
        _logger.LogDebug($"Philosopher {_name} started");
        _appLifetime.ApplicationStarted.Register(() =>
        {
            Task.Run(async () =>
            {
                try
                {
                    await Phiosopher.Live(_cts.Token);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogDebug($"philosopher {_name} was stopped");
                }
                finally
                {
                    await gracefulExit();
                    _appLifetime.StopApplication();
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

    private async Task gracefulExit()
    {
        var request = new PhilosopherExitRequest
        {
            PhilosopherName = _name
        };

        var done = false;
        int countTries = 0;
        while (!done)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_tableServiceUrl}/philosopher/exit", request);
            done = response.IsSuccessStatusCode;
        }
    }
}