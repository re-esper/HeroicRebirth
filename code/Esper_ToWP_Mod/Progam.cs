using System;
using System.IO;
using System.Threading;
using HarmonyLib;
using System.Reflection;
using System.Diagnostics;
using UnityEngine;

namespace ToW_Esper_Plugin
{
    public static class Progam
    {
        public static void Main(string[] args)
        {
            #if DEBUG
            Harmony.DEBUG = true;
            #endif

            // Patching
            Console.WriteLine("ToW_Esper_Plugin patch start!!");
            var harmony = new Harmony("com.esper.towp_mod");
            var assembly = Assembly.GetExecutingAssembly();
            harmony.PatchAll(assembly);
            Console.WriteLine("ToW_Esper_Plugin patch done!!");

            new GameObject("ModStartup").AddComponent<ModStartup>();            
        }     
    }
}