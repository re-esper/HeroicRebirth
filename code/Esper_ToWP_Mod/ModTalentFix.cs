using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;
using Heluo.Wulin;
using Heluo.Wulin.UI;

namespace ToW_Esper_Plugin
{
    /// 默认开启原采集系天赋效果
    [HarmonyPatch(typeof(MouseEventCube), "ReCheck")]
    class Patch_MouseEventCube_ReCheck
    {
        static void Postfix(MouseEventCube __instance)
        {
            if (!__instance.gameObject.activeSelf) return;
            var effobj = Utils.GetField(__instance, "EffectObj") as GameObject;
            if (effobj != null)
            {
                if (Utils.GetField(__instance, "m_ItemDataNode") != null)
                {
                    var ps = effobj.GetComponent<ParticleSystem>();
                    ps.startColor = new Color(ps.startColor.r, ps.startColor.g, ps.startColor.b, 0.161f * 0.75f);
                    effobj.SetActive(true);
                }
            }
        }
    }
    [HarmonyPatch(typeof(MouseEventCube), "CheckItemShowEffect")]
    class Patch_MouseEventCube_CheckItemShowEffect
    {
        static void Postfix(ref bool __result)
        {
            __result = false;
        }
    }

    /// 重做神农百草, 矿石冶炼, 毒物探索
    /// 默认阅历点获得量增加
    /// 重做天赋 见多识广
    [HarmonyPatch(typeof(RewardDataManager), "DoRewardID")]
    class Patch_RewardDataManager_DoRewardID
    {
        static void Prefix(RewardDataManager __instance, int iRewardID, ref RewardDataNode _RewardDataNode)
        {
            if (_RewardDataNode == null)
            {
                _RewardDataNode = __instance.GetMapRewardNode(iRewardID);
                if (_RewardDataNode == null) return;
            }
            _RewardDataNode = Utils.CreateDeepCopy<RewardDataNode>(_RewardDataNode);
            if (iRewardID == 999999 && _RewardDataNode.m_iRewardID == 999999)
            {
                if (_RewardDataNode.m_MapRewardNodeList.Count != 1) return;
                MapRewardNode mrn = _RewardDataNode.m_MapRewardNodeList[0];
                if (mrn._RewardType == RewardType.AddItem && mrn.m_iMsgID == 210043)
                {
                    ItemDataNode itemDataNode = Game.ItemData.GetItemDataNode((int)(mrn.m_Parameter));
                    if (itemDataNode != null)
                    {
                        float talentvalue = 0;
                        switch (itemDataNode.m_iItemKind)
                        {
                            case 1:
                                talentvalue = GameEx.GetTeamTalentEffectPercentTotal(TalentEffect.ExploreHerbs);
                                break;
                            case 2:
                                talentvalue = GameEx.GetTeamTalentEffectPercentTotal(TalentEffect.ExploreMine);
                                break;
                            case 3:
                                talentvalue = GameEx.GetTeamTalentEffectPercentTotal(TalentEffect.ExplorePoison);
                                break;
                        }
                        if (talentvalue > 0)
                        {
                            mrn.m_iMsgID = 800001;
                            mrn.m_iAmount = Mathf.RoundToInt(mrn.m_iAmount * (1 + talentvalue));
                        }
                    }
                }
            }
            else
            {
                int rewardattr = 0;
                for (int i = 0; i < _RewardDataNode.m_MapRewardNodeList.Count; i++)
                {
                    MapRewardNode mapRewardNode = _RewardDataNode.m_MapRewardNodeList[i];
                    if (mapRewardNode._RewardType == RewardType.AttributePoint)
                    {
                        mapRewardNode.m_iAmount = Mathf.RoundToInt((mapRewardNode.m_iAmount + 1) * 1.3f);
                        rewardattr += mapRewardNode.m_iAmount;
                    }
                }
                if (rewardattr > 0)
                {
                    rewardAttributePoint[iRewardID] = rewardattr;
                }
            }
        }
        static void Postfix(RewardDataManager __instance, int iRewardID, RewardDataNode _RewardDataNode)
        {
            if (rewardAttributePoint.ContainsKey(iRewardID))
            {
                int rewardattr = rewardAttributePoint[iRewardID];
                foreach (var npc in TeamStatus.m_Instance.GetTeamMemberList())
                {
                    if (npc.TalentList.IndexOf(207) != -1)
                    {
                        int point = rewardattr;
                        int[] attr = new int[] { npc.iStr, npc.iCon, npc.iInt, npc.iDex };
                        int[] attrmax = new int[] { npc.iMaxStr, npc.iMaxCon, npc.iMaxInt, npc.iMaxDex };
                        while (point > 0)
                        {
                            if (attr[0] >= attrmax[0] && attr[1] >= attrmax[1] && attr[2] >= attrmax[2] && attr[3] >= attrmax[3])
                            {
                                break;
                            }
                            int idx = UnityEngine.Random.Range(0, 4);
                            while (attr[idx] >= attrmax[idx]) idx = UnityEngine.Random.Range(0, 4);
                            attr[idx]++;
                            point--;
                        }
                        if (point < rewardattr)
                        {
                            npc.iStr = attr[0];
                            npc.iCon = attr[1];
                            npc.iInt = attr[2];
                            npc.iDex = attr[3];
                            npc.setTotalProperty();
                            string text = string.Format(Game.StringTable.GetString(800008), npc._NpcDataNode.m_strNpcName, rewardattr - point);
                            Game.UI.Get<UIMapMessage>().SetMsg(text);
                        }
                    }
                }
                rewardAttributePoint.Remove(iRewardID);
            }
        }
        // 该函数有递归调用情况, 为保证正确执行顺序, 做此workaround
        static Dictionary<int, int> rewardAttributePoint = new Dictionary<int, int>();
    }

