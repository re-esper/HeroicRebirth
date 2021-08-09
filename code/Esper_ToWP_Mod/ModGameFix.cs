using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Heluo.Wulin;
using Heluo.Wulin.UI;

namespace ToW_Esper_Plugin
{
    /// 自定义游戏数据读档处理
    [HarmonyPatch(typeof(Save), "LoadSaveVersionFix")]
    class Patch_Save_LoadSaveVersionFix
    {
        static void Postfix(List<string> saveVersionFix)
        {
            foreach (var str in saveVersionFix)
            {
                if (str.StartsWith(GlobalEx.ModTAG))
                {
                    byte[] bytes = Convert.FromBase64String(str.Substring(GlobalEx.ModTAG.Length));
                    var jsonstr = Encoding.UTF8.GetString(bytes);
                    GlobalEx.hero = ModSaveData.CreateFromJSON(jsonstr);
                    GameEx.ApplyHeroBasicData();
                    GameEx.FixGameStaticData();
                    break;
                }
            }

        }
    }
    [HarmonyPatch(typeof(Save), "LoadData")]
    class Patch_Save_LoadData
    {
        static void Postfix()
        {
            GameEx.FixGameStaticData2();
        }
    }
    [HarmonyPatch(typeof(TeamStatus), "Reset")]
    class Patch_Save_AddTeamMember
    {
        static void Postfix(TeamStatus __instance)
        {
            // 修荆棘和紫绫可剔除
            if (!GameGlobal.m_bDLCMode)
            {
                Utils.SetField(__instance, "iXyg_New_GingiID", -1);
                Utils.SetField(__instance, "iXiaoyao_PurpleID", -1);
                Utils.SetField(__instance, "MaxTeamMember", 9);
            }
        }
    }

    /// 存档标题修改
    [HarmonyPatch(typeof(MissionStatus), "getNewMissionID")]
    class Patch_MissionStatus_getNewMissionID
    {
        static void Postfix(ref string __result)
        {
            __result += "," + GlobalEx.hero.difficulty.ToString() + "," + GlobalEx.hero_fullname;
        }
    }
    [HarmonyPatch(typeof(CtrlSaveAndLoad), "SetTipData")]
    class Patch_CtrlSaveAndLoad_SetTipData
    {
        static bool Prefix(CtrlSaveAndLoad __instance, int index)
        {
            var saveLoadDateList = Utils.GetField(__instance, "saveLoadDateList") as List<SaveTitleDataNode>;
            if (saveLoadDateList[index].m_bHaveData)
            {
                string strMissionID = saveLoadDateList[index].m_strMissionID.Split(',')[0];
                QuestNode questNode = Game.QuestData.GetQuestNode(strMissionID);
                if (questNode != null)
                {
                    __instance.setTipView.Invoke(questNode.m_strQuestName, questNode.m_strQuestTip, saveLoadDateList[index].m_Texture);
                }
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(CtrlSaveAndLoad), "SetSaveLoadData")]
    class Patch_CtrlSaveAndLoad_SetSaveLoadData
    {
        static bool Prefix(CtrlSaveAndLoad __instance)
        {
            if (GameGlobal.m_bDLCMode) return true;

            var saveLoadDateList = Utils.GetField(__instance, "saveLoadDateList") as List<SaveTitleDataNode>;
            for (int i = 0; i < saveLoadDateList.Count; i++)
            {
                bool bHaveData = saveLoadDateList[i].m_bHaveData;
                __instance.setViewActive.Invoke(i, bHaveData);
                if (bHaveData)
                {
                    string strMissionID = saveLoadDateList[i].m_strMissionID;
                    string text = Game.MapID.GetMapName(saveLoadDateList[i].m_strPlaceName);
                    string strTrueYear = saveLoadDateList[i].m_strTrueYear;
                    string text2 = Game.StringTable.GetString(100304) + saveLoadDateList[i].m_PlayGameTime.TimeFormt();
                    string text3 = "";
                    var strarr = strMissionID.Split(',');
                    QuestNode questNode = Game.QuestData.GetQuestNode(strarr[0]);
                    if (questNode != null) text3 = questNode.m_strQuestName;                    
                    if (strarr.Length >= 3)
                    {
                        int diff = Convert.ToInt32(strarr[1]);
                        string cspace = "　";
                        text3 = strarr[2] + cspace + text3;
                        if (text3.Length > 13) text3 = text3.Substring(0, 13);
                        text3 += new string('　', 14 - text3.Length) + GlobalEx.difficulty_name[diff] + cspace;
                    }
                    if (text == null)
                    {
                        GameDebugTool.Log("沒有這個MapID");
                        text = string.Empty;
                    }
                    __instance.setSaveLoadView.Invoke(i, new string[] { text3, text, strTrueYear, text2 });
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(UIEnd), "AssignControl")]
    class Patch_UIEnd_AssignControl
    {
        static void Postfix(Transform sender)
        {
            if (sender.name == "StaffList")
            {
                var labelList = new List<UILabel>();
                for (int i = 0; i < sender.childCount; i++)
                {
                    var label = sender.GetChild(i).GetComponent<UILabel>();
                    if (label != null) labelList.Add(label);
                }
                labelList[3].text = "侠道再起模组";
                labelList[4].text = "　esper";
                labelList[5].text = "特别鸣谢";
                labelList[6].text = "　暂无";
                labelList[7].text = "原版制作群";
                labelList[8].text = "　台湾河洛工作室";
                for (int i = 9; i < labelList.Count; i++)
                {
                    labelList[i].text = "";
                }
                labelList[labelList.Count - 2].text = "感谢游玩！";
            }
        }
    }
    [HarmonyPatch(typeof(UIEnd), "Play", new Type[] { typeof(List<EndMovieData>), typeof(bool) })]
    class Patch_UIEnd_Play
    {
        static void Postfix(List<EndMovieData> endingList, bool isPlayStaff)
        {
            if (isPlayStaff) endingList.Clear();
        }
    }
    /// 临时处理 等待后续制作
    [HarmonyPatch(typeof(UIEnd), "ReturnToTrueEnd")]
    class Patch_UIEnd_ReturnToTrueEnd
    {
        static bool Prefix(UIEnd __instance)
        {
            Utils.InvokeMethod(__instance, "ReturnToTitle");
            return false;
        }
    }
}
