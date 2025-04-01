using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AIRpgAgents.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AIRpgAgents.Core.Models;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAIRpgAgentsCore(this IServiceCollection services)
    {
        services.AddScoped<AppState>()./*AddScoped<CharacterCreationState>().*/AddSingleton<CosmosService>().AddSingleton<RpgState>().AddSingleton<CombatService>();
        return services;
    }
}