    /// 重做妙手空空神偷盗帅算法    
    [HarmonyPatch(typeof(UnitTB), "CheckTalentWhenAttack")]
    class Patch_UnitTB_CheckTalentWhenAttack
    {
        static bool Prefix(UnitTB __instance, UnitTB targetUnit)
        {
            if (targetUnit == null) return false;
            foreach (int iID in __instance.characterData.TalentList)
            {
                TalentNewDataNode talentData = Game.TalentNewData.GetTalentData(iID);
                if (talentData != null)
                {
                    foreach (var talentResultPart in talentData.m_cEffetPartList)
                    {
                        if (talentResultPart == null) continue;
                        if (talentResultPart.m_TalentEffect == TalentEffect.Steal)
                        {
                            if ((bool)Utils.GetField(__instance, "bSteal")) continue;
                            float num = 1f - (float)targetUnit.HP / (float)targetUnit.fullHP;
                            num = Math.Max(num * talentResultPart.iValue / 100f, 0.05f);
                            if (UnityEngine.Random.Range(0f, 1f) < num)
                            {
                                int itemTypeCount = targetUnit.characterData.Itemlist.Count;
                                bool hasMoney = targetUnit.factionID == 0 || targetUnit.characterData.iMoney > 0;
                                if (itemTypeCount == 0 && !hasMoney) return false;

                                int rand = UnityEngine.Random.Range(0, itemTypeCount + (hasMoney ? targetUnit.iBeStealCount / 4 + 1 : 0));
                                if (rand >= itemTypeCount) // 偷到钱了
                                {
                                    int num3;
                                    if (targetUnit.factionID == 0)
                                    {
                                        num3 = BackpackStatus.m_Instance.GetMoney() / UnityEngine.Random.Range(20, 31);
                                        num3 = Math.Min(Math.Max(num3, 50), BackpackStatus.m_Instance.GetMoney());
                                        __instance.characterData.AddMoney(num3);
                                        BackpackStatus.m_Instance.ChangeMoney(-num3);
                                    }
                                    else
                                    {
                                        num3 = targetUnit.characterData.iMoney / UnityEngine.Random.Range(20, 31);
                                        num3 = Math.Min(Math.Max(num3, 50), targetUnit.characterData.iMoney);
                                        targetUnit.characterData.LessMoney(num3);
                                        BackpackStatus.m_Instance.ChangeMoney(num3);
                                    }
                                    string text3 = string.Format(Game.StringTable.GetString(260021), num3);
                                    UINGUI.DisplayMessage(text3);
                                    text3 = string.Format(Game.StringTable.GetString(260019), __instance.unitName, talentData.m_strTalentName) + text3;
                                    UINGUI.BattleMessage(text3);
                                    targetUnit.AddThreatValue(__instance, num3);
                                }
                                else // 偷到宝了
                                {
                                    int npcItemIndexID = targetUnit.characterData.Itemlist[rand].m_iItemID;
                                    ItemDataNode node = Game.ItemData.GetItemDataNode(npcItemIndexID);
                                    if (node != null)
                                    {
                                        if (__instance.factionID == 0)
                                        {
                                            targetUnit.characterData.LessNpcItem(npcItemIndexID, 1);
                                            BackpackStatus.m_Instance.AddPackItem(npcItemIndexID, 1, true);
                                        }
                                        else
                                        {
                                            targetUnit.characterData.LessNpcItem(npcItemIndexID, 1);
                                            __instance.characterData.AddNpcItem(npcItemIndexID, 1);
                                        }
                                        string text4 = string.Format(Game.StringTable.GetString(260020), node.m_strItemName);
                                        UINGUI.DisplayMessage(text4);
                                        text4 = string.Format(Game.StringTable.GetString(260019), __instance.unitName, talentData.m_strTalentName) + text4;
                                        UINGUI.BattleMessage(text4);
                                    }
                                    targetUnit.AddThreatValue(__instance, node.m_iItemSell);
                                    targetUnit.iBeStealCount++;
                                }
                                new EffectOverlay(__instance.thisT.position + new Vector3(0f, 1.5f, 0f), talentData.m_strTalentName.ToString(), _OverlayType.Talent, 0f);
                                Utils.SetField(__instance, "bSteal", true);
                            }
                        }
                    }
                }
            }
            return false;
        }
    }

