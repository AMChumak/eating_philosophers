using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PhilosopherLib;
using SimulationContextLib;
using View;

if (args.Length == 0)
{
    Console.WriteLine("Set moment of time in seconds as first argument");
    return;
}


var serviceCollection = new ServiceCollection();

serviceCollection.AddDbContextFactory<SimulationContext>(options =>
            options.UseNpgsql("Server=localhost;Port=5432;Database=EatingPhilosophers;User Id=admin;Password=1234"));



var serviceProvider = serviceCollection.BuildServiceProvider();
var contextFactory = serviceProvider.GetRequiredService<IDbContextFactory<SimulationContext>>();
var stateLoader = new StateLoader(contextFactory);

var forkIds = stateLoader.GetAllForkIds();
var philosopherNames = stateLoader.GetAllPhilosopherNames();


double thresholdSeconds = 0;
if (!double.TryParse(args[0], out thresholdSeconds) || thresholdSeconds < 0)
{
    Console.WriteLine($"invalid TimeSpan");
    return;
}

TimeSpan threshold = TimeSpan.FromSeconds(thresholdSeconds);

Console.WriteLine($"============ State at {thresholdSeconds} ============");

Console.WriteLine($"\nPhilosophers:");

foreach (var philosopherName in philosopherNames)
{
    PhilosopherState state = stateLoader.GetLastPhilosopherState(philosopherName, threshold);

    Console.WriteLine($"{philosopherName} was {state}");
}

Console.WriteLine($"\nForks:");

foreach (var forkId in forkIds)
{
    string owner = stateLoader.GetLastForkState(forkId, threshold);

    if (owner == "")
        Console.WriteLine($"Fork {forkId} was available");
    else
        Console.WriteLine($"Fork {forkId} was owned by {owner}");
}