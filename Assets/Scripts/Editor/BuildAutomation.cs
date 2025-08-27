using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;
using System;

/// <summary>
/// 自动化构建脚本，提供一键构建和发布功能
/// 作者：黄畅修
/// 创建时间：2025-07-20
/// </summary>
namespace FanXing.Editor
{
    public class BuildAutomation : FXEditorBase
    {
        #region 菜单项
        [MenuItem(MENU_ROOT + "构建工具/构建管理器", false, 20)]
        public static void ShowWindow()
        {
            var window = GetWindow<BuildAutomation>("构建管理器");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }

        [MenuItem(MENU_ROOT + "构建工具/快速构建 Windows", false, 21)]
        public static void QuickBuildWindows()
        {
            BuildGame(BuildTarget.StandaloneWindows64, "FanXing-Demo-Windows");
        }

        [MenuItem(MENU_ROOT + "构建工具/快速构建 Android", false, 22)]
        public static void QuickBuildAndroid()
        {
            BuildGame(BuildTarget.Android, "FanXing-Demo-Android");
        }
        #endregion

        #region 字段定义
        private BuildTarget _selectedBuildTarget = BuildTarget.StandaloneWindows64;
        private string _buildPath = "Builds";
        private string _productName = "FanXing-Demo";
        private string _version = "1.0.0";
        private bool _developmentBuild = false;
        private bool _autoRunAfterBuild = false;
        private bool _compressWithLz4 = true;
        
        private static readonly string[] BUILD_SCENES = {
            "Assets/Scenes/MainScene.unity"
        };
        #endregion

        #region 生命周期
        protected override void LoadData()
        {
            _buildPath = EditorPrefs.GetString("FX_BuildPath", "Builds");
            _productName = EditorPrefs.GetString("FX_ProductName", "FanXing-Demo");
            _version = EditorPrefs.GetString("FX_Version", "1.0.0");
            _developmentBuild = EditorPrefs.GetBool("FX_DevelopmentBuild", false);
            _autoRunAfterBuild = EditorPrefs.GetBool("FX_AutoRunAfterBuild", false);
            _compressWithLz4 = EditorPrefs.GetBool("FX_CompressWithLz4", true);
        }

        protected override void SaveData()
        {
            EditorPrefs.SetString("FX_BuildPath", _buildPath);
            EditorPrefs.SetString("FX_ProductName", _productName);
            EditorPrefs.SetString("FX_Version", _version);
            EditorPrefs.SetBool("FX_DevelopmentBuild", _developmentBuild);
            EditorPrefs.SetBool("FX_AutoRunAfterBuild", _autoRunAfterBuild);
            EditorPrefs.SetBool("FX_CompressWithLz4", _compressWithLz4);
        }
        #endregion

        #region GUI绘制
        protected override void OnGUI()
        {
            DrawTitle("自动化构建工具");
            
            DrawBuildSettings();
            
            EditorGUILayout.Space(10);
            DrawHorizontalLine();
            EditorGUILayout.Space(10);
            
            DrawBuildActions();
            
            EditorGUILayout.Space(10);
            DrawHorizontalLine();
            EditorGUILayout.Space(10);
            
            DrawBuildHistory();
        }

