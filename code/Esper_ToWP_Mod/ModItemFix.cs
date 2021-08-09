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
    [HarmonyPatch(typeof(CtrlShop), "InitAmountPrice")]
    class Patch_CtrlShop_InitAmountPrice
    {
        static void Postfix(CtrlShop __instance, int state)
        {
            if (GameGlobal.m_bDLCMode || state != 4) return;
            var currentNode = Utils.GetField(__instance, "currentNode") as BackpackNewDataNode;
            if (currentNode != null)
            {
                foreach (var dd in GlobalEx.mod.shop_limits)
                {
                    if (currentNode._ItemDataNode.m_iItemID == dd.Key)
                    {
                        var val = "ShopLimit_" + dd.Key.ToString();
                        int buy = Game.Variable[val] == -1 ? 0 : Game.Variable[val];
                        int max = Math.Max(dd.Value - buy, 0);
                        Utils.SetField(__instance, "max", max);
                        __instance.setInputView(0, 0, max);
                        break;
                    }
                }
            }
        }
    }
    [HarmonyPatch(typeof(CtrlShop), "CheckOut")]
    class Patch_CtrlShop_CheckOut
    {
        static void Prefix(CtrlShop __instance, int state, out bool __state)
        {
            __state = false;
            if (GameGlobal.m_bDLCMode || state != 4) return;
            if ((int)Utils.GetField(__instance, "amount") <= 0) return;
            if ((int)Utils.GetField(__instance, "packMoney") < (int)Utils.GetField(__instance, "price")) return;
            __state = true;
        }
        static void Postfix(CtrlShop __instance, int state, bool __state)
        {
            if (__state)
            {
                var currentNode = Utils.GetField(__instance, "currentNode") as BackpackNewDataNode;
                foreach (var dd in GlobalEx.mod.shop_limits)
                {
                    if (currentNode._ItemDataNode.m_iItemID == dd.Key)
                    {
                        var val = "ShopLimit_" + dd.Key.ToString();
                        int buy = Game.Variable[val] == -1 ? 0 : Game.Variable[val];
                        buy += (int)Utils.GetField(__instance, "amount");
                        Game.Variable[val] = buy;
                        break;
                    }
                }
            }
        }
    }
}
