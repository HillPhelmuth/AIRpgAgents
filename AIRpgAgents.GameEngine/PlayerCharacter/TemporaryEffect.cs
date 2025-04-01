namespace AIRpgAgents.GameEngine.PlayerCharacter;

public class TemporaryEffect
{
    public string Name { get; set; } = string.Empty;
    public string Duration { get; set; } = "1 round";
    public int Value { get; set; }
    public EffectType Type { get; set; }
}

public enum EffectType
{
    ArmorClass,
    DamageReduction,
    AttackBonus,
    SaveBonus,
    Movement,
    Initiative,
    DamageBonus,
    SkillBonus
}