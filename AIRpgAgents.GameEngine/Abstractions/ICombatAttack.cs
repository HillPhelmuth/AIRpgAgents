using AIRpgAgents.GameEngine.Rules;
using SkPluginComponents.Models;

namespace AIRpgAgents.GameEngine.Abstractions;

public interface ICombatAttack
{
    DieType DamageDie { get; }
    int DamageDieCount { get; }
    DamageType DamageType { get; set; }
    int AttackBonus { get; set; }
    string Range { get; set; }
    //List<string> Properties { get; set; }
    int DamageBonus { get; set; }
}