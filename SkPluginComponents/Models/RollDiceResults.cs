namespace SkPluginComponents.Models;

public class RollDiceResults
{
    public RollDiceResults(bool success, RollDiceParameters parameters)
    {
        IsSuccess = success;
        Parameters = parameters;
    }
    public bool IsSuccess { get; private set; }
    public RollDiceParameters Parameters { get; }

    public static RollDiceResults Empty(bool success)
    {
        return new RollDiceResults(success, new RollDiceParameters());
    }
}
