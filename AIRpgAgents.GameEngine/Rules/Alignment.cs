using System.Text.Json.Serialization;

namespace AIRpgAgents.GameEngine.Rules;

public class AlignmentValue
{
    public MoralAxis MoralAlignment { get; set; }
    public EthicalAxis EthicalAlignment { get; set; }
        
    public AlignmentValue(MoralAxis moral = MoralAxis.Neutral, EthicalAxis ethical = EthicalAxis.Neutral)
    {
        MoralAlignment = moral;
        EthicalAlignment = ethical;
    }
        
    public override string ToString()
    {
        return $"{MoralAlignment}, {EthicalAlignment}";
    }
}
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MoralAxis
{
    Good,
    Neutral,
    Evil
}
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EthicalAxis
{
    Passionate,
    Neutral,
    Reasonable
}