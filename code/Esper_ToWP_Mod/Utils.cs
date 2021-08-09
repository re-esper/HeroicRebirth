using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Heluo.Wulin;
using Heluo.Wulin.UI;
using UnityEngine;
using Newtonsoft.Json;
using System.Xml.Serialization;

namespace ToW_Esper_Plugin
{
    static class Utils
    {
        public static Texture2D LoadTexture2D(string sFileName, int width = 1024, int height = 1024)
        {
            if (!File.Exists(sFileName))
            {
                return null;
            }
            FileStream fileStream = new FileStream(sFileName, FileMode.Open, FileAccess.Read);
            fileStream.Seek(0L, SeekOrigin.Begin);
            byte[] array = new byte[fileStream.Length];
            fileStream.Read(array, 0, (int)fileStream.Length);
            fileStream.Close();
            fileStream.Dispose();
            Texture2D texture2D = new Texture2D(width, height);
            if (!texture2D.LoadImage(array))
            {
                return null;
            }
            texture2D.name = Path.GetFileNameWithoutExtension(sFileName);
            return texture2D;
        }
        public static string GetAppPath()
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            return Path.GetDirectoryName(executingAssembly.Location);
        }
        public static object GetField(Type t, string sFieldName, object obj = null)
        {
            object result = null;
            try
            {
                FieldInfo field = t.GetField(sFieldName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (object.Equals(field, null))
                {
                    PropertyInfo property = t.GetProperty(sFieldName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (!object.Equals(property, null))
                    {
                        result = property.GetValue(obj, null);
                    }
                }
                else
                {
                    result = field.GetValue(obj);
                }
            }
            catch
            {
            }
            return result;
        }
        public static object GetField(object obj, string sFieldName)
        {
            if (obj == null)
            {
                return null;
            }
            Type type = obj.GetType();
            return Utils.GetField(type, sFieldName, obj);
        }
        public static bool SetField(object obj, string sFieldName, object val)
        {
            if (obj == null)
            {
                return false;
            }
            Type type = obj.GetType();
            return Utils.SetField(type, sFieldName, val, obj);
        }
        public static bool SetField(Type t, string sFieldName, object val, object obj = null)
        {
            try
            {
                FieldInfo field = t.GetField(sFieldName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (object.Equals(field, null))
                {
                    PropertyInfo property = t.GetProperty(sFieldName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (object.Equals(property, null))
                    {
                        return false;
                    }
                    property.SetValue(obj, val, null);
                }
                else
                {
                    field.SetValue(obj, val);
                }
                return true;
            }
            catch
            {
            }
            return false;
        }
        public static object InvokeMethod(Type t, string sMethodName, params object[] aParams)
        {
            object result;
            try
            {
                result = t.InvokeMember(sMethodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, null, aParams);
            }
            catch (Exception)
            {
                result = null;
            }
            return result;
        }
        public static object InvokeMethod(object obj, string sMethodName, params object[] aParams)
        {
            object result;
            try
            {
                result = obj.GetType().InvokeMember(sMethodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, obj, aParams);
            }
            catch (Exception)
            {
                result = null;
            }
            return result;
        }
        public static string LoadTextFile(string sFileName)
        {
            string result = null;
            if (File.Exists(sFileName))
            {
                try
                {
                    result = File.ReadAllText(sFileName);
                }
                catch
                {
                }
            }
            return result;
        }
        public static void SaveTextFile(string sFileName, string sText, bool bAppend = false, bool bIsUnicode = false)
        {
            Encoding encoding = bIsUnicode ? Encoding.Unicode : Encoding.ASCII;
            using (StreamWriter streamWriter = new StreamWriter(sFileName, bAppend, encoding))
            {
                streamWriter.Write(sText);
                streamWriter.Flush();
                streamWriter.Close();
            }
        }
        public static GameObject FindChild(GameObject parent, string name)
        {
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                var go = parent.transform.GetChild(i).gameObject;
                if (go.name == name) { return go; }
                go = FindChild(go, name);
                if (go != null) return go;
            }
            return null;
        }
        private static void PrintGameObject(GameObject go, bool printComponents)
        {
            if (!printComponents)
            {
                Console.WriteLine(go.name);
                return;
            }
            Console.Write(go.name + "{");
            bool first = true;
            foreach (var comp in go.GetComponents<Component>())
            {
                if (first) first = false;
                else Console.Write(",");
                Console.Write(comp.GetType().ToString());
            }
            Console.WriteLine("}");
        }
        public static GameObject PrintChild(GameObject go, bool printComponents = false, int depth = 0)
        {
            for (int i = 0; i < depth; i++) Console.Write("\t");
            PrintGameObject(go, printComponents);
            for (int i = 0; i < go.transform.childCount; i++)
            {
                PrintChild(go.transform.GetChild(i).gameObject, printComponents, depth + 1);
            }
            return null;
        }
        public static string ToJSON(object o)
        {
            return JsonConvert.SerializeObject(o, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
        public static T CreateDeepCopy<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                XmlSerializer serializer = new XmlSerializer(obj.GetType());
                serializer.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                return (T)serializer.Deserialize(ms);
            }
        }
    }
}
