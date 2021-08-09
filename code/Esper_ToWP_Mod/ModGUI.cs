using System;
using System.Collections.Generic;
using System.Linq;
using Heluo.Wulin;
using Heluo.Wulin.UI;
using UnityEngine;

namespace ToW_Esper_Plugin
{
    public class ModGUI : MonoBehaviour
    {
        private void Start()
        {
        }
        private static float lastPlayerBaseSpeed = 0;
        private static float lastPlayerAddSpeed = 0;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2) && GameGlobal.m_bBattle)
            {
                UINGUI.instance.tlBattleMessage.SetActive(!UINGUI.instance.tlBattleMessage.activeSelf);
            }
            if (Input.GetKeyDown(KeyCode.F3) && GameGlobal.m_bBattle)
            {
                GameEx.switchBattleSpeed();
            }
            if (!GameGlobal.m_bBattle && !GameGlobal.m_bLoading && !GameGlobal.m_bMovie)
            {
                var playerFSM = EngineEx.GetPlayerFSM();
                if (playerFSM != null)
                {
                    float factor = Input.GetKey(KeyCode.LeftShift) ? 1.5f : 1f;
                    float basespd = (GameGlobal.m_bBigMapMode ? 30f : 5f) * factor;
                    float addspd = (GameGlobal.m_bBigMapMode ? 1f : 0.5f) * factor;
                    if (basespd != lastPlayerBaseSpeed || addspd != lastPlayerAddSpeed)
                    {
                        playerFSM.SetSpeed(basespd, addspd);
                        lastPlayerBaseSpeed = basespd;
                        lastPlayerAddSpeed = addspd;
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.F11))
            {
                if (m_DebugConsole != null) m_DebugConsole.toggleVisible();
            }
        }
        private void initCreatePlayerGUI()
        {
            GUI.skin.box.normal.background = GUI.skin.textField.normal.background;

            backgroundTex = Game.g_DevelopBackground.Load("2dtexture/gameui/develop/developbackground/dm99999") as Texture2D;

            string basepath = EngineEx.GetModPath() + "Image/";

            foreach (var id in GlobalEx.mod.male_heads)
            {
                sHeadTexList[0].Add(Game.g_BigHeadBundle.Load("2dtexture/gameui/bighead/b" + id) as Texture2D);
                bHeadTexList[0].Add(Game.g_MapHeadBundle.Load("2dtexture/gameui/maphead/" + id) as Texture2D);
            }
            foreach (var id in GlobalEx.mod.female_heads)
            {
                sHeadTexList[1].Add(Game.g_BigHeadBundle.Load("2dtexture/gameui/bighead/b" + id) as Texture2D);
                bHeadTexList[1].Add(Game.g_MapHeadBundle.Load("2dtexture/gameui/maphead/" + id) as Texture2D);
            }

            GUI.skin.button.normal.background = Utils.LoadTexture2D(basepath + "button2.png");
            GUI.skin.button.hover.background = Utils.LoadTexture2D(basepath + "button1.png");
            GUI.skin.button.active.background = null;

            GUI.skin.label.normal.textColor = new Color(0, 0, 0);
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.skin.textField.normal.textColor = new Color(1, 1, 1);
            GUI.skin.textField.alignment = TextAnchor.MiddleLeft;

            var roleToggleTex = Utils.LoadTexture2D(basepath + "role_roleOn.png");
            styleHead.active.background = roleToggleTex;
            styleHead.hover.background = roleToggleTex;
            var toggleOnTex = Utils.LoadTexture2D(basepath + "toggle_on.png");
            var toggleOffTex = Utils.LoadTexture2D(basepath + "toggle_off.png");
            styleToggleOn.normal.background = toggleOnTex;
            styleToggleOn.hover.background = toggleOnTex;
            styleToggleOff.normal.background = toggleOffTex;
            styleToggleOff.hover.background = toggleOnTex;

            var diceTex = Utils.LoadTexture2D(basepath + "richman_dice06.png");
            var diceTex2 = Utils.LoadTexture2D(basepath + "richman_dice062.png");
            styleDice.normal.background = diceTex;
            styleDice.hover.background = diceTex2;
            styleDice.active.background = diceTex2;

            styleNumber.normal.textColor = new Color(1, 1, 1);
            styleNumber.richText = true;
            styleNumberBack.normal.textColor = new Color(0, 0, 0);

            martialTexList = new Texture2D[7];
            for (int i = 0; i < 7; i++)
            {
                martialTexList[i] = Utils.LoadTexture2D(basepath + martialAssetList[i]);
            }
            var texSelector = Utils.LoadTexture2D(basepath + "selector.png");
            styleWeapon.active.background = texSelector;
            styleWeapon.hover.background = texSelector;
            styleWeapon2.normal.background = texSelector;
            styleWeapon2.hover.background = texSelector;

            styleInfo.normal.textColor = new Color(0.2f, 0.2f, 0.2f);
            styleInfo.richText = true;

            var texDummy = Utils.LoadTexture2D(basepath + "dummy_bg.png");
            styleTalent.alignment = TextAnchor.MiddleLeft;
            styleTalent.normal.background = texDummy;
            styleTalent.normal.textColor = new Color(0.75f, 0.75f, 0.75f);
            styleTalent.hover.background = texDummy;
            styleTalent.hover.textColor = new Color(1, 1, 1);

            int talent_count = Math.Min(GlobalEx.mod.male_talents.Length, GlobalEx.mod.female_talents.Length);
            for (int i = 0; i < talent_count; i++)
            {
                talents_unselected.Add(i);
            }

            styleTip.alignment = TextAnchor.MiddleRight;

            gender = 0;
            nick = GlobalEx.mod.male_nick;
            rollDice();
        }
        private void OnGUI()
        {
            if (!bInited)
            {
                var lbl = UnityEngine.Object.FindObjectOfType<UILabel>();
                if (lbl != null)
                {
                    GUI.skin.font = lbl.trueTypeFont;
                    initCreatePlayerGUI();
                    bInited = true;
                }
            }
            if (bShowVersion) renderVersion();
            if (bShowCreatePlayerUI) renderCreatePlayerGUI();
        }
        private void renderVersion()
        {
            sizeFactor = new Vector2(Screen.width / 1920.0f, Screen.height / 1080.0f);
            GUI.skin.button.fontSize = Screen.height * 50 / 1000;
            GUI.skin.label.fontSize = Screen.height * 20 / 1000;
            GUI.skin.label.normal.textColor = new Color(1, 1, 1);
            GUI.Label(makeRect(20, 1040, 400, 40), GlobalEx.mod.version);
        }

        private void renderCreatePlayerGUI()
        {
            soundId = 0;
            sizeFactor = new Vector2(Screen.width / 1920.0f, Screen.height / 1080.0f);
            int fontSize1 = Screen.height * 44 / 1000; // Title Label
            int fontSize2 = Screen.height * 30 / 1000; // Edit
            int fontSize3 = Screen.height * 50 / 1000; // Title Label
            int fontSize4 = Screen.height * 34 / 1000; // Number

            if (backgroundTex) GUI.DrawTexture(makeRect(0, 60, 1920, 960), backgroundTex, ScaleMode.ScaleAndCrop);

            GUI.skin.label.fontSize = fontSize1;
            GUI.skin.textField.fontSize = fontSize4;
            GUI.skin.button.fontSize = fontSize3;

            GUI.Label(makeRect(60, 140, 100, 60), "姓");
            name1 = GUI.TextField(makeRect(120, 144, 160, 52), name1, 2);
            GUI.Label(makeRect(320, 140, 100, 60), "名");
            name2 = GUI.TextField(makeRect(380, 144, 160, 52), name2, 2);
            GUI.Label(makeRect(580, 140, 120, 60), "称号");
            nick = GUI.TextField(makeRect(684, 144, 280, 52), nick, 5);

            string sound = "audio/ui/sfx/silde03";
            if (GUI.Button(makeRect(598, 302, 43, 46), new GUIContent("", makeId(sound)), gender == 0 ? styleToggleOn : styleToggleOff))
            {
                if (gender == 1 && nick == GlobalEx.mod.female_nick) nick = GlobalEx.mod.male_nick;
                gender = 0;
                head = head % sHeadTexList[0].Count;
            }
            if (GUI.Button(makeRect(772, 302, 43, 46), new GUIContent("", makeId(sound)), gender == 1 ? styleToggleOn : styleToggleOff))
            {
                if (gender == 0 && nick == GlobalEx.mod.male_nick) nick = GlobalEx.mod.female_nick;
                gender = 1;
                head = head % sHeadTexList[1].Count;
            }
            GUI.Label(makeRect(660, 298, 100, 60), "男");
            GUI.Label(makeRect(834, 298, 100, 60), "女");

            for (int i = 0; i < 3; i++)
            {
                if (i < sHeadTexList[gender].Count)
                {
                    GUI.DrawTexture(makeRect(585 + i * 128, 400, 120, 120), sHeadTexList[gender][i], ScaleMode.ScaleAndCrop);
                    if (GUI.Button(makeRect(585 + i * 128, 400, 120, 120), new GUIContent("", makeId(sound)), styleHead))
                    {
                        head = i;
                    }
                }
                if (i + 3 < sHeadTexList[gender].Count)
                {
                    GUI.DrawTexture(makeRect(585 + i * 128, 530, 120, 120), sHeadTexList[gender][i + 3], ScaleMode.ScaleAndCrop);
                    if (GUI.Button(makeRect(585 + i * 128, 530, 120, 120), new GUIContent("", makeId(sound)), styleHead))
                    {
                        head = i + 3;
                    }
                }
            }
            GUI.DrawTexture(makeRect(72, 228, 500, 500), bHeadTexList[gender][head]);

            sound = "audio/ui/sfx/select01";
            if (GUI.Button(makeRect(108, 792, 105, 105), new GUIContent("", makeId(sound)), styleDice))
            {
                rollDice();
                EngineEx.PlaySound("audio/ui/sfx/select06");
            }
            styleNumber.fontSize = fontSize4;
            styleNumberBack.fontSize = fontSize4;
            for (int i = 0; i < 4; ++i)
            {
                int offset = i * 164;
                GUI.Label(makeRect(272 + offset, 780, 120, 60), attributeNames[i]);
                GUI.Label(makeRect(268 + offset + 1, 852 + 1, 50, 160), string.Format("{0}/{1}", attrPoints[i], attrMaxPoints[i]), styleNumberBack);
                GUI.Label(makeRect(268 + offset, 852, 150, 60), string.Format("{0}/<color=#00ff00ff>{1}</color>", attrPoints[i], attrMaxPoints[i]), styleNumber);
            }

            GUI.Label(makeRect(1080, 140, 400, 60), "主武学");
            sound = "audio/ui/sfx/silde03";
            for (int i = 0; i < 7; i++)
            {
                GUI.DrawTexture(makeRect(1084 + i * 100, 220, 62, 62), martialTexList[i]);
                if (GUI.Button(makeRect(1084 + i * 100 - 8, 220 - 8, 80, 80), new GUIContent("", makeId(sound)), weapon == i ? styleWeapon2 : styleWeapon))
                {
                    if (weapon != i)
                    {
                        weapon = i;
                    }
                    EngineEx.PlaySound("audio/ui/sfx/select09");
                }
            }
            styleInfo.fontSize = Screen.height * 24 / 1000;
            GUI.Label(makeRect(1080, 304, 1000, 60), "主武学影响<color=red>可学秘籍</color>，<color=red>装备类型</color>，<color=red>战场造型</color>和<color=red>部分剧情</color>", styleInfo);

            styleTalent.fontSize = fontSize2;
            GUI.Label(makeRect(1080, 368, 200, 60), "选择天赋");
            GUI.Label(makeRect(1280, 392, 200, 40), "剩余<color=red>" + talentPoints.ToString() + "</color>点", styleInfo);
            GUI.Box(makeRect(1088, 436, 320, 440), "");
            scrollViewVector = GUI.BeginScrollView(makeRect(1088, 436, 320, 440), scrollViewVector, makeRect(0, 0, 300, 440));
            int remove_idx = -1;
            for (int i = 0; i < talents_unselected.Count; i++)
            {
                int idx = talents_unselected[i];
                int tid = gender == 0 ? GlobalEx.mod.male_talents[idx] : GlobalEx.mod.female_talents[idx];
                if (GUI.Button(makeRect(4, i * 40, 280, 40), Game.TalentNewData.GetTalentName(tid), styleTalent))
                {
                    EngineEx.PlaySound("audio/ui/sfx/select09");
                    if (talentPoints > 0)
                    {
                        remove_idx = idx;
                        talentPoints--;
                    }
                }
            }
            if (remove_idx != -1)
            {
                talents_unselected.Remove(remove_idx);
                talents_selected.Add(remove_idx);
            }
            GUI.EndScrollView();

            GUI.Label(makeRect(1440, 368, 200, 60), "拥有天赋");
            GUI.Box(makeRect(1448, 436, 320, 440), "");
            scrollViewVector2 = GUI.BeginScrollView(makeRect(1448, 436, 320, 440), scrollViewVector2, makeRect(0, 0, 300, 440));
            remove_idx = -1;
            for (int i = 0; i < talents_selected.Count; i++)
            {
                int idx = talents_selected[i];
                int tid = gender == 0 ? GlobalEx.mod.male_talents[idx] : GlobalEx.mod.female_talents[idx];
                if (GUI.Button(makeRect(4, i * 40, 280, 40), Game.TalentNewData.GetTalentName(tid), styleTalent))
                {
                    EngineEx.PlaySound("audio/ui/sfx/select09");
                    remove_idx = idx;
                    talentPoints++;
                }
            }
            if (remove_idx != -1)
            {
                talents_selected.Remove(remove_idx);
                talents_unselected.Add(remove_idx);
            }
            GUI.EndScrollView();

            sound = "audio/ui/sfx/select20";
            if (GUI.Button(makeRect(1920 - 525, 920, 525, 63), new GUIContent("创建完成　　", makeId(sound))))
            {
                bool ok = true;
                if (string.IsNullOrEmpty(name1) || string.IsNullOrEmpty(name2))
                {
                    tipStartTime = Time.realtimeSinceStartup;
                    tip = "姓名不能为空！";
                    ok = false;
                }
                else if (string.IsNullOrEmpty(nick))
                {
                    tipStartTime = Time.realtimeSinceStartup;
                    tip = "称号不能为空！";
                    ok = false;
                }
                else if (weapon == -1)
                {
                    tipStartTime = Time.realtimeSinceStartup;
                    tip = "必须选择一项主武学！";
                    ok = false;
                }
                EngineEx.PlaySound(ok ? "audio/ui/sfx/select15" : "audio/ui/sfx/select16");
                if (ok)
                {
                    var headstr = (gender == 0 ? GlobalEx.mod.male_heads : GlobalEx.mod.female_heads)[head];
                    var talents = Enumerable.Range(0, talents_selected.Count).Select(i => (gender == 0 ? GlobalEx.mod.male_talents : GlobalEx.mod.female_talents)[talents_selected[i]]).ToArray();
                    GameEx.ApplyHeroData(name1, name2, nick, headstr, gender, attrPoints, attrMaxPoints, weapon, talents);
                    GameEx.FixGameCharacterData();
                    GameEx.FixGameStaticData();
                    GameEx.FixGameStaticData2();
                    bShowCreatePlayerUI = false;
                    if (OnDone != null) OnDone();
                }
            }

            if (Time.realtimeSinceStartup - tipStartTime < 5)
            {
                styleTip.fontSize = fontSize4;
                styleTip.normal.textColor = new Color(1, 0, 0, 1 - (Time.realtimeSinceStartup - tipStartTime) / 5);
                GUI.Label(makeRect(944, 920, 400, 60), tip, styleTip);
            }
            if (Event.current.type == EventType.Repaint && GUI.tooltip != lastToolTip)
            {
                lastToolTip = GUI.tooltip;
                var arr = lastToolTip.Split(',');
                if (arr.Length > 1) EngineEx.PlaySound(arr[1]);
            }
        }
        private void rollDice()
        {
            int points = totalAttrPoints - 40;
            for (int i = 0; i < 4; ++i)
            {
                attrMaxPoints[i] = 10;
            }
            while (points > 0)
            {
                int index = UnityEngine.Random.Range(0, 4);
                int num = 0;
                if (points <= 2)
                {
                    num = points;
                }
                else
                {
                    num = UnityEngine.Random.Range(1, 10);
                }
                attrMaxPoints[index] += num;
                points -= num;
            }
            for (int i = 0; i < 4; ++i)
            {
                attrPoints[i] = (int)(attrMaxPoints[i] * UnityEngine.Random.Range(0.48f, 0.52f));
            }
        }
        private Rect makeRect(float left, float top, float width, float height)
        {
            return new Rect(left * sizeFactor.x, top * sizeFactor.y, width * sizeFactor.x, height * sizeFactor.y);
        }
        private string makeId(string sound)
        {
            soundId++;
            return soundId.ToString() + "," + sound;
        }
        public static void ShowCreatePlayerUI(VoidDelegate onDoneFunc)
        {
            m_Instance.OnDone = onDoneFunc;
            m_Instance.bShowCreatePlayerUI = true;
        }

        public static void Create(bool showVer)
        {
            GameObject objectGUI = GameObject.Find("ModGUI");
            if (objectGUI != null)
            {
                UnityEngine.Object.Destroy(objectGUI);
            }
            objectGUI = new GameObject("ModGUI");
            m_Instance = objectGUI.AddComponent<ModGUI>();
            m_Instance.bShowVersion = showVer;
        }

        public static void CreateDebugConsole()
        {
            GameObject objectGUI = GameObject.Find("cPureConsole");
            if (objectGUI != null)
            {
                UnityEngine.Object.Destroy(objectGUI);
            }
            objectGUI = new GameObject("cPureConsole");
            m_DebugConsole = objectGUI.AddComponent<cPureConsole>();
        }

        public static ModGUI m_Instance;
        public delegate void VoidDelegate();
        private VoidDelegate OnDone = null;

        private Vector2 sizeFactor;
        private Texture2D backgroundTex = null;
        private List<Texture2D>[] sHeadTexList = { new List<Texture2D>(), new List<Texture2D>() };
        private List<Texture2D>[] bHeadTexList = { new List<Texture2D>(), new List<Texture2D>() };

        private Texture2D[] martialTexList;
        private string[] martialAssetList = new string[]
        {
            "sword.png",
            "blade.png",
            "arrow.png",
            "fist.png",
            "gas.png",
            "short.png",
            "pike.png"
        };
        private string[] attributeNames = new string[]
        {
            "力量", "体魄", "意志", "灵巧"
        };
        private GUIStyle styleHead = new GUIStyle();
        private GUIStyle styleToggleOn = new GUIStyle();
        private GUIStyle styleToggleOff = new GUIStyle();
        private GUIStyle styleDice = new GUIStyle();
        private GUIStyle styleNumber = new GUIStyle();
        private GUIStyle styleNumberBack = new GUIStyle();
        private GUIStyle styleWeapon = new GUIStyle();
        private GUIStyle styleWeapon2 = new GUIStyle();
        private GUIStyle styleInfo = new GUIStyle();
        private GUIStyle styleTalent = new GUIStyle();
        private GUIStyle styleTip = new GUIStyle();

        private Vector2 scrollViewVector = Vector2.zero;
        private Vector2 scrollViewVector2 = Vector2.zero;

        private string name1 = "";
        private string name2 = "";
        private string nick = "";
        private int gender = 0;
        private int head = 0;
        private int[] attrPoints = new int[4];
        private int[] attrMaxPoints = new int[4];
        private int totalAttrPoints = 348;
        private int weapon = -1;

        private List<int> talents_unselected = new List<int>();
        private List<int> talents_selected = new List<int>();
        private int talentPoints = 3;

        private string tip = "";
        private float tipStartTime = 0;

        private bool bShowVersion = false;
        private bool bShowCreatePlayerUI = false;
        private bool bInited = false;

        private int soundId;
        private string lastToolTip;

        private static cPureConsole m_DebugConsole = null;
    }
}