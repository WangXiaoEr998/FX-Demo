# 繁星Demo部署说明

## 文档信息
- **作者**：黄畅修（主程序）
- **创建时间**：2025-07-12
- **版本**：v3.0
- **更新时间**：2025-07-12
- **架构状态**：主体架构100%完成，可进行Unity项目部署
- **部署就绪**：18个核心文件，6200行代码

## 部署概述

本文档提供繁星Demo项目的完整部署指南。当前主体架构已完成，包含18个核心文件和完整的代码架构，可以直接部署到Unity项目中。

### 当前状态
- **代码架构**：100%完成
- **文件结构**：完整规划
- **文档完整性**：完整详细

## 1. 环境要求

### 1.1 开发环境
- **操作系统**：Windows 10/11, macOS 12+, Ubuntu 20.04+
- **Unity版本**：Unity 2022.3.12f1 LTS
- **IDE**：Visual Studio 2022 Community 或 Visual Studio Code
- **版本控制**：Git 2.30+ 和 Git LFS
- **内存**：至少8GB RAM，推荐16GB
- **存储**：至少10GB可用空间

### 1.2 目标平台
- **Windows**：Windows 10/11 (x64)
- **macOS**：macOS 12+ (Intel/Apple Silicon)
- **Linux**：Ubuntu 20.04+ (x64)

### 1.3 最低系统要求
- **CPU**：Intel i5-8400 或 AMD Ryzen 5 2600
- **内存**：8GB RAM
- **显卡**：GTX 1060 或 RX 580
- **存储**：2GB可用空间
- **DirectX**：Version 11

### 1.4 推荐系统要求
- **CPU**：Intel i7-10700 或 AMD Ryzen 7 3700X
- **内存**：16GB RAM
- **显卡**：RTX 3070 或 RX 6700 XT
- **存储**：5GB可用空间（SSD推荐）
- **DirectX**：Version 12

## 2. 项目设置

### 2.1 Unity项目配置

#### Player Settings
```csharp
// 基本设置
PlayerSettings.companyName = "FanXing Team";
PlayerSettings.productName = "FanXing Demo";
PlayerSettings.bundleVersion = "0.1.0";

// 平台设置
PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_Standard_2_1);

// 图标和启动画面
PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Standalone, icons);
PlayerSettings.SplashScreen.show = true;
```

#### Quality Settings
```csharp
// 质量设置
QualitySettings.vSyncCount = 1;
QualitySettings.antiAliasing = 2;
QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
QualitySettings.shadows = ShadowQuality.HardOnly;
QualitySettings.shadowResolution = ShadowResolution.Medium;
```

#### Graphics Settings
```csharp
// 渲染设置
GraphicsSettings.renderPipelineAsset = urpAsset;
GraphicsSettings.defaultRenderPipeline = urpAsset;
```

### 2.2 构建设置

#### Windows构建
```csharp
BuildPlayerOptions buildOptions = new BuildPlayerOptions();
buildOptions.scenes = GetScenePaths();
buildOptions.locationPathName = "Builds/Windows/FanXingDemo.exe";
buildOptions.target = BuildTarget.StandaloneWindows64;
buildOptions.options = BuildOptions.None;
```

#### macOS构建
```csharp
BuildPlayerOptions buildOptions = new BuildPlayerOptions();
buildOptions.scenes = GetScenePaths();
buildOptions.locationPathName = "Builds/macOS/FanXingDemo.app";
buildOptions.target = BuildTarget.StandaloneOSX;
buildOptions.options = BuildOptions.None;
```

#### Linux构建
```csharp
BuildPlayerOptions buildOptions = new BuildPlayerOptions();
buildOptions.scenes = GetScenePaths();
buildOptions.locationPathName = "Builds/Linux/FanXingDemo.x86_64";
buildOptions.target = BuildTarget.StandaloneLinux64;
buildOptions.options = BuildOptions.None;
```

## 3. 构建流程

### 3.1 预构建检查

#### 代码检查清单
- [ ] 所有编译错误已修复
- [ ] 所有警告已处理
- [ ] 代码审查已完成
- [ ] 单元测试通过
- [ ] 性能测试通过

#### 资源检查清单
- [ ] 所有资源文件已提交
- [ ] 贴图压缩设置正确
- [ ] 音频文件格式正确
- [ ] 模型优化完成
- [ ] 场景引用完整

