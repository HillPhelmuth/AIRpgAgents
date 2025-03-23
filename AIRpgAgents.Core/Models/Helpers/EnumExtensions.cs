namespace AIRpgAgents.Core.Models.Helpers;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = (System.ComponentModel.DescriptionAttribute)field
            .GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false).FirstOrDefault();
        return attribute?.Description ?? value.ToString();
    }

    public static CharacterCreationStep NextStep(this CharacterCreationStep currentStep)
    {
        // Get the next step based on enum integer value
        var nextStep = currentStep + 1;
        if (!Enum.IsDefined(typeof(CharacterCreationStep), nextStep))
        {
            nextStep = CharacterCreationStep.Completed;
        }
        return nextStep;
    }
}

    