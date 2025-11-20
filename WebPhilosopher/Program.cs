using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PhilosopherLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SimulationSettingsLib;
using StrategyContractLib;
using StrategyImplementationLib;
using ForkLib;
using WebPhilosopher;
using NetworkContracts;
using System.Net.Http.Json;

HttpClient httpClient = new HttpClient();

string tableServiceUrl = Environment.GetEnvironmentVariable("TABLE_SERVICE_URL") ?? "localhost:8080";

void philosopherUpdateHandler(IPhilosopher philosopher, PhilosopherState newState)
{
    var done = false;
    while (!done)
    {
        var request = new PhilosopherUpdateRequest
        {
            PhilosopherName = philosopher.Name,
            NewState = philosopher.State
        };

        var response = httpClient.PostAsJsonAsync($"{tableServiceUrl}/philosopher/update", request).Result;

        done = response.IsSuccessStatusCode;
    }
}

(int,int) initPhilosopher(string name)
{
    int leftIdx = 0;
    int rightIdx = 0;
    var entered = false;
    while (!entered)
    {
        var request = new PhilosopherEnterRequest
        {
            PhilosopherName = name
        };

        var response = httpClient.PostAsJsonAsync($"{tableServiceUrl}/philosopher/enter", request).Result;

        entered = response.IsSuccessStatusCode;

        if (response.IsSuccessStatusCode)
        {
            var result = response.Content.ReadFromJsonAsync<PhilosopherEnterResponse>();
            result.Wait();
            leftIdx = result.Result?.LeftFork ?? -1;
            rightIdx = result.Result?.RightFork ?? -1;

            if (leftIdx == -1 || rightIdx == -1)
            {
                entered = false;
            }
        }
    }
    return (leftIdx,rightIdx);
}

await Host.CreateDefaultBuilder(args)
    .UseContentRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddOptions<SimulationSettings>().Bind(hostContext.Configuration.GetSection("Simulation"));

        services.AddSingleton<ITakingForksStrategy,SimpleTakingForksStrategy>();
        services.AddSingleton<ITableManager, TableManager>();

        services.AddHostedService(provider =>
        {
            int leftId;
            int rightId;
            string name = Environment.GetEnvironmentVariable("PHILOSOPHER_NAME") ?? "Unknown";
            (leftId, rightId) = initPhilosopher(name);

            var leftFork = new NetFork(leftId, httpClient, tableServiceUrl);
            var rightFork = new NetFork(rightId, httpClient, tableServiceUrl);

            var logger = provider.GetService<ILogger<PhilosopherService>>();
            var settings = provider.GetService<IOptions<SimulationSettings>>();
            var strategy = provider.GetService<ITakingForksStrategy>();
            var appLifeTime = provider.GetService<IHostApplicationLifetime>();

            var service =  new NetPhilosopherService(leftFork, rightFork, settings, logger, appLifeTime, strategy, httpClient, tableServiceUrl);

            service.Phiosopher.ChangedState += philosopherUpdateHandler;

            return service;
        });
    })
    .RunConsoleAsync();