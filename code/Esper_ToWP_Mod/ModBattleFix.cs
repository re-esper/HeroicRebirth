using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Heluo.Wulin;
using Heluo.Wulin.UI;
using System.Collections;

namespace ToW_Esper_Plugin
{
    public class ModBattleFix
    {
        public static void TalentButtonOnClick()
        {
            var unit = Utils.GetField(UINGUI.instance.uiAbilityButtons, "selectedUnit") as UnitTB;
            if (unit == null) return;
            if (!_CharacterTactics.ContainsKey(unit.iUnitID)) return;
            var tacticData = _CharacterTactics[unit.iUnitID];
            var talentData = Game.TalentNewData.GetTalentData(tacticData.iID);
            int cooldown = talentData.m_cEffetPartList[0].iUpDown;
            _CharacterTactics[unit.iUnitID].cooldown = cooldown;
            if (cooldown > 0)
            {
                buttonTacticCD.text = cooldown.ToString();
                buttonTactic.isEnabled = false;
            }
            if (tacticData.type == TacticData.TacticType.BuddhaMercy)
            {
                var tile0 = unit.occupiedTile;
                var tilelist = GridManager.GetTilesWithinRange(tile0, 1, 1);
                ConditionNode cond = Game.g_BattleControl.m_battleAbility.GetConditionNode(GlobalEx.BuddhaMercyBuffID);
                foreach (var tile in tilelist)
                {
                    if (tile.unit != null && !tile.unit.IsDestroyed() && unit.CheckFriendFaction(tile.unit.factionID))
                    {
                        // 增加buff慈悲心
                        tile.unit.unitConditionList.RemoveAll(x => x.m_iConditionID == GlobalEx.BuddhaMercyBuffID);
                        tile.unit.unitConditionList.Add(cond.Clone());
                        tile.unit.animationTB.PlayUseSchedule(tile.unit, talentData.m_cEffetPartList[0].iValue);
                    }
                }
                unit.unitConditionList.Add(cond.Clone());
                unit.StartCoroutine(PlayTacticEffect(unit, talentData.m_cEffetPartList[0].iValue, talentData.m_strTalentName));
            }
            else if (tacticData.type == TacticData.TacticType.FastEat || tacticData.type == TacticData.TacticType.FastDrink)
            {
                List<int> list = new List<int>();
                BackpackStatus.m_Instance.SortBattleItem();
                List<BackpackNewDataNode> sortTeamBackpack = BackpackStatus.m_Instance.GetSortTeamBackpack();
                int itemkind = tacticData.type == TacticData.TacticType.FastEat ? 1 : 2;
                foreach (BackpackNewDataNode backpackNewDataNode in sortTeamBackpack)
                {
                    if (backpackNewDataNode._ItemDataNode.m_iItemType == 4 && backpackNewDataNode._ItemDataNode.m_iItemKind == itemkind)
                    {
                        list.Add(backpackNewDataNode._ItemDataNode.m_iItemID);
                    }
                }
                if (list.Count <= 0)
                {
                    UINGUI.DisplayMessage(Game.StringTable.GetString(tacticData.type == TacticData.TacticType.FastEat ? 800005 : 800006));
                    return;
                }
                int num = UnityEngine.Random.Range(0, list.Count);
                var itemDataNode = Game.ItemData.GetItemDataNode(list[num]);
                BackpackStatus.m_Instance.LessPackItem(itemDataNode.m_iItemID, 1, null);
                Utils.InvokeMethod(unit, "ApplyFood", itemDataNode);
                string text2 = string.Format(Game.StringTable.GetString(tacticData.type == TacticData.TacticType.FastEat ? 260042 : 260044), itemDataNode.m_strItemName);
                UINGUI.DisplayMessage(unit.unitName + text2);
                text2 = string.Format(Game.StringTable.GetString(260019), unit.unitName, talentData.m_strTalentName) + text2;
                UINGUI.BattleMessage(text2);
                unit.StartCoroutine(PlayTacticEffect(unit, talentData.m_cEffetPartList[0].iValue, talentData.m_strTalentName));
            }
            else if (tacticData.type == TacticData.TacticType.QuitCombat)
            {
                var list = UnitControl.GetAllUnitsOfFaction(0);
                if (list.Count <= 1)
                {
                    UINGUI.DisplayMessage(Game.StringTable.GetString(800011));
                    return;
                }
                new EffectOverlay(unit.thisT.position + new Vector3(0f, 1.5f, 0f), talentData.m_strTalentName, _OverlayType.Talent, 0f);
                unit.LeaveBattle(true);
                unit.StartCoroutine(TeamMateJoinWithDelay(unit));
            }
            else if (tacticData.type == TacticData.TacticType.BeastMaster)
            {
                var tile0 = unit.occupiedTile;
                var tilelist = GridManager.GetTilesWithinRange(tile0, 1, 1);
                Tile tileToSummon = null;
                foreach (var tile in tilelist)
                {
                    if (tile.unit == null && tile.walkable) tileToSummon = tile;
                }
                if (tileToSummon == null)
                {
                    UINGUI.DisplayMessage(Game.StringTable.GetString(800012));
                    return;
                }
                unit.StartCoroutine(PlayTacticEffect(unit, talentData.m_cEffetPartList[0].iValue, talentData.m_strTalentName));
                unit.StartCoroutine(SummonBeast(tileToSummon));
            }
        }
        private static IEnumerator PlayTacticEffect(UnitTB unit, int effectID, string text)
        {
            while (!GameControlTB.ActionCommenced())
            {
                yield return null;
            }
            UINGUI.instance.uiAbilityButtons.Hide();
            UINGUI.instance.uiHUD.HideControlUnitInfo();
            unit.animationTB.PlayUseSchedule(unit, effectID);
            yield return new WaitForSeconds(0.2f);
            unit.animationTB.PlayStand();
            new EffectOverlay(unit.thisT.position + new Vector3(0f, 1.5f, 0f), text, _OverlayType.Talent, 0f);
            unit.StartCoroutine(Utils.InvokeMethod(unit, "ActionComplete", 1.8f) as IEnumerator);
            yield break;
        }
        private static IEnumerator TeamMateJoinWithDelay(UnitTB unit)
        {
            var tile = unit.occupiedTile;
            while (!GameControlTB.ActionCommenced())
            {
                yield return null;
            }
            UINGUI.instance.uiAbilityButtons.Hide();
            UINGUI.instance.uiHUD.HideControlUnitInfo();
            yield return new WaitForSeconds(1.5f);
            if (UnitControl.instance.playerUnitsList[0].starting.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, UnitControl.instance.playerUnitsList[0].starting.Count);
                UnitTB unitTB = UnitControl.instance.playerUnitsList[0].starting[index];
                UnitControl.InsertUnit(unitTB, tile, 0, 0);
                UnitControl.instance.playerUnitsList[0].starting.Remove(unitTB);
                var backunit = BattleControl.instance.GenerateBattleUnit(unit.iUnitID, null);
                UnitControl.instance.playerUnitsList[0].starting.Add(backunit);
            }
            unit.StartCoroutine(Utils.InvokeMethod(unit, "ActionComplete", 0.2f) as IEnumerator);
            yield break;
        }
        private static IEnumerator SummonBeast(Tile tileToSummon)
        {
            yield return new WaitForSeconds(1.5f);
            int[] beastTypes = new int[] { 210110, 210109, 210116, 600063, 200037 };
            var beast = BattleControl.instance.GenerateBattleUnit(beastTypes[UnityEngine.Random.Range(0, beastTypes.Length)], null);
            UnitControl.InsertUnit(beast, tileToSummon, 10, 0);
            ConditionNode cond = Game.g_BattleControl.m_battleAbility.GetConditionNode(GlobalEx.BeastMasterBuffID);
            beast.unitConditionList.Add(cond.Clone());
            yield return null;
            UnitTB unitNear = UnitControl.GetNearestHostile(beast);
            beast.RotateToUnit(unitNear);
            yield break;
        }
        public static void onNewRoundE()
        {
            foreach (var item in _CharacterTactics)
            {
                var td = item.Value;
                if (td.cooldown > 0) td.cooldown--;
            }
        }
        public static void onUnitSelectedE(UnitTB unit)
        {
            int uid = unit.iUnitID;
            if (!_CharacterTactics.ContainsKey(uid))
            {
                for (int i = 0; i < unit.characterData.TalentList.Count; i++)
                {
                    int iID = unit.characterData.TalentList[i];
                    if (iID == 308 || iID == 405)
                    {
                        _CharacterTactics[uid] = new TacticData(TacticData.TacticType.BuddhaMercy, iID);
                        break;
                    }
                    else if (iID == 331)
                    {
                        _CharacterTactics[uid] = new TacticData(TacticData.TacticType.FastEat, iID);
                        break;
                    }
                    else if (iID == 338)
                    {
                        _CharacterTactics[uid] = new TacticData(TacticData.TacticType.FastDrink, iID);
                        break;
                    }
                    else if (iID == 306)
                    {
                        _CharacterTactics[uid] = new TacticData(TacticData.TacticType.QuitCombat, iID);
                        break;
                    }
                    else if (iID == 407)
                    {
                        _CharacterTactics[uid] = new TacticData(TacticData.TacticType.BeastMaster, iID);
                        break;
                    }
                }
            }
            bool hasTactic = _CharacterTactics.ContainsKey(uid);
            goButtonTactic.SetActive(hasTactic);
            if (hasTactic)
            {
                goButtonTactic.SetActive(true);
                goLabelTactic.GetComponent<LabelData>().m_iStringID = 0;
                goLabelTactic.GetComponent<UILabel>().text = Game.TalentNewData.GetTalentName(_CharacterTactics[uid].iID);
                if (_CharacterTactics[uid].cooldown > 0)
                {
                    buttonTacticCD.text = _CharacterTactics[uid].cooldown.ToString();
                    buttonTactic.isEnabled = false;
                }
                else
                {
                    buttonTacticCD.text = "";
                    buttonTactic.isEnabled = true;
                }
            }
        }
        public static void CreateTacticButton(GameObject itembtn)
        {
            if (goButtonTactic != null) GameObject.Destroy(goButtonTactic);

            goButtonTactic = NGUITools.AddChild(itembtn.transform.parent.gameObject, itembtn);
            goButtonTactic.name = "ButtonTalent";
            goButtonTactic.transform.localPosition = new Vector3(660, 184, 0);
            buttonTactic = goButtonTactic.GetComponent<UIButton>();
            buttonTactic.normalSprite = "UI_fi_03_03";
            buttonTactic.pressedSprite = "";
            buttonTactic.hoverSprite = "UI_fi_03_03";
            buttonTactic.disabledSprite = "UI_fi_03_03";
            buttonTactic.onClick.Clear();
            buttonTactic.onClick.Add(new EventDelegate(TalentButtonOnClick));

            var labelobj = goButtonTactic.transform.GetChild(0).gameObject;
            var label = labelobj.GetComponent<UILabel>();
            label.pivot = UIWidget.Pivot.Left;
            var pos = labelobj.transform.localPosition;
            pos.x += 9;
            labelobj.transform.localPosition = pos;
            label.width = 400;
            goLabelTactic = labelobj;
            buttonTacticCD = goButtonTactic.transform.GetChild(1).gameObject.GetComponent<UILabel>();

            goButtonTactic.SetActive(false);
        }

