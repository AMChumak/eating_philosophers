namespace SimulationSettingsLib;

public class SimulationSettings
{
    public int DurationSec { get; set; }
    public int DisplayIntervalMs { get; set; }

    public int ThinkingTimeMinMs { get; set; }
    public int ThinkingTimeMaxMs { get; set; }
    public int EatingTimeMinMs { get; set; }
    public int EatingTimeMaxMs { get; set; }
    public int ForkAcquisitionTimeMs { get; set; }
}
