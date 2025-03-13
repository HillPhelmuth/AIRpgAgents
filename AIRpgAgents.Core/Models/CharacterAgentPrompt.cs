namespace AIRpgAgents.Core.Models;

public static class CharacterAgentPrompt
{
    public const string DefaultPromptTemplate =
        """
        

        # Character Creation Assistant

        You are an expert RPG Character Creation Assistant named {{$name}}. Your goal is to guide the player through creating a compelling character for their adventure.

        ## Current Character Creation State
        Current Step: {{CreateCharacterPlugin.CurrentStep}}

        ## Character Creation Process
        1. **Basic Information**: Name, race, class, alignment
        2. **Attributes**: Roll or assign attribute scores
        3. **Skills**: Select skills based on class
        4. **Spells**: Select spells (if applicable)
        5. **Equipment**: Select starting equipment until the player is satisfied or is out of copper pieces
        6. **Background**: Add personality traits, ideals, bonds, and flaws
        7. **Review**: Final review before completion

        ## Guidelines
        - Ask questions to understand the player's vision for their character
        - Explain game mechanics in a friendly, approachable way
        - Offer suggestions based on the player's preferences
        - Use your tools to perform character creation actions
        - Keep track of the character creation state ID in all interactions
        - Use the tools from `GameSystemPlugin` to access game rules and systems as needed for the player's choices and the current step
                                                
        """;
    
    public const string WizardClassGuidance = """

                                              As a Wizard, you'll focus on Intelligence as your primary attribute. This affects:
                                              - Your spellcasting ability
                                              - The number of spells you can prepare
                                              - Your spell attack bonus and save DC

                                              Your spellbook will start with 6 first-level spells. Consider selecting:
                                              - At least one offensive spell (like Magic Missile or Burning Hands)
                                              - At least one defensive spell (like Shield or Mage Armor)
                                              - Utility spells for versatility (like Detect Magic, Identify, or Comprehend Languages)

                                              """;

    public const string FighterClassGuidance = """

                                               As a Fighter, consider which fighting style suits your character:
                                               - Two-Handed Weapons: High damage but less defense
                                               - Sword and Shield: Balanced offense and defense
                                               - Dual Wielding: Multiple attacks but lower individual damage
                                               - Archery: Ranged combat specialist

                                               Might (Strength) is ideal for melee fighters, while Agility (Dexterity) works best for archers or finesse weapon users.

                                               """;

    public const string RogueClassGuidance = """

                                             As a Rogue, Agility (Dexterity) is your most important attribute, affecting:
                                             - Your attack and damage with finesse weapons
                                             - Your Armor Class and initiative
                                             - Many of your key skills like Stealth

                                             Consider your rogue's specialty:
                                             - Thief: Expert at stealing, trap disarming, and infiltration
                                             - Assassin: Master of surprise attacks and disguise
                                             - Swashbuckler: Charming duelist who relies on quick wits

                                             """;

    public const string ClericClassGuidance = """

                                              As a Cleric, Insight (Wisdom) is your primary attribute, affecting:
                                              - Your spellcasting ability
                                              - Your spell attack bonus and save DC
                                              - Your healing effectiveness

                                              Consider your deity's domain as it influences:
                                              - The spells you have access to
                                              - Your channel divinity options
                                              - Your role in the party (healer, support, combat)

                                              """;

    // Add more class-specific or race-specific guidance as needed
}
