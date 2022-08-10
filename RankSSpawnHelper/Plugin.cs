using System;
using ClickLib;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using RankSSpawnHelper.Features;

namespace RankSSpawnHelper;

public class Plugin : IDalamudPlugin
{
    private readonly WindowSystem _windowSystem;

    public Plugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();
        Click.Initialize();
        Utils.Initialize();

        Service.Configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        Service.Commands = new Commands();
        Service.ConfigWindow = new ConfigWindow();
        Service.AutoDiscardItem = new AutoDiscardItem();
        Service.LeaveDuty = new LeaveDuty();
        Service.Journal = new JournalStuff();

        _windowSystem = new WindowSystem("RankSSpawnHelper-Extra");
        _windowSystem.AddWindow(Service.ConfigWindow);

        Service.Interface.UiBuilder.OpenConfigUi += OpenConfigUi;
        Service.Interface.UiBuilder.Draw += _windowSystem.Draw;

        // Utils.ExecuteCommand("/e Initialized");
    }

    public string Name => "S怪触发小助手-Extra";

    public void Dispose()
    {
        Service.Commands.Dispose();
        GC.SuppressFinalize(this);
        Service.AutoDiscardItem.Dispose();
        Service.LeaveDuty.Dispose();
        Service.Journal.Dispose();

        Service.Interface.UiBuilder.OpenConfigUi -= OpenConfigUi;
        Service.Interface.UiBuilder.Draw -= _windowSystem.Draw;
    }

    private static void OpenConfigUi()
    {
        Service.ConfigWindow.IsOpen = true;
    }
}