﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIRpgAgents.Core.Services;

public class AgentBuilderService
{
    private readonly AppState _appState;
    private readonly CosmosService _cosmosService;

    public AgentBuilderService(AppState appState, CosmosService cosmosService)
    {
        _appState = appState;
        _cosmosService = cosmosService;
    }
}