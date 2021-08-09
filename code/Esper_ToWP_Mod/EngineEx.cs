using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Reflection;
using System.Threading;
using Heluo.Wulin;
using Heluo.Wulin.UI;
using UnityEngine;

namespace ToW_Esper_Plugin
{
    public class EngineEx
    {
        public static string GetModPath()
        {
            return Game.g_strDataPathToApplicationPath + "Mods/" + GlobalEx.ModPath + "/";
        }
        public static bool PlaySoundFile(string filename)
        {
            string filePath = GetModPath() + filename;
            if (!File.Exists(filePath)) return false;
            WWW www = new WWW("file:///" + filePath);
            AudioClip ac = www.audioClip;
            if (ac != null)
            {
                while(!ac.isReadyToPlay) Thread.Sleep(1);
                EngineEx.PlayAudioClip(ac);
                return true;
            }
            return false;
        }
        public static bool PlaySound(string name)
        {
            AudioClip ac = Game.g_AudioBundle.Load(name) as AudioClip;
            if (ac != null)
            {
                EngineEx.PlayAudioClip(ac);
                return true;
            }
            return false;
        }
        private static void PlayAudioClip(AudioClip audioClip)
        {
            float length = audioClip.length;
            if (EngineEx.SoundPlayer == null)
            {
                EngineEx.SoundPlayer = new GameObject("SoundPlayer");
            }
            AudioSource audioSource = EngineEx.SoundPlayer.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = EngineEx.SoundPlayer.AddComponent<AudioSource>();
            }
            audioSource.minDistance = float.MaxValue;
            audioSource.playOnAwake = false;
            audioSource.volume = GameGlobal.m_fSoundValue;
            audioSource.ignoreListenerVolume = true;
            audioSource.clip = audioClip;
            audioSource.loop = false;
            audioSource.Play();
        }
        public static UILayer GetUI(string sTypeName)
        {
            Type type = Game.UI.GetType().Assembly.GetType("Heluo.Wulin.UI." + sTypeName);
            if (object.Equals(type, null))
            {
                return null;
            }
            UILayer result = null;
            if (!Game.UI.TryGetValue(type, out result))
            {
                return null;
            }
            return result;
        }
        public static bool HideUI(string sTypeName)
        {
            UILayer ui = EngineEx.GetUI(sTypeName);
            if (ui == null)
            {
                return false;
            }
            if (EngineEx.IsUIShow(ui))
            {
                ui.Hide();
                return true;
            }
            return false;
        }
        public static bool IsUIShow(UILayer ui)
        {
            return !(ui == null) && ui.isActiveAndEnabled && (bool)Utils.GetField(ui, "m_bShow");
        }
        public static bool IsUIShow(string sTypeName)
        {
            UILayer ui = EngineEx.GetUI(sTypeName);
            return EngineEx.IsUIShow(ui);
        }
        public static GameObject GetPlayer()
        {
            return GameObject.FindGameObjectWithTag("Player");
        }
        public static PlayerFSM GetPlayerFSM()
        {
            if (PlayerController.m_Instance == null)
            {
                return null;
            }
            return Utils.GetField(PlayerController.m_Instance, "m_PlayerFSM") as PlayerFSM;
        }
        public static bool IsInBattle()
        {
            return GameGlobal.m_bBattle && !GameControlTB.battleEnded && !GameControlTB.IsUnitPlacementState();
        }
        public static void AddMessage(string msg)
        {
            if (GameGlobal.m_bBattle)
            {
                UINGUI.DisplayMessage(msg);
                return;
            }
            Game.UI.Get<UIMapMessage>().SetMsg(msg);
        }
        public static string ForamtColor(object s, int color)
        {
            return string.Format("[c][{0:X6}]{1}[/c]", color, s);
        }
        public static Rect GetNGUIWidgetRect(UIWidget widget)
        {
            Rect result = new Rect(0f, (float)(-(float)widget.height), (float)widget.width, (float)widget.height);
            Transform transform = widget.transform;
            while (transform != null)
            {
                result.x += transform.localPosition.x;
                result.y += transform.localPosition.y;
                transform = transform.parent;
            }
            result.x += (float)Screen.width / 2f;
            result.y += (float)Screen.height / 2f;
            return result;
        }

        private static GameObject SoundPlayer = null;
    }
}
