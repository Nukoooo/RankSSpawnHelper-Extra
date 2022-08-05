using System;
using System.Collections.Generic;
using Dalamud.Game.Command;

namespace RankSSpawnHelper;

public class Commands : IDisposable
{
    private const string CommandName = "/shelperex";
    private const string DebugCommand = "/debug_stuffex";
    private const string LeaveDuty = "/leavedt";

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
            ShowInHelp = false
        });

    }

    public void Dispose()
    {
        Service.CommandManager.RemoveHandler(CommandName);
        Service.CommandManager.RemoveHandler(DebugCommand);
        Service.CommandManager.RemoveHandler(LeaveDuty);
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