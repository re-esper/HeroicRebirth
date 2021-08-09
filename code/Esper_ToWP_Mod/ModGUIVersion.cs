using System;
using System.Collections.Generic;
using Heluo.Wulin;
using Heluo.Wulin.UI;
using UnityEngine;

namespace ToW_Esper_Plugin
{
    public class ModGUIVersion : MonoBehaviour
    {
        private void Start()
        {
        }
        private void Update()
        {
        }
        private void onOk()
        {

        }
        private void onFail()
        {

        }
        private void initCreatePlayerGUI(Font font)
        {
            GUI.skin.font = font;

            backgroundTex = Game.g_DevelopBackground.Load("2dtexture/gameui/develop/developbackground/dm99999") as Texture2D;
            smallHeadTexList = new Texture2D[6];
            bigHeadTexList = new Texture2D[6];
            for (int i = 0; i < 6; i++)
            {
                smallHeadTexList[i] = Utils.LoadTexture2D("Image/maphead/" + smallHeadAssetList[i] + ".png") as Texture2D;
                bigHeadTexList[i] = Utils.LoadTexture2D("Image/maphead/" + bigHeadAssetList[i] + ".png") as Texture2D;
            }

            GUI.skin.button.normal.background = Utils.LoadTexture2D("Image/button2.png");
            GUI.skin.button.hover.background = Utils.LoadTexture2D("Image/button1.png");

            GUI.skin.label.normal.textColor = new Color(0, 0, 0);
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            GUI.skin.textField.normal.textColor = new Color(1, 1, 1);
            GUI.skin.textField.alignment = TextAnchor.MiddleLeft;

            var roleToggleTex = Utils.LoadTexture2D("Image/role_roleOn.png");
            styleHead.active.background = roleToggleTex;
            styleHead.hover.background = roleToggleTex;
            var toggleOnTex = Utils.LoadTexture2D("Image/toggle_on.png");
            var toggleOffTex = Utils.LoadTexture2D("Image/toggle_off.png");
            styleToggleOn.normal.background = toggleOnTex;
            styleToggleOn.hover.background = toggleOnTex;
            styleToggleOff.normal.background = toggleOffTex;
            styleToggleOff.hover.background = toggleOnTex;

            var diceTex = Utils.LoadTexture2D("Image/richman_dice06.png");
            var diceTex2 = Utils.LoadTexture2D("Image/richman_dice062.png");
            styleDice.normal.background = diceTex;
            styleDice.hover.background = diceTex2;
            styleDice.active.background = diceTex2;

            styleNumber.normal.textColor = new Color(1, 1, 1);
            styleNumber2.normal.textColor = new Color(0, 1, 0);
            styleNumber3.normal.textColor = new Color(0, 0, 0);

            martialTexList = new Texture2D[7];
            for (int i = 0; i < 7; i++)
            {
                martialTexList[i] = Utils.LoadTexture2D(martialAssetList[i]);
            }
        }
        private void OnGUI()
        {
            if (!bInited)
            {
                var tmplbl = UnityEngine.Object.FindObjectOfType<UILabel>();
                if (tmplbl != null)
                {
                    initCreatePlayerGUI(tmplbl.trueTypeFont);
                    bInited = true;
                }
            }

            sizeFactor = new Vector2(Screen.width / 1920.0f, Screen.height / 1080.0f);
            int fontSize1 = Screen.height * 44 / 1000; // Title Label
            int fontSize2 = Screen.height * 30 / 1000; // Edit
            int fontSize3 = Screen.height * 50 / 1000; // Title Label
            int fontSize4 = Screen.height * 34 / 1000; // Number
            elementId = 0;

            GUI.skin.button.fontSize = fontSize2;

            if (bShowCreatePlayerUI)
            {
                if (backgroundTex) GUI.DrawTexture(makeRect(0, 60, 1920, 960), backgroundTex, ScaleMode.ScaleAndCrop);

                GUI.skin.label.fontSize = fontSize1;
                GUI.skin.textField.fontSize = fontSize2;
                GUI.skin.button.fontSize = fontSize3;

                GUI.Label(makeRect(60, 140, 100, 60), "姓");
                name1 = GUI.TextField(makeRect(120, 144, 160, 52), name1, 2);
                GUI.Label(makeRect(320, 140, 100, 60), "名");
                name2 = GUI.TextField(makeRect(380, 144, 160, 52), name2, 2);
                GUI.Label(makeRect(580, 140, 120, 60), "称号");
                nick = GUI.TextField(makeRect(684, 144, 280, 52), nick, 5);

                string sound = "audio/ui/sfx/silde03";
                if (GUI.Button(makeRect(598, 302, 43, 46), new GUIContent("", makeSID(sound)), sex == 0 ? styleToggleOn : styleToggleOff))
                {
                    sex = 0;
                }
                if (GUI.Button(makeRect(772, 302, 43, 46), new GUIContent("", makeSID(sound)), sex == 1 ? styleToggleOn : styleToggleOff))
                {
                    sex = 1;
                }
                GUI.Label(makeRect(660, 298, 100, 60), "男");
                GUI.Label(makeRect(834, 298, 100, 60), "女");

                for (int i = 0; i < 3; i++)
                {
                    GUI.DrawTexture(makeRect(585 + i * 128, 400, 120, 120), smallHeadTexList[i], ScaleMode.ScaleAndCrop);
                    if (GUI.Button(makeRect(585 + i * 128, 400, 120, 120), new GUIContent("", makeSID(sound)), styleHead))
                    {
                        head = i;
                    }
                    GUI.DrawTexture(makeRect(585 + i * 128, 530, 120, 120), smallHeadTexList[i + 3], ScaleMode.ScaleAndCrop);
                    if (GUI.Button(makeRect(585 + i * 128, 530, 120, 120), new GUIContent("", makeSID(sound)), styleHead))
                    {
                        head = i + 3;
                    }
                }
                GUI.DrawTexture(makeRect(72, 228, 500, 500), bigHeadTexList[head]);

                sound = "audio/ui/sfx/select01";
                if (GUI.Button(makeRect(108, 792, 105, 105), new GUIContent("", makeSID(sound)), styleDice))
                {
                }
                styleNumber.fontSize = fontSize4;
                styleNumber2.fontSize = fontSize4;
                styleNumber3.fontSize = fontSize4;
                for (int i = 0; i < 4; ++i)
                {
                    int offset = i * 164;
                    GUI.Label(makeRect(272 + offset, 780, 120, 60), attributeNames[i]);
                    GUI.Label(makeRect(266 + offset + 1, 852 + 1, 50, 60), "50", styleNumber3);
                    GUI.Label(makeRect(266 + offset, 852, 50, 60), "50", styleNumber);
                    
                    GUI.Label(makeRect(312 + offset, 852, 50, 60), "/", styleNumber);

                    GUI.Label(makeRect(338 + offset + 1, 852 + 1, 50, 60), "100", styleNumber3);
                    GUI.Label(makeRect(338 + offset, 852, 50, 60), "100", styleNumber2);
                }


                GUI.Label(makeRect(1120, 140, 400, 60), "主武学");
                for (int i = 0; i < 7; i++)
                {
                    GUI.DrawTexture(makeRect(1124 + i * 100, 220, 62, 62), martialTexList[i]);
                }


                sound = "audio/ui/sfx/select03";
                GUI.Button(makeRect(1920 - 525, 920, 525, 63), new GUIContent("创建完成　　", makeSID(sound)));

                if (Event.current.type == EventType.Repaint && GUI.tooltip != lastToolTip)
                {
                    lastToolTip = GUI.tooltip;
                    if (!string.IsNullOrEmpty(lastToolTip))
                    {
                        var arr = lastToolTip.Split(',');
                        if (arr.Length > 1) EngineEx.PlaySound(arr[1]);
                    }
                }
            }

            if (GUI.Button(new Rect(0, 0, 100, 40), "我的侠道 v1.0.0"))
            {
                bShowCreatePlayerUI = !bShowCreatePlayerUI;
            }
        }