        public static void onBattleStartE()
        {
            _CharacterTactics.Clear();
            if (!unitInfoHandlersInstalled)
            {
                unitInfoHandlersInstalled = true;
                UINGUI.instance.Move += onUnitInfoMouseMove;
            }
        }
        public static void onBattleEndE()
        {
            if (unitInfoHandlersInstalled)
            {
                unitInfoHandlersInstalled = false;
                UINGUI.instance.Move -= onUnitInfoMouseMove;
            }
        }

        private static void onUnitInfoMouseMove(Vector2 direction)
        {
            if (GameGlobal.m_bDLCMode) return;
            if (!GameEx.CheckTeamTalent(GlobalEx.ObservantTalentID)) return;
            if (UINGUI.instance.battleControlState != BattleControlState.UnitInfo)
            {
                npcItemsTooltipShown = false;
                return;
            }
            
            UINGUIUnitInfo uiUnitInfo = UINGUI.instance.uiUnitInfo;
            bool flag = EngineEx.GetNGUIWidgetRect(uiUnitInfo.lbIntroduction).Contains(Input.mousePosition);
            if (!uiUnitInfo.neigongTooltipObj.activeSelf && flag)
            {
                var unit = Utils.GetField(uiUnitInfo, "currentUnit") as UnitTB;
                if (unit == null) return;
                uiUnitInfo.lbNeigongTooltip.text = "因天赋[FFC880]明察秋毫[-]察知其携带";
                uiUnitInfo.lbNeigongTooltipDesc.text = GenerateNpcItemsDescription(unit);
                uiUnitInfo.neigongTooltipObj.SetActive(true);
                npcItemsTooltipShown = true;
            }
            else if (npcItemsTooltipShown && !flag && (GameObject)Utils.GetField(uiUnitInfo, "goFocus") != uiUnitInfo.lbNeigongStr.gameObject)
            {
                uiUnitInfo.neigongTooltipObj.SetActive(false);
                npcItemsTooltipShown = false;
            }
        }
        private static string GenerateNpcItemsDescription(UnitTB unit)
        {
            string text = "";
            foreach (NpcItem npcItem in unit.characterData.Itemlist)
            {
                ItemDataNode itemDataNode = Game.ItemData.GetItemDataNode(npcItem.m_iItemID);
                if (itemDataNode != null)
                {
                    text += string.Format("{0} ({1})\n", itemDataNode.m_strItemName, npcItem.m_iCount);
                }
            }
            return text.TrimEnd();
        }

