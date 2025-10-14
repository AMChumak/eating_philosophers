using ForkLib;
using StrategyContractLib;
using PhilosopherLib;
using StrategyImplementationLib;
using EatingPhilosophers;

List<Philosopher> philosophers = [];
List<Fork> forks = [];

if (args.Length < 1)
{
    Console.Error.WriteLine("Missing file with names");
    return;
}

if (args.Length > 2)
{
    Console.Error.WriteLine("Too many arguments");
    return;
}

int durationMs;

if (!int.TryParse(args[1], out durationMs) || durationMs <= 0)
{
    Console.Error.WriteLine("Incorrect duration (That must be integer > 0 ) ");
}

string[] lines = File.ReadAllLines(args[0]);
List<string> names = [];

for (int i = 0; i < lines.Length; i++)
{
    string name = string.Intern(lines[i].Trim());

    if (name == "")
        continue;

    names.Add(string.Intern(name));
}

if (names.Count < 2)
{
    Console.Error.WriteLine("Too few philosophers. Their count must be greater than 1");
    return;
}

forks.Add(new Fork());

for (int i = 0; i < names.Count; i++)
{
    string name = string.Intern(names[i]);

    if (i < names.Count - 1)
    {
        forks.Add(new Fork());
    }

    ITakingForksStrategy strategy = new SimpleTakingForksStrategy();

    philosophers.Add(new Philosopher(string.Intern(name), forks[philosophers.Count], forks[(philosophers.Count + 1) % forks.Count], strategy));
}

Statistics statistics = new Statistics(philosophers, forks, 200);
TimeSpan totalTime = TimeSpan.FromMilliseconds(durationMs);
int countIntervals = durationMs / statistics.IntervalDuration;

Console.WriteLine("{0}!\n", philosophers.Count);

foreach (var philosopher in philosophers)
{
    philosopher.Start();
}

statistics.Overwatch(countIntervals);

foreach (var philosopher in philosophers)
{
    philosopher.Stop();
}

statistics.PrintStatistics(totalTime);