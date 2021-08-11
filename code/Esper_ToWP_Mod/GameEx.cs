using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Heluo.Wulin;
using Heluo.Wulin.UI;
using UnityEngine;

namespace ToW_Esper_Plugin
{
    public class GameEx
    {
        public static void ModGameInit()
        {
            GameEventEx.instance.Init();
            ModGUI.Create(true);
            GameGlobal.m_bCanJumpMovie = true;
            // 屏蔽大地图Tab键切换模型
            var inputs = Utils.GetField(Game.g_InputManager, "input") as List<global::Control>;
            foreach (var ctrl in inputs)
            {
                var arr = Utils.GetField(ctrl, "key") as KeyCode[];
                arr[(int)Control.Key.ChangeModel] = KeyCode.None;
            }
        }
        public static void ApplyHeroBasicData()
        {
            var data = GlobalEx.hero;
            var npcdata = Game.NpcData.GetNpcData(GlobalEx.HeroID);
            if (npcdata == null)
            {
                Console.WriteLine("FATAL ERROR: Can't find main hero data!");
                return;
            }
            npcdata.m_strNpcName = data.name1 + data.name2;
            npcdata.m_strTitle = data.nick;
            npcdata.m_Gender = data.gender == 0 ? GenderType.Male : GenderType.Female;
            npcdata.m_str3DModel = data.gender == 0 ? GlobalEx.mod.male_model : GlobalEx.mod.female_model;
            npcdata.m_strBigHeadImage = "B" + data.head;
            npcdata.m_strSmallImage = "S" + data.head;
            npcdata.m_strHalfImage = data.head;
            npcdata.m_strMemberImage = "M" + data.head;
            npcdata.m_strDescription = string.Format(data.gender == 0 ? GlobalEx.mod.male_desc : GlobalEx.mod.female_desc, data.nick);
            GlobalEx.hero_fullname = data.name1 + data.name2;
            GlobalEx.hero_martial = new string[] { "剑法", "刀法", "箭器", "拳掌", "气功", "短柄", "枪棍" }[data.weapon];
            Game.Variable["HeroMartialType"] = data.weapon;
            GameGlobal.m_iBattleDifficulty = 10;
        }
        public static void ApplyHeroData(string name1, string name2, string nick, string head, int gender, int[] attrPoints, int[] attrMaxPoints, int weaponIndex, int[] talents)
        {
            // 主角基本数据
            var data = GlobalEx.hero;
            data.name1 = name1;
            data.name2 = name2;
            data.nick = nick;
            data.head = head;
            data.gender = gender;
            data.weapon = weaponIndex;
            var bytes = Encoding.UTF8.GetBytes(GlobalEx.hero.ToJSON());
            var saveVersionFix = Save.m_Instance.m_SaveVersionFix;
            for (int i = 0; i < saveVersionFix.Count; i++)
            {
                if (saveVersionFix[i].StartsWith(GlobalEx.ModTAG))
                {
                    saveVersionFix.RemoveAt(i);
                    break;
                }
            }
            Save.m_Instance.m_SaveVersionFix.Add(GlobalEx.ModTAG + Convert.ToBase64String(bytes));
            ApplyHeroBasicData();
            // 主角四维
            var herodata = NPC.m_instance.GetCharacterData(GlobalEx.HeroID);
            if (herodata == null)
            {
                Console.WriteLine("FATAL ERROR: Can't find main hero data!");
                return;
            }
            herodata.iStr = attrPoints[0];
            herodata.iMaxStr = attrMaxPoints[0];
            herodata.iCon = attrPoints[1];
            herodata.iMaxCon = attrMaxPoints[1];
            herodata.iInt = attrPoints[2];
            herodata.iMaxInt = attrMaxPoints[2];
            herodata.iDex = attrPoints[3];
            herodata.iMaxDex = attrMaxPoints[3];
            // 主角主武学及武器
            herodata.WeaponTypeList.Clear();
            MartialArts martials = herodata._MartialArts;
            for (int i = 0; i < 8; ++i)
            {
                martials.getdata(i, 1);
            }
            WeaponType weapon = GlobalEx.toWeaponType(weaponIndex);
            if (weapon == WeaponType.Sword)
            {
                martials.getdata((int)CharacterData.PropertyType.UseSword - 21, 61);
                martials.getdata((int)CharacterData.PropertyType.UseGas - 21, 41);
                martials.getdata((int)CharacterData.PropertyType.UseArrow - 21, 21);
                herodata.WeaponTypeList.Add(WeaponType.Blade);
            }
            else if (weapon == WeaponType.Blade)
            {
                martials.getdata((int)CharacterData.PropertyType.UseBlade - 21, 61);
                martials.getdata((int)CharacterData.PropertyType.UseGas - 21, 41);
                martials.getdata((int)CharacterData.PropertyType.UseArrow - 21, 21);
                herodata.WeaponTypeList.Add(WeaponType.Sword);
            }
            else if (weapon == WeaponType.Arrow)
            {
                martials.getdata((int)CharacterData.PropertyType.UseArrow - 21, 61);
                martials.getdata((int)CharacterData.PropertyType.UseFist - 21, 41);
                martials.getdata((int)CharacterData.PropertyType.UseGas - 21, 21);
                herodata.WeaponTypeList.Add(WeaponType.Fist);
                herodata.WeaponTypeList.Add(WeaponType.Arrow);
            }
            else if (weapon == WeaponType.Fist)
            {
                martials.getdata((int)CharacterData.PropertyType.UseFist - 21, 51);
                martials.getdata((int)CharacterData.PropertyType.UseGas - 21, 41);
                martials.getdata((int)CharacterData.PropertyType.UseArrow - 21, 21);
                herodata.WeaponTypeList.Add(WeaponType.Fist);
            }
            else if (weapon == WeaponType.Gas)
            {
                martials.getdata((int)CharacterData.PropertyType.UseGas - 21, 51);
                martials.getdata((int)CharacterData.PropertyType.UseFist - 21, 41);
                martials.getdata((int)CharacterData.PropertyType.UseArrow - 21, 21);
                herodata.WeaponTypeList.Add(WeaponType.Fist);
            }
            else if (weapon == WeaponType.Whip)
            {
                martials.getdata((int)CharacterData.PropertyType.UseWhip - 21, 61);
                martials.getdata((int)CharacterData.PropertyType.UseGas - 21, 41);
                martials.getdata((int)CharacterData.PropertyType.UseArrow - 21, 21);
                herodata.WeaponTypeList.Add(WeaponType.Whip);
            }
            else if (weapon == WeaponType.Pike)
            {
                martials.getdata((int)CharacterData.PropertyType.UsePike - 21, 61);
                martials.getdata((int)CharacterData.PropertyType.UseGas - 21, 41);
                martials.getdata((int)CharacterData.PropertyType.UseArrow - 21, 21);
                herodata.WeaponTypeList.Add(WeaponType.Pike);
            }
            herodata.SetEquip(ItemDataNode.ItemType.Weapon, GlobalEx.mod.martials[weaponIndex].weapon);
            NPC.m_instance.AddRoutine(GlobalEx.HeroID, GlobalEx.mod.martials[weaponIndex].routine[0], 4);
            NPC.m_instance.AddRoutine(GlobalEx.HeroID, GlobalEx.mod.martials[weaponIndex].routine[1], 3);
            NPC.m_instance.AddRoutine(GlobalEx.HeroID, GlobalEx.mod.martials[weaponIndex].routine[2], 3);
            // 主角天赋
            herodata.TalentList.Clear();
            foreach (int tid in talents)
            {
                Console.WriteLine("Talent {0}", tid);
                herodata.TalentList.Add(tid);
            }
            // 女主角初始内功
            if (gender == 1)
            {
                NpcNeigong npcNeigong = new NpcNeigong();
                npcNeigong.iSkillID = 90001;
                if (npcNeigong.SetNeigongData(GlobalEx.HeroID))
                {
                    npcNeigong.SetLv(5);
                    herodata.NeigongList.Clear();
                    herodata.NeigongList.Add(npcNeigong);
                    herodata.SetNowUseNeigong(npcNeigong.iSkillID);
                }
            }
            herodata.setTotalProperty();
        }
        public static void FixGameCharacterData()
        {
            var npclist = NPC.m_instance.NpcList;
            foreach (var data in npclist)
            {
                // 钢鞭与软索合并为短柄
                var whip = data._MartialArts.Get(CharacterData.PropertyType.UseWhip);
                var rope = data._MartialArts.Get(CharacterData.PropertyType.UseRope);
                data._MartialArts.Set(CharacterData.PropertyType.UseWhip, Math.Max(whip, rope));
                data._MartialArts.Set(CharacterData.PropertyType.UseRope, 0);
                var whipd = data._MartialDef.Get(CharacterData.PropertyType.DefWhip);
                var roped = data._MartialDef.Get(CharacterData.PropertyType.DefRope);
                data._MartialDef.Set(CharacterData.PropertyType.DefWhip, (whipd + roped) / 2);
                data._MartialArts.Set(CharacterData.PropertyType.DefRope, 1);
                if (data.WeaponTypeList.IndexOf(WeaponType.Rope) >= 0)
                {
                    data.WeaponTypeList.Remove(WeaponType.Rope);
                    data.WeaponTypeList.Add(WeaponType.Whip);
                }
            }
        }
        public static void FixGameStaticData()
        {
            // 短柄相关 修RoutineData
            var routinelist = Game.RoutineNewData.GetRoutineList();
            foreach (var data in routinelist)
            {
                if (data.m_RoutineType == WeaponType.Rope || data.m_RoutineType == WeaponType.Whip)
                {
                    data.m_RoutineType = WeaponType.Whip;
                    foreach (var levelup in data.m_iLevelUP)
                    {
                        if (levelup.m_Type == CharacterData.PropertyType.UseRope)
                        {
                            levelup.m_Type = CharacterData.PropertyType.UseWhip;
                        }
                    }
                    StringBuilder sb = new StringBuilder(data.m_strUpgradeNotes);
                    sb.Replace("钢鞭", "短柄");
                    sb.Replace("软索", "短柄");
                    data.m_strUpgradeNotes = sb.ToString();
                }
            }
            // 短柄相关 修ItemData
            var itemlist = Game.ItemData.GetItemList();
            var shortweaponer = new List<int> { 210048, 200022, 100054, 200044 };
            foreach (var data in itemlist)
            {
                if (data.m_AmsType == WeaponType.Rope)
                {
                    data.m_AmsType = WeaponType.Whip;
                }
                if (data.m_iItemID == 130123) continue; // 乌衣宝典
                if (data.m_iItemID == 140015) continue; // 千里江山书卷
                if (data.m_ItmeEffectNodeList.Count == 0) continue;
                if (data.m_ItmeEffectNodeList[0].m_iItemType == (int)ItmeEffectNode.ItemEffectType.AddRoutine)
                {
                    var rdata = Game.RoutineNewData.GetRoutineNewData(data.m_ItmeEffectNodeList[0].m_iRecoverType);
                    if (rdata.m_RoutineType == WeaponType.Whip)
                    {
                        data.m_CanUseiNpcIDList.Clear();
                        data.m_CanUseiNpcIDList.AddRange(shortweaponer);
                    }
                }
                foreach (var effnode in data.m_ItmeEffectNodeList)
                {
                    if (effnode.m_iItemType == (int)ItmeEffectNode.ItemEffectType.NpcProperty)
                    {
                        if (effnode.m_iRecoverType == (int)CharacterData.PropertyType.UseRope)
                        {
                            effnode.m_iRecoverType = (int)CharacterData.PropertyType.UseWhip;
                        }
                        else if (effnode.m_iRecoverType == (int)CharacterData.PropertyType.DefRope)
                        {
                            effnode.m_iRecoverType = (int)CharacterData.PropertyType.DefWhip;
                        }
                    }
                }
            }
            // 修主角秘籍可用性
            WeaponType herowt = GlobalEx.toWeaponType(GlobalEx.hero.weapon);
            // RoutineNewData.txt中Sword=1, Blade=2, 但游戏代码中Blade=1, Sword=2
            if (herowt == WeaponType.Sword) herowt = WeaponType.Blade;
            else if (herowt == WeaponType.Blade) herowt = WeaponType.Sword;

            var specialMartialBooks = new List<int> {
                130123, // 乌衣宝典
                120097, // 鳄鱼的眼泪
                120098, // 戏子无义
                120112, // 意乱情迷
                120091, // 静莲蝶雨
                120092 // 点血截脉
            };
            foreach (var data in itemlist)
            {
                if (data.m_iItemType != 6) continue;
                if (specialMartialBooks.IndexOf(data.m_iItemID) != -1) continue; // 特殊武功不处理
                if (data.m_ItmeEffectNodeList.Count == 0) continue;
                if (data.m_ItmeEffectNodeList[0].m_iItemType == (int)ItmeEffectNode.ItemEffectType.AddRoutine)
                {
                    var rdata = Game.RoutineNewData.GetRoutineNewData(data.m_ItmeEffectNodeList[0].m_iRecoverType);
                    // 主角不应能学琴类武功
                    if (rdata.m_RoutineType == WeaponType.Gas && rdata.m_strSkillIconName == "UI_fi_02_13") continue;
                    // 以前可以学的拳掌, 现在可能不行了
                    if (rdata.m_RoutineType == WeaponType.Fist && herowt != WeaponType.Arrow && herowt != WeaponType.Fist && herowt != WeaponType.Gas)
                    {
                        data.m_CanUseiNpcIDList.Remove(GlobalEx.HeroID);
                    }
                    // 主角可学主武学武功
                    if (rdata.m_RoutineType == herowt && data.m_CanUseiNpcIDList.Count > 0 && data.m_CanUseiNpcIDList.IndexOf(GlobalEx.HeroID) == -1)
                    {
                        data.m_CanUseiNpcIDList.Add(GlobalEx.HeroID);
                    }
                }
            }
            // 女主角可学意乱情迷/明玉功/九阴飞絮
            if (GlobalEx.hero.gender == 1)
            {
                foreach (var iID in new int[] { 120112, 121012, 121014 })
                {
                    var node = Game.ItemData.GetItemDataNode(iID);
                    if (node != null && node.m_CanUseiNpcIDList.IndexOf(GlobalEx.HeroID) == -1) node.m_CanUseiNpcIDList.Add(GlobalEx.HeroID);
                }
            }
            // 修物品掉落概率
            var battleAreaList = Utils.GetField(BattleControl.instance.m_battleArea, "m_BattleAreaList") as List<BattleAreaNode>;
            foreach (var node in battleAreaList)
            {
                foreach (var drop in node.m_DropItemList)
                {
                    drop.DropRate = Math.Min(drop.DropRate * (1 + GlobalEx.hero.difficulty), 100);
                }
            }
            // 修主角主武学相关物品
            List<int> blockItems = new List<int>();
            for (int i = 0; i < GlobalEx.mod.martials.Length; i++)
            {
                if (i == GlobalEx.hero.weapon) continue;
                blockItems.AddRange(GlobalEx.mod.martials[i].spec_items);
            }
            foreach (int iItemID in GlobalEx.mod.martials[GlobalEx.hero.weapon].spec_items)
            {
                blockItems.Remove(iItemID);
            }
            foreach (var node in itemlist)
            {
                if (blockItems.IndexOf(node.m_iItemID) != -1) node.m_iLock = 1;
            }
            foreach (var node in battleAreaList)
            {
                for (int i = node.m_DropItemList.Count - 1; i >= 0; i--)
                {
                    if (blockItems.IndexOf(node.m_DropItemList[i].ItemID) != -1) node.m_DropItemList.RemoveAt(i);
                }
            }
        }
        public static void FixGameStaticData2()
        {
            // 主角无语音
            var herodata = NPC.m_instance.GetCharacterData(GlobalEx.HeroID);
            if (herodata != null) herodata.sVoicList.Clear();
        }
        public static void CreateModel(string sModelPrefab)
        {
            if (GameGlobal.m_bBattle || GameGlobal.m_bBigMapMode)
            {
                return;
            }
            string sModelName = sModelPrefab;
            if (!sModelPrefab.Contains("_ModelPrefab"))
            {
                sModelPrefab += "_ModelPrefab";
            }
            GameObject player = EngineEx.GetPlayer();
            if (player == null)
            {
                return;
            }
            GameObject gameObject = Game.g_ModelBundle.Load(sModelPrefab) as GameObject;
            if (gameObject == null)
            {
                return;
            }
            GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, new Vector3(0f, 0f, 0f), Quaternion.identity) as GameObject;
            gameObject2.name = gameObject.name;
            gameObject2.tag = "Player";
            gameObject2.layer = 8;
            gameObject2.transform.parent = player.transform.parent;
            gameObject2.transform.localEulerAngles = player.transform.localEulerAngles;
            gameObject2.transform.localPosition = player.transform.localPosition;
            gameObject2.transform.localRotation = player.transform.localRotation;
            MakePlayer(gameObject2, player);
            SetNpcCollider(gameObject2, sModelName);
            MakeSurePortalEffect(gameObject2);
            DestroyPlayer(player);
        }
        public static bool DestroyPlayer(GameObject player)
        {
            if (player == null)
            {
                player = EngineEx.GetPlayer();
                if (player != null) PlayerController.m_Instance = null;
            }
            if (player == null)
            {
                return false;
            }
            player.SetActive(false);
            UnityEngine.Object.Destroy(player);
            return true;
        }
        public static void SetNpcCollider(GameObject gameObject2, string sModelName, GameObject parent = null)
        {
            PlayerController component = (parent != null ? parent : gameObject2).GetComponent<PlayerController>();
            NpcCollider component2 = gameObject2.GetComponent<NpcCollider>();
            if (component != null)
            {
                component.m_strModelName = sModelName;
            }
            if (component2 != null)
            {
                component2.enabled = true;
                component2.m_strModelName = sModelName;
                if (component != null)
                {
                    component2.enabled = false;
                }
            }
        }
        private static void MakeSurePortalEffect(GameObject gameObject2)
        {
            PlayerController component = gameObject2.GetComponent<PlayerController>();
            PortalEffect[] array = UnityEngine.Object.FindObjectsOfType<PortalEffect>();
            foreach (PortalEffect portalEffect in array)
            {
                if (portalEffect != null)
                {
                    Utils.SetField(portalEffect, "Player", component.transform);
                }
            }
        }
        private static bool MakePlayer(GameObject gameObject2, GameObject basePlayer)
        {
            PlayerController playerController = gameObject2.GetComponent<PlayerController>();
            if (playerController == null)
            {
                playerController = gameObject2.AddComponent<PlayerController>();
                if (playerController.gameObject.GetComponent(typeof(CharacterController)) == null)
                {
                    playerController.gameObject.AddComponent(typeof(CharacterController));
                }
                if (playerController.gameObject.GetComponent(typeof(NavMeshAgent)) == null)
                {
                    playerController.gameObject.AddComponent(typeof(NavMeshAgent));
                }
            }
            var comp = gameObject2.GetComponent<NavMeshAgent>();
            var basecomp = basePlayer.GetComponent<NavMeshAgent>();
            comp.speed = basecomp.speed;
            comp.angularSpeed = basecomp.angularSpeed;
            comp.acceleration = basecomp.acceleration;
            comp.stoppingDistance = basecomp.stoppingDistance;
            comp.radius = basecomp.radius;
            PlayerController.m_Instance = playerController;
            Utils.InvokeMethod(playerController, "Awake");
            Utils.InvokeMethod(playerController, "Start");
            playerController.ReSetMoveDate();
            MeleeWeaponTrail[] componentsInChildren = gameObject2.GetComponentsInChildren<MeleeWeaponTrail>();
            foreach (MeleeWeaponTrail meleeWeaponTrail in componentsInChildren)
            {
                meleeWeaponTrail.Emit = false;
            }
            return true;
        }
        public static void CreateBigMapModel(string sModelPrefab, float scale = 1.5f)
        {
            if (!GameGlobal.m_bBigMapMode)
            {
                return;
            }
            string sModelName = sModelPrefab;
            if (!sModelPrefab.Contains("_ModelPrefab"))
            {
                sModelPrefab += "_ModelPrefab";
            }
            GameObject gameObject = Game.g_ModelBundle.Load(sModelPrefab) as GameObject;
            if (gameObject == null)
            {
                return;
            }
            PlayerController.m_Instance.DestoryModel();
            GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, new Vector3(0f, 0f, 0f), Quaternion.identity) as GameObject;
            gameObject2.name = gameObject2.name.Replace("(Clone)", string.Empty);
            GameObject bmplayer = Utils.GetField(BigMapController.m_Instance, "m_Player") as GameObject;
            gameObject2.transform.parent = bmplayer.transform;
            gameObject2.transform.localPosition = Vector3.zero;
            gameObject2.transform.localScale = Vector3.one * scale;
            gameObject2.transform.localEulerAngles = Vector3.zero;
            SetNpcCollider(gameObject2, sModelName, bmplayer);
            EngineEx.GetPlayerFSM().ReSetAnimation();
        }
        private static int LCSubStr(string X, string Y)
        {
            int m = X.Length, n = Y.Length;
            int[,] LCStuff = new int[m + 1, n + 1];
            int result = 0;
            for (int i = 0; i <= m; i++)
            {
                for (int j = 0; j <= n; j++)
                {
                    if (i == 0 || j == 0)
                    {
                        LCStuff[i, j] = 0;
                    }
                    else if (X[i - 1] == Y[j - 1])
                    {
                        LCStuff[i, j] = LCStuff[i - 1, j - 1] + 1;
                        result = Math.Max(result, LCStuff[i, j]);
                    }
                    else
                    {
                        LCStuff[i, j] = 0;
                    }
                }
            }
            return result;
        }
        public static void LoadMapTalkManagerPatch(string filePath)
        {
            if (m_TempMapTalkManager == null)
            {
                m_TempMapTalkManager = new MapTalkManager();
                TextDataManager.m_TextDataManagerList.Remove(m_TempMapTalkManager);
                TextDataManager.m_DLCTextDataManagerList.Remove(m_TempMapTalkManager);
            }
            Utils.InvokeMethod(m_TempMapTalkManager, "LoadFile", filePath);
            List<MapTalkTypeNode> real_mttNodeList = Utils.GetField(Game.MapTalkData, "m_MapTalkTypeNodeList") as List<MapTalkTypeNode>;
            List<MapTalkTypeNode> patch_mttNodeList = Utils.GetField(m_TempMapTalkManager, "m_MapTalkTypeNodeList") as List<MapTalkTypeNode>;
            for (int i = 0; i < patch_mttNodeList.Count; i++)
            {
                MapTalkTypeNode mttNode = Game.MapTalkData.GetMapTalkTypeNode(patch_mttNodeList[i].m_strTalkGroupID);
                if (mttNode == null)
                {
                    real_mttNodeList.Add(patch_mttNodeList[i]);
                }
                else
                {
                    foreach (var node in patch_mttNodeList[i].m_MapTalkNodeList)
                    {
                        int idx = mttNode.m_MapTalkNodeList.FindIndex(item => item.m_iOrder.Equals(node.m_iOrder));
                        if (patch_mttNodeList[i].m_strTalkGroupID.StartsWith("N"))
                        {
                            int best = -1, bestIdx = -1;
                            for (int j = 0; j < mttNode.m_MapTalkNodeList.Count; j++)
                            {
                                var node2 = mttNode.m_MapTalkNodeList[j];
                                if (node.m_iOrder != node2.m_iOrder) continue;
                                if (node.m_strManager == "" && node2.m_strManager != "") continue;
                                int l = LCSubStr(node.m_strManager, node2.m_strManager);
                                if (l > best)
                                {
                                    best = l;
                                    bestIdx = j;
                                }
                            }
                            idx = bestIdx;
                        }
                        if (idx >= 0)
                        {
                            mttNode.m_MapTalkNodeList[idx] = node;
                        }
                        else
                        {
                            mttNode.m_MapTalkNodeList.Add(node);
                        }
                    }
                }
            }
        }
        public static void LoadStringTablePatch(string filePath)
        {
            if (m_tempStringTableManager == null)
            {
                m_tempStringTableManager = new StringTableManager();
                TextDataManager.m_TextDataManagerList.Remove(m_tempStringTableManager);
                TextDataManager.m_DLCTextDataManagerList.Remove(m_tempStringTableManager);
            }
            Utils.InvokeMethod(m_tempStringTableManager, "LoadFile", filePath);
            var realStringTable = Utils.GetField(m_tempStringTableManager, "instance") as StringTableManager;
            var real_stringNodeList = Utils.GetField(realStringTable, "m_StringNodeList") as Dictionary<int, string>;
            var patch_stringNodeList = Utils.GetField(m_tempStringTableManager, "m_StringNodeList") as Dictionary<int, string>;
            foreach (int k in patch_stringNodeList.Keys)
            {
                real_stringNodeList[k] = patch_stringNodeList[k];
            }
        }
        private static MapTalkManager m_TempMapTalkManager = null;
        private static StringTableManager m_tempStringTableManager = null;

        public static float GetTeamTalentEffectPercentTotal(TalentEffect te)
        {
            float total = 0;
            var memberlist = Utils.GetField(TeamStatus.m_Instance, "m_TeamMemberList") as List<CharacterData>;
            for (int i = 0; i < memberlist.Count; i++)
            {
                total += memberlist[i].GetNpcTalentPercentValue(te);
            }
            return total;
        }
        public static bool CheckTeamTalent(int TalentID)
        {
            var memberlist = Utils.GetField(TeamStatus.m_Instance, "m_TeamMemberList") as List<CharacterData>;
            for (int i = 0; i < memberlist.Count; i++)
            {
                if (memberlist[i].TalentList.IndexOf(TalentID) != -1) return true;
            }
            return false;
        }

        public static void ApplyBattleSpeed(int level = -1)
        {
            if (!GameGlobal.m_bBattle) return;
            if (level != -1) battleSpeedLevel = level;
            Time.timeScale = (GameControlTB.IsPlayerTurn() ? playerBattleSpeeds : enemyBattleSpeeds)[battleSpeedLevel];
        }
        public static void switchBattleSpeed()
        {
            battleSpeedLevel = (battleSpeedLevel + 1) % playerBattleSpeeds.Length;
            ApplyBattleSpeed();
            EngineEx.AddMessage(string.Format("战斗速度：[c][FF0000]{0}[-][/c]", battleSpeedTexts[battleSpeedLevel]));
        }
        private static float[] playerBattleSpeeds = new float[] { 1f, 1.5f, 2f };
        private static float[] enemyBattleSpeeds = new float[] { 1f, 2f, 3f };
        private static string[] battleSpeedTexts = new string[] { "普通", "高速", "超高" };
        private static int battleSpeedLevel = 0;
    }
}