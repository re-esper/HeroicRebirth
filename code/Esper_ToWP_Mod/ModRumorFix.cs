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
    /// NPC传闻
    [HarmonyPatch(typeof(NpcRandomEvent), "SetNpcDoSomething", new Type[] { typeof(CharacterData) })]
    class Patch_NpcRandomEvent
    {
        static bool Prefix(CharacterData CD)
        {
            if (GameGlobal.m_bDLCMode) return false;
            NpcRandomGroup npcRandomNode = Game.NpcRandomEventData.GetNpcRandomNode(CD.iNpcID);
            if (npcRandomNode == null) return false;
            if (CD.NpcType == eNPCType.NothingCanDo) return false;

            // 当前NPC身上的传闻生效
            string lastNPCQuest = "";
            if (CD.NpcType == eNPCType.DoSomething)
            {
                lastNPCQuest = CD.strNowDoQuest;
                CD.NpcType = eNPCType.Nothing;
                NpcQuestNode npcquest = Game.NpcQuestData.GetNPCQuest(CD.strNowDoQuest);
                if (!npcquest.m_bOnly && !CD.FinishQuestList.Contains(CD.strNowDoQuest))
                {
                    CD.FinishQuestList.Add(CD.strNowDoQuest);
                }
                // 如果是特殊任务, 重置iFinishTime
                if (IsRareQuest(npcquest))
                {
                    // iFinishTime表示上次特殊任务完成回合
                    CD.iFinishTime = YoungHeroTime.m_instance.GetNowRound();
                }
                // 练功练满时练基础能力
                int practiceType = -1;
                if (IsPracticeQuest(npcquest) && IsNPCPracticeDone(CD))
                {
                    if (!IsNPCAttributeFull(CD) && UnityEngine.Random.Range(0f, 1f) > 0.5)
                    {
                        // 练四维 4 点
                        int point = 4;
                        int[] attr = new int[] { CD.iStr, CD.iCon, CD.iInt, CD.iDex };
                        int[] attrmax = new int[] { CD.iMaxStr, CD.iMaxCon, CD.iMaxInt, CD.iMaxDex };
                        while (point > 0)
                        {
                            if (attr[0] >= attrmax[0] && attr[1] >= attrmax[1] && attr[2] >= attrmax[2] && attr[3] >= attrmax[3]) break;                            
                            int idx = UnityEngine.Random.Range(0, 4);
                            while (attr[idx] >= attrmax[idx]) idx = UnityEngine.Random.Range(0, 4);
                            attr[idx]++;
                            point--;
                        }
                        CD.iStr = attr[0]; CD.iCon = attr[1]; CD.iInt = attr[2]; CD.iDex = attr[3];
                        practiceType = 0;
                    }
                    else // 练血内
                    {
                        if (UnityEngine.Random.Range(0f, 1f) > 0.5)
                        {
                            practiceType = 1;
                            CD.SetValue(CharacterData.PropertyType.MaxHP, 200);
                        }
                        else
                        {
                            practiceType = 2;
                            CD.SetValue(CharacterData.PropertyType.MaxSP, 100);
                        }
                    }
                    CD.setTotalProperty();
                }
                else
                {
                    NpcRandomEvent.DoNpcQuestReward(CD, npcquest.m_NpcRewardList);
                }
                if (npcquest.m_bShow)
                {
                    string text = string.Format(npcquest.m_strNote, CD._NpcDataNode.m_strNpcName);
                    Rumor rumor = new Rumor(CD._NpcDataNode.m_strBigHeadImage, CD._NpcDataNode.m_strNpcName, text, npcquest.m_NpcLines);
                    SaveRumor sr = new SaveRumor(CD.iNpcID, npcquest.m_strQuestID, SaveRumor.RumorType.NpcQuset);
                    Game.UI.Get<UIRumor>().AddStrMsg(rumor, sr);
                }
                // 购买高级物品
                if (CD.iMoney >= 20000)
                {
                    Utils.InvokeMethod(typeof(NpcRandomEvent), "BuyItem", CD);
                }
                // 自动补基础药
                if (Game.NpcBuyItem.GetBuyList(CD.iNpcID) != null)
                {
                    int medicine = 0;
                    foreach (NpcItem npcItem in CD.Itemlist)
                    {
                        if (IsMedicine(npcItem.m_iItemID)) medicine += npcItem.m_iCount;
                    }
                    if (medicine < 3)
                    {
                        CD.AddNpcItem(110016, 3 - medicine); // 胭脂泪
                    }
                }
                CD.strNowDoQuest = "";
                // 记录
                if (practiceType != -1)
                {
                    string[] practiceStr = new string[] { "4 点属性", "200 点气血", "100 点内力" };
                    RumorToFile(false, "[{0}]继续练功，得到 {1}", CD._NpcDataNode.m_strNpcName, practiceStr[practiceType]);
                }
                else
                {
                    RumorToFile(false, string.Format(npcquest.m_strNote, "[" + CD._NpcDataNode.m_strNpcName + "]"));
                }
            }
            if (CD.NpcType == eNPCType.Nothing)
            {
                string nextQuestID = "";
                int weightTotal = 0;
                var weightsList = new Dictionary<string, int>();
                for (int i = 0; i < npcRandomNode.m_NpcRandomEvent.Count; i++)
                {
                    var start = npcRandomNode.m_NpcRandomEvent[i].m_strStartQuest;
                    var over = npcRandomNode.m_NpcRandomEvent[i].m_strOverQuest;
                    bool bStart = start == "1" || MissionStatus.m_instance.CheckCollectionQuest(start);
                    bool bNotOver = over == "1" || !MissionStatus.m_instance.CheckCollectionQuest(over);
                    if (!bStart || !bNotOver) return false;

                    int lastRareQuestRound = Math.Max(CD.iFinishTime, 1);
                    float factor = (YoungHeroTime.m_instance.GetNowRound() - lastRareQuestRound) / 4f + 1; // 每 4 回合概率增加1倍 无论这些回合NPC是否行动
                    for (int j = 0; j < npcRandomNode.m_NpcRandomEvent[i].m_ReandomEventList.Count; j++)
                    {
                        string strQuestID = npcRandomNode.m_NpcRandomEvent[i].m_ReandomEventList[j].m_strQuestID;
                        if (strQuestID != lastNPCQuest) // 不重复发生同件事
                        {
                            NpcQuestNode npcquest2 = Game.NpcQuestData.GetNPCQuest(strQuestID);
                            if (npcquest2 == null)
                            {
                                Debug.Log(npcRandomNode.NpcID + "NPCQuest 沒有   " + strQuestID);
                            }
                            else if (!npcquest2.m_bOnly || !NpcRandomEvent.g_NpcQuestList.Contains(npcquest2.m_strQuestID))
                            {
                                if (Game.NpcQuestData.CheckNPCCondition(CD.iNpcID, npcquest2.m_NpcConditionList))
                                {
                                    var nrq = npcRandomNode.m_NpcRandomEvent[i].m_ReandomEventList[j];
                                    int weight = IsRareQuest(npcquest2) ? Mathf.RoundToInt(nrq.m_iWeights * factor) : nrq.m_iWeights;
                                    weightTotal += weight;
                                    if (!weightsList.ContainsKey(nrq.m_strQuestID))
                                    {
                                        weightsList[nrq.m_strQuestID] = weight;
                                    }
                                }
                            }
                        }
                    }
                }
                if (weightsList.Count > 0)
                {
                    int num2 = UnityEngine.Random.Range(0, weightTotal);
                    foreach (KeyValuePair<string, int> keyValuePair in weightsList)
                    {
                        if (num2 < keyValuePair.Value)
                        {
                            nextQuestID = keyValuePair.Key;
                            break;
                        }
                        num2 -= keyValuePair.Value;
                    }
                }
                CD.strNowDoQuest = nextQuestID;
                NpcQuestNode npcquest3 = Game.NpcQuestData.GetNPCQuest(nextQuestID);
                if (npcquest3 == null)
                {
                    Debug.LogError(string.Concat("NPCRandomEvent 表    ", CD.iNpcID, "    ", nextQuestID, " ", weightsList.Count));
                    return false;
                }
                CD.NpcType = eNPCType.DoSomething;
                if (npcquest3.m_bOnly)
                {
                    NpcRandomEvent.g_NpcQuestList.Add(nextQuestID);
                }
                // 记录
                RumorToFile(true, string.Format(npcquest3.m_strNote, "[" + CD._NpcDataNode.m_strNpcName + "]"));
            }
            return false;
        }
        private static bool IsNPCAttributeFull(CharacterData CD)
        {
            return CD.iStr == CD.iMaxStr && CD.iCon == CD.iMaxCon && CD.iInt == CD.iMaxInt && CD.iDex == CD.iMaxDex;
        }
        private static bool IsNPCPracticeDone(CharacterData CD)
        {
            foreach (NpcRoutine npcRoutine in CD.RoutineList)
            {
                if (npcRoutine.iLevel < 10) return false;
            }
            foreach (NpcNeigong npcNeigong in CD.NeigongList)
            {
                if (npcNeigong.iLevel < 10) return false;
            }
            return true;
        }
        private static bool IsPracticeQuest(NpcQuestNode quest)
        {
            foreach (var reward in quest.m_NpcRewardList)
            {
                if (reward.m_Type == NpcRewardType.NowPracticeExp) return true;
            }
            return false;
        }
        private static bool IsRareQuest(NpcQuestNode quest)
        {
            if (!quest.m_bShow) return false;
            foreach (var cond in quest.m_NpcConditionList)
            {
                if (cond._iType == ConditionKind.NpcQuest && cond.m_iAmount == 1) return true;
            }
            return false;
        }
        private static bool IsMedicine(int iItemID)
        {
            var pItemDataNode = Game.ItemData.GetItemDataNode(iItemID);
            if (pItemDataNode.m_iItemType != 4) return false;
            foreach (ItmeEffectNode itmeEffectNode in pItemDataNode.m_ItmeEffectNodeList)
            {
                if (itmeEffectNode.m_iItemType == (int)ItmeEffectNode.ItemEffectType.ReplyHp)
                    return true;
                if (itmeEffectNode.m_iItemType == (int)ItmeEffectNode.ItemEffectType.AddHpRate)
                    return true;
            }
            return false;
        }
        public static void RumorToFile(bool next, string format, params object[] args)
        {
            if (rumorStreamWriter == null)
            {
                var rumorFile = new FileStream("npc_rumors.txt", FileMode.Create);
                rumorStreamWriter = new StreamWriter(rumorFile, Encoding.Unicode);
                rumorFile = new FileStream("npc_rumors_next.txt", FileMode.Create);
                rumorStreamWriter2 = new StreamWriter(rumorFile, Encoding.Unicode);
            }
            var sw = next ? rumorStreamWriter2 : rumorStreamWriter;
            sw.Write(string.Format(format, args));
            sw.Write("\n");
            sw.Flush();
        }
        private static StreamWriter rumorStreamWriter = null;
        private static StreamWriter rumorStreamWriter2 = null;
    }

    [HarmonyPatch(typeof(NpcRandomEvent), "getActionNpcIndex")]
    class Patch_NpcRandomEvent_getActionNpcIndex
    {
        static bool Prefix(NpcRandomEvent __instance, ref int[] __result)
        {
            if (NPC.m_instance == null) return true;
            int count = Game.NpcRandomEventData.m_NpcRandomList.Count;
            if (count == 0) return true;
            int num = 30 + GlobalEx.hero.difficulty * 2; // 依难度30-32-34-36            
            int[] array = new int[num];
            if (NpcRandomEvent.BeDoThings == null) NpcRandomEvent.BeDoThings = new List<int>();
            for (int i = 0; i < num; i++)
            {
                array[i] = UnityEngine.Random.Range(0, count);
                while (NpcRandomEvent.BeDoThings.Contains(array[i]) || IsDuplicated(array, i))
                {
                    array[i] = UnityEngine.Random.Range(0, count);
                }
            }
            __result = array;
            return false;            
        }
        private static bool IsDuplicated(int[] array, int index)
        {
            for (int i = 0; i < index; i++)
            {
                if (array[i] == array[index]) return true;
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(YoungHeroTime), "AddRound")]
    class Patch_YoungHeroTime_AddRound
    {
        static void Postfix()
        {
            if (!first)
            {
                Patch_NpcRandomEvent.RumorToFile(false, "");
                Patch_NpcRandomEvent.RumorToFile(true, "");
            }
            first = false;
            Patch_NpcRandomEvent.RumorToFile(false, "回合 {0}", YoungHeroTime.m_instance.GetNowRound());
            Patch_NpcRandomEvent.RumorToFile(true, "回合 {0}", YoungHeroTime.m_instance.GetNowRound());
        }
        static bool first = true;
    }
}
