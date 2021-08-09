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
    /// AI强度固定为10
    [HarmonyPatch(typeof(GameGlobal), "AddBattleDifficulty")]
    class Patch_GameGlobal_AddBattleDifficulty
    {
        static void Postfix(UnitTB __instance)
        {
            GameGlobal.m_iBattleDifficulty = 10;
        }
    }
    [HarmonyPatch(typeof(GameGlobal), "LessBattleDifficulty")]
    class Patch_GameGlobal_LessBattleDifficulty
    {
        static void Postfix(UnitTB __instance)
        {
            GameGlobal.m_iBattleDifficulty = 10;
        }
    }
    /// 增加难度buff
    [HarmonyPatch(typeof(UnitControl), "InsertUnit")]
    class Patch_UnitControl_InsertUnit
    {
        static void Postfix(UnitTB unit, Tile tile, int factionID, int duration)
        {
            int diff = GlobalEx.hero.difficulty;
            if (!GameGlobal.m_bDLCMode && diff > 0 && factionID != GameControlTB.GetPlayerFactionID())
            {
                ConditionNode cond = Game.g_BattleControl.m_battleAbility.GetConditionNode(990000 + diff);
                if (cond != null)
                {
                    unit.unitConditionList.Insert(0, cond.Clone());
                }
            }
        }
    }
    /// 增加难度buff
    [HarmonyPatch(typeof(UnitTB), "UpdateEffectCompare")]
    class Patch_UnitTB_UpdateEffectCompare
    {
        static void Postfix(UnitTB __instance)
        {
            int diff = GlobalEx.hero.difficulty;
            if (!GameGlobal.m_bDLCMode && diff > 0)
            {
                if (__instance.factionID != GameControlTB.GetPlayerFactionID())
                {
                    if (__instance.unitConditionList.Find(x => x.m_iConditionID == 990000 + diff) == null)
                    {
                        ConditionNode cond = Game.g_BattleControl.m_battleAbility.GetConditionNode(990000 + diff);
                        if (cond != null)
                        {
                            __instance.unitConditionList.Insert(0, cond.Clone());
                        }
                    }
                }
            }
        }
    }
    /// 最终伤害/最终免伤机制
    [HarmonyPatch(typeof(AttackInstance), "Process")]
    class Patch_AttackInstance_Process
    {
        static void Postfix(AttackInstance __instance)
        {
            if (GameGlobal.m_bDLCMode) return;
            if (__instance.unitAbility.effectType == _EffectType.Heal || __instance.unitAbility.effectType == _EffectType.Buff) return;
            var atkunit = __instance.srcUnit;
            var defunit = (__instance.protect && __instance.protectUnit != null) ? __instance.protectUnit : __instance.targetUnit;
            int diff = GlobalEx.hero.difficulty;

            var atkunit_str = atkunit.characterData._TotalProperty.Get(CharacterData.PropertyType.Strength);
            float atk_factor = 1 + atkunit_str * 0.0015f;
            if (atkunit.factionID != GameControlTB.GetPlayerFactionID())    
            {
                atk_factor *= (1 + GlobalEx.difficulty_attack[diff]);
            }
            var defunit_con = defunit.characterData._TotalProperty.Get(CharacterData.PropertyType.Constitution);
            var defunit_dex = defunit.characterData._TotalProperty.Get(CharacterData.PropertyType.Dexterity);
            var ar = (defunit_con + defunit_dex) * 0.001f;
            float def_factor = 1 - ar / (1 + ar);
            if (defunit.factionID != GameControlTB.GetPlayerFactionID())
            {
                def_factor *= 1 - GlobalEx.difficulty_resist[diff];
            }
            __instance.damageDone = Mathf.RoundToInt(__instance.damageDone * atk_factor * def_factor);
            __instance.plusDamage = Mathf.RoundToInt(__instance.plusDamage * atk_factor * def_factor);
        }
    }
    /// 难度导致的最终免伤, 影响各种非攻击产生伤害
    [HarmonyPatch(typeof(UnitTB), "ApplyDamage")]
    class Patch_UnitTB_ApplyDamage
    {
        static void Prefix(UnitTB __instance, ref int dmg, UnitTB unitSrc)
        {
            if (unitSrc == null)
            {
                if (__instance.factionID != GameControlTB.GetPlayerFactionID())
                {
                    float def_factor = 1 - GlobalEx.difficulty_resist[GlobalEx.hero.difficulty];
                    dmg = Mathf.RoundToInt(dmg * def_factor);
                }
            }
        }
    }
    /// 四维增强
    [HarmonyPatch(typeof(CharacterData), "setTotalProperty")]
    class Patch_CharacterData_setTotalProperty
    {
        static void Prefix(CharacterData __instance)
        {
            Utils.SetField(__instance, "m_Rate", new float[] { 0, 0, 0, 0 });
        }
        static void Postfix(CharacterData __instance)
        {
            var totalProperty = __instance._TotalProperty;
            // 力量 -> 最终伤害 + 暴击
            int mstr = totalProperty.Get(CharacterData.PropertyType.StrengthMax);
            int str = totalProperty.Get(CharacterData.PropertyType.Strength);
            totalProperty.SetPlus(CharacterData.PropertyType.Critical, (int)(str * Mathf.Max(mstr) * 0.001f));
            // 体魄 -> 大气血 + 最终减伤
            int mcon = totalProperty.Get(CharacterData.PropertyType.ConstitutionMax);
            int con = totalProperty.Get(CharacterData.PropertyType.Constitution);
            totalProperty.SetPlus(CharacterData.PropertyType.MaxHP, (int)(con * Mathf.Max(mcon, 50) * 0.3f));
            // 意志 -> 气血 + 内力
            int mwill = totalProperty.Get(CharacterData.PropertyType.IntelligenceMax);
            int will = totalProperty.Get(CharacterData.PropertyType.Intelligence);
            totalProperty.SetPlus(CharacterData.PropertyType.MaxHP, (int)(will * Mathf.Max(mwill, 50) * 0.15f));
            totalProperty.SetPlus(CharacterData.PropertyType.MaxSP, (int)(will * Mathf.Max(mwill, 50) * 0.15f));
            // 灵巧 -> 反击 + 命中 + 最终减伤
            int mdex = totalProperty.Get(CharacterData.PropertyType.DexterityMax);
            int dex = totalProperty.Get(CharacterData.PropertyType.Dexterity);
            totalProperty.SetPlus(CharacterData.PropertyType.Counter, (int)(dex * Mathf.Max(mdex, 50) * 0.001f));
            totalProperty.SetPlus(CharacterData.PropertyType.DefendDodge, (int)(dex * Mathf.Max(mdex, 50) * 0.001f));
        }
    }
    /// 钢鞭软索合并为短柄 界面修改
    [HarmonyPatch(typeof(UICharacter), "SetCharaDefType")]
    class Patch_UICharacter_SetCharaDefType
    {
        static bool Prefix(UICharacter __instance, int index, CharacterData.PropertyType type, int val)
        {
            var defwidgetlist = Utils.GetField(__instance, "m_DefTypeList") as List<WgCharaDefType>;
            if (defwidgetlist.Count == 8)
            {
                var widget = defwidgetlist[7];
                defwidgetlist.RemoveAt(defwidgetlist.Count - 1);
                UnityEngine.Object.Destroy(widget.gameObject);
            }
            if (type < CharacterData.PropertyType.DefRope)
            {
                defwidgetlist[index].SetDefTypeText(type, val);
            }
            else if (type > CharacterData.PropertyType.DefRope)
            {
                defwidgetlist[index - 1].SetDefTypeText(type, val);
            }
            return false;
        }
    }
    /// 钢鞭软索合并为短柄 战场界面修改
    [HarmonyPatch(typeof(UINGUIUnitInfo), "Show")]
    class Patch_UINGUIUnitInfo_Show
    {
        static void Postfix(UINGUIUnitInfo __instance)
        {
            __instance.lbArmorVal6.text = __instance.lbArmorVal7.text;
            __instance.lbArmorVal7.text = __instance.lbArmorVal8.text;
            __instance.lbArmorVal8.text = "";
            var labelobj = Utils.FindChild(__instance.gameObject, "ArmorLabel6") as GameObject;
            if (labelobj != null)
            {
                var ld = labelobj.GetComponent<LabelData>();
                ld.m_iStringID = 110147;
            }
            labelobj = Utils.FindChild(__instance.gameObject, "ArmorLabel7") as GameObject;
            if (labelobj != null)
            {
                var ld = labelobj.GetComponent<LabelData>();
                ld.m_iStringID = 110148;
            }
            labelobj = Utils.FindChild(__instance.gameObject, "ArmorLabel8") as GameObject;
            if (labelobj != null)
            {
                var ld = labelobj.GetComponent<LabelData>();
                ld.m_iStringID = 800000;
            }
        }
    }
}