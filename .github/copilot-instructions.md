I'm creating an Agentic AI Rpg application with the following agentic components. You'll help build out the c# models and services required for the application to accomodate these components.
All new c# should be added to one of:
- AIRpgAgents.Core\Models
- AIRpgAgents.Core\Services
- AIRpgAgents.Core\Plugins

**I. Core Agents (Essential Foundation):**

1.  **Game Master Agent:**
    *   Orchestrates the game, drives narrative, manages context, adjudicates actions, and prompts other agents. The central "brain" of the game.

2.  **Player/Character Manager Agent:**
    *   Manages player character(s): stats, skills, inventory, progression, and interprets player input.

3.  **RPG Rules Agent:**
    *   Defines and enforces game rules: combat, skills, magic, etc. Provides action resolution.

**II. World and Immersion Agents (Enhancing the Game World):**

4.  **World State Manager Agent:**
    *   Tracks the *state* of the game world: locations, NPCs, objects, events. The single source of truth for world information.

5.  **Lore/Knowledge Agent:**
    *   Repository of lore, history, and background information. Provides context and detail to other agents.

6.  **NPC Agent (Dynamic):**
    *   Manages individual NPCs: personality, goals, behavior, interactions. Can be dynamically generated via meta-prompting.


**III. Gameplay Mechanics Agents (Enhancing Specific Systems):**

8.  **Combat Resolution Agent (Specialized):**
    *   Handles all aspects of combat: tactical AI, dynamic encounters, detailed simulation of combat rules.

9.  **Quest Generator Agent:**
    *   Generates new quests: based on templates or dynamically based on world state and player actions.

10. **Economy Agent:**
    *   Manages the game's economy: prices, trade, resource availability.

11. **Dialogue Manager Agent:**
    *   Manages conversations, relationship tracking, lie detection, and incorporates non-combat skill checks into dialogue.

**IV. Utility and Meta-Agents (We'll decide later how much of this should be done programatically and/or whether agents are required here and to what degree):**

12. **Error Handling/Recovery Agent:**
    *   Monitors for errors and attempts to resolve them or provide graceful fallbacks.

13. **User Interface (UI) Agent (Optional but useful):**
    *   Intermediary between LLM agents and the user interface (text, graphical, etc.). Translates input/output.

14. **Memory Management Agent:**
    *   Manages the memory (context) of the other agents, especially the LLMs. Crucial for long-term consistency.

**Key Considerations and Relationships:**

*   **Dynamic NPC Generation:** The Game Master Agent, using meta-prompting, creates temporary NPC Agent instances as needed, leveraging the World State Manager, Lore/Knowledge Agent, and RPG Rules Agent to inform the generation.
*   **Agent Hierarchy:** The Game Master Agent acts as the top-level coordinator, delegating tasks to other specialized agents.
*   **Data Flow:** The World State Manager is the central hub for persistent world data. It will have access to tools that interact with the application data layer. Other agents query and update it.
*   **Modularity:** Each agent has a clearly defined role, making the system easier to develop, debug, and extend.
*   **Communication:** Agents communicate primarily through well-defined requests and responses (e.g., the Game Master requesting a combat resolution from the Combat Resolution Agent) either using the Multi-agent framework, or by using an "agent as tool call" technique, depending on the requirements.
*  **Prioritization:** While all of the agents are beneficial, We should build your agents in order of importance. Core Agents are a must. World State Manager and NPC Agent should also probably be a high-priority.
