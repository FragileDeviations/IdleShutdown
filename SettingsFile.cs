using Newtonsoft.Json;

namespace IdleShutdown;

[Serializable]
public class SettingsFile
{
    public List<string> KeepAliveProcesses { get; set; }
    public int IdleCheckInterval { get; set; }
    public int IdleTimeThreshold { get; set; }
    public int StartHour { get; set; }
    public int EndHour { get; set; }

    public SettingsFile(int idleCheckInterval = 10, int startHour = 1, int endHour = 7, int idleTimeThreshold = 1800, List<string> keepAliveProcesses = null)
    {
        IdleCheckInterval = idleCheckInterval;
        StartHour = startHour;
        EndHour = endHour;
        KeepAliveProcesses = keepAliveProcesses ?? new List<string>();
        IdleTimeThreshold = idleTimeThreshold;
    }
}