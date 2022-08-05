using System;
using System.Runtime.InteropServices;
using System.Text;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace RankSSpawnHelper;

internal static unsafe class Utils
{
    private static UIModule* _uiModule;
    private static ProcessChatBoxDelegate _processChatBox;

    public static void Initialize()
    {
        _uiModule = Framework.Instance()->GetUiModule();
        var easierProcessChatBoxPtr = Service.SigScanner.ScanText("48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9");
        _processChatBox = Marshal.GetDelegateForFunctionPointer<ProcessChatBoxDelegate>(easierProcessChatBoxPtr);
    }

    public static void ExecuteCommand(string cmd)
    {
        try
        {
            var bytes = Encoding.UTF8.GetBytes(cmd);

            var mem1 = Marshal.AllocHGlobal(400);
            var mem2 = Marshal.AllocHGlobal(bytes.Length + 30);

            Marshal.Copy(bytes, 0, mem2, bytes.Length);
            Marshal.WriteByte(mem2 + bytes.Length, 0);
            Marshal.WriteInt64(mem1, mem2.ToInt64());
            Marshal.WriteInt64(mem1 + 8, 64);
            Marshal.WriteInt64(mem1 + 8 + 8, bytes.Length + 1);
            Marshal.WriteInt64(mem1 + 8 + 8 + 8, 0);

            _processChatBox!(_uiModule, mem1, IntPtr.Zero, 0);

            Marshal.FreeHGlobal(mem1);
            Marshal.FreeHGlobal(mem2);
        }
        catch (Exception err)
        {
            Service.ChatGui.PrintError(err.Message);
        }
    }

    public static DateTime LocalTimeToEorzeaTime()
    {
        const double eorzeaMultiplier = 3600D / 175D;

        var epochTicks = DateTimeOffset.Now.DateTime.ToUniversalTime().AddSeconds(2.998).Ticks - new DateTime(1970, 1, 1).Ticks;

        var eorzeaTicks = (long)Math.Round(epochTicks * eorzeaMultiplier);

        return new DateTime(eorzeaTicks);
    }

    private delegate void ProcessChatBoxDelegate(UIModule* uiModule, IntPtr message, IntPtr unused, byte a4);
}