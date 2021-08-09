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
using System.Threading;

namespace ToW_Esper_Plugin
{
    /// 修主角场景模型和背景音乐
    [HarmonyPatch(typeof(Game), "ProducePlayer")]
    class Patch_Game_ProducePlayer
    {
        static void Postfix()
        {
            if ((string)Utils.GetField(typeof(Game), "sceneName") == "M0000_01") return;
            var model = GlobalEx.hero.gender == 0 ? GlobalEx.mod.male_model : GlobalEx.mod.female_model;
            GameEx.CreateModel(model);
            GameGlobal.m_strMainPlayer = model;
            if (emptyAudioClip == null)
            {
                string filePath = EngineEx.GetModPath() + "Audio/empty.ogg";
                if (!File.Exists(filePath)) return;
                WWW www = new WWW("file:///" + filePath);
                emptyAudioClip = www.audioClip;
            }
            NGUITools.PlaySound(emptyAudioClip, GameGlobal.m_fSoundValue, 1f);
        }
        static AudioClip emptyAudioClip = null;
    }
    /// 修主角战场模型
    [HarmonyPatch(typeof(BattleControl), "GenerateBattleUnit")]
    class Patch_BattleControl_GenerateBattleUnit
    {
        static void Postfix(int iUnitID, ref UnitTB __result)
        {
            if (GameGlobal.m_bDLCMode) return;
            if (iUnitID != GlobalEx.HeroID) return;

            int widx = GlobalEx.hero.weapon;
            ModBattleActorData mbad = GlobalEx.hero.gender == 0 ? GlobalEx.mod.male_battle[widx] : GlobalEx.mod.female_battle[widx];
            if (string.IsNullOrEmpty(mbad.weapon)) return;

            var arr = mbad.weapon.Split(',');
            if (arr.Length != 8) return;

            GameObject goPrefab = Game.g_ModelBundle.Load(arr[1] + "_ModelPrefab") as GameObject;
            GameObject goWeaponOwner = UnityEngine.Object.Instantiate(goPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity) as GameObject;
            goWeaponOwner.name = goPrefab.name;

            var goHero = __result.gameObject;
            var weapon = Utils.FindChild(goWeaponOwner, arr[0]);

            var hand = Utils.FindChild(goHero, "Bip001 R Hand");
            if (weapon != null && hand != null)
            {
                weapon.transform.parent = hand.transform;
                var position = new Vector3();
                for (int i = 0; i < 3; i++)
                {
                    position[i] = (float)Convert.ToDouble(arr[2 + i]);
                }
                var angle = new Vector3();
                for (int i = 0; i < 3; i++)
                {
                    angle[i] = (float)Convert.ToDouble(arr[5 + i]);
                }
                weapon.transform.localPosition = position;
                weapon.transform.localEulerAngles = angle;
                //GlobalEx.weaponobj = weapon;
            }
            GameObject.Destroy(goWeaponOwner);
        }
    }
    /// 修主角战场动作
    [HarmonyPatch(typeof(ModelAssetBundle), "LoadAllBattleAction")]
    class Patch_ModelAssetBundle_LoadAllBattleAction
    {
        static bool Prefix(ModelAssetBundle __instance, GameObject goModel)
        {
            if (goModel.name == GameGlobal.m_strMainPlayer)
            {
                int widx = GlobalEx.hero.weapon;
                ModBattleActorData mbad = GlobalEx.hero.gender == 0 ? GlobalEx.mod.male_battle[widx] : GlobalEx.mod.female_battle[widx];
                if (string.IsNullOrEmpty(mbad.anim)) return true;

                string anim = mbad.anim;
                string fileName = anim + "_AnimationText";
                TextAsset textAsset = __instance.Load(fileName) as TextAsset;
                if (textAsset == null)
                {
                    return false;
                }
                string[] array = textAsset.text.Split('\n');
                foreach (string text in array)
                {
                    string text2 = text.Trim();
                    text2 = text2.Replace("\r", string.Empty);
                    if ((text2.IndexOf("die") >= 0 || text2.IndexOf("dodge") >= 0 || text2.IndexOf("idle") >= 0 || text2.IndexOf("run") >= 0 || text2.IndexOf("hurt01") >= 0 || text2.IndexOf("hurt02") >= 0 || text2.IndexOf("stand") >= 0) && goModel.animation[text2] == null)
                    {
                        string fileName2 = anim + "@" + text2;
                        AnimationClip animationClip = __instance.Load(fileName2) as AnimationClip;
                        if (animationClip != null)
                        {
                            goModel.animation.AddClip(animationClip, text2);
                        }
                    }
                }
                // 短柄女主角模型动画有点浮空 硬编码修一下
                if (anim == "TL_bitch")
                {
                    goModel.transform.localPosition = new Vector3(0, -0.1f, 0); 
                }
                return false;
            }
            return true;
        }
    }
    /// 修主角动作
    [HarmonyPatch(typeof(ModelAssetBundle), "LoadAnimation")]
    class Patch_ModelAssetBundle_LoadAnimation
    {
        static bool Prefix(ModelAssetBundle __instance, Animation anim, string clipName)
        {
            string str = string.Empty;
            if (anim[clipName] != null)
            {
                return false;
            }
            NpcCollider component = anim.gameObject.GetComponent<NpcCollider>();
            PlayerController component2 = anim.gameObject.GetComponent<PlayerController>();
            if (component != null)
            {
                str = component.m_strModelName;
            }
            else if (component2 != null)
            {
                str = component2.m_strModelName;
            }
            else
            {
                str = anim.gameObject.name;
            }
            string fileName = str + "@" + clipName;
            AnimationClip animationClip = __instance.Load(fileName) as AnimationClip;
            if (animationClip != null)
            {
                anim.AddClip(animationClip, clipName);
            }
            else if (str == GameGlobal.m_strMainPlayer)
            {
                fileName = "Xyg_NEW_Xuan@" + clipName;
                if (GlobalEx.hero.gender == 1)
                {
                    if (clipName == "act01_salute") fileName = "xiaoyao_purple@act01_salute";
                    if (clipName == "hurt03") fileName = "Menpai_water@hurt03";
                    if (clipName == "act02_sigh") fileName = "xiaoyao_purple@act06_shakehead";
                }
                animationClip = __instance.Load(fileName) as AnimationClip;
                if (animationClip != null)
                {
                    anim.AddClip(animationClip, clipName);
                }
            }
            return false;
        }
    }
    /// 修可剔除荆棘卫紫绫
    [HarmonyPatch(typeof(UICharacter), "SetCharaInfo")]
    class Patch_UICharacter_SetCharaInfo
    {
        static void Postfix(UICharacter __instance, string[] text)
        {
            if (text[5] == "200000" || text[5] == "210002")
            {
                var btnList = Utils.GetField(__instance, "m_FunctionBtnList") as List<Heluo.Wulin.Control>;
                btnList[3].GameObject.SetActive(true);
            }
        }
    }
    [HarmonyPatch(typeof(UITeam), "TeamMemberAllDataOnClick")]
    class Patch_UITeam_TeamMemberAllDataOnClick
    {
        static void Postfix(UITeam __instance)
        {
            int npcID = (int)Utils.GetField(__instance, "m_iOnClickNpcID");
            if (npcID == 210002 || npcID == 200000)
            {
                Utils.InvokeMethod(__instance, "SetGetOutStatus", 1f);
                var optionList = Utils.GetField(__instance, "m_TeamSelectOptionList") as List<Heluo.Wulin.Control>;
                optionList[2].Collider.enabled = true;
            }
        }
    }
}
