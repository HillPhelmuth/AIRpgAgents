namespace AIRpgAgents.GameEngine.PlayerCharacter;

public class InventoryItem
{
    public string Name { get; set; }
    public int Quantity { get; set; }
    public string Description { get; set; }
    public double Weight { get; set; }
    public double Value { get; set; } // In copper coins
    public bool IsEquippable { get; set; }
    public bool IsConsumable { get; set; }
}
    
public class Weapon : InventoryItem
{
    public string DamageFormula { get; set; }
    public string DamageType { get; set; }
    public int AttackBonus { get; set; }
    public string Range { get; set; }
    public WeaponType Type { get; set; }
    public List<string> Properties { get; set; } = [];
        
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
        
    public Armor()
    {
        IsEquippable = true;
    }
}
public class AdventuringGear : InventoryItem
{
    public List<string> Properties { get; set; } = [];
}
