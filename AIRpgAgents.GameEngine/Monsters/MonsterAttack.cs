using AIRpgAgents.GameEngine.Abstractions;
using AIRpgAgents.GameEngine.PlayerCharacter;
using AIRpgAgents.GameEngine.Rules;
using SkPluginComponents.Models;

namespace AIRpgAgents.GameEngine.Monsters;

public class MonsterAttack : ICombatAttack
{
    public string Name { get; set; } = "";
    public int AttackBonus { get; set; }
    public int DamageDieCount { get; set; }
    public DieType DamageDie { get; set; }
    public int DamageBonus { get; set; }
    public DamageType DamageType { get; set; }
    public string Range { get; set; } = "Melee"; // In feet or melee
    //public List<string> Properties { get; set; } = [];
    public string? Description { get; set; }
}