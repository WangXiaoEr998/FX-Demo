// Shader Graph Baker (Free) <https://u3d.as/3ycp>
// Copyright (c) Amazing Assets <https://amazingassets.world>

using System;
using System.IO;
using System.Linq;
using System.Reflection;

using UnityEngine;
using UnityEditor;


namespace AmazingAssets.ShaderGraphBakerFree.Editor
{
    static internal class EditorUtilities
    {
        static string thisAssetPath = string.Empty;

        static internal void DestroyUnityObject(UnityEngine.Object obj)
        {
            if (obj != null)
            {
                StackTraceLogType save = Application.GetStackTraceLogType(LogType.Error);
                Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);


                if (Application.isEditor)
                    GameObject.DestroyImmediate(obj);
                else
                    GameObject.Destroy(obj);


                Application.SetStackTraceLogType(LogType.Error, save);
            }
        }

        static internal string GetThisAssetProjectPath()
        {

            if (string.IsNullOrEmpty(thisAssetPath))
            {
                string fileName = "AmazingAssets.ShaderGraphBakerFree.Editor";

                string[] assets = AssetDatabase.FindAssets(fileName, null);
                if (assets != null && assets.Length > 0)
                {
                    string currentFilePath = AssetDatabase.GUIDToAssetPath(assets[0]);
                    thisAssetPath = Path.GetDirectoryName(Path.GetDirectoryName(currentFilePath));
                }
                else
                {
                    ShaderGraphBakerDebug.Log(LogType.Error, $"Cannot detect '{ShaderGraphBakerAbout.name}' editor path.");
                }
            }
            return thisAssetPath;
        }
        static internal Texture2D LoadIcon(string name)
        {
            string iconPath = Path.Combine(EditorUtilities.GetThisAssetProjectPath(), "Editor", "Icons", name);
            if (File.Exists(iconPath) == false)
                iconPath += ".png";

            byte[] bytes = File.ReadAllBytes(iconPath);
            Texture2D icon = new Texture2D(2, 2);
            icon.LoadImage(bytes);

            return icon;
        }
        static string RemoveWhiteSpace(this string str)
        {
            return string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }


        static internal string ConvertPathToProjectRelative(string path)
        {
            //Before using this method, make sure path 'is' project relative

            return NormalizePath("Assets" + path.Substring(Application.dataPath.Length));
        }
        static internal bool IsPathProjectRelative(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            return NormalizePath(path).Contains(NormalizePath(Application.dataPath));
        }
        static internal string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;
            else
                return path.Replace("//", "/").Replace("\\\\", "/").Replace("\\", "/");
        }


        static internal float TryParseFloat(string value, float defaultValue)
        {
            if (string.IsNullOrEmpty(value))
                return defaultValue;


            value = new string(value.Where(c => char.IsDigit(c) || c == '.' || c == ',').ToArray());

            float retValue = 0;
            if (float.TryParse(value, out retValue) == false)
                retValue = defaultValue;

            return retValue;
        }
        static internal int TryParseInt(string value, int defaultValue)
        {
            if (string.IsNullOrEmpty(value))
                return defaultValue;


            value = new string(value.Where(c => char.IsDigit(c)).ToArray());

            int retValue = 0;
            if (int.TryParse(value, out retValue) == false)
                retValue = defaultValue;

            return retValue;
        }
    }
}
