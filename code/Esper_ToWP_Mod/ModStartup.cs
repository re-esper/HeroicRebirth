using System;
using System.Collections.Generic;
using System.Linq;
using Heluo.Wulin;
using Heluo.Wulin.UI;
using UnityEngine;

namespace ToW_Esper_Plugin
{
    public class ModStartup : MonoBehaviour
    {
        private void SkipStartupMovie()
        {
            UIMovie uimovie = Game.UI.Get<UIMovie>();
            if (uimovie == null) return;
            if ((string)Utils.GetField(uimovie, "movieName") == "Opening.ogv")
            {
                uimovie.EndPlayMovie();
                bStartupMovieSkipped = true;
            }
            if (GameGlobal.m_bMovie)
            {
                bStartupMovieSkipped = true;
            }
        }
        private void RemoveMainMenuMask()
        {
            UIStart uistart = Game.UI.Get<UIStart>();
            if (uistart == null) return;
            var mask = Utils.GetField(uistart, "m_pMaskTexture01") as Heluo.Wulin.Control;
            mask.Texture = null;
            bMainMenuMaskRemoved = true;
        }
        private void ModInit()
        {
            bModInited = true;
            if (!GlobalEx.Init())
            {
                Console.WriteLine("Mod initialize failed!");
                return;
            }
            GameGlobal.m_strVersion = GlobalEx.ModPath;
            GameEx.ModGameInit();
            Console.WriteLine("Mod initialize okey!");
        }
        private void Update()
        {
            if (!bStartupMovieSkipped) SkipStartupMovie();
            if (!bMainMenuMaskRemoved) RemoveMainMenuMask();
            if (!bModInited) ModInit();
            if (bStartupMovieSkipped && bMainMenuMaskRemoved && bModInited)
            {
                UnityEngine.Object.Destroy(this);
            }
        }
        private bool bStartupMovieSkipped = false;
        private bool bMainMenuMaskRemoved = false;
        private bool bModInited = false;
    }
}
