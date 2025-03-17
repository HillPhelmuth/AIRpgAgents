using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace AIRpgAgents.Core.Models;

public class Config
{
    public static string? OpenAIApiKey { get; set; }
    public static string? GrokApiKey { get; set; }
    public static string? GrokEndpoint { get; set; }

    public static void LoadConfig(IConfiguration configuration)
    {
        OpenAIApiKey = configuration["OpenAI:ApiKey"];
        GrokApiKey = configuration["Grok:ApiKey"];
        GrokEndpoint = configuration["Grok:Endpoint"];
    }
}