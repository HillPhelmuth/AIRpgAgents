using System.Text.Json;
using AIRpgAgents.GameEngine.Extensions;

namespace AIRpgAgents.GameEngine.PlayerCharacter;


public class EquipmentDatabase
{
    private static bool _isInitialized = false;
    private static EquipmentData? _equipmentData;
    public static EquipmentData EquipmentData => _equipmentData ??= FileHelper.ExtractFromAssembly<EquipmentData>("Equipment.json");
    public static List<Weapon> Weapons => EquipmentData.Weapons;

    public static List<Armor> Armor => EquipmentData.Armor;

    public static List<AdventuringGear> AdventuringGear => EquipmentData.AdventuringGear;

    public static Weapon GetWeaponByName(string name)
    {
        return Weapons.FirstOrDefault(w => w.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
    
    public static Armor GetArmorByName(string name)
    {
        return Armor.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
    
    public static AdventuringGear GetGearByName(string name)
    {
        return AdventuringGear.FirstOrDefault(g => g.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
    
    public static InventoryItem GetItemByName(string name)
    {
        return (InventoryItem)GetWeaponByName(name) ?? 
               (InventoryItem)GetArmorByName(name) ?? 
               (InventoryItem)GetGearByName(name);
    }
    
    
}
public class EquipmentData
{
    public List<Weapon> Weapons { get; set; } = [];
    public List<Armor> Armor { get; set; } = [];
    public List<AdventuringGear> AdventuringGear { get; set; } = [];
}