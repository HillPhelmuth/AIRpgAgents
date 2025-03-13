using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AIRpgAgents.GameEngine.Helpers;

public readonly struct NumericRange
{
    public double Start { get; }
    public double End { get; }
    public static NumericRange Lower => new(0, 5);
    public static NumericRange Mid => new(6, 10);
    public static NumericRange High => new(11, double.MaxValue);

    public NumericRange(double start, double end)
    {
        if (start > end)
            throw new ArgumentException("Start must be less than or equal to End.", nameof(start));
        Start = start;
        End = end;
    }

    public bool Contains(double value) =>
        value >= Start && value <= End;

    public override string ToString() => $"[{Start}, {End}]";
}
public class NumericRangeConverter : JsonConverter<NumericRange>
{
    public override NumericRange Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string rangeName = reader.GetString();
            return rangeName switch
            {
                "Lower" => NumericRange.Lower,
                "Mid" => NumericRange.Mid,
                "High" => NumericRange.High,
                _ => throw new JsonException($"Unknown range: {rangeName}")
            };
        }
        throw new JsonException($"Unexpected token parsing NumericRange. Expected String, got {reader.TokenType}.");
    }

    public override void Write(Utf8JsonWriter writer, NumericRange value, JsonSerializerOptions options)
    {
        if (value.Equals(NumericRange.Lower))
            writer.WriteStringValue("Lower");
        else if (value.Equals(NumericRange.Mid))
            writer.WriteStringValue("Mid");
        else if (value.Equals(NumericRange.High))
            writer.WriteStringValue("High");
        else
            writer.WriteStringValue(value.ToString());
    }
}