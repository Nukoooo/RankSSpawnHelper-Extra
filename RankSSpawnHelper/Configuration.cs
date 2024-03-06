using System.Collections.Generic;
using Dalamud.Configuration;

namespace RankSSpawnHelper;

public class Configuration : IPluginConfiguration
{
    public bool _autoDiscardItem { get; set; } = false;
    
    public bool _autoLeaveDuty { get; set; } = false;
    
    public bool _autoJournal { get; set; } = false;

    public List<uint> _itemsToDiscard { get; set; } = new();

    public bool _summonMinion { get; set; } = new();

    public int _clickDelay { get; set; } = 100;

    public string _mainSetTimeMessage { get; set; } = "在{tpos}发现了S级狩猎怪{tname} {etmsg}";
    public string _etMessageSet { get; set; } = "将会在ET{et}开打";
    public string _etMessageUnset { get; set; } = "ET未定，请勿抢开跟开";
    public bool _printInYell { get; set; } = false;

    int IPluginConfiguration.Version { get; set; }

    public void Save()
    {
        DalamudApi.Interface.SavePluginConfig(this);
    }
}