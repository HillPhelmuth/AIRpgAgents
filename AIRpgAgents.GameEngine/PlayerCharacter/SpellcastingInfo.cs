using AIRpgAgents.GameEngine.Rules;

namespace AIRpgAgents.GameEngine.PlayerCharacter;

public class SpellcastingInfo
{
    public RpgAttribute SpellcastingAbility { get; set; } // "Wits" or "Presence"
    public int SpellSaveDC { get; set; }
    public int SpellAttackModifier { get; set; }
    private CharacterClass? _characterClass;
    private int _level;
    public List<SpellSlot> SpellSlots
    {
        get
        {
            if (_characterClass == null) return [];
        
            var slots = new List<SpellSlot>();
        
            // Band.Lower slots (levels 1-5)
            var lowerBandCount = _characterClass.Type switch
            {
                ClassType.Cleric or ClassType.Wizard => 3,
                ClassType.Paladin or ClassType.WarMage => 2,
                _ => 0
            };
        
            // Add additional Lower band slots for levels 2-5
            if (_level is >= 2 and <= 5)
            {
                // Add one slot per level from 2-5
                lowerBandCount += (_level - 1);
            }
        
            if (lowerBandCount > 0)
            {
                slots.Add(new SpellSlot(Band.Lower) { Count = lowerBandCount });
            }
        
            // Band.Mid slots (levels 6-10)
            if (_level >= 6)
            {
                // Starting count at level 6 based on class
                var midBandCount = _characterClass.Type switch
                {
                    ClassType.Cleric or ClassType.Wizard => 3,
                    ClassType.Paladin or ClassType.WarMage => 2,
                    _ => 0
                };
            
                // Add one slot per level from 7-10
                if (_level is > 6 and <= 10)
                {
                    midBandCount += (_level - 6);
                }
                // Cap at max slots for levels beyond 10
                else if (_level > 10)
                {
                    midBandCount += 4; // Add 4 additional slots at higher levels
                }
            
                slots.Add(new SpellSlot(Band.Mid) { Count = midBandCount });
            }
        
            // Band.High slots (levels 11+)
            if (_level >= 11)
            {
                // Starting count at level 11 based on class
                var highBandCount = _characterClass.Type switch
                {
                    ClassType.Cleric or ClassType.Wizard => 3,
                    ClassType.Paladin or ClassType.WarMage => 2,
                    _ => 0
                };
                
                // Add one slot for each level beyond 11
                highBandCount += (_level - 11);
                
                slots.Add(new SpellSlot(Band.High) { Count = highBandCount });
            }

            return slots;
        }
    } 
    public List<Spell> Spells { get; set; }
    public MagicTradition MagicTradition { get; set; }
    public SpellcastingInfo(CharacterClass characterClass, int level)
    {
        _characterClass = characterClass;
        _level = level;
    }
    public SpellcastingInfo()
    {
      
    }
}

public class SpellSlot
{
    public SpellSlot(Band level)
    {
        Level = level;
    }
    public Band Level { get; set; }
    public int Count { get; set; }
}