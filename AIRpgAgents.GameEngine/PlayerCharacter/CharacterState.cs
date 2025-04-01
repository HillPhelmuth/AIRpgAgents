using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIRpgAgents.GameEngine.Rules;
using AIRpgAgents.GameEngine.World;

namespace AIRpgAgents.GameEngine.PlayerCharacter;

public class CharacterState(CharacterSheet characterSheet)
{
    private Weapon? _equippedWeapon;
    private Armor? _equippedArmor;

    // Character sheet containing basic information and stats
    public CharacterSheet CharacterSheet { get; set; } = characterSheet;
    public string Id => CharacterSheet.Id;

    // Campaign tracking
    public List<string> ActiveCampaignIds { get; set; } = [];
    public string? CurrentCampaignId { get; set; }
    public WorldState? CurrentCampaignState { get; set; }

    // Rest and exhaustion tracking
    public ExhaustionLevel ExhaustionLevel { get; set; } = ExhaustionLevel.None;
    public DateTime LastLongRest { get; set; }
    public DateTime LastShortRest { get; set; }
    public int LongRestsRemaining { get; set; } = 1; // Typically 1 long rest per day
    public int ShortRestsUsed { get; set; } = 0;
    public int ShortRestsAvailable { get; set; } = 2; // Typically 2 short rests per day

    // Types of damage recently taken for tracking vulnerabilities/resistances
    public Dictionary<string, int> DamageByType { get; set; } = new();
    
    // Action history
    public List<CharacterAction> RecentActions { get; set; } = [];
    public int MaxActionHistory { get; set; } = 20;
    
    // Temporary hit points are managed separately from CharacterSheet
    public int TemporaryHitPoints { get; set; }
    
    // Temporary effects
    public List<TemporaryEffect> TemporaryEffects { get; set; } = [];
    
    // Death save tracking
    public bool IsUnconscious { get; set; }
    public bool IsStabilized { get; set; }
    public int DeathSavesSuccesses { get; set; }
    public int DeathSavesFailures { get; set; }

    public Weapon EquippedWeapon
    {
        get
        {
            return _equippedWeapon ??= CharacterSheet.Weapons.FirstOrDefault(x => x.IsEquipped) 
                                       ?? CharacterSheet.Weapons.FirstOrDefault() 
                                       ?? new Weapon() { DamageFormula = "1D4", Name = "Fisticuffs", DamageType = DamageType.Bludgeoning };
        }
        set => _equippedWeapon = value;
    }

    public Armor? EquippedArmor
    {
        get
        {
            return _equippedArmor ?? CharacterSheet.Armor.Find(x => x.IsEquipped);
        }
        set => _equippedArmor = value;
    }

    // Constructors
    public CharacterState() : this(new CharacterSheet())
    {
    }

    // Properties to access CharacterSheet data directly
    public string CharacterId => CharacterSheet.Id;
    public string CharacterName
    {
        get => CharacterSheet.CharacterName;
        set => CharacterSheet.CharacterName = value;
    }
    public int CurrentHitPoints
    {
        get => CharacterSheet.CurrentHP;
        set => CharacterSheet.CurrentHP = value;
    }
    public int MaxHitPoints
    {
        get => CharacterSheet.MaxHP;
        set => CharacterSheet.MaxHP = value;
    }

    // Methods for managing action history
    public void AddAction(string description, string? targetId = null)
    {
        var action = new CharacterAction
        {
            Timestamp = DateTime.UtcNow,
            Description = description,
            TargetId = targetId,
            CampaignId = CurrentCampaignId
        };
        
        RecentActions.Add(action);
        
        // Keep history at reasonable size
        if (RecentActions.Count > MaxActionHistory)
        {
            RecentActions = RecentActions.OrderByDescending(a => a.Timestamp)
                .Take(MaxActionHistory)
                .ToList();
        }
    }

    // Method for recording damage taken
    public void TakeDamage(int amount, string damageType)
    {
        // Record damage by type
        if (!DamageByType.TryAdd(damageType, amount))
        {
            DamageByType[damageType] += amount;
        }

        // Apply damage to hit points
        TemporaryHitPoints -= amount;
        
        // Overflow damage applies to regular hit points
        if (TemporaryHitPoints < 0)
        {
            CurrentHitPoints += TemporaryHitPoints; // Add negative value
            TemporaryHitPoints = 0;
        }

        // Check for unconsciousness
        if (CurrentHitPoints <= 0)
        {
            CurrentHitPoints = 0;
            IsUnconscious = true;
        }

        // Record action
        AddAction($"Took {amount} {damageType} damage");
    }

    // Get summary of recent actions
    public string GetRecentActionsSummary(int count = 5)
    {
        var recentActions = RecentActions
            .OrderByDescending(a => a.Timestamp)
            .Take(count);
            
        var sb = new StringBuilder();
        foreach (var action in recentActions)
        {
            sb.AppendLine($"[{action.Timestamp:HH:mm:ss}] {action.Description}");
        }
        
        return sb.ToString().TrimEnd();
    }
}