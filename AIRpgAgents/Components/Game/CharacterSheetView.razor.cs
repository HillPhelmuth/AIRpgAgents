using AIRpgAgents.GameEngine.PlayerCharacter;
using AIRpgAgents.GameEngine.Rules;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using AIRpgAgents.Core;
using AIRpgAgents.Core.Models.Events;
using AIRpgAgents.Core.Services;

namespace AIRpgAgents.Components.Game;

public partial class CharacterSheetView
{
    [Parameter]
    public CharacterSheet CharacterSheet { get; set; } = new();
    
    [Parameter]
    public EventCallback<CharacterSheet> CharacterSheetChanged { get; set; }
    [Inject]
    private ICharacterCreationService CharacterCreationService { get; set; } = default!;
    

    protected List<RpgAttribute> AttributeNames => Enum.GetValues<RpgAttribute>().ToList();

    protected string FormatModifier(int modifier) => modifier >= 0 ? $"+{modifier}" : modifier.ToString();

    protected override Task OnInitializedAsync()
    {
        CharacterCreationService.CharacterCreationStateChanged += CharacterCreationStateChanged;
        return base.OnInitializedAsync();
    }

    private void CharacterCreationStateChanged(object sender, CharacterCreationEventArgs e)
    {
        CharacterSheet = e.CharacterCreationState.DraftCharacter;
        Console.WriteLine("Character Sheet Updated");
        InvokeAsync(StateHasChanged);
    }

    //protected int GetAttributeScore(string attributeName)
    //{
    //    return CharacterSheet.AttributeSet?.GetAttributeValue(attributeName) ?? 0;
    //}

    //protected void SetAttributeScore(string attributeName, int value)
    //{
    //    if (CharacterSheet.AttributeSet != null)
    //    {
    //        CharacterSheet.AttributeSet.SetAttributeValue(attributeName, value);
    //        CharacterSheetChanged.InvokeAsync(CharacterSheet);
    //    }
    //}

    protected int GetAttributeModifier(RpgAttribute attributeName)
    {
        return CharacterSheet.GetAttributeModifier(attributeName);
    }

    private int _savingThrow;
    protected int GetSavingThrow(RpgAttribute attributeName)
    {
        _savingThrow = GetAttributeModifier(attributeName);
        return _savingThrow; // Add proficiency if applicable
    }

    protected string GetWeaponProperties(int index)
    {
        if (CharacterSheet.Weapons == null || index >= CharacterSheet.Weapons.Count)
            return string.Empty;
        
        return string.Join(", ", CharacterSheet.Weapons[index].Properties);
    }

    protected void SetWeaponProperties(int index, string value)
    {
        if (CharacterSheet.Weapons != null && index < CharacterSheet.Weapons.Count)
        {
            CharacterSheet.Weapons[index].Properties = value.Split(',', StringSplitOptions.TrimEntries).ToList();
            CharacterSheetChanged.InvokeAsync(CharacterSheet);
        }
    }

    protected RpgAttribute SpellcastingAbility
    {
        get => CharacterSheet.Spellcasting?.SpellcastingAbility ?? RpgAttribute.Wits;
        set
        {
            if (CharacterSheet.Spellcasting == null) return;
            CharacterSheet.Spellcasting.SpellcastingAbility = value;
            CharacterSheetChanged.InvokeAsync(CharacterSheet);
        }
    }

    protected int SpellSaveDC
    {
        get => 8 + GetAttributeModifier(SpellcastingAbility) + CharacterSheet.Level / 2;
        set => _ = value;
    }

    protected int SpellAttackModifier
    {
        get => GetAttributeModifier(SpellcastingAbility) + CharacterSheet.Level / 2;
        set => _ = value;
    }

    protected string GetKnownSpellsByLevel(Band band)
    {
        var spells = CharacterSheet.Spellcasting?.Spells?.Where(s => s.Band == band);
        if (spells?.Any() == true)
        {
            return string.Join(", ", spells.Select(s => s.Name));
        }

        
        return spells != null ? string.Join(", ", spells.Select(s => s.Name)) : string.Empty;
    }

    // Keep the int version for cantrips (level 0)
    protected string GetKnownSpellsByLevel(int level)
    {
        var spells = CharacterSheet.Spellcasting?.Spells?.Where(s => s.Level == level);
        return spells != null ? string.Join(", ", spells.Select(s => s.Name)) : string.Empty;
    }

    protected string PrimaryArmorName
    {
        get => CharacterSheet.Armor?.FirstOrDefault(a => !a.IsShield)?.Name ?? string.Empty;
        set
        {
            var armor = CharacterSheet.Armor?.FirstOrDefault(a => !a.IsShield);
            if (armor != null)
            {
                armor.Name = value;
                CharacterSheetChanged.InvokeAsync(CharacterSheet);
            }
        }
    }

    protected int PrimaryArmorBonus => CharacterSheet.Armor?.FirstOrDefault(a => !a.IsShield)?.ArmorBonus ?? 0;

    protected string ShieldName
    {
        get => CharacterSheet.Armor?.FirstOrDefault(a => a.IsShield)?.Name ?? string.Empty;
        set
        {
            var shield = CharacterSheet.Armor?.FirstOrDefault(a => a.IsShield);
            if (shield != null)
            {
                shield.Name = value;
                CharacterSheetChanged.InvokeAsync(CharacterSheet);
            }
        }
    }

    protected int ShieldBonus => CharacterSheet.Armor?.FirstOrDefault(a => a.IsShield)?.ArmorBonus ?? 0;

    // Enum lists for dropdowns
    protected IEnumerable<MoralAxis> MoralAlignments => Enum.GetValues<MoralAxis>();
    protected IEnumerable<EthicalAxis> EthicalAlignments => Enum.GetValues<EthicalAxis>();
    protected IEnumerable<WeaponType> WeaponTypes => Enum.GetValues<WeaponType>();
    protected IEnumerable<RaceType> RaceTypes => Enum.GetValues<RaceType>();
    protected IEnumerable<ClassType> ClassTypes => Enum.GetValues<ClassType>();
    protected IEnumerable<MagicTradition> MagicTraditions => Enum.GetValues<MagicTradition>();

    // Add this method to the CharacterSheetView partial class
    protected string GetBandDescription(Band band)
    {
        return band switch
        {
            Band.Lower => "Levels 1-5",
            Band.Mid => "Levels 6-10",
            Band.High => "Levels 11-15",
            _ => string.Empty
        };
    }
}