    /// 新主角天赋 过目不忘
    [HarmonyPatch(typeof(BackpackStatus), "UseItem")]
    class Patch_BackpackStatus_UseItem
    {
        static void Prefix(BackpackStatus __instance, CharacterData characterData, BackpackNewDataNode backpackNewDataNode)
        {
            if (characterData.TalentList.IndexOf(404) == -1) return;
            if (__instance.CheclItemAmount(backpackNewDataNode.ItemID) <= 0) return;
            if (!__instance.CheckItemUse(characterData, backpackNewDataNode._ItemDataNode)) return;

            List<ItmeEffectNode> itmeEffectNodeList = backpackNewDataNode._ItemDataNode.m_ItmeEffectNodeList;
            ItemDataNode.ItemType iItemType = (ItemDataNode.ItemType)backpackNewDataNode._ItemDataNode.m_iItemType;
            if (iItemType != ItemDataNode.ItemType.TipsBook) return;

            for (int i = 0; i < itmeEffectNodeList.Count; i++)
            {
                int iItemType2 = itmeEffectNodeList[i].m_iItemType;
                int iRecoverType = itmeEffectNodeList[i].m_iRecoverType;
                int iValue = itmeEffectNodeList[i].m_iValue;
                if (iItemType2 == 3 && characterData.CheckRoutine(iRecoverType))
                {
                    regain = true;
                    break;
                }
                else if (iItemType2 == 4 && characterData.CheckNeigong(iRecoverType))
                {
                    regain = true;
                    break;
                }
            }
        }
        static void Postfix(BackpackStatus __instance, CharacterData characterData, BackpackNewDataNode backpackNewDataNode)
        {
            if (regain)
            {
                __instance.AddPackItem(backpackNewDataNode.ItemID, 1, false);
                EngineEx.AddMessage(Game.StringTable.GetString(800007));
            }
            regain = false;
        }
        private static bool regain = false;
    }
}
