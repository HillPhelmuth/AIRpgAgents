using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AIRpgAgents.Core.Agents;
using AIRpgAgents.Core.Models;
using AIRpgAgents.GameEngine.World;

namespace AIRpgAgents.Core;

public class AppState : INotifyPropertyChanged
{
    private WorldState _worldState = new();
    private Player _player = new();
    private List<AgentData> _activeAgents = [];
    public event PropertyChangedEventHandler? PropertyChanged;

    public WorldState WorldState
    {
        get => _worldState;
        set => SetField(ref _worldState, value);
    }

    public Player Player
    {
        get => _player;
        set => SetField(ref _player, value);
    }

    public List<AgentData> ActiveAgents
    {
        get => _activeAgents;
        set => SetField(ref _activeAgents, value);
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}