using System.Text.Json;
using AIRpgAgents.GameEngine.Extensions;

namespace AIRpgAgents.GameEngine.PlayerCharacter;


public class EquipmentDatabase
{
    private static bool _isInitialized = false;
    private static List<Weapon> _weapons = [];
    private static List<Armor> _armor = [];
    private static List<AdventuringGear> _adventuringGear = [];
    private static EquipmentData? _equipmentData;
    public static EquipmentData EquipmentData => _equipmentData ??= FileHelper.ExtractFromAssembly<EquipmentData>("Equipment.json");
    public static List<Weapon> Weapons 
    {
        get
        {
            EnsureInitialized();
            return _weapons;
        }
    }
    
    public static List<Armor> Armor
    {
        get
        {
            EnsureInitialized();
            return _armor;
        }
    }
    
    public static List<AdventuringGear> AdventuringGear
    {
        get
        {
            EnsureInitialized();
            return _adventuringGear;
        }
    }
    
    private static void EnsureInitialized()
    {
        if (_isInitialized)
            return;
            
        try
        {
            var equipmentData = FileHelper.ExtractFromAssembly<EquipmentData>("Equipment.json");
            
            if (equipmentData != null)
            {
                _weapons = equipmentData.Weapons ?? [];
                _armor = equipmentData.Armor ?? [];
                _adventuringGear = equipmentData.AdventuringGear ?? [];
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading equipment data: {ex.Message}");
            // Initialize with empty collections in case of error
            _weapons = [];
            _armor = [];
            _adventuringGear = [];
        }
        
        _isInitialized = true;
    }
    
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