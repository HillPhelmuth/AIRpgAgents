namespace AIRpgAgents.GameEngine.WorldState;

public class GameTime
{
    public int Day { get; set; } = 1;
    public int Hour { get; set; } = 8; // Start at 8 AM
    public int Minute { get; set; } = 0;
        
    public void AdvanceTime(int minutes)
    {
        Minute += minutes;
        while (Minute >= 60)
        {
            Minute -= 60;
            Hour++;
        }
            
        while (Hour >= 24)
        {
            Hour -= 24;
            Day++;
        }
    }
        
    public string GetTimeOfDay()
    {
        return Hour switch
        {
            >= 5 and < 12 => "Morning",
            >= 12 and < 17 => "Afternoon",
            >= 17 and < 21 => "Evening",
            _ => "Night"
        };
    }
        
    public override string ToString()
    {
        return $"Day {Day}, {Hour:00}:{Minute:00} ({GetTimeOfDay()})";
    }
}