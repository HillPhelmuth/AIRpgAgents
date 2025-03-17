using AIRpgAgents.GameEngine.Enums;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using AIRpgAgents.GameEngine.Rules;
using AIRpgAgents.GameEngine.World;
using SkPluginComponents.Models;

namespace AIRpgAgents.GameEngine.Monsters;

public class RpgMonster
{
    // Basic identification
    public string Id => Name ?? Guid.NewGuid().ToString();
    public string? Name { get; set; }
    public string? Description { get; set; }

    // Challenge information
    public int ChallengeRating { get; set; } = 1;
    public string Type { get; set; } = "Humanoid";

    // Core attributes as simple properties
    public int Might { get; set; }
    public int Agility { get; set; }
    public int Wits { get; set; }
    public int Vitality { get; set; }
    public int Presence { get; set; }

    // Combat stats
    public int CurrentHP { get; set; }
    public int MaxHP { get; set; }
    public int ArmorClass { get; set; } = 10;
    public int Initiative => Wits + Agility;
    public int Speed { get; set; } = 30;

    // Display information
    public string? ImageUrl { get; set; }

    // Combat abilities
    public List<MonsterAttack> Attacks { get; set; } = [];

    // State tracking
    public NPCDisposition Disposition { get; set; } = NPCDisposition.Hostile;

    // Factory method to create a simple monster
    public static RpgMonster CreateSimpleMonster(
        string name,
        string description,
        int hp,
        int armorClass,
        string type = "MonsterType.Humanoid",
        int challengeRating = 1)
    {
        var monster = new RpgMonster
        {
            Name = name,
            Description = description,
            MaxHP = hp,
            CurrentHP = hp,
            ArmorClass = armorClass,
            Type = type,
            ChallengeRating = challengeRating
        };

        // Add a default attack
        monster.Attacks.Add(new MonsterAttack
        {
            Name = "Claw",
            AttackBonus = 3,
            DamageDie = DieType.D6,
            DamageBonus = 1,
            DamageType = CombatSystem.DamageType.Slashing,
            Range = 5
        });

        return monster;
    }
}