using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ToW_Esper_Plugin
{
    public class ModData
    {
        public string version;
        public string male_model;
        public string[] male_heads;
        public string male_nick;
        public string male_desc;
        public int[] male_talents;
        public string female_model;
        public string[] female_heads;
        public string female_nick;
        public string female_desc;
        public int[] female_talents;
        public ModMartialData[] martials;
        public static ModData CreateFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<ModData>(jsonString);
        }
    }

    public class ModMartialData
    {
        public string item; // 初始装备
        public int[] routine; // 初始套路
        public string weapon; // 武器模型
        public float[] angle; // 武器角度
        public string anim; // 动画模型
    }

    public class ModSaveData
    {
        public string name1;    // 姓
        public string name2;    // 名
        public string nick;     // 称号
        public int gender; // 0 - 男, 1 - 女
        public string head;
        public int weapon;
        public static ModSaveData CreateFromJSON(string jsonString)
        {
            return JsonConvert.DeserializeObject<ModSaveData>(jsonString);
        }
        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }

    }
}
