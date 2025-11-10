using PhilosopherLib;

namespace SimulationContextLib;


public class PhilosopherUpdate
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public PhilosopherState State { get; set; }
    public TimeSpan UpdateTime { get; set; }
}

public class ForkUpdate
{
    public int Id { get; set; }
    public int ForkId { get; set; }
    public required string ForkOwner { get; set; }
    public TimeSpan UpdateTime { get; set; }
}