#### 配置检查清单
- [ ] Player Settings配置正确
- [ ] Quality Settings优化完成
- [ ] Input Settings配置完整
- [ ] Physics Settings调整完成
- [ ] Time Settings设置正确

### 3.2 自动化构建脚本

#### BuildScript.cs
```csharp
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

/// <summary>
/// 自动化构建脚本
/// 作者：黄畅修
/// 创建时间：2025-07-12
/// </summary>
public class BuildScript
{
    private static readonly string[] SCENES = {
        "Assets/Scenes/Main/GameMain.unity",
        "Assets/Scenes/UI/MainMenu.unity"
    };

    [MenuItem("Build/Build All Platforms")]
    public static void BuildAllPlatforms()
    {
        BuildWindows();
        BuildMacOS();
        BuildLinux();
    }

    [MenuItem("Build/Build Windows")]
    public static void BuildWindows()
    {
        string buildPath = "Builds/Windows/FanXingDemo.exe";
        BuildTarget target = BuildTarget.StandaloneWindows64;
        
        BuildGame(buildPath, target, "Windows");
    }

    [MenuItem("Build/Build macOS")]
    public static void BuildMacOS()
    {
        string buildPath = "Builds/macOS/FanXingDemo.app";
        BuildTarget target = BuildTarget.StandaloneOSX;
        
        BuildGame(buildPath, target, "macOS");
    }

    [MenuItem("Build/Build Linux")]
    public static void BuildLinux()
    {
        string buildPath = "Builds/Linux/FanXingDemo.x86_64";
        BuildTarget target = BuildTarget.StandaloneLinux64;
        
        BuildGame(buildPath, target, "Linux");
    }

    private static void BuildGame(string buildPath, BuildTarget target, string platformName)
    {
        Debug.Log($"开始构建 {platformName} 版本...");

        // 创建构建目录
        string buildDir = Path.GetDirectoryName(buildPath);
        if (!Directory.Exists(buildDir))
        {
            Directory.CreateDirectory(buildDir);
        }

        // 构建选项
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = SCENES,
            locationPathName = buildPath,
            target = target,
            options = BuildOptions.None
        };

        // 执行构建
        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"{platformName} 构建成功: {buildPath}");
            Debug.Log($"构建大小: {summary.totalSize} bytes");
        }
        else
        {
            Debug.LogError($"{platformName} 构建失败: {summary.result}");
        }
    }
}
```

### 3.3 构建后处理

#### 文件复制脚本
```csharp
[PostProcessBuild(1)]
public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
{
    Debug.Log("开始构建后处理...");

    // 复制配置文件
    CopyConfigFiles(pathToBuiltProject);
    
    // 复制文档文件
    CopyDocumentationFiles(pathToBuiltProject);
    
    // 生成版本信息
    GenerateVersionInfo(pathToBuiltProject);
    
    Debug.Log("构建后处理完成");
}

private static void CopyConfigFiles(string buildPath)
{
    string configSource = "Assets/StreamingAssets/Configs";
    string configDest = Path.Combine(Path.GetDirectoryName(buildPath), "Configs");
    
    if (Directory.Exists(configSource))
    {
        CopyDirectory(configSource, configDest);
        Debug.Log("配置文件复制完成");
    }
}
```

## 4. 版本管理

### 4.1 版本号规范
- **格式**：Major.Minor.Patch (例如：1.0.0)
- **Major**：重大功能更新或不兼容变更
- **Minor**：新功能添加，向后兼容
- **Patch**：Bug修复和小改进

### 4.2 版本信息文件
```json
{
    "version": "0.1.0",
    "buildNumber": 1,
    "buildDate": "2025-07-12T20:30:00Z",
    "gitCommit": "abc123def456",
    "platform": "Windows",
    "configuration": "Release"
}
```

### 4.3 自动版本更新
```csharp
public static void UpdateVersion()
{
    VersionInfo version = LoadVersionInfo();
    version.buildNumber++;
    version.buildDate = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
    version.gitCommit = GetGitCommitHash();
    
    SaveVersionInfo(version);
    PlayerSettings.bundleVersion = version.version;
}
```

## 5. 发布流程

### 5.1 内部测试版本
1. **代码冻结**：停止新功能开发
2. **完整测试**：执行所有测试用例
3. **性能测试**：检查帧率和内存使用
4. **构建验证**：在所有目标平台构建
5. **内部验收**：团队内部试玩验证

