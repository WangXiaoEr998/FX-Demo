using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// 资源命名规范检查工具，确保项目资源命名符合团队规范
/// 作者：黄畅修
/// 创建时间：2025-07-20
/// </summary>
namespace FanXing.Editor
{
    public class AssetNamingValidator : FXEditorBase
    {
        #region 菜单项
        [MenuItem(MENU_ROOT + "质量工具/资源命名检查", false, 10)]
        public static void ShowWindow()
        {
            var window = GetWindow<AssetNamingValidator>("资源命名检查");
            window.minSize = new Vector2(600, 500);
            window.Show();
        }

        [MenuItem(MENU_ROOT + "质量工具/快速检查资源命名", false, 11)]
        public static void QuickValidateAssets()
        {
            var validator = new AssetNamingValidator();
            validator.LoadData();
            validator.ValidateAllAssets();
            
            if (validator._validationResults.Count == 0)
            {
                EditorUtility.DisplayDialog("检查完成", "所有资源命名都符合规范！", "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("发现问题", 
                    $"发现 {validator._validationResults.Count} 个命名问题，请打开资源命名检查窗口查看详情。", "确定");
            }
        }
        #endregion

        #region 字段定义
        private List<ValidationResult> _validationResults = new List<ValidationResult>();
        private Dictionary<string, NamingRule> _namingRules = new Dictionary<string, NamingRule>();
        private bool _autoFixEnabled = false;
        private string _searchFilter = "";
        private ValidationSeverity _severityFilter = ValidationSeverity.All;
        #endregion

        #region 生命周期
        protected override void LoadData()
        {
            InitializeNamingRules();
        }

        protected override void SaveData()
        {
            // 保存配置到EditorPrefs
            EditorPrefs.SetBool("FX_AutoFixEnabled", _autoFixEnabled);
        }
        #endregion

        #region GUI绘制
        protected override void OnGUI()
        {
            DrawTitle("资源命名规范检查工具");
            
            // 控制面板
            DrawControlPanel();
            
            EditorGUILayout.Space(10);
            DrawHorizontalLine();
            EditorGUILayout.Space(10);
            
            // 检查结果
            DrawValidationResults();
        }

        private void DrawControlPanel()
        {
            EditorGUILayout.BeginVertical(_boxStyle);
            
            DrawHeader("检查控制");
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("检查所有资源", _buttonStyle, GUILayout.Width(120)))
            {
                ValidateAllAssets();
            }
            
            if (GUILayout.Button("检查选中资源", _buttonStyle, GUILayout.Width(120)))
            {
                ValidateSelectedAssets();
            }
            
            if (GUILayout.Button("清除结果", _buttonStyle, GUILayout.Width(100)))
            {
                _validationResults.Clear();
            }
            
            GUILayout.FlexibleSpace();
            
