using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Microsoft.SemanticKernel;
using AIRpgAgents.GameEngine.PlayerCharacter;
using AIRpgAgents.GameEngine.World;

namespace AIRpgAgents.Core.Plugins.NativePlugins;

public class CharacterManagerPlugin
{
    private readonly CharacterState _characterState;

    public CharacterManagerPlugin(CharacterState characterState)
    {
        _characterState = characterState;
    }

    [KernelFunction, Description("Apply damage to the character")]
    public string TakeDamage(
        [Description("Amount of damage to apply")] int amount,
        [Description("Type of damage being dealt (e.g. slashing, fire, etc)")] string damageType)
    {
        _characterState.TakeDamage(amount, damageType);
        return $"Applied {amount} {damageType} damage. Current HP: {_characterState.CurrentHitPoints}";
    }

    [KernelFunction, Description("Heal the character")]
    public string Heal(
        [Description("Amount of healing to apply")] int amount)
    {
        int oldHp = _characterState.CurrentHitPoints;
        _characterState.CurrentHitPoints = Math.Min(_characterState.CurrentHitPoints + amount, _characterState.MaxHitPoints);
        int healedFor = _characterState.CurrentHitPoints - oldHp;
        
        _characterState.AddAction($"Healed for {healedFor} hit points");
        return $"Healed for {healedFor}. Current HP: {_characterState.CurrentHitPoints}/{_characterState.MaxHitPoints}";
    }

    [KernelFunction, Description("Take a short rest")]
    public string TakeShortRest()
    {
        if (_characterState.ShortRestsUsed >= _characterState.ShortRestsAvailable)
        {
            return "No short rests remaining today.";
        }

        _characterState.LastShortRest = DateTime.UtcNow;
        _characterState.ShortRestsUsed++;
        _characterState.AddAction("Took a short rest");
        
        return $"Short rest taken. {_characterState.ShortRestsAvailable - _characterState.ShortRestsUsed} remaining today.";
    }

    [KernelFunction, Description("Take a long rest")]
    public string TakeLongRest()
    {
        if (_characterState.LongRestsRemaining <= 0)
        {
            return "No long rests remaining today.";
        }

        _characterState.LastLongRest = DateTime.UtcNow;
        _characterState.LongRestsRemaining--;
        _characterState.ShortRestsUsed = 0;
        _characterState.CurrentHitPoints = _characterState.MaxHitPoints;
        _characterState.AddAction("Took a long rest");
        
        return "Long rest completed. HP restored to maximum.";
    }

    [KernelFunction, Description("Record a character action")]
    public string RecordAction(
        [Description("Description of the action")] string description,
        [Description("Optional ID of the target of the action")] string? targetId = null)
    {
        _characterState.AddAction(description, targetId);
        return "Action recorded.";
    }

    [KernelFunction, Description("Get a summary of recent character actions")]
    public string GetRecentActions(
        [Description("Number of recent actions to return")] int count = 5)
    {
        return _characterState.GetRecentActionsSummary(count);
    }

    [KernelFunction, Description("Set the character's exhaustion level")]
    public string SetExhaustionLevel(
        [Description("New exhaustion level (0-6)")] int level)
    {
        if (level < 0 || level > 6)
        {
            return "Invalid exhaustion level. Must be between 0 and 6.";
        }

        _characterState.ExhaustionLevel = (ExhaustionLevel)level;
        _characterState.AddAction($"Exhaustion level changed to {_characterState.ExhaustionLevel}");
        
        return $"Exhaustion level set to {_characterState.ExhaustionLevel}";
    }

    [KernelFunction, Description("Set the current campaign context")]
    public string SetCampaignContext(
        [Description("ID of the campaign")] string campaignId,
        [Description("Current state of the RPG world")] WorldState worldState)
    {
        _characterState.CurrentCampaignId = campaignId;
        _characterState.CurrentCampaignState = worldState;
        
        if (!_characterState.ActiveCampaignIds.Contains(campaignId))
        {
            _characterState.ActiveCampaignIds.Add(campaignId);
        }

        _characterState.AddAction($"Entered campaign {campaignId}");
        return $"Campaign context set to {campaignId}";
    }
}