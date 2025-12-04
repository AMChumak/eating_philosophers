using PhilosopherLib;
namespace NetworkContracts;


public class PhilosopherUpdateRequest
{
    public required string PhilosopherName { get; set; }
    public PhilosopherState NewState { get; set; }
}

public class TakeForkRequest
{
    public required string PhilosopherName { get; set; }
    public int ForkId { get; set; }
}

public class TakeForkResponse
{
    public bool IsSuccess { get; set; }
}


public class ReleaseForksRequest
{
    public required string PhilosopherName { get; set; }
    public int ForkId { get; set; }
}


public class PhilosopherEnterRequest
{
    public required string PhilosopherName { get; set; }
}

public class PhilosopherEnterResponse
{
    public int  LeftFork { get; set; }
    public int  RightFork { get; set; }
}


public class PhilosopherExitRequest
{
    public required string PhilosopherName { get; set; }
}
