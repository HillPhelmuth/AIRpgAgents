using System.Text.Json.Serialization;
using AIRpgAgents.GameEngine.Abstractions;
using AIRpgAgents.GameEngine.Rules;
using SkPluginComponents.Models;
using CosmosIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
namespace AIRpgAgents.GameEngine.PlayerCharacter;

public class InventoryItem
{
    public string Name { get; set; }
    public int Quantity { get; set; }
    public string Description { get; set; }
    public double Weight { get; set; }
    public int Value { get; set; } // In copper coins
    public bool IsEquippable { get; set; }
    public bool IsConsumable { get; set; }
}

public class Weapon : InventoryItem, ICombatAttack
{
    public string DamageFormula { get; set; } = "1D6";

    [JsonIgnore]
    [CosmosIgnore]
    public DieType DamageDie
    {
        get
        {
            // Remove the leading integer from the damage formula
            var dieTypeString = DamageFormula[1..];
            return Enum.TryParse<DieType>(dieTypeString, out var dieType) ? dieType : DieType.D6;
        }
    }
    [JsonIgnore]
    [CosmosIgnore]
    public int DamageDieCount
    {
        get
        {
            // Get the leading integer from the damage formula
            var dieCountString = DamageFormula[..1];
            return int.TryParse(dieCountString, out var dieCount) ? dieCount : 1;
        }
    }
    public DamageType DamageType { get; set; }
    public int AttackBonus { get; set; }
    public string Range { get; set; }
    public WeaponType Type { get; set; }
    public List<string> Properties { get; set; } = [];
    public int DamageBonus { get; set; }
    public bool IsEquipped { get; set; }
    public void Equip()
    {
        IsEquipped = true;
    }

    public Weapon()
    {
        IsEquippable = true;
    }
}

public class Armor : InventoryItem
{
    public int ArmorBonus { get; set; }
    public bool IsShield { get; set; }
    public int? StrengthRequirement { get; set; }
    public List<string> Properties { get; set; } = [];
    public bool IsEquipped { get; set; }
    public void Equip()
    {
        IsEquipped = true;
    }

    public Armor()
    {
        IsEquippable = true;
    }
}
public class AdventuringGear : InventoryItem
{
    public List<string> Properties { get; set; } = [];
}
