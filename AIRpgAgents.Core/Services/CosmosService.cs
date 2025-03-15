using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIRpgAgents.Core.Agents;
using AIRpgAgents.Core.Models;
using AIRpgAgents.GameEngine.PlayerCharacter;
using AIRpgAgents.GameEngine.World;
using Microsoft.Azure.Cosmos;

namespace AIRpgAgents.Core.Services;

public class CosmosService(CosmosClient cosmosClient)
{
    private const string DatabaseId = "AIRpgAgentsDb";
    private Container WorldStateContainer => cosmosClient.GetContainer(DatabaseId, "Worlds");
    private Container PlayerContainer => cosmosClient.GetContainer(DatabaseId, "Player");
    private Container AgentContainer => cosmosClient.GetContainer(DatabaseId, "Agents");

    public async Task<Player> GetOrCreatePlayerAsync(string playerId)
    {
        try
        {
            var player = await PlayerContainer.ReadItemAsync<Player>(playerId, new PartitionKey(playerId));
            return player.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return new Player() { Name = playerId };
        }
    }
    public async Task<CharacterSheet?> GetCharacterAsync(string playerId, string characterId)
    {
        try
        {
            var player = await GetOrCreatePlayerAsync(playerId);
            return player?.Characters.FirstOrDefault(c => c.CharacterId == characterId)?.CharacterSheet;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }
    public async Task<Player> SavePlayerAsync(Player player)
    {
        await PlayerContainer.UpsertItemAsync(player, new PartitionKey(player.Id));
        return player;
    }
    public async Task SaveCharacterAsync(string playerId, CharacterState character)
    {
        var player = await GetOrCreatePlayerAsync(playerId);
        if (player == null)
        {
            player = new Player(){Name = playerId};
        }
        var existingCharacter = player.Characters.FirstOrDefault(c => c.CharacterId == character.CharacterId);
        if (existingCharacter != null)
        {
            player.Characters.Remove(existingCharacter);
        }
        player.Characters.Add(character);
        await PlayerContainer.UpsertItemAsync(player, new PartitionKey(player.Id));
    }
    public async Task SaveWorldStateAsync(WorldState worldState)
    {
        await WorldStateContainer.UpsertItemAsync(worldState, new PartitionKey(worldState.id));
    }

    public async Task SaveAgent(AgentData agent)
    {
        await AgentContainer.UpsertItemAsync(agent, new PartitionKey(agent.Name));
    }

    public async Task<AgentData?> GetAgent(string agentId)
    {
        try
        {
            return await AgentContainer.ReadItemAsync<AgentData>(agentId, new PartitionKey(agentId));
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public async Task<List<AgentData>> GetAllAgents()
    {
        // Return all agent data from agent container
        var query = AgentContainer.GetItemQueryIterator<AgentData>();
        var agents = new List<AgentData>();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            agents.AddRange(response.ToList());
        }

        return agents;
    }
    public async Task<List<WorldState>> GetWorldStatesAsync()
    {
        // Get all the world states
        var query = WorldStateContainer.GetItemQueryIterator<WorldState>();
        var worldStates = new List<WorldState>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            worldStates.AddRange(response.ToList());
            Console.WriteLine($"Retrieved {worldStates.Count} world states");
        }
        return worldStates;
    }
}