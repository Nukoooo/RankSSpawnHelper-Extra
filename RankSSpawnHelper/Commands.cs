using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Dalamud.Game.ClientState.Objects.Enums;

namespace RankSSpawnHelper;

public class Commands : IDisposable
{
    private const string CommandName = "/shelperex";
    private const string DebugCommand = "/debug_stuffex";
    private const string LeaveDuty = "/leavedt";
    private readonly List<string> _printEt = new() { "/printet", "/喊et" };
    private readonly List<string> _setEt = new() { "/setet", "/定et" };

    public Commands()
    {
        DalamudApi.CommandManager.AddHandler(CommandName, new(OnCommand)
        {
            HelpMessage = "打开菜单",
            ShowInHelp = true
        });

        DalamudApi.CommandManager.AddHandler(DebugCommand, new(OnCommand)
        {
            ShowInHelp = false
        });

        DalamudApi.CommandManager.AddHandler(LeaveDuty, new(OnCommand)
        {
            ShowInHelp = true,
            HelpMessage = "退出副本(无需确认)"
        });

        foreach (var cmd in _setEt)
            DalamudApi.CommandManager.AddHandler(cmd, new(SetET)
            {
                ShowInHelp = true,
                HelpMessage = "定ET"
            });

        foreach (var cmd in _printEt)
            DalamudApi.CommandManager.AddHandler(cmd, new(PrintEt)
            {
                ShowInHelp = true,
                HelpMessage = "喊ET"
            });
    }

    public void Dispose()
    {
        DalamudApi.CommandManager.RemoveHandler(CommandName);
        DalamudApi.CommandManager.RemoveHandler(DebugCommand);
        DalamudApi.CommandManager.RemoveHandler(LeaveDuty);
        foreach (var cmd in _setEt) DalamudApi.CommandManager.RemoveHandler(cmd);
        foreach (var cmd in _printEt) DalamudApi.CommandManager.RemoveHandler(cmd);
    }

    private static void SetET(string command, string args)
    {
        if (args == string.Empty)
        {
            DalamudApi.ChatGui.PrintError($"使用方法: {command} 分:秒. 可用格式: 00:00, 00:0, 0:0, 0:00 最长可到59:59");
            return;
        }

        var obj = DalamudApi.ObjectTable.Where(i => i.IsValid() && i.ObjectKind == ObjectKind.BattleNpc && Utils.IsSRankMonster(i.Name.TextValue)).Select(i => i).ToList();
        if (obj.Count == 0)
        {
            DalamudApi.ChatGui.PrintError("[定ET] 地图里没有S怪");
            return;
        }

        var match = Regex.Match(args, @"([0-5]?\d):([0-5]?\d)");
        if (match.Length == 0)
        {
            DalamudApi.ChatGui.PrintError("[定ET] ET的格式不对. 可用格式: 00:00, 00:0, 0:0, 0:00 最长可到59:59");
            return;
        }

        var minutes = int.Parse(match.Groups[1].ToString());
        var seconds = int.Parse(match.Groups[2].ToString());
        Utils.TargetEorzeaTime = Utils.LocalTimeToEorzeaTime(minutes, seconds);
        DalamudApi.ChatGui.Print($"[定ET] ET已设置为 {Utils.TargetEorzeaTime.Hour:D2}:{Utils.TargetEorzeaTime.Minute:D2}");
    }

    private static void PrintEt(string command, string args)
    {
        Utils.PrintSetTimeMessage(false, DalamudApi.Configuration._printInYell);
    }

    private static void OnCommand(string command, string args)
    {
        switch (command)
        {
            case CommandName:
            {
                DalamudApi.ConfigWindow.Toggle();
                break;
            }
            case DebugCommand:
            {
                DalamudApi.ChatGui.Print($"territoryId: {DalamudApi.ClientState.TerritoryType}, classJob:{DalamudApi.ClientState.LocalPlayer.ClassJob.Id}, PartyLength: {DalamudApi.PartyList.Length}");

                break;
            }
            case LeaveDuty:
            {
                DalamudApi.LeaveDuty.Execute();
                break;
            }
        }
    }
}