using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Heluo.Wulin;
using Heluo.Wulin.UI;
using System.Threading;

namespace ToW_Esper_Plugin
{   
    /// Load Textfile Patches
    [HarmonyPatch(typeof(Game), "LoadScene")]
    class Patch_Game_LoadScene
    {
        static private MapTalkNode GetMapTalkNode(int movieID, int step)
        {
            var mttnode = Game.MapTalkData.GetMapTalkTypeNode(movieID.ToString());
            if (mttnode == null) return null;
            if (step > mttnode.m_MapTalkNodeList.Count) return null;
            return mttnode.m_MapTalkNodeList[step - 1];
        }
        static void Prefix(string name)
        {
            //GameEx.LoadMapTalkManagerPatch("MapTalkManager_patch");
            if (GlobalEx.hero.gender == 1)
            {
                GameEx.LoadMapTalkManagerPatch("MapTalkManager_female");
            }
            int[] specTalkIDs   = new int[] { 200090, 200193, 200193 };
            int[] specTalkSteps = new int[] { 3,      6,      7 };
            for (int i = 0; i < specTalkIDs.Length; i++)
            {
                var node = GetMapTalkNode(specTalkIDs[i], specTalkSteps[i]);
                if (node != null)
                {
                    int martial = GlobalEx.hero.weapon;
                    var spec_talks = GlobalEx.mod.martials[martial].spec_talks;
                    if (i < spec_talks.Length) node.m_strManager = spec_talks[i];
                }
            }
            GameEx.LoadStringTablePatch("string_table_patch");
        }
    }

    /// PatchAssetBundle patches
    [HarmonyPatch(typeof(PatchAssetBundle), "CreateFromFile")]
    class Patch_PatchAssetBundle_CreateFromFile
    {
        static void Postfix()
        {
            Utils.SetField(typeof(PatchAssetBundle), "strOutSidePath", EngineEx.GetModPath());
        }
    }
    [HarmonyPatch(typeof(PatchAssetBundle), "Load")]
    class Patch_PatchAssetBundle_Load
    {
        static bool Prefix(ref UnityEngine.Object __result, string name)
        {
            string strOutSidePath = Utils.GetField(typeof(PatchAssetBundle), "strOutSidePath") as string;
            string filePath;
            if (name.StartsWith("Audio"))
            {
                // 女主角使用招式时去男声处理
                if (GlobalEx.hero.gender == 1)
                {
                    filePath = strOutSidePath + name + "f.ogg";
                    if (File.Exists(filePath))
                    {
                        WWW www = new WWW("file:///" + filePath);
                        if (www.audioClip != null)
                        {
                            while (!www.audioClip.isReadyToPlay) Thread.Sleep(1);
                            __result = www.audioClip;
                            return false;
                        }
                    }
                }
                filePath = strOutSidePath + name + ".ogg";
                if (File.Exists(filePath))
                {
                    WWW www = new WWW("file:///" + filePath);
                    if (www.audioClip != null)
                    {
                        while (!www.audioClip.isReadyToPlay) Thread.Sleep(1);
                        __result = www.audioClip;
                        return false;
                    }
                }
            }
            filePath = strOutSidePath + "Image/" + name + ".png";
            if (File.Exists(filePath))
            {
                Texture2D texture2D = new Texture2D(2, 2, TextureFormat.ARGB32, false, false);
                texture2D.LoadImage(File.ReadAllBytes(filePath));
                __result = texture2D;
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(PatchAssetBundle), "Contains")]
    class Patch_PatchAssetBundle_Contains
    {
        static bool Prefix(ref bool __result, string name)
        {
            string strOutSidePath = Utils.GetField(typeof(PatchAssetBundle), "strOutSidePath") as string;
            string filePath = strOutSidePath + "Audio/" + name + ".ogg";
            if (File.Exists(filePath))
            {
                __result = true;
                return false;
            }
            filePath = strOutSidePath + "Image/" + name + ".png";
            if (File.Exists(filePath))
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
