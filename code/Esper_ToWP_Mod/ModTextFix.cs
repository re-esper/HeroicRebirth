using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Heluo.Wulin;
using Heluo.Wulin.UI;

namespace ToW_Esper_Plugin
{
    [HarmonyPatch(typeof(UILabel), "text", MethodType.Setter)]
    class Patch_UILabel_SetText
    {
        static void Prefix(UILabel __instance, ref string value)
        {
            StringBuilder sb = new StringBuilder(value);
            sb.Replace("谷月轩", GlobalEx.hero_fullname);
            sb.Replace("谷兄弟", GlobalEx.hero.name1 + (GlobalEx.hero.gender == 0 ? "兄弟" : "姑娘"));
            sb.Replace("谷兄", GlobalEx.hero.name1 + (GlobalEx.hero.gender == 0 ? "兄" : "姑娘"));
            sb.Replace("谷大哥", GlobalEx.hero.name1 + (GlobalEx.hero.gender == 0 ? "大哥" : "姐姐"));
            sb.Replace("谷公子", GlobalEx.hero.name1 + (GlobalEx.hero.gender == 0 ? "公子" : "姑娘"));
            sb.Replace("谷哥哥", GlobalEx.hero.name1 + (GlobalEx.hero.gender == 0 ? "哥哥" : "姐姐"));
            sb.Replace("谷贤侄", GlobalEx.hero.name1 + (GlobalEx.hero.gender == 0 ? "贤侄" : "贤侄女"));
            sb.Replace("轩儿", GlobalEx.hero.name2 + "儿");
            sb.Replace("谷某", GlobalEx.hero.gender == 0 ? GlobalEx.hero.name1 + "某" : GlobalEx.hero.name2);
            sb.Replace("谷大侠", GlobalEx.hero.name1 + (GlobalEx.hero.gender == 0 ? "大侠" : "女侠"));
            sb.Replace("谷少侠", GlobalEx.hero.name1 + (GlobalEx.hero.gender == 0 ? "少侠" : "女侠"));
            sb.Replace("谷先生", GlobalEx.hero.name1 + (GlobalEx.hero.gender == 0 ? "先生" : "女士"));
            sb.Replace("谷施主", GlobalEx.hero.name1 + "施主");
            sb.Replace("谷云飞", GlobalEx.hero.name1 + "云飞");            
            sb.Replace("月轩", GlobalEx.hero.name2);
            sb.Replace("逍遥拳不平", GlobalEx.hero.nick);
            sb.Replace("{martial}", GlobalEx.hero_martial);
            sb.Replace("{name1}", GlobalEx.hero.name1);
            sb.Replace("{name2}", GlobalEx.hero.name2);
            sb.Replace("{senior}", GlobalEx.hero.gender == 0 ? "师兄" : "师姐");
            value = sb.ToString();
        }
    }
    /// 特定人物对话
    [HarmonyPatch(typeof(UITalk), "SetIconAndManager")]
    class Patch_UITalk_SetIconAndManager
    {
        static void Prefix(UITalk __instance)
        {            
            if (GlobalEx.hero.gender == 1)
            {
                var mapTalkTypeNode = Utils.GetField(__instance, "m_MapTalkTypeNode") as MapTalkTypeNode;
                var index = (int)Utils.GetField(__instance, "m_iIndex");
                MapTalkNode mapTalkNode = mapTalkTypeNode.m_MapTalkNodeList[index];
                if (mapTalkNode.m_iNpcID == 100017 || mapTalkNode.m_iNpcID == 210002 || mapTalkNode.m_iNpcID == 200000 || mapTalkNode.m_iNpcID == 199999)
                {
                    StringBuilder sb = new StringBuilder(mapTalkNode.m_strManager);
                    sb.Replace("师兄", "师姐");
                    mapTalkNode.m_strManager = sb.ToString();
                }
                else if (mapTalkNode.m_iNpcID == 210001)
                {
                    StringBuilder sb = new StringBuilder(mapTalkNode.m_strManager);
                    sb.Replace("师兄弟", "师姐弟");
                    if (mapTalkNode.m_strManager.IndexOf("阿棘") != -1)
                    {
                        sb.Replace("师兄", "师姐");
                    }
                    mapTalkNode.m_strManager = sb.ToString();
                }
                else if (mapTalkNode.m_iNpcID == 100019)
                {
                    StringBuilder sb = new StringBuilder(mapTalkNode.m_strManager);
                    sb.Replace("大少爷", "大小姐");
                    mapTalkNode.m_strManager = sb.ToString();
                }
            }
        }
    }
    /// 特定人物战场对话
    [HarmonyPatch(typeof(MapTalkManager), "GetTalkString")]
    class Patch_MapTalkManager_GetTalkString
    {
        static bool Prefix(MapTalkManager __instance, int movieID, int Step, ref string __result)
        {
            if (Step < 0) return true;
            MapTalkTypeNode mapTalkTypeNode = __instance.GetMapTalkTypeNode(movieID.ToString());
            if (mapTalkTypeNode == null) return true;
            if (Step >= mapTalkTypeNode.m_MapTalkNodeList.Count) return true;
            MapTalkNode mapTalkNode = mapTalkTypeNode.m_MapTalkNodeList[Step];
            if (GlobalEx.hero.gender == 1)
            {
                if (mapTalkNode.m_iNpcID == 100017 || mapTalkNode.m_iNpcID == 210002 || mapTalkNode.m_iNpcID == 200000 || mapTalkNode.m_iNpcID == 210001 || mapTalkNode.m_iNpcID == 199999 || mapTalkNode.m_iNpcID == 600069)
                {
                    StringBuilder sb = new StringBuilder(mapTalkNode.m_strManager);
                    sb.Replace("师兄", "师姐");
                    mapTalkNode.m_strManager = sb.ToString();
                }
            }
            __result = mapTalkNode.m_strManager;
            return false;
        }
    }
}