        private static bool unitInfoHandlersInstalled = false;
        private static bool npcItemsTooltipShown = false;
        private static Dictionary<int, TacticData> _CharacterTactics = new Dictionary<int, TacticData>();
        private static GameObject goButtonTactic = null;
        private static GameObject goLabelTactic = null;
        private static UIButton buttonTactic = null;
        private static UILabel buttonTacticCD = null;
    }

    public class TacticData
    {
        public TacticData(TacticType t, int id, int cd = 0)
        {
            type = t;
            iID = id;
            cooldown = cd;
        }
        public int index() { return (int)type; }
        public enum TacticType { BuddhaMercy = 0, FastEat, FastDrink, QuitCombat, BeastMaster };
        public TacticType type;
        public int iID;
        public int cooldown;
    }
    [HarmonyPatch(typeof(UnitTB), "CheckTalentBuddhaMercy")]
    class Patch_UnitTB_CheckTalentBuddhaMercy
    {
        static bool Prefix(UnitTB __instance, ref bool __result)
        {
            var cond = __instance.unitConditionList.Find(x => x.m_iConditionID == GlobalEx.BuddhaMercyBuffID);
            __result = cond != null;
            if (cond != null)
            {
                new EffectOverlay(__instance.thisT.position + new Vector3(0f, 1.5f, 0f), cond.m_strName, _OverlayType.Talent, 0f);
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(UINGUIAbilityButtons), "Awake")]
    class Patch_UINGUIAbilityButtons_Awake
    {
        static void Postfix(UINGUIAbilityButtons __instance)
        {
            var itembtn = Utils.FindChild(__instance.gameObject, "ButtonItem");
            if (itembtn != null)
            {
                ModBattleFix.CreateTacticButton(itembtn);
            }
        }
    }

    /// 连斩重做
    [HarmonyPatch(typeof(UnitTB), "GetUnitTileAbsoluteBuff")]
    class Patch_UnitTB_GetUnitTileAbsoluteBuff
    {
        static bool Prefix(UnitTB __instance, _EffectPartType typePart, UnitAbility uAb, ref bool __result)
        {
            if (typePart == _EffectPartType.KillOneMore && __instance.iDeadPlus > 0)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
    [HarmonyPatch(typeof(UnitTB), "GetAbilityCost")]
    class Patch_UnitTB_GetAbilityCost
    {
        static void Prefix(UnitTB __instance, out int __state)
        {
            __state = __instance.iDeadPlus;
            __instance.iDeadPlus = 0;
        }
        static void Postfix(UnitTB __instance, int __state)
        {
            __instance.iDeadPlus = __state;
        }
    }
}
