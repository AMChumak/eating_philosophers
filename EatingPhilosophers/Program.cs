using ForkLib;
using StrategyContractLib;
using PhilosopherLib;
using StrategyImplementationLib;

List<Philosopher> philosophers = [];
List<Fork> forks = [];
SimpleArbitrator arbitrator = new(philosophers, forks);
List<int> hungryStatistics = [];
List<int> forksAvailable = [];
List<int> forksInEating = [];

int countIterations = 1000000;

if (args.Length < 1)
{
    Console.Error.WriteLine("Missing file with names");
    return;
}

if (args.Length >= 2 && args[1] != "simple" && args[1] != "arbitrator")
{
    Console.Error.WriteLine("Error mode of program choose between \"simple\" and \"arbitrator\"");
}

bool simple = args.Length < 2 || args[1] == "simple";
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
forksAvailable.Add(0);
forksInEating.Add(0);

for (int i = 0; i < names.Count; i++)
{
    string name = string.Intern(names[i]);

    if (i < names.Count - 1)
    {
        forks.Add(new Fork());
        forksAvailable.Add(0);
        forksInEating.Add(0);
    }

    ITakingForksStrategy strategy;

    if (simple)
    {
        strategy = new SimpleTakingForksStrategy();
    }
    else
    {
        strategy = new ArbitratorTakingForksStrategy(arbitrator, string.Intern(name));
    }

    philosophers.Add(new Philosopher(string.Intern(name), forks[philosophers.Count], forks[(philosophers.Count + 1) % forks.Count], strategy));
    hungryStatistics.Add(0);
}

for (int i = 0; i < countIterations; i++)
{
    if (!simple)
    {
        arbitrator.Manage();
    }

    for (int j = 0; j < philosophers.Count; j++)
    {
        philosophers[j].Move();
    }

    int hungryAndNone = 0;

    for (int j = 0; j < philosophers.Count; j++)
    {
        if (philosophers[j].State == PhilosopherState.Hungry)
        {
            hungryStatistics[j]++;
            if (philosophers[j].Action == PhilosopherAction.None)
            {
                hungryAndNone++;
            }
        }
    }

    if (hungryAndNone == philosophers.Count)
    {
        Console.WriteLine("THERE IS DEADLOCK ON {0} STEP!", i);
        return;
    }

    for (int j = 0; j < forks.Count; j++)
        {
            if (forks[j].Owner != "")
            {
                for (int k = 0; k < philosophers.Count; k++)
                {
                    if (forks[j].Owner == philosophers[k].Name && philosophers[k].State == PhilosopherState.Eating)
                    {
                        forksInEating[j]++;
                        break;
                    }
                }
            }
            else
            {
                forksAvailable[j]++;
            }
        }

    if (i % 1000 == 0)
    {
        Console.WriteLine("---------------- STEP {0} ----------------", i);

        for (int j = 0; j < philosophers.Count; j++)
        {
            Console.WriteLine("{0}: {1} (In state: {2} steps) [Current action: {3}] Throuhput: {4}", philosophers[j].Name, philosophers[j].State.ToString(), philosophers[j].InStateDuration, philosophers[j].Action.ToString(), philosophers[j].Score / 1000);
        }

        for (int j = 0; j < forks.Count; j++)
        {
            if (forks[j].Owner != "")
            {
                Console.WriteLine("Fork {0}: In Use (Owner: {1}) ", j, forks[j].Owner);
            }
            else
            {
                Console.WriteLine("Fork {0}: Available", j);
            }
        }

        Console.WriteLine("------------------------------------------");
    }

    for (int j = 0; j < philosophers.Count; j++)
    {
        philosophers[j].UpdateState();
    }
}

Console.WriteLine("---------------- STATISTICS ----------------");

Console.WriteLine("Score:");

int total = 0;

foreach (var philosopher in philosophers)
{
    total += philosopher.Score;
    Console.WriteLine("\t{0}: {1}", philosopher.Name, philosopher.Score);
}

Console.WriteLine("\tTotal: {0}\n", total);
Console.WriteLine("Throughput:");

int totalThroughput = 0;

foreach (var philosopher in philosophers)
{
    totalThroughput += philosopher.Score / (countIterations / 1000);
    Console.WriteLine("\t{0}: {1}", philosopher.Name, philosopher.Score / (countIterations / 1000));
}

Console.WriteLine("\tAverage: {0}\n", totalThroughput / philosophers.Count);
Console.WriteLine("Hungry Statistics:");

string maxHungyPhilosopher = philosophers[0].Name;
int maxHungryCount = hungryStatistics[0];

for (int i = 0; i < philosophers.Count; ++i)
{
    if (maxHungryCount < hungryStatistics[i])
    {
        maxHungyPhilosopher = philosophers[i].Name;
        maxHungryCount = hungryStatistics[i];
    }

    Console.WriteLine("\t{0}: {1}", philosophers[i].Name, hungryStatistics[i]);
}

Console.WriteLine("\tMaximum: {0} ({1})\n", maxHungryCount, maxHungyPhilosopher);
Console.WriteLine("Fork utility quotient:");

for (int i = 0; i < forks.Count; ++i)
{
    double availablePercents = (double)forksAvailable[i] / countIterations * 100;
    double blockedPercents = (double)(countIterations - forksAvailable[i] - forksInEating[i]) / countIterations * 100;
    double inEatingPercents = (double)forksInEating[i] / countIterations * 100;
    Console.WriteLine("\tFork {0}: Available: {1} / Blocked: {2} / In Use: {3}", i, availablePercents, blockedPercents, inEatingPercents);
}


Console.WriteLine("--------------------------------------------");
