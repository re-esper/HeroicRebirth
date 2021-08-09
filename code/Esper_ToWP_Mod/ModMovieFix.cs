using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;
using Heluo.Wulin;
using JsonFx.Json;
using System.Collections;

namespace ToW_Esper_Plugin
{
    /// 修正游戏LoadMovieFormAssetBundle无法正确处理本地文件的bug
    [HarmonyPatch(typeof(MovieEventMap), "LoadMovieFormAssetBundle", typeof(string))]
    class Patch_MovieEventMap_LoadMovieFormAssetBundle
    {
        static bool Prefix(MovieEventMap __instance, string strSceneName)
        {
            Console.WriteLine("MovieEventMap load: " + strSceneName);
            string sFileName = EngineEx.GetModPath() + "Config/Movie/" + strSceneName + ".txt";
            string[] array = null;
            string text = Utils.LoadTextFile(sFileName);
            if (!string.IsNullOrEmpty(text))
            {
                array = text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                TextAsset textAsset = Game.g_Movie.Load(strSceneName) as TextAsset;
                if (textAsset == null)
                {
                    Debug.LogError(strSceneName + " no have movie");
                    return false;
                }
                array = textAsset.text.Split('\n');
            }
            if (array != null)
            {
                foreach (string text3 in array)
                {
                    string name = strSceneName + "/" + text3.Trim();
                    text = Utils.LoadTextFile(EngineEx.GetModPath() + "Config/Movie/" + name + ".txt");
                    if (string.IsNullOrEmpty(text))
                    {
                        TextAsset textAsset2 = Game.g_Movie.Load(name) as TextAsset;
                        if (textAsset2 != null)
                        {
                            text = textAsset2.text;
                        }
                    }
                    if (!string.IsNullOrEmpty(text))
                    {
                        JsonReader jsonReader = new JsonReader(text);
                        MovieEventGroupJson movieEventGroupJson = jsonReader.Deserialize<MovieEventGroupJson>();
                        MovieEventGroup item = movieEventGroupJson.ToPlayMode();
                        __instance.movieEventGroupList.Add(item);
                    }
                }
            }
            return false;
        }
    }

    /// 修正剧本命令 以及 处理自定义剧本命令
    [HarmonyPatch(typeof(MovieEventMap), "PlayMovieNode")]
    class Patch_MovieEventMap_PlayMovieNode
    {
        static bool Prefix(MovieEventMap __instance, ref MovieEventNode men)
        {
            if (men == null)
            {
                return false;
            }
            var name = men.strEventName;
            if (name == null || !name.StartsWith("Custom_"))
            {
                if (men.strActorName == "Xyg_NEW_Xuan")
                {
                    men.strActorName = GameGlobal.m_strMainPlayer;
                    if (men.strLookatName == "Xyg_NEW_Xuan") men.strLookatName = GameGlobal.m_strMainPlayer;
                }
                return true;
            }
            var nextClipList = Utils.GetField(__instance, "NextClipList") as List<int>;
            bool bMovieStop = (bool)Utils.InvokeMethod(__instance, "CheckMovieStop", men.GroupID);
            if (men.TriggerNodeID >= 0 && !bMovieStop)
            {
                nextClipList.Add(men.TriggerNodeID);
            }
            if (men.NextNodeID >= 0 && !bMovieStop)
            {
                nextClipList.Add(men.NextNodeID);
            }
            nextClipList.Remove(men.NodeID);

            if (name == "Custom_Difficulty")
            {
                string info = string.Format("游戏难度选择: [c][FF0000]{0}[-][/c]", GlobalEx.difficulty_name[men.iTextOrder]);
                GlobalEx.hero.difficulty = men.iTextOrder;
                EngineEx.AddMessage(info);
                if (men.NextNodeID >= 0)
                {
                    men = Utils.InvokeMethod(__instance, "FindMovieEventNodeByID", men.NextNodeID, men.GroupID) as MovieEventNode;
                    return true; // call PlayMovieNode with the next node.
                }
            }
            else if (men.strEventName == "Custom_CreatePlayer")
            {
                __instance.StartCoroutine(Custom_CreatePlayer(__instance, men));
            }
            else
            {
                Console.WriteLine("Error Custom MovieEventNode! ID = {1} name = {0}", men.NodeID, men.strEventName);
            }
            if (men.TriggerNodeID >= 0)
            {
                men = Utils.InvokeMethod(__instance, "FindMovieEventNodeByID", men.TriggerNodeID, men.GroupID) as MovieEventNode;
                return true; // call PlayMovieNode with the trigger node.
            }
            return false;
        }
        static private IEnumerator Custom_CreatePlayer(MovieEventMap instance, MovieEventNode men)
        {
            bool bStartJump = (bool)Utils.GetField(instance, "bStartJump");
            if (!bStartJump)
            {
                var delayClipList = Utils.GetField(instance, "DelayClipList") as List<int>;
                delayClipList.Add(men.NodeID);
                yield return new WaitForSeconds(men.fDelayTime);
                delayClipList.Remove(men.NodeID);
            }
            bool bMovieStop = (bool)Utils.InvokeMethod(instance, "CheckMovieStop", men.GroupID);
            if (!bMovieStop)
            {
                if (men.goActor == null)
                {
                    men.goActor = Utils.InvokeMethod(instance, "FindActor", men.strActorName, men.strActorTag) as GameObject;
                }
                if (bStartJump)
                {
                    // stop movie skipping
                    instance.StartCoroutine(Utils.InvokeMethod(instance, "JumpMovieFadeIn") as IEnumerator);
                }
                m_Instance = instance;
                m_Node = men;
                ModGUI.ShowCreatePlayerUI(CreatePlayerOnDone);
            }
            yield break;
        }
        static private void CreatePlayerOnDone()
        {
            if (m_Node.NextNodeID >= 0)
            {
                var men = Utils.InvokeMethod(m_Instance, "FindMovieEventNodeByID", m_Node.NextNodeID, m_Node.GroupID) as MovieEventNode;
                Utils.InvokeMethod(m_Instance, "PlayMovieNode", men);
            }
        }
        static MovieEventMap m_Instance;
        static MovieEventNode m_Node;
    }
    /// 修正剧本Update
    [HarmonyPatch(typeof(MovieEventMap), "UpdatePlayingNode")]
    class Patch_MovieEventMap_UpdatePlayingNode
    {
        static void Prefix(MovieEventMap __instance, ref MovieEventNode men)
        {
            if (men.strActorName == "Xyg_NEW_Xuan")
            {
                men.strActorName = GameGlobal.m_strMainPlayer;
                men.goActor = Utils.InvokeMethod(__instance, "FindActor", men.strActorName, men.strActorTag) as GameObject;
            }
        }
    }
}
