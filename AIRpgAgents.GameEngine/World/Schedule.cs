namespace AIRpgAgents.GameEngine.World;

public class Schedule
{
    public List<ScheduleEntry> Entries { get; set; } = [];
        
    public ScheduleEntry GetCurrentActivity(GameTime time)
    {
        // Get entry for current time
        return Entries.FirstOrDefault(e => 
            e.StartHour <= time.Hour && 
            (e.EndHour > time.Hour || e.EndHour == time.Hour && e.EndMinute > time.Minute));
    }
}

public class ScheduleEntry
{
    public int StartHour { get; set; }
    public int StartMinute { get; set; }
    public int EndHour { get; set; }
    public int EndMinute { get; set; }
    public string? LocationId { get; set; }
    public string? Activity { get; set; }
}