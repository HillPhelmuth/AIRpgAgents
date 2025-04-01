namespace AIRpgAgents.GameEngine.Monsters;

public class MonsterEncounter(List<RpgMonster> monsters)
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public List<RpgMonster> Monsters { get; set; } = monsters;
    public DateTime EncounterStart { get; set; } = DateTime.Now;
    public DateTime EncounterEnd { get; set; }

}