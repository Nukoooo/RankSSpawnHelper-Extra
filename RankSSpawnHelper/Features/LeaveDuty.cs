using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ClickLib.Clicks;
using Dalamud.Game.ClientState.Conditions;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace RankSSpawnHelper.Features;

public class LeaveDuty : IDisposable
{
    // taken from https://github.com/UnknownX7/OOBlugin/blob/master/Game.cs
    private static IntPtr _contentsFinderMenuAgent = IntPtr.Zero;
    private static OpenAbandonDutyDelegate _openAbandonDuty;

    public LeaveDuty()
    {
        unsafe
        {
            try
            {
                _openAbandonDuty = Marshal.GetDelegateForFunctionPointer<OpenAbandonDutyDelegate>(Service.SigScanner.ScanText("E8 ?? ?? ?? ?? EB 90 48 8B CB"));
                _contentsFinderMenuAgent = (IntPtr)Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalId(AgentId.ContentsFinderMenu);
                Service.Condition.ConditionChange += OnConditionChange;
            }
            catch (Exception e)
            {
                throw new InvalidDataException($"Failed to load leave duty. r:{e}");
            }
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

        if (Service.ClientState.TerritoryType != 157)
        {
            Service.ChatGui.PrintError("[自动退本] 该功能只能在溶洞用");
            return;
        }

        _contentsFinderMenuAgent = _contentsFinderMenuAgent != IntPtr.Zero ? _contentsFinderMenuAgent : Service.GameGui.FindAgentInterface("ContentsFinderMenu");
        if (_contentsFinderMenuAgent == IntPtr.Zero) return;

        _openAbandonDuty(_contentsFinderMenuAgent);

        Task.Run(async () =>
        {
            await Task.Delay(Service.Configuration._clickDelay);

            var yesNoAddon = Service.GameGui.GetAddonByName("SelectYesno", 1);
            if (yesNoAddon == IntPtr.Zero) return;

            ClickSelectYesNo.Using(yesNoAddon).Yes();
        });
    }

    private delegate void OpenAbandonDutyDelegate(IntPtr agent);
}