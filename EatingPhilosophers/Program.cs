using System.Reflection;
using ForkLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PhilosopherLib;
using SimulationSettingsLib;
using StatisticsLib;
using StrategyContractLib;
using StrategyImplementationLib;

namespace EatingPhilosophers;

internal sealed class Program
{
    private static async Task Main(string[] args)
    {
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
                services.AddSingleton<IStatistics, Statistics>();

                services.AddHostedService<Platoo>();
                services.AddHostedService<Aristotle>();
                services.AddHostedService<Socrates>();
                services.AddHostedService<Decartes>();
                services.AddHostedService<Kant>();

                services.AddHostedService<StatisticsService>();
            })
            .RunConsoleAsync();
    }
}