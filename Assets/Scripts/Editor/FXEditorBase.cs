using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// FX编辑器工具基类，提供通用的编辑器功能
/// 作者：黄畅修
/// 创建时间：2025-07-20
/// </summary>
namespace FanXing.Editor
{
    public abstract class FXEditorBase : EditorWindow
    {
        #region 常量定义
        protected const string MENU_ROOT = "FanXing/";
        protected const string CONFIG_PATH = "Assets/StreamingAssets/Configs/";
        protected const string SCRIPTABLE_OBJECT_PATH = "Assets/ScriptableObjects/";
        protected const string RESOURCES_PATH = "Assets/Resources/";
        #endregion

        #region 通用样式
        protected GUIStyle _titleStyle;
        protected GUIStyle _headerStyle;
        protected GUIStyle _buttonStyle;
        protected GUIStyle _boxStyle;
        protected Vector2 _scrollPosition;
        #endregion

        #region 生命周期
        protected virtual void OnEnable()
        {
            InitializeStyles();
            LoadData();
        }

        protected virtual void OnDisable()
        {
            SaveData();
        }
        #endregion

        #region 抽象方法
        /// <summary>
        /// 加载数据
        /// </summary>
        protected abstract void LoadData();

        /// <summary>
        /// 保存数据
        /// </summary>
        protected abstract void SaveData();

        /// <summary>
        /// 绘制GUI
        /// </summary>
        protected abstract void OnGUI();
        #endregion

        #region 样式初始化
        protected virtual void InitializeStyles()
        {
            _titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };

            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft
            };

            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fixedHeight = 25
            };

            _boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10)
            };
        }
        #endregion

        #region 通用工具方法
        /// <summary>
        /// 确保目录存在
        /// </summary>
        protected void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// 绘制标题
        /// </summary>
        protected void DrawTitle(string title)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(title, _titleStyle);
            EditorGUILayout.Space(10);
            DrawHorizontalLine();
            EditorGUILayout.Space(10);
        }

        /// <summary>
        /// 绘制分组标题
        /// </summary>
        protected void DrawHeader(string header)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField(header, _headerStyle);
            EditorGUILayout.Space(5);
        }

        /// <summary>
        /// 绘制水平分割线
        /// </summary>
        protected void DrawHorizontalLine()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.gray);
        }

        /// <summary>
        /// 绘制按钮组
        /// </summary>
        protected void DrawButtonGroup(params (string text, System.Action action)[] buttons)
        {
            EditorGUILayout.BeginHorizontal();
            foreach (var button in buttons)
            {
                if (GUILayout.Button(button.text, _buttonStyle))
                {
                    button.action?.Invoke();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 显示成功消息
        /// </summary>
        protected void ShowSuccessMessage(string message)
        {
            EditorUtility.DisplayDialog("成功", message, "确定");
        }

        /// <summary>
        /// 显示错误消息
        /// </summary>
        protected void ShowErrorMessage(string message)
        {
            EditorUtility.DisplayDialog("错误", message, "确定");
        }

        /// <summary>
        /// 显示确认对话框
        /// </summary>
        protected bool ShowConfirmDialog(string title, string message)
        {
            return EditorUtility.DisplayDialog(title, message, "确定", "取消");
        }

        /// <summary>
        /// 保存ScriptableObject资源
        /// </summary>
        protected void SaveScriptableObject<T>(T obj, string fileName) where T : ScriptableObject
        {
            EnsureDirectoryExists(SCRIPTABLE_OBJECT_PATH);
            string path = Path.Combine(SCRIPTABLE_OBJECT_PATH, fileName + ".asset");
            
            AssetDatabase.CreateAsset(obj, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"ScriptableObject已保存: {path}");
        }

        /// <summary>
        /// 加载ScriptableObject资源
        /// </summary>
        protected T LoadScriptableObject<T>(string fileName) where T : ScriptableObject
        {
            string path = Path.Combine(SCRIPTABLE_OBJECT_PATH, fileName + ".asset");
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        /// <summary>
        /// 导出JSON配置文件
        /// </summary>
        protected void ExportJsonConfig(object data, string fileName)
        {
            EnsureDirectoryExists(CONFIG_PATH);
            string path = Path.Combine(CONFIG_PATH, fileName + ".json");
            string json = JsonUtility.ToJson(data, true);
            
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
            
            Debug.Log($"JSON配置已导出: {path}");
        }

        /// <summary>
        /// 导入JSON配置文件
        /// </summary>
        protected T ImportJsonConfig<T>(string fileName) where T : class, new()
        {
            string path = Path.Combine(CONFIG_PATH, fileName + ".json");
            
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<T>(json);
            }
            
            return new T();
        }
        #endregion

        #region 日志工具
        protected void LogInfo(string message)
        {
            Debug.Log($"[FX编辑器] {message}");
        }

        protected void LogWarning(string message)
        {
            Debug.LogWarning($"[FX编辑器] {message}");
        }

        protected void LogError(string message)
        {
            Debug.LogError($"[FX编辑器] {message}");
        }
        #endregion
    }
}
