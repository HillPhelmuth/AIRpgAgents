using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AIRpgAgents.Core.Models;
using AIRpgAgents.GameEngine.Game;

namespace AIRpgAgents.Core;

public class RpgState : INotifyPropertyChanged
{
    private RpgCampaign _activeCampaign = new();
    public event PropertyChangedEventHandler? PropertyChanged;

    public RpgCampaign ActiveCampaign
    {
        get => _activeCampaign;
        set => SetField(ref _activeCampaign, value);
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