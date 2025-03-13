using AIRpgAgents.GameEngine.Rules;

namespace AIRpgAgents.GameEngine.PlayerCharacter;

/// <summary>
/// Represents a complete set of character attributes.
/// </summary>
public class AttributeSet
{
    private readonly Dictionary<RpgAttribute, int> _attributeValues;
        
    public AttributeSet()
    {
        _attributeValues = new Dictionary<RpgAttribute, int>();
            
        // Initialize all attributes with the default value
        foreach (RpgAttribute attribute in Enum.GetValues(typeof(RpgAttribute)))
        {
            _attributeValues[attribute] = AttributeSystem.BaseAttributeValue;
        }
    }
    public AttributeSet(Dictionary<RpgAttribute, int> attributeValues)
    {
        _attributeValues = new Dictionary<RpgAttribute, int>();

        // Initialize all attributes with the default value
        foreach (var attribute in Enum.GetValues<RpgAttribute>())
        {
            _attributeValues[attribute] = attributeValues[attribute];
        }

        //// Assign values from the dictionary
        //foreach (var kvp in attributeValues)
        //{
        //    _attributeValues[kvp.Key] = kvp.Value;
        //}
    }

    /// <summary>
    /// Gets or sets the value of an attribute (3-20).
    /// </summary>
    public int this[RpgAttribute attribute]
    {
        get => _attributeValues[attribute];
        set
        {
            if (value < AttributeSystem.MinAttributeValue)
                value = AttributeSystem.MinAttributeValue;
            if (value > AttributeSystem.AbsoluteMaxAttributeValue)
                value = AttributeSystem.AbsoluteMaxAttributeValue;
                    
            _attributeValues[attribute] = value;
        }
    }
        
    /// <summary>
    /// Gets the modifier for an attribute.
    /// </summary>
    public int GetModifier(RpgAttribute attribute)
    {
        return AttributeSystem.CalculateModifier(_attributeValues[attribute]);
    }
        
    /// <summary>
    /// Sets attributes using the standard array.
    /// </summary>
    public void ApplyStandardArray(Dictionary<RpgAttribute, int> attributeAssignments)
    {
        var standardArray = AttributeSystem.GetStandardArray();
            
        foreach (var kvp in attributeAssignments)
        {
            if (Array.IndexOf(standardArray, kvp.Value) >= 0)
            {
                _attributeValues[kvp.Key] = kvp.Value;
            }
        }
    }
        
    /// <summary>
    /// Rolls for random attributes using the 4d6 drop lowest method.
    /// </summary>
    public void RollAttributes()
    {
        List<int> rolls = new List<int>();
            
        // Generate 5 rolls
        for (int i = 0; i < 5; i++)
        {
            rolls.Add(AttributeSystem.RollAttribute());
        }
            
        // Sort rolls in descending order
        rolls.Sort();
        rolls.Reverse();
            
        // Assign to attributes (this is a simple assignment - players might want to customize)
        int index = 0;
        foreach (RpgAttribute attribute in Enum.GetValues(typeof(RpgAttribute)))
        {
            _attributeValues[attribute] = rolls[index++];
            if (index >= rolls.Count) break;
        }
    }
}