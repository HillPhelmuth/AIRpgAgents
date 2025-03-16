using Microsoft.AspNetCore.Components;
using System;
using System.Linq;
using AIRpgAgents.GameEngine.World;

namespace AIRpgAgents.Components.Game;

public partial class WorldDetails
{
    [Parameter] 
    public WorldState? World { get; set; }
    [Parameter]
    public string CustomStyle { get; set; } = "";

    private Location? SelectedLocation { get; set; }
    private NPC? SelectedNPC { get; set; }
    private Quest? SelectedQuest { get; set; }
    private WorldEvent? SelectedEvent { get; set; }
    
    private bool ShowWorldDescription { get; set; } = true;
    private bool ShowLocations { get; set; } = true;
    private bool ShowNPCs { get; set; } = true;
    private bool ShowQuests { get; set; } = true;
    private bool ShowEvents { get; set; } = true;
    
    private void ToggleWorldDescription() => ShowWorldDescription = !ShowWorldDescription;
    private void ToggleLocations() => ShowLocations = !ShowLocations;
    private void ToggleNPCs() => ShowNPCs = !ShowNPCs;
    private void ToggleQuests() => ShowQuests = !ShowQuests;
    private void ToggleEvents() => ShowEvents = !ShowEvents;
    
    private void SelectLocation(Location location)
    {
        if (SelectedLocation?.Id == location.Id)
            SelectedLocation = null;
        else
            SelectedLocation = location;
    }
    
    private void SelectNPC(NPC npc)
    {
        if (SelectedNPC?.Id == npc.Id)
            SelectedNPC = null;
        else
            SelectedNPC = npc;
    }
    
    private void SelectQuest(Quest quest)
    {
        if (SelectedQuest?.Id == quest.Id)
            SelectedQuest = null;
        else
            SelectedQuest = quest;
    }
    
    private void SelectEvent(WorldEvent worldEvent)
    {
        if (SelectedEvent?.Name == worldEvent.Name)
            SelectedEvent = null;
        else
            SelectedEvent = worldEvent;
    }
    
    private string GetRelationshipClass(int value)
    {
        return value switch
        {
            < -50 => "hostile",
            < 0 => "negative",
            < 50 => "neutral",
            < 80 => "positive",
            _ => "friendly"
        };
    }
    
    private string FormatDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return "<p>No description available</p>";
        
        // Convert line breaks to proper HTML paragraphs
        return string.Join("", description
            .Split(["\r\n", "\r", "\n"], StringSplitOptions.RemoveEmptyEntries)
            .Select(p => $"<p>{p}</p>"));
    }
}
