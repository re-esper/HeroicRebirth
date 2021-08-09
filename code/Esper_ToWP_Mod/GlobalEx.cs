using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Heluo.Wulin;
using UnityEngine;

namespace ToW_Esper_Plugin
{
    public static class GlobalEx
    {
        public static bool Init()
        {
            if (!File.Exists(EngineEx.GetModPath() + "mod.json")) return false;
            mod = ModConfigData.CreateFromJSON(File.ReadAllText(EngineEx.GetModPath() + "mod.json"));
            return true;
        }
        public static WeaponType toWeaponType(int index)
        {
            return new WeaponType[] { WeaponType.Sword, WeaponType.Blade, WeaponType.Arrow, WeaponType.Fist, WeaponType.Gas, WeaponType.Whip, WeaponType.Pike }[index];
        }
        public static CharacterData.PropertyType toMartialType(int index)
        {
            return new CharacterData.PropertyType[] { CharacterData.PropertyType.UseSword, CharacterData.PropertyType.UseBlade, CharacterData.PropertyType.UseArrow,
                CharacterData.PropertyType.UseFist, CharacterData.PropertyType.UseGas, CharacterData.PropertyType.UseWhip, CharacterData.PropertyType.UsePike }[index];
        }

        public const int HeroID = 210001;
        public const string ModTAG = "Esper:";
        public const int BuddhaMercyBuffID = 990010;
        public const int BeastMasterBuffID = 990011;
        public const int ObservantTalentID = 408;

        public static string ModPath = "HeroicRebirth";
        public static ModConfigData mod;
        public static ModSaveData hero = new ModSaveData();

        public static string hero_fullname;
        public static string hero_martial;

        public static float[] difficulty_attack = new float[] { 0f, 0.5f, 0.75f, 1.0f };
        public static float[] difficulty_resist = new float[] { 0f, 0.3f, 0.4f, 0.5f };
        public static string[] difficulty_name = new string[] { "普通", "困难", "噩梦", "炼狱" };

        //public static GameObject weaponobj = null;
    }
}