            _autoFixEnabled = EditorGUILayout.Toggle("自动修复", _autoFixEnabled, GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // 过滤器
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("过滤器:", GUILayout.Width(50));
            _searchFilter = EditorGUILayout.TextField(_searchFilter, GUILayout.Width(200));
            _severityFilter = (ValidationSeverity)EditorGUILayout.EnumPopup(_severityFilter, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }

        private void DrawValidationResults()
        {
            DrawHeader($"检查结果 ({GetFilteredResults().Count} 项)");
            
            if (_validationResults.Count == 0)
            {
                EditorGUILayout.LabelField("暂无检查结果，请点击检查按钮开始检查。", EditorStyles.centeredGreyMiniLabel);
                return;
            }
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            var filteredResults = GetFilteredResults();
            foreach (var result in filteredResults)
            {
                DrawValidationResult(result);
            }
            
            EditorGUILayout.EndScrollView();
        }

        private void DrawValidationResult(ValidationResult result)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            
            EditorGUILayout.BeginHorizontal();
            
            // 严重程度图标
            GUIContent icon = GetSeverityIcon(result.severity);
            EditorGUILayout.LabelField(icon, GUILayout.Width(20));
            
            // 资源路径
            EditorGUILayout.LabelField(result.assetPath, EditorStyles.boldLabel);
            
            GUILayout.FlexibleSpace();
            
            // 操作按钮
            if (GUILayout.Button("定位", GUILayout.Width(50)))
            {
                var asset = AssetDatabase.LoadAssetAtPath<Object>(result.assetPath);
                if (asset != null)
                {
                    EditorGUIUtility.PingObject(asset);
                    Selection.activeObject = asset;
                }
            }
            
            if (result.canAutoFix && GUILayout.Button("修复", GUILayout.Width(50)))
            {
                AutoFixAsset(result);
            }
            
            EditorGUILayout.EndHorizontal();
            
            // 问题描述
            EditorGUILayout.LabelField("问题:", EditorStyles.miniLabel);
            EditorGUILayout.LabelField(result.message, EditorStyles.wordWrappedMiniLabel);
            
            if (!string.IsNullOrEmpty(result.suggestion))
            {
                EditorGUILayout.LabelField("建议:", EditorStyles.miniLabel);
                EditorGUILayout.LabelField(result.suggestion, EditorStyles.wordWrappedMiniLabel);
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }
        #endregion

        #region 验证逻辑
        private void ValidateAllAssets()
        {
            _validationResults.Clear();
            
            string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
            int totalAssets = allAssetPaths.Length;
            
            for (int i = 0; i < totalAssets; i++)
            {
                string assetPath = allAssetPaths[i];
                
                if (EditorUtility.DisplayCancelableProgressBar("检查资源命名", 
                    $"正在检查: {Path.GetFileName(assetPath)}", (float)i / totalAssets))
                {
                    break;
                }
                
                ValidateAsset(assetPath);
            }
            
            EditorUtility.ClearProgressBar();
            LogInfo($"资源命名检查完成，发现 {_validationResults.Count} 个问题");
        }

        private void ValidateSelectedAssets()
        {
            _validationResults.Clear();
            
            var selectedAssets = Selection.GetFiltered<Object>(SelectionMode.Assets);
            
            foreach (var asset in selectedAssets)
            {
                string assetPath = AssetDatabase.GetAssetPath(asset);
                ValidateAsset(assetPath);
            }
            
            LogInfo($"选中资源检查完成，发现 {_validationResults.Count} 个问题");
        }

        private void ValidateAsset(string assetPath)
        {
            // 跳过系统文件和文件夹
            if (assetPath.StartsWith("Packages/") || 
                assetPath.StartsWith("Library/") || 
                Directory.Exists(assetPath))
            {
                return;
            }
            
            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            string extension = Path.GetExtension(assetPath).ToLower();
            string directory = Path.GetDirectoryName(assetPath);
            
            // 根据文件类型应用相应的命名规则
            foreach (var rule in _namingRules.Values)
            {
                if (rule.IsApplicable(assetPath, extension))
                {
                    var result = rule.Validate(assetPath, fileName);
                    if (result != null)
                    {
                        _validationResults.Add(result);
                        
                        if (_autoFixEnabled && result.canAutoFix)
                        {
                            AutoFixAsset(result);
                        }
                    }
                }
            }
        }

        private void AutoFixAsset(ValidationResult result)
        {
            if (!result.canAutoFix || string.IsNullOrEmpty(result.suggestedName))
                return;
            
            string oldPath = result.assetPath;
            string directory = Path.GetDirectoryName(oldPath);
            string extension = Path.GetExtension(oldPath);
            string newPath = Path.Combine(directory, result.suggestedName + extension);
            
            string error = AssetDatabase.MoveAsset(oldPath, newPath);
            if (string.IsNullOrEmpty(error))
            {
                LogInfo($"已自动修复: {oldPath} -> {newPath}");
                _validationResults.Remove(result);
            }
            else
            {
                LogError($"自动修复失败: {error}");
            }
        }
        #endregion

        #region 命名规则初始化
        private void InitializeNamingRules()
        {
            _namingRules.Clear();
            
            // C# 脚本命名规则
            _namingRules.Add("CSharpScript", new NamingRule
            {
                name = "C# 脚本",
                pattern = @"^[A-Z][a-zA-Z0-9]*$",
                extensions = new[] { ".cs" },
                description = "C# 脚本应使用 PascalCase 命名",
                severity = ValidationSeverity.Error
            });
            
            // 贴图资源命名规则
            _namingRules.Add("Texture", new NamingRule
            {
                name = "贴图资源",
                pattern = @"^(ui_|tex_|icon_)[a-z][a-z0-9_]*$",
                extensions = new[] { ".png", ".jpg", ".jpeg", ".tga", ".psd" },
                description = "贴图应以 ui_、tex_ 或 icon_ 开头，使用小写字母和下划线",
                severity = ValidationSeverity.Warning
            });
            
            // 音频资源命名规则
            _namingRules.Add("Audio", new NamingRule
            {
                name = "音频资源",
                pattern = @"^(bgm_|sfx_|voice_)[a-z][a-z0-9_]*$",
                extensions = new[] { ".wav", ".mp3", ".ogg", ".aiff" },
                description = "音频应以 bgm_、sfx_ 或 voice_ 开头，使用小写字母和下划线",
                severity = ValidationSeverity.Warning
            });
            
            // 3D模型命名规则
            _namingRules.Add("Model", new NamingRule
            {
                name = "3D模型",
                pattern = @"^(character_|prop_|environment_)[a-z][a-z0-9_]*$",
                extensions = new[] { ".fbx", ".obj", ".dae", ".3ds" },
                description = "3D模型应以 character_、prop_ 或 environment_ 开头",
                severity = ValidationSeverity.Warning
            });
            
            // Prefab 命名规则
            _namingRules.Add("Prefab", new NamingRule
            {
                name = "Prefab 预制体",
                pattern = @"^[A-Z][a-zA-Z0-9]*$",
                extensions = new[] { ".prefab" },
                description = "Prefab 应使用 PascalCase 命名",
                severity = ValidationSeverity.Warning
            });
        }
        #endregion

        #region 辅助方法
        private List<ValidationResult> GetFilteredResults()
        {
            var filtered = new List<ValidationResult>();
            
            foreach (var result in _validationResults)
            {
                // 严重程度过滤
                if (_severityFilter != ValidationSeverity.All && result.severity != _severityFilter)
                    continue;
                
                // 搜索过滤
                if (!string.IsNullOrEmpty(_searchFilter) && 
                    !result.assetPath.ToLower().Contains(_searchFilter.ToLower()))
                    continue;
                
                filtered.Add(result);
            }
            
            return filtered;
        }

        private GUIContent GetSeverityIcon(ValidationSeverity severity)
        {
            switch (severity)
            {
                case ValidationSeverity.Error:
                    return EditorGUIUtility.IconContent("console.erroricon");
                case ValidationSeverity.Warning:
                    return EditorGUIUtility.IconContent("console.warnicon");
                case ValidationSeverity.Info:
                    return EditorGUIUtility.IconContent("console.infoicon");
                default:
                    return GUIContent.none;
            }
        }
        #endregion
    }

    #region 数据结构
    public enum ValidationSeverity
    {
        All,
        Error,
        Warning,
        Info
    }

    [System.Serializable]
    public class ValidationResult
    {
        public string assetPath;
        public string message;
        public string suggestion;
        public ValidationSeverity severity;
        public bool canAutoFix;
        public string suggestedName;
    }

    [System.Serializable]
    public class NamingRule
    {
        public string name;
        public string pattern;
        public string[] extensions;
        public string description;
        public ValidationSeverity severity;

        public bool IsApplicable(string assetPath, string extension)
        {
            if (extensions == null || extensions.Length == 0)
                return true;
            
            foreach (string ext in extensions)
            {
                if (extension.Equals(ext, System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            
            return false;
        }

        public ValidationResult Validate(string assetPath, string fileName)
        {
            if (Regex.IsMatch(fileName, pattern))
                return null;
            
            return new ValidationResult
            {
                assetPath = assetPath,
                message = $"{name} 命名不符合规范: {fileName}",
                suggestion = $"应该符合规则: {description}",
                severity = severity,
                canAutoFix = false
            };
        }
    }
    #endregion
}
