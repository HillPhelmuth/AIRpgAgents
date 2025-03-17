using AIRpgAgents.GameEngine.Rules;
using SkPluginComponents.Models;

namespace AIRpgAgents.GameEngine.Monsters;

public class MonsterAttack
{
    public string Name { get; set; } = "";
    public int AttackBonus { get; set; }
    public DieType DamageDie { get; set; }
    public int DamageBonus { get; set; }
    public CombatSystem.DamageType DamageType { get; set; }
    public int Range { get; set; } // In feet
    public string? Description { get; set; }
}