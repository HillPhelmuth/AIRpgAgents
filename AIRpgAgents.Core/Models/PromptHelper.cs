using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AIRpgAgents.Core.Models;

public class PromptHelper
{
    public static Dictionary<string, string> PromptDictionary = new();
    public static string ExtractPromptFromFile(string fileName)
    {
        if (PromptDictionary.TryGetValue(fileName, out var promptFile))
        {
            return promptFile;
        }
        var assembly = Assembly.GetExecutingAssembly();
        var jsonName = assembly.GetManifestResourceNames()
            .SingleOrDefault(s => s.EndsWith(fileName, StringComparison.OrdinalIgnoreCase)) ?? "";
        using var stream = assembly.GetManifestResourceStream(jsonName);
        using var reader = new StreamReader(stream);
        object result = reader.ReadToEnd();
        var promptFromFile = result?.ToString() ?? "";
        PromptDictionary[fileName] = promptFromFile;
        return promptFromFile;
    }

    public static Stream ExtractStreamFromFile(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var jsonName = assembly.GetManifestResourceNames()
            .SingleOrDefault(s => s.EndsWith(fileName, StringComparison.OrdinalIgnoreCase)) ?? "";
        var stream = assembly.GetManifestResourceStream(jsonName);
        return stream;
    }
}