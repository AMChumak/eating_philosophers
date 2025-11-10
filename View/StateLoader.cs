using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PhilosopherLib;
using SimulationContextLib;

namespace View;


public class StateLoader
{
    private IDbContextFactory<SimulationContext> _contextFactory;


    public StateLoader(IDbContextFactory<SimulationContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }


    public List<int> GetAllForkIds()
    {
        List<int> forkIds = [];
        using (var context = _contextFactory.CreateDbContext())
        {
            forkIds = context.ForkUpdates
            .Select(f => f.ForkId)
            .Distinct()
            .ToList();
        }

        return forkIds;
    }


    public List<string> GetAllPhilosopherNames()
    {
        List<string> philosopherNames = [];

        using (var context = _contextFactory.CreateDbContext())
        {
            philosopherNames = context.PhilosopherUpdates
            .Select(p => p.Name)
            .Distinct()
            .ToList();
        }

        return philosopherNames;
    }

    public PhilosopherState GetLastPhilosopherState(string philosopherName, TimeSpan threshold)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var lastRecord = context.PhilosopherUpdates
            .Where(p => p.Name == philosopherName && p.UpdateTime <= threshold)
            .OrderByDescending(p => p.UpdateTime)
            .FirstOrDefault();

            return lastRecord?.State ?? PhilosopherState.Thinking;
        }
    }

    public string GetLastForkState(int forkId, TimeSpan threshold)
    {
        using (var context = _contextFactory.CreateDbContext())
        {
            var lastRecord = context.ForkUpdates
            .Where(f => f.ForkId == forkId && f.UpdateTime <= threshold)
            .OrderByDescending(p => p.UpdateTime)
            .FirstOrDefault();

            return lastRecord?.ForkOwner ?? "";
        }
    }
}