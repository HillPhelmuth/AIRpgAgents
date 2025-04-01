using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using AIRpgAgents.GameEngine.Enums;
using AIRpgAgents.GameEngine.Extensions;
using AIRpgAgents.GameEngine.Monsters;
using AIRpgAgents.GameEngine.PlayerCharacter;
using Json.More;
using Newtonsoft.Json.Converters;

namespace AIRpgAgents.Core.Models;

/// <summary>
/// Represents a combat encounter between players and monsters
/// </summary>
public class CombatEncounter
{
    /// <summary>
    /// Unique identifier for the combat encounter
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// The player party involved in the encounter
    /// </summary>
    public required RpgParty PlayerParty { get; init; }
    
    /// <summary>
    /// The monster encounter involved in the combat
    /// </summary>
    public required MonsterEncounter MonsterEncounter { get; init; }

    /// <summary>
    /// Current round of combat (1-based)
    /// </summary>
    public int CurrentRound { get; set; } = 0;
    
    /// <summary>
    /// Indicates if the combat is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Tracks the initiative order of all combatants
    /// </summary>
    public List<Combatant> InitiativeOrder { get; set; } = [];
    
    /// <summary>
    /// The current combatant who is taking their turn
    /// </summary>
    public Combatant? ActiveCombatant { get; set; }
    
    /// <summary>
    /// Narrative description of the combat environment
    /// </summary>
    public string? EnvironmentDescription { get; set; }
    
    /// <summary>
    /// Combat log containing history of actions and events
    /// </summary>
    public CombatLogs CombatLog { get; set; } = new();
    
    /// <summary>
    /// The current status of the combat encounter
    /// </summary>
    public CombatStatus Status { get; set; } = CombatStatus.NotStarted;
    
    /// <summary>
    /// Gets a summary of the current combat state
    /// </summary>
    public string GetCombatSummary()
    {
        return
            $"""
            Combat Round: {CurrentRound}
            Status: {Status}
            Current Turn: {ActiveCombatant?.Name ?? "None"}
            Party HP: {GetPartyHealthSummary()}
            Monsters: {GetMonstersSummary()}
            """;
    }

    public string GetCombatantsInfo()
    {
        var players = PlayerParty.PartyMembers.Select(p => p.CharacterSheet.PrimaryDetailsMarkdown());
        var monsters = MonsterEncounter.Monsters.Select(m => m.ToMarkdown());
        return $"""
            ### Combatants
            
            **Players:**
            {string.Join("\n", players)}
            **Monsters:**
            {string.Join("\n", monsters)}
            """;
    }
    private string GetPartyHealthSummary()
    {
        var summary = new List<string>();
        foreach (var member in PlayerParty.PartyMembers)
        {
            summary.Add($"{member.CharacterName}: {member.CurrentHitPoints}/{member.MaxHitPoints} HP");
        }
        return string.Join(", ", summary);
    }
    
    private string GetMonstersSummary()
    {
        var summary = new List<string>();
        foreach (var monster in MonsterEncounter.Monsters)
        {
            summary.Add($"{monster.Name}: {monster.CurrentHP}/{monster.MaxHP} HP");
        }
        return string.Join(", ", summary);
    }

    public string ToJson()
    {
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions() { WriteIndented = true });
        Console.WriteLine(
            $"\n-----------------------------------------------\n{json}\n-----------------------------------------------\n");
        return json;
    }
}

/// <summary>
/// Represents the status of a combat encounter
/// </summary>
[CosmosConverter(typeof(StringEnumConverter))]
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CombatStatus
{
    NotStarted,
    InProgress,
    PlayerVictory,
    PlayerDefeat,
    Fled,
    Resolved
}

public class CombatLogs : IEnumerable<CombatLog>
{
    public List<CombatLog> Logs { get; set; } = new();
    public void Add(string log)
    {
        Logs.Add(new CombatLog(log, DateTime.Now));
    }

    public IEnumerator<CombatLog> GetEnumerator()
    {
        // Implement this method
        return Logs.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public record CombatLog(string Log, DateTime Timestamp)
{
    public override string ToString()
    {
        return $"{Log} --- {Timestamp:g}";
    }
}