using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIRpgAgents.GameEngine.PlayerCharacter;

public class RpgParty
{
    public List<CharacterState> PartyMembers { get; set; } = [];
    public string? Name { get; set; }
}