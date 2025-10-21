using ForkLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PhilosopherLib;
using SimulationSettingsLib;
using StrategyContractLib;


namespace EatingPhilosophers;

public class Platoo : PhilosopherService
{
    public Platoo(IServiceProvider provider)
        : base("Platoo", provider.GetRequiredService<ITableManager>(), provider.GetRequiredService<IOptions<SimulationSettings>>(), provider.GetRequiredService<ILogger<PhilosopherService>>(), provider.GetRequiredService<IHostApplicationLifetime>(), provider.GetRequiredService<ITakingForksStrategy>(), provider.GetRequiredService<IStatistics>())
    {
    }
}

public class Aristotle : PhilosopherService
{
    public Aristotle(IServiceProvider provider)
        : base("Aristotle", provider.GetRequiredService<ITableManager>(), provider.GetRequiredService<IOptions<SimulationSettings>>(), provider.GetRequiredService<ILogger<PhilosopherService>>(), provider.GetRequiredService<IHostApplicationLifetime>(), provider.GetRequiredService<ITakingForksStrategy>(), provider.GetRequiredService<IStatistics>())
    {
    }
}

public class Socrates : PhilosopherService
{
    public Socrates(IServiceProvider provider)
        : base("Socrates", provider.GetRequiredService<ITableManager>(), provider.GetRequiredService<IOptions<SimulationSettings>>(), provider.GetRequiredService<ILogger<PhilosopherService>>(), provider.GetRequiredService<IHostApplicationLifetime>(), provider.GetRequiredService<ITakingForksStrategy>(), provider.GetRequiredService<IStatistics>())
    {
    }
}

public class Decartes : PhilosopherService
{
    public Decartes(IServiceProvider provider)
        : base("Decartes", provider.GetRequiredService<ITableManager>(), provider.GetRequiredService<IOptions<SimulationSettings>>(), provider.GetRequiredService<ILogger<PhilosopherService>>(), provider.GetRequiredService<IHostApplicationLifetime>(), provider.GetRequiredService<ITakingForksStrategy>(), provider.GetRequiredService<IStatistics>())
    {
    }
}

public class Kant : PhilosopherService
{
    public Kant(IServiceProvider provider)
        : base("Kant", provider.GetRequiredService<ITableManager>(), provider.GetRequiredService<IOptions<SimulationSettings>>(), provider.GetRequiredService<ILogger<PhilosopherService>>(), provider.GetRequiredService<IHostApplicationLifetime>(), provider.GetRequiredService<ITakingForksStrategy>(), provider.GetRequiredService<IStatistics>())
    {
    }
}