        private void DrawBuildSettings()
        {
            EditorGUILayout.BeginVertical(_boxStyle);
            DrawHeader("构建设置");
            
            // 基础设置
            _selectedBuildTarget = (BuildTarget)EditorGUILayout.EnumPopup("目标平台", _selectedBuildTarget);
            _productName = EditorGUILayout.TextField("产品名称", _productName);
            _version = EditorGUILayout.TextField("版本号", _version);
            
            EditorGUILayout.Space(5);
            
            // 路径设置
            EditorGUILayout.BeginHorizontal();
            _buildPath = EditorGUILayout.TextField("构建路径", _buildPath);
            if (GUILayout.Button("浏览", GUILayout.Width(60)))
            {
                string selectedPath = EditorUtility.OpenFolderPanel("选择构建路径", _buildPath, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    _buildPath = selectedPath;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // 构建选项
            _developmentBuild = EditorGUILayout.Toggle("开发构建", _developmentBuild);
            _autoRunAfterBuild = EditorGUILayout.Toggle("构建后自动运行", _autoRunAfterBuild);
            _compressWithLz4 = EditorGUILayout.Toggle("LZ4压缩", _compressWithLz4);
            
            EditorGUILayout.EndVertical();
        }

        private void DrawBuildActions()
        {
            EditorGUILayout.BeginVertical(_boxStyle);
            DrawHeader("构建操作");
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("构建游戏", _buttonStyle, GUILayout.Height(30)))
            {
                string buildName = $"{_productName}-{GetPlatformName(_selectedBuildTarget)}";
                BuildGame(_selectedBuildTarget, buildName);
            }
            
            if (GUILayout.Button("构建并运行", _buttonStyle, GUILayout.Height(30)))
            {
                _autoRunAfterBuild = true;
                string buildName = $"{_productName}-{GetPlatformName(_selectedBuildTarget)}";
                BuildGame(_selectedBuildTarget, buildName);
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("清理构建文件", _buttonStyle))
            {
                CleanBuildFiles();
            }
            
            if (GUILayout.Button("打开构建目录", _buttonStyle))
            {
                OpenBuildDirectory();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }

        private void DrawBuildHistory()
        {
            EditorGUILayout.BeginVertical(_boxStyle);
            DrawHeader("构建历史");
            
            EditorGUILayout.LabelField("最近构建:", EditorStyles.boldLabel);
            
            string buildHistoryPath = Path.Combine(_buildPath, "build_history.txt");
            if (File.Exists(buildHistoryPath))
            {
                string[] lines = File.ReadAllLines(buildHistoryPath);
                int maxLines = Mathf.Min(lines.Length, 5);
                
                for (int i = lines.Length - maxLines; i < lines.Length; i++)
                {
                    EditorGUILayout.LabelField(lines[i], EditorStyles.miniLabel);
                }
            }
            else
            {
                EditorGUILayout.LabelField("暂无构建历史", EditorStyles.centeredGreyMiniLabel);
            }
            
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region 构建逻辑
        public static void BuildGame(BuildTarget buildTarget, string buildName)
        {
            var automation = new BuildAutomation();
            automation.LoadData();
            
            // 设置构建选项
            BuildOptions buildOptions = BuildOptions.None;
            
            if (automation._developmentBuild)
                buildOptions |= BuildOptions.Development;
            
            if (automation._autoRunAfterBuild)
                buildOptions |= BuildOptions.AutoRunPlayer;
            
            if (automation._compressWithLz4)
                buildOptions |= BuildOptions.CompressWithLz4;
            
            // 确保构建目录存在
            string buildDirectory = Path.Combine(automation._buildPath, buildName);
            if (!Directory.Exists(buildDirectory))
            {
                Directory.CreateDirectory(buildDirectory);
            }
            
            // 设置构建路径
            string buildPath = Path.Combine(buildDirectory, GetExecutableName(buildTarget, automation._productName));
            
            // 更新项目设置
            Debug.Log($"开始构建配置: 产品名={automation._productName}, 版本={automation._version}");

            // 注意：在某些Unity版本中，PlayerSettings的某些属性可能需要通过Project Settings手动设置
            // 这里我们专注于构建过程，避免API兼容性问题

            // 设置应用标识符（如果需要）
            if (buildTarget == BuildTarget.Android)
            {
                try
                {
                    string packageName = $"com.fanxing.{automation._productName.ToLower().Replace("-", "").Replace(" ", "")}";
                    Debug.Log($"Android包名设置为: {packageName}");
                    // 如果需要设置包名，建议在Project Settings中手动配置
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Android设置警告: {e.Message}");
                }
            }
            
            // 执行构建
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = BUILD_SCENES,
                locationPathName = buildPath,
                target = buildTarget,
                options = buildOptions
            };
            
            Debug.Log($"开始构建: {buildTarget} -> {buildPath}");
            
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            
            // 记录构建结果
            string resultMessage = "";
            if (summary.result == BuildResult.Succeeded)
            {
                resultMessage = $"构建成功! 大小: {FormatBytes(summary.totalSize)}";
                Debug.Log(resultMessage);
                
                // 记录构建历史
                automation.RecordBuildHistory(buildTarget, buildName, summary);
                
                EditorUtility.DisplayDialog("构建完成", resultMessage, "确定");
            }
            else
            {
                resultMessage = $"构建失败: {summary.result}";
                Debug.LogError(resultMessage);
                EditorUtility.DisplayDialog("构建失败", resultMessage, "确定");
            }
        }

        private void RecordBuildHistory(BuildTarget buildTarget, string buildName, BuildSummary summary)
        {
            string buildHistoryPath = Path.Combine(_buildPath, "build_history.txt");
            string historyEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {buildTarget} | {buildName} | {summary.result} | {FormatBytes(summary.totalSize)}";
            
            try
            {
                File.AppendAllText(buildHistoryPath, historyEntry + Environment.NewLine);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"无法记录构建历史: {e.Message}");
            }
        }

        private void CleanBuildFiles()
        {
            if (Directory.Exists(_buildPath))
            {
                if (ShowConfirmDialog("清理确认", "确定要删除所有构建文件吗？此操作不可撤销。"))
                {
                    try
                    {
                        Directory.Delete(_buildPath, true);
                        ShowSuccessMessage("构建文件已清理完成！");
                    }
                    catch (Exception e)
                    {
                        ShowErrorMessage($"清理失败: {e.Message}");
                    }
                }
            }
            else
            {
                ShowSuccessMessage("构建目录不存在，无需清理。");
            }
        }

        private void OpenBuildDirectory()
        {
            if (Directory.Exists(_buildPath))
            {
                EditorUtility.RevealInFinder(_buildPath);
            }
            else
            {
                ShowErrorMessage("构建目录不存在！");
            }
        }
        #endregion

        #region 辅助方法
        private static string GetPlatformName(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Windows";
                case BuildTarget.StandaloneOSX:
                    return "macOS";
                case BuildTarget.StandaloneLinux64:
                    return "Linux";
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.iOS:
                    return "iOS";
                case BuildTarget.WebGL:
                    return "WebGL";
                default:
                    return buildTarget.ToString();
            }
        }

        private static string GetExecutableName(BuildTarget buildTarget, string productName)
        {
            switch (buildTarget)
            {
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return $"{productName}.exe";
                case BuildTarget.Android:
                    return $"{productName}.apk";
                case BuildTarget.StandaloneOSX:
                    return $"{productName}.app";
                case BuildTarget.StandaloneLinux64:
                    return productName;
                default:
                    return productName;
            }
        }

        private static string FormatBytes(ulong bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            
            while (Math.Round(number / 1024) >= 1)
            {
                number /= 1024;
                counter++;
            }
            
            return $"{number:n1} {suffixes[counter]}";
        }
        #endregion
    }
}
