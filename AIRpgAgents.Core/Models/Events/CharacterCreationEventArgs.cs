using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIRpgAgents.Core.Models.Events;

public class CharacterCreationEventArgs(string name, CharacterCreationState characterCreationState) : EventArgs
{
    public string Name { get; set; } = name;
    public CharacterCreationState CharacterCreationState { get; set; } = characterCreationState;
}
// Create delegate for the event
public delegate void CharacterCreationEventHandler(object sender, CharacterCreationEventArgs e);