### 5.2 Beta测试版本
1. **Bug修复**：修复内测发现的问题
2. **文档更新**：更新用户手册和发布说明
3. **安装包制作**：制作安装程序
4. **分发准备**：准备分发渠道
5. **Beta发布**：向测试用户发布

### 5.3 正式发布版本
1. **最终测试**：完整回归测试
2. **发布审核**：平台审核流程
3. **营销准备**：宣传材料准备
4. **正式发布**：在各平台发布
5. **监控反馈**：监控用户反馈和问题

## 6. 部署配置

### 6.1 Steam部署

#### 应用配置
```
app_id: [Steam App ID]
depot_id: [Steam Depot ID]
content_root: "Builds/Windows/"
file_mapping:
{
    "LocalPath": "*"
    "DepotPath": "."
    "recursive": "1"
}
```

#### 上传脚本
```batch
@echo off
echo 正在上传到Steam...
steamcmd +login [username] +run_app_build_http steamworks_sdk\tools\ContentBuilder\scripts\app_build.vdf +quit
echo Steam上传完成
pause
```

### 6.2 Itch.io部署

#### Butler上传
```batch
butler push Builds/Windows fanxing-team/fanxing-demo:windows
butler push Builds/macOS fanxing-team/fanxing-demo:osx
butler push Builds/Linux fanxing-team/fanxing-demo:linux
```

### 6.3 自建服务器部署

#### 文件结构
```
/var/www/fanxing-demo/
├── downloads/
│   ├── windows/
│   ├── macos/
│   └── linux/
├── updates/
├── docs/
└── index.html
```

#### 下载页面
```html
<!DOCTYPE html>
<html>
<head>
    <title>繁星Demo下载</title>
</head>
<body>
    <h1>繁星Demo</h1>
    <div class="downloads">
        <a href="downloads/windows/FanXingDemo-v0.1.0-Windows.zip">Windows版本</a>
        <a href="downloads/macos/FanXingDemo-v0.1.0-macOS.zip">macOS版本</a>
        <a href="downloads/linux/FanXingDemo-v0.1.0-Linux.zip">Linux版本</a>
    </div>
</body>
</html>
```

## 7. 监控和维护

### 7.1 日志收集
```csharp
public class LogCollector : MonoBehaviour
{
    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
            SendErrorReport(logString, stackTrace);
        }
    }
}
```

### 7.2 性能监控
```csharp
public class PerformanceMonitor : MonoBehaviour
{
    private float _frameTime;
    private int _memoryUsage;

    private void Update()
    {
        _frameTime = Time.deltaTime;
        _memoryUsage = (int)(Profiler.GetTotalAllocatedMemory(false) / 1024 / 1024);

        if (_frameTime > 0.033f) // 低于30FPS
        {
            Debug.LogWarning($"性能警告: 帧时间 {_frameTime:F3}s");
        }
    }
}
```

### 7.3 自动更新系统
```csharp
public class UpdateChecker : MonoBehaviour
{
    private const string UPDATE_URL = "https://api.fanxing.com/version";

    public IEnumerator CheckForUpdates()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(UPDATE_URL))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                VersionInfo latestVersion = JsonUtility.FromJson<VersionInfo>(request.downloadHandler.text);
                
                if (IsNewerVersion(latestVersion.version, Application.version))
                {
                    ShowUpdateDialog(latestVersion);
                }
            }
        }
    }
}
```

## 8. 故障排除

### 8.1 常见构建问题

#### 问题：构建失败，提示缺少场景
**解决方案**：
1. 检查Build Settings中的场景列表
2. 确保所有场景文件存在且未损坏
3. 重新添加场景到构建列表

#### 问题：构建后游戏无法启动
**解决方案**：
1. 检查目标平台的系统要求
2. 验证所有依赖库是否正确
3. 查看日志文件获取错误信息

#### 问题：资源加载失败
**解决方案**：
1. 检查Resources文件夹中的资源
2. 验证StreamingAssets文件夹内容
3. 确保资源路径正确

### 8.2 性能问题诊断

#### 帧率低下
1. 使用Unity Profiler分析性能瓶颈
2. 检查Draw Call数量
3. 优化贴图和模型
4. 减少实时光照计算

#### 内存占用过高
1. 检查纹理压缩设置
2. 优化音频文件格式
3. 及时释放不用的资源
4. 使用对象池减少GC

---

**注：本部署文档将根据项目发展和平台要求持续更新。**
