using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Dalamud.Game.Command;

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
        Service.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "打开菜单",
            ShowInHelp = true
        });

        Service.CommandManager.AddHandler(DebugCommand, new CommandInfo(OnCommand)
        {
            ShowInHelp = false
        });

        Service.CommandManager.AddHandler(LeaveDuty, new CommandInfo(OnCommand)
        {
            ShowInHelp = true,
            HelpMessage = "退出副本(无需确认)"
        });

        foreach (var cmd in _setEt)
            Service.CommandManager.AddHandler(cmd, new CommandInfo(SetET)
            {
                ShowInHelp = true,
                HelpMessage = "定ET"
            });

        foreach (var cmd in _printEt)
            Service.CommandManager.AddHandler(cmd, new CommandInfo(PrintET)
            {
                ShowInHelp = true,
                HelpMessage = "喊ET"
            });
    }

    public void Dispose()
    {
        Service.CommandManager.RemoveHandler(CommandName);
        Service.CommandManager.RemoveHandler(DebugCommand);
        Service.CommandManager.RemoveHandler(LeaveDuty);
        foreach (var cmd in _setEt) Service.CommandManager.RemoveHandler(cmd);
        foreach (var cmd in _printEt) Service.CommandManager.RemoveHandler(cmd);
    }

    private static void SetET(string command, string args)
    {
        if (args == string.Empty)
        {
            Service.ChatGui.PrintError($"使用方法: {command} 分:秒. 可用格式: 00:00, 00:0, 0:0, 0:00 最长可到59:59");
            return;
        }

        var objectAtTwo = Service.ObjectTable[2];
        if (objectAtTwo == null)
        {
            Service.ChatGui.PrintError("[定ET] 地图里没有S怪");
            return;
        }

        if (Utils.IsSRankMonster(objectAtTwo.Name.TextValue))
        {
            Service.ChatGui.PrintError("[定ET] 地图里没有S怪");
            return;
        }

        var match = Regex.Match(args, @"([0-5]?\d):([0-5]?\d)");
        if (match.Length == 0)
        {
            Service.ChatGui.PrintError("[定ET] ET的格式不对. 可用格式: 00:00, 00:0, 0:0, 0:00 最长可到59:59");
            return;
        }

        var minutes = int.Parse(match.Groups[1].ToString());
        var seconds = int.Parse(match.Groups[2].ToString());
        Utils.TargetEorzeaTime = Utils.LocalTimeToEorzeaTime(minutes, seconds);
        Service.ChatGui.Print($"[定ET] ET已设置为 {Utils.TargetEorzeaTime.Hour}:{Utils.TargetEorzeaTime.Minute}");
    }

    private static void PrintET(string command, string args)
    {
        Utils.PrintSetTimeMessage(false, Service.Configuration._printInYell);
    }

    private static void OnCommand(string command, string args)
    {
        switch (command)
        {
            case CommandName:
            {
                Service.ConfigWindow.Toggle();
                break;
            }
            case DebugCommand:
            {
                Service.ChatGui.Print($"territoryId: {Service.ClientState.TerritoryType}, classJob:{Service.ClientState.LocalPlayer.ClassJob.Id}, PartyLength: {Service.PartyList.Length}");

                var objectAt2 = Service.ObjectTable[2];
                if (objectAt2 != null) Service.ChatGui.Print($"{Utils.IsSRankMonster(objectAt2.Name.TextValue)}");

                break;
            }
            case LeaveDuty:
            {
                Service.LeaveDuty.Execute();
                break;
            }
        }
    }
}