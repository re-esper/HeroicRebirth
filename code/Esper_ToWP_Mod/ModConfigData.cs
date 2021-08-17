using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ToW_Esper_Plugin
{
    public class ModConfigData
    {
        public string version;
        public string male_model;
        public string[] male_heads;
        public string male_nick;
        public string male_desc;
        public int[] male_talents;
        public string male_voices;
        public ModBattleActorData[] male_battle;
        public string female_model;
        public string[] female_heads;
        public string female_nick;
        public string female_desc;
        public int[] female_talents;
        public string female_voices;
        public ModBattleActorData[] female_battle;
        public ModMartialData[] martials;
        public Dictionary<int, int> shop_limits;
        public static ModConfigData CreateFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<ModConfigData>(jsonString);
        }
    }
    public class ModBattleActorData
    {
        public string weapon;
        public string anim;
    }
    public class ModMartialData
    {
        public int weapon;
        public int[] routine;
        public int[] spec_items;
        public string[] spec_talks;
    }

    public class ModSaveData
    {
        public string name1;    // 姓
        public string name2;    // 名
        public string nick;     // 称号
        public int gender; // 0 - 男, 1 - 女
        public string head;
        public int weapon; // 0 - 剑, 1 - 刀, 2 - 箭, 3 - 拳, 4 - 气, 5 - 短, 6 - 枪

        public int difficulty;
        public static ModSaveData CreateFromJSON(string jsonString)
        {
            Console.WriteLine("Load from json " + jsonString);
            return JsonConvert.DeserializeObject<ModSaveData>(jsonString);
        }
        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }
    }
}
