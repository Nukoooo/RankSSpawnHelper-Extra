using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using RankSSpawnHelper.Features;

// ReSharper disable InvertIf
namespace RankSSpawnHelper;

public class ConfigWindow : Window
{
    // private const double EORZEA_TIME_CONSTANT = 3600D / 175D;
    private const ImGuiTableFlags TableFlags = ImGuiTableFlags.Borders | ImGuiTableFlags.SizingStretchProp;
    // private const string TimeRegexPattern = @"([0-5]?\d):([0-5]?\d)";
    private string _searchText = string.Empty;
    private string _serverUrl = string.Empty;
    // private string _timeInput = string.Empty;
    // private string _trackerInputText = string.Empty;

    public ConfigWindow() : base("S怪触发小助手-Ex##RankSSpawnHelperEx")
    {
        Flags = ImGuiWindowFlags.AlwaysAutoResize;
    }

    private void DrawItemSearchTable()
    {
        static ImGuiTableFlags BuildFlag(ImGuiTableFlags flags, bool empty)
        {
            if (!empty)
                flags |= ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable;
            return flags;
        }

        if (Service.AutoDiscardItem.ItemInfos.Count == 0)
            return;

        ImGui.InputTextWithHint("##搜索物品名字", "输入你要搜索的物品名", ref _searchText, 256);

        ImGui.Text("物品列表:");
        ImGui.SameLine();
        ImGui.TextColored(ImGuiColors.DalamudGrey, "(?)");
        if (ImGui.IsItemHovered()) ImGui.SetTooltip("如果搜索框里没有输入文本或者搜索无结果，表格就会显示已添加的物品\n如果搜索无结果就会显示已添加的物品\n加号是添加，减号是移除，如果这都看不懂那你就是猪猪");

        var result = Service.AutoDiscardItem.ItemInfos.Where(i => i.name.Contains(_searchText)).Select(i => new ItemInfo_t(i.id, i.name)).ToArray();

        var isEmpty = _searchText == string.Empty || result.Length == 0;

        if (ImGui.BeginTable("##可选择的物品列表Extraaa", 2, BuildFlag(TableFlags, isEmpty),
                new Vector2(-1, isEmpty ? -1 : 300)))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn("物品名字");
            ImGui.TableSetupColumn("##按钮操作", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 20 * ImGuiHelpers.GlobalScale);
            ImGui.TableHeadersRow();

            if (isEmpty)
            {
                result = Service.AutoDiscardItem.ItemInfos.Where(i => Service.Configuration._itemsToDiscard.Contains(i.id)).Select(i => new ItemInfo_t(i.id, i.name)).ToArray();

                if (result.Length != 0)
                {
                    foreach (var info in result)
                    {
                        ImGui.PushID($"##物品{info.id}");
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();

                        ImGui.Text(info.name);
                        ImGui.TableNextColumn();

                        ImGui.PushFont(UiBuilder.IconFont);
                        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(2, ImGui.GetStyle().FramePadding.Y));

                        if (ImGui.Button("\xF068"))
                        {
                            Service.Configuration._itemsToDiscard.Remove(info.id);
                            Service.Configuration.Save();
                        }

                        ImGui.PopStyleVar();
                        ImGui.PopFont();

                        ImGui.PopID();
                    }

                    ImGui.EndTable();
                    return;
                }

                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.Text("-");
                ImGui.TableNextColumn();

                ImGui.EndTable();
                return;
            }

            foreach (var info in result)
            {
                ImGui.PushID($"##物品{info.id}");
                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                ImGui.Text(info.name);
                ImGui.TableNextColumn();

                ImGui.PushFont(UiBuilder.IconFont);
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(2, ImGui.GetStyle().FramePadding.Y));

                var isItemInList = Service.Configuration._itemsToDiscard.Contains(info.id);

                if (ImGui.Button(isItemInList ? "\xF068" : "\xF067"))
                {
                    if (isItemInList)
                        Service.Configuration._itemsToDiscard.Remove(info.id);
                    else
                        Service.Configuration._itemsToDiscard.Add(info.id);
                    Service.Configuration.Save();
                }

                ImGui.PopStyleVar();
                ImGui.PopFont();

                ImGui.PopID();
            }

            ImGui.EndTable();
        }
    }

    public override void Draw()
    {
        ImGui.BeginTabBar("exxxtra主菜单aaaaa");

        {
            if (ImGui.BeginTabItem("自动扔物品"))
            {
                var autoThrow = Service.Configuration._autoDiscardItem;
                if (ImGui.Checkbox("启用", ref autoThrow))
                {
                    Service.Configuration._autoDiscardItem = autoThrow;
                    Service.Configuration.Save();
                }
                DrawItemSearchTable();
                ImGui.EndTabItem();
            }

            /*if (ImGui.BeginTabItem("定ET+喊话"))
            {
                unsafe
                {
                    var eorzeaTime = DateTimeOffset.FromUnixTimeSeconds(Framework.Instance()->EorzeaTime).DateTime;
                    var localTime = Utils.LocalTimeToEorzeaTime();
                    
                    ImGui.Text($"FrameWork->eorzeaTime: {eorzeaTime.Hour}:{eorzeaTime.Minute}:{eorzeaTime.Second}");
                    ImGui.Text($"LocalTimeToEorzeaTime: {localTime.Hour}:{localTime.Minute}:{localTime.Second}");
                    
                    ImGui.Text("在多久后定时(本地时间):");
                    ImGui.InputTextWithHint("##timeInputYes", "格式: 分钟:秒 如00:24, 00:01", ref _timeInput, 32);


                }
                ImGui.EndTabItem();
            }*/

            if (ImGui.BeginTabItem("其他"))
            {
                var autoLeaveDuty = Service.Configuration._autoLeaveDuty;
                if (ImGui.Checkbox("自动退本(青魔消debuff)", ref autoLeaveDuty))
                {
                    Service.Configuration._autoLeaveDuty = autoLeaveDuty;
                    Service.Configuration.Save();
                }

                var autoJournal = Service.Configuration._autoJournal;
                if (ImGui.Checkbox("自动开始/放弃理符", ref autoJournal))
                {
                    Service.Configuration._autoJournal = autoJournal;
                    Service.Configuration.Save();
                }

                var clickDelay = Service.Configuration._clickDelay;
                if (ImGui.SliderInt("自动点击延迟", ref clickDelay, 30, 1000, "%dms"))
                {
                    Service.Configuration._clickDelay = clickDelay;
                    Service.Configuration.Save();
                }

                ImGui.SameLine();
                ImGui.TextColored(ImGuiColors.DalamudGrey, "(?)");
                if (ImGui.IsItemHovered()) ImGui.SetTooltip("可以把鼠标放在滑条上，按下ctrl然后点鼠标左键就输入指定的数值");

                ImGui.EndTabItem();
            }
        }

        ImGui.EndTabBar();
    }
}