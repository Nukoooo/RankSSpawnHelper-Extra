using System;
using System.IO;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Conditions;

namespace RankSSpawnHelper.Features;

public class LeaveDuty : IDisposable
{
    // taken from https://github.com/marimelon/LeaveDutyCmdPlugin/blob/master/LeaveDutyCmdPlugin/LeaveDutyCmdPlugin.cs
    private readonly LeaveDutyDelegate _leaveDungeon;

    public LeaveDuty()
    {
        try
        {
            _leaveDungeon = Marshal.GetDelegateForFunctionPointer<LeaveDutyDelegate>(Service.SigScanner.ScanText("40 53 48 83 ec 20 48 8b 05 ?? ?? ?? ?? 0f b6 d9"));
            Service.Condition.ConditionChange += OnConditionChange;
        }
        catch (Exception e)
        {
            throw new InvalidDataException($"Failed to load leave duty. r:{e}");
        }
    }

    public void Dispose()
    {
        Service.Condition.ConditionChange -= OnConditionChange;
    }

    private void OnConditionChange(ConditionFlag flag, bool value)
    {
        // PluginLog.Debug($"ConditionFlag: {flag}, value: {value}");

        if (flag == ConditionFlag.OccupiedInCutSceneEvent && !value) Execute();
    }

    public void Execute()
    {
        if (!Service.Configuration._autoLeaveDuty)
            return;

        if (Service.PartyList.Length > 0)
            return;

        if (Service.ClientState.LocalPlayer.ClassJob.Id != 36)
        {
            Service.ChatGui.PrintError("[自动退本] 职业只能是青魔");
            return;
        }

        if (Service.ClientState.TerritoryType != 1045)
        {
            Service.ChatGui.PrintError("[自动退本] 该功能只能在假火神(伊芙利特讨伐战)用");
            return;
        }

        _leaveDungeon((char)0);
    }

    private delegate void LeaveDutyDelegate(char isTimeout);
}