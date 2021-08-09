using System;
using System.Collections.Generic;
using Heluo.Wulin;
using UnityEngine;

namespace ToW_Esper_Plugin
{
    public class GameEventEx
    {
        public void Init()
        {
            GameControlTB.onBattleEndE += this.GameControlTB_onBattleEndE;
            GameControlTB.onBattleStartE += this.GameControlTB_onBattleStartE;
            GameControlTB.onNewRoundE += this.GameControlTB_onNewRoundE;
            GameControlTB.onNextTurnE += this.GameControlTB_onNextTurnE;
            UnitTB.onUnitSelectedE += this.UnitTB_onUnitSelectedE;
            Game.LoadLevelOnFinish = (Game.LoadSenceOnFinish)Delegate.Combine(Game.LoadLevelOnFinish, new Game.LoadSenceOnFinish(LoadLevelOnFinish));
        }
        private bool IsNpcUseInBattle(int npcID, List<BattleAreaNode> balist)
        {
            foreach (BattleAreaNode banode in balist)
            {
                foreach (var node in banode.m_EnemyList)
                {
                    if (node.m_iCharID == npcID) return true;
                }
                foreach (var node in banode.m_TeamList)
                {
                    if (node.m_iCharID == npcID) return true;
                }
            }
            return false;
        }
        private void LoadLevelOnFinish()
        {            
            if (GameGlobal.m_bBigMapMode)
            {
                var model = GlobalEx.hero.gender == 0 ? GlobalEx.mod.male_model : GlobalEx.mod.female_model;
                GameEx.CreateBigMapModel(model);
            }
            ModGUI.Create(false);
            //PrintNpcRandomEvent();
#if DEBUG
            ModGUI.CreateDebugConsole();
#endif
        }
        private void PrintNpcRandomEvent()
        {
            var npclist = Game.NpcRandomEventData.m_NpcRandomList;
            var balist = Utils.GetField(BattleControl.instance.m_battleArea, "m_BattleAreaList") as List<BattleAreaNode>;
            foreach (var nn in npclist)
            {
                Console.Write(Game.NpcData.GetNpcName(nn.NpcID));
                Console.Write('\t');
                foreach (var node in nn.m_NpcRandomEvent[0].m_ReandomEventList)
                {
                    var quest = Game.NpcQuestData.GetNPCQuest(node.m_strQuestID);
                    var reward = quest.m_NpcRewardList[0];
                    switch (reward.m_Type)
                    {
                        case NpcRewardType.AddNpcProperty:
                            Console.Write("属性{0}*{1}\t", reward.m_iID, reward.m_iValue);
                            break;
                        case NpcRewardType.NowPracticeExp:
                            Console.Write("练功{0}\t", reward.m_iValue);
                            break;
                        case NpcRewardType.GetNewRoutine:
                            Console.Write("{0}\t", Game.RoutineNewData.GetGetRoutineName(reward.m_iID));
                            break;
                        case NpcRewardType.GetNewNeigong:
                            Console.Write("{0}\t", Game.NeigongData.GetNeigongName(reward.m_iID));
                            break;
                        case NpcRewardType.GetNewWeapon:
                        case NpcRewardType.GetNewArror:
                        case NpcRewardType.GetNewNecklace:
                        case NpcRewardType.GetItem:
                            Console.Write("{0}*{1}\t", Game.ItemData.GetItemName(reward.m_iID), Math.Max(reward.m_iValue, 1));
                            break;
                        case NpcRewardType.GetMoney:
                            Console.Write("钱{0}\t", reward.m_iID);
                            break;
                        case NpcRewardType.AddTalent:
                            Console.Write("{0}\t", Game.TalentNewData.GetTalentName(reward.m_iID));
                            break;
                    }
                    Console.Write("{0}\t", node.m_iWeights);
                }
                Console.Write('\n');
            }
        }
        private void GameControlTB_onBattleStartE()
        {
            ModBattleFix.onBattleStartE();
        }
        private void GameControlTB_onBattleEndE(int vicFactionID)
        {
            ModBattleFix.onBattleEndE();
            Time.timeScale = 1;
        }        
        private void GameControlTB_onNextTurnE()
        {
            GameEx.ApplyBattleSpeed();
        }
        private void GameControlTB_onNewRoundE(int round)
        {
            ModBattleFix.onNewRoundE();
        }
        private void UnitTB_onUnitSelectedE(UnitTB unit)
        {
            ModBattleFix.onUnitSelectedE(unit);
        }

        static public GameEventEx instance = new GameEventEx();
    }
}