        private Rect makeRect(float left, float top, float width, float height)
        {
            return new Rect(left * sizeFactor.x, top * sizeFactor.y,
                width * sizeFactor.x, height * sizeFactor.y);
        }
        private string makeSID(string sound)
        {
            elementId++;
            return Convert.ToInt32(elementId) + "," + sound;
        }

        public static void Create()
        {
            GameObject objectGUI = GameObject.Find("ModGUI");
            if (objectGUI != null)
            {
                UnityEngine.Object.Destroy(objectGUI);
            }
            objectGUI = new GameObject("ModGUI");
            objectGUI.AddComponent<ModGUIVersion>();
        }

        private Vector2 sizeFactor;        
        private Texture2D backgroundTex = null;
        private Texture2D[] smallHeadTexList;
        private Texture2D[] bigHeadTexList;
        private string[] smallHeadAssetList = new string[] {
            "2dtexture/gameui/bighead/b900002",
            "2dtexture/gameui/bighead/b900003",
            "2dtexture/gameui/bighead/b900004",
            "2dtexture/gameui/bighead/b900005",
            "2dtexture/gameui/bighead/b900006",
            "2dtexture/gameui/bighead/b210001"
        };
        private string[] bigHeadAssetList = new string[] {
            "2dtexture/gameui/maphead/900002",
            "2dtexture/gameui/maphead/900003",
            "2dtexture/gameui/maphead/900004",
            "2dtexture/gameui/maphead/900005",
            "2dtexture/gameui/maphead/900006",
            "2dtexture/gameui/maphead/210001"
        };
        private Texture2D[] martialTexList;
        private string[] martialAssetList = new string[]
        {
            "Image/sword.png",
            "Image/blade.png",
            "Image/arrow.png",
            "Image/fist.png",
            "Image/gas.png",
            "Image/short.png",
            "Image/pike.png"
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
        private GUIStyle styleNumber2 = new GUIStyle();
        private GUIStyle styleNumber3 = new GUIStyle();
        private string name1 = "楚";
        private string name2 = "天碧";
        private string nick = "虎溪三笑";
        private int sex = 0;
        private int head = 0;
        private bool bShowCreatePlayerUI = false;
        private bool bInited = false;
        private string lastToolTip;
        private int elementId = 0;
    }
}