using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ClickLib.Clicks;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace RankSSpawnHelper.Features;

public class JournalStuff : IDisposable
{
    private static Hook<AddonSetupDelegate> _addonSetupHook;

    public JournalStuff()
    {
        unsafe
        {
            _addonSetupHook = Hook<AddonSetupDelegate>.FromAddress(Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 8B 83 ?? ?? ?? ?? C1 E8 14"), hk_AddonSetup);
            _addonSetupHook.Enable();
        }
    }

    public void Dispose()
    {
        _addonSetupHook.Dispose();
    }

    private static void ProcessYesNo()
    {
        Task.Run(async () =>
        {
            await Task.Delay(Service.Configuration._clickDelay);

            var yesNoAddon = Service.GameGui.GetAddonByName("SelectYesno", 1);
            if (yesNoAddon == IntPtr.Zero) return;

            ClickSelectYesNo.Using(yesNoAddon).Yes();
        });
    }

    private static void ProcessGuildLeveDifficulty(IntPtr addon)
    {
        Task.Run(async () =>
        {
            await Task.Delay(200);

            ClickGuildLeveDifficulty.Using(addon).Confirm();
        });
    }

    private static void ProcessAfterDeclination(IntPtr addonBase, bool canRestart = false)
    {
        Task.Run(async () =>
        {
            await Task.Delay(200);
            ClickJournalDetail.Using(addonBase).Start();

            ProcessYesNo();

            if (canRestart)
            {
                await Task.Delay(1000);

                ClickJournalDetail.Using(addonBase).Start();
                ProcessYesNo();
            }
        });
    }

    private static unsafe void ProcessJournalDetail(IntPtr addonBase)
    {
        var addon = (AtkUnitBase*)addonBase;
        var startButtonBase = addon->UldManager.NodeList[6];
        // var startButton = (AtkComponentButton*)startButtonBase;

        var component = (AtkTextNode*)startButtonBase->GetComponent()->UldManager.NodeList[2];
        var text = Marshal.PtrToStringUTF8(new IntPtr(component->NodeText.StringPtr)) ?? "empty";

        // PluginLog.Debug($"{startButton->IsEnabled} | {startButtonBase->IsVisible} | {text}");

        if (!startButtonBase->IsVisible) return;

        switch (text)
        {
            case "开始":
                ClickJournalDetail.Using(addonBase).Start();
                ProcessYesNo();
                break;
            case "变更难度":
                ClickJournalDetail.Using(addonBase).Decline_();
                ProcessYesNo();
                ProcessAfterDeclination(addonBase);
                break;
            case "重新挑战":
                ClickJournalDetail.Using(addonBase).Start();
                ProcessYesNo();
                ProcessAfterDeclination(addonBase, true);
                break;
        }
    }

    private static void ProcessJournalDetailWrapper(IntPtr addon)
    {
        Task.Run(async () =>
        {
            await Task.Delay(Service.Configuration._clickDelay);
            ProcessJournalDetail(addon);
        });
    }

    private static unsafe void* hk_AddonSetup(AtkUnitBase* addon)
    {
        var original = _addonSetupHook.Original(addon);

        if (!Service.Configuration._autoJournal)
            return original;

        if (Service.ClientState.TerritoryType != 152 && Service.ClientState.TerritoryType != 145)
            return original;

        var name = Marshal.PtrToStringUTF8((IntPtr)addon->Name);

        switch (name)
        {
            case "JournalDetail":
                ProcessJournalDetailWrapper(new IntPtr(addon));
                break;
            case "GuildLeveDifficulty":
                ProcessGuildLeveDifficulty(new IntPtr(addon));
                break;
        }

        return original;
    }

    private unsafe delegate void* AddonSetupDelegate(AtkUnitBase* addon);
}