# 繁星Demo代码规范

## 文档信息
- **作者**：黄畅修（主程序）
- **创建时间**：2025-07-12
- **版本**：v5.0
- **更新时间**：2025-07-20
- **执行状态**：100%严格执行，编辑器工具规范已完成
- **遵循率**：33个核心文件100%遵循规范

## 规范概述

本文档定义了繁星Demo项目的完整代码规范，所有代码必须严格遵循这些标准。当前主体架构的33个核心文件已100%遵循这些规范。

### 规范执行情况
- **命名规范**：100%遵循
- **注释规范**：100%覆盖
- **结构规范**：100%执行

## 1. Global架构编码规范

### 1.1 Global统一入口使用规范

#### 强制要求
- **禁止直接访问Manager.Instance**：所有管理器访问必须通过Global入口
- **禁止硬编码事件名称**：所有事件必须使用EventConstants中的常量
- **禁止模块间直接依赖**：系统间通信必须通过事件系统

#### 正确示例
```csharp
// 正确：使用Global统一入口
Global.Data.SaveGameData();
Global.Player.AddExperience(100);
Global.TriggerEvent(Global.Events.Player.LEVEL_UP, args);

// 错误：直接访问Manager.Instance
DataManager.Instance.SaveGameData();
EventManager.Instance.TriggerEvent("OnPlayerLevelUp", args);
```

### 1.2 事件系统使用规范

#### 事件注册规范
```csharp
// 正确：使用事件常量和类型安全接口
Global.RegisterEvent<PlayerLevelUpEventArgs>(
    Global.Events.Player.LEVEL_UP,
    OnPlayerLevelUp
);

// 错误：使用硬编码字符串
EventManager.Instance.RegisterEvent("OnPlayerLevelUp", OnPlayerLevelUp);
```

#### 事件触发规范
```csharp
// 正确：使用事件常量
Global.TriggerEvent(Global.Events.Player.HEALTH_CHANGED, healthArgs);

// 错误：使用硬编码字符串
TriggerEvent("OnHealthChanged", healthArgs);
```

### 1.3 系统设计规范

#### 系统继承规范
```csharp
// 正确：继承BaseGameSystem
public class CustomSystem : BaseGameSystem
{
    // 系统实现
}

// 错误：直接实现IGameSystem
public class CustomSystem : MonoBehaviour, IGameSystem
{
    // 不推荐的实现方式
}
```

#### 系统通信规范
```csharp
// 正确：通过事件系统通信
public void SomeMethod()
{
    Global.TriggerEvent(Global.Events.Quest.COMPLETED, questArgs);
}

// 错误：直接调用其他系统
public void SomeMethod()
{
    Global.Player.AddExperience(100); // 避免直接调用
}
```

### 1.4 命名空间使用规范

#### 必需的using语句
```csharp
// 所有需要使用事件常量的文件必须包含
using FanXing.Data;

// 所有需要使用Global的文件自动包含（Global在全局命名空间）
```

#### 事件常量使用
```csharp
// 正确：使用完整的事件常量路径
Global.Events.Player.LEVEL_UP
EventConstants.Player.LEVEL_UP

// 错误：使用硬编码字符串
"OnPlayerLevelUp"
"PlayerLevelUp"
```

## 2. 总体原则

### 1.1 代码质量原则
- **可读性优先**：代码应该易于理解和维护
- **一致性**：整个项目保持统一的编码风格
- **简洁性**：避免不必要的复杂性
- **性能考虑**：在保证可读性的前提下优化性能
- **安全性**：避免常见的编程错误和安全漏洞

### 1.2 开发流程
- **代码审查**：所有代码合并前必须经过审查
- **单元测试**：关键功能需要编写单元测试
- **文档更新**：代码变更时同步更新相关文档
- **版本控制**：使用Git进行版本控制，遵循提交规范
- **质量检查**：定期进行代码质量检查和重构

## 2. C#命名规范

### 2.1 命名约定

#### 类名 (PascalCase)
```csharp
// 正确示例
public class GameManager { }
public class PlayerController { }
public class UIMainMenu { }

// 错误示例
public class gameManager { }
public class player_controller { }
public class uiMainMenu { }
```

#### 方法名 (PascalCase)
```csharp
// 正确示例
public void StartGame() { }
public bool IsPlayerAlive() { }
public void GetPlayerData() { }

// 错误示例
public void startGame() { }
public bool isPlayerAlive() { }
public void get_player_data() { }
```

#### 变量名 (camelCase)
```csharp
// 正确示例
private int currentHealth;
private float moveSpeed;
private bool isGamePaused;

// 错误示例
private int CurrentHealth;
private float move_speed;
private bool IsGamePaused;
```

#### 常量 (UPPER_CASE)
```csharp
// 正确示例
public const int MAX_LEVEL = 100;
public const string GAME_VERSION = "1.0.0";
private const float DEFAULT_SPEED = 5.0f;

// 错误示例
public const int maxLevel = 100;
public const string gameVersion = "1.0.0";
```

#### 私有字段 (下划线前缀 + camelCase)
```csharp
// 正确示例
private int _currentHealth;
private PlayerData _playerData;
private bool _isInitialized;

// 错误示例
private int currentHealth;
private PlayerData playerData;
private bool m_isInitialized;
```

#### 属性 (PascalCase)
```csharp
// 正确示例
public int CurrentHealth { get; set; }
public bool IsAlive => _currentHealth > 0;
public PlayerData PlayerData { get; private set; }

// 错误示例
public int currentHealth { get; set; }
public bool isAlive => _currentHealth > 0;
```

#### 枚举 (PascalCase)
```csharp
// 正确示例
public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver
}

public enum NPCType
{
    Merchant,
    Guard,
    Villager
}

// 错误示例
public enum gameState
{
    mainMenu,
    playing,
    paused,
    gameOver
}
```

### 2.2 命名空间
```csharp
// 正确示例
namespace FanXing.Core
{
    // 核心功能
}

namespace FanXing.UI
{
    // UI相关
}

namespace FanXing.Systems
{
    // 游戏系统
}

namespace FanXing.Data
{
    // 数据相关
}
```

## 3. 代码结构规范

### 3.1 文件头注释
```csharp
/// <summary>
/// 游戏管理器，负责游戏状态管理和核心系统初始化
/// 作者：黄畅修
/// 创建时间：2025-07-12
/// 最后修改：2025-07-12
/// </summary>
public class GameManager : MonoBehaviour
{
    // 类实现
}
```

### 3.2 类结构组织
```csharp
public class ExampleClass : MonoBehaviour
{
    #region 常量定义
    private const int MAX_ITEMS = 100;
    private const string DEFAULT_NAME = "Player";
    #endregion

    #region 字段定义
    [Header("基础设置")]
    [SerializeField] private int _health = 100;
    [SerializeField] private float _speed = 5.0f;
    
    [Header("引用组件")]
    [SerializeField] private Transform _targetTransform;
    [SerializeField] private Rigidbody _rigidbody;
    
    private bool _isInitialized = false;
    private PlayerData _playerData;
    #endregion

    #region 属性
    /// <summary>
    /// 当前生命值
    /// </summary>
    public int Health => _health;
    
    /// <summary>
    /// 是否已初始化
    /// </summary>
    public bool IsInitialized => _isInitialized;
    #endregion

    #region Unity生命周期
    private void Awake()
    {
        Initialize();
    }

    private void Start()
    {
        SetupComponents();
    }

    private void Update()
    {
        HandleInput();
        UpdateLogic();
    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 初始化组件
    /// </summary>
    public void Initialize()
    {
        // 初始化逻辑
    }

    /// <summary>
    /// 设置生命值
    /// </summary>
    /// <param name="newHealth">新的生命值</param>
    public void SetHealth(int newHealth)
    {
        _health = Mathf.Clamp(newHealth, 0, MAX_HEALTH);
    }
    #endregion

    #region 私有方法
    /// <summary>
    /// 设置组件引用
    /// </summary>
    private void SetupComponents()
    {
        // 组件设置逻辑
    }

    /// <summary>
    /// 处理输入
    /// </summary>
    private void HandleInput()
    {
        // 输入处理逻辑
    }

    /// <summary>
    /// 更新逻辑
    /// </summary>
    private void UpdateLogic()
    {
        // 更新逻辑
    }
    #endregion

    #region 事件处理
    /// <summary>
    /// 处理玩家死亡事件
    /// </summary>
    private void OnPlayerDeath()
    {
        // 事件处理逻辑
    }
    #endregion
}
```

### 3.3 方法注释规范
```csharp
/// <summary>
/// 计算两点之间的距离
/// </summary>
/// <param name="pointA">起始点</param>
/// <param name="pointB">目标点</param>
/// <returns>两点之间的距离</returns>
/// <exception cref="ArgumentNullException">当参数为null时抛出</exception>
public float CalculateDistance(Vector3 pointA, Vector3 pointB)
{
    if (pointA == null || pointB == null)
    {
        throw new ArgumentNullException("点坐标不能为null");
    }
    
    return Vector3.Distance(pointA, pointB);
}
```

## 4. Unity特定规范

### 4.1 MonoBehaviour规范
```csharp
// 正确的MonoBehaviour使用示例
public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float _moveSpeed = 5.0f;
    [Range(0, 10)]
    [SerializeField] private float _jumpHeight = 2.0f;
    
    [Header("组件引用")]
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Animator _animator;
    
    private void Awake()
    {
        // 获取组件引用
        if (_rigidbody == null)
            _rigidbody = GetComponent<Rigidbody>();
    }
}
```

### 4.2 ScriptableObject规范
```csharp
/// <summary>
/// NPC配置数据
/// </summary>
[CreateAssetMenu(fileName = "NPCConfig", menuName = "FanXing/NPCConfig")]
public class NPCConfig : ScriptableObject
{
    [Header("基础信息")]
    public string npcName;
    public NPCType npcType;
    
    [Header("对话内容")]
    [TextArea(3, 5)]
    public string[] dialogues;
    
    [Header("交易物品")]
    public ItemData[] tradeItems;
}
```

### 4.3 协程使用规范
```csharp
/// <summary>
/// 淡入淡出效果
/// </summary>
/// <param name="duration">持续时间</param>
/// <param name="targetAlpha">目标透明度</param>
/// <returns>协程</returns>
public IEnumerator FadeCoroutine(float duration, float targetAlpha)
{
    CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
    float startAlpha = canvasGroup.alpha;
    float elapsedTime = 0f;
    
    while (elapsedTime < duration)
    {
        elapsedTime += Time.deltaTime;
        float progress = elapsedTime / duration;
        canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
        yield return null;
    }
    
    canvasGroup.alpha = targetAlpha;
}
```

## 5. 性能优化规范

### 5.1 避免在Update中进行昂贵操作
```csharp
// 错误示例 - 每帧查找组件
private void Update()
{
    Transform target = GameObject.FindWithTag("Player").transform;
    // 其他逻辑
}

// 正确示例 - 缓存引用
private Transform _playerTransform;

private void Start()
{
    _playerTransform = GameObject.FindWithTag("Player").transform;
}

private void Update()
{
    if (_playerTransform != null)
    {
        // 使用缓存的引用
    }
}
```

### 5.2 字符串操作优化
```csharp
// 错误示例 - 频繁的字符串拼接
private void UpdateUI()
{
    string text = "Score: " + score + " Level: " + level;
    scoreText.text = text;
}

// 正确示例 - 使用StringBuilder或string.Format
private void UpdateUI()
{
    scoreText.text = string.Format("Score: {0} Level: {1}", score, level);
    // 或者使用StringBuilder
}
```

### 5.3 对象池使用
```csharp
/// <summary>
/// 简单对象池实现
/// </summary>
public class ObjectPool<T> where T : MonoBehaviour
{
    private Queue<T> _pool = new Queue<T>();
    private T _prefab;
    private Transform _parent;
    
    public ObjectPool(T prefab, Transform parent, int initialSize = 10)
    {
        _prefab = prefab;
        _parent = parent;
        
        for (int i = 0; i < initialSize; i++)
        {
            T obj = Object.Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }
    
    public T Get()
    {
        if (_pool.Count > 0)
        {
            T obj = _pool.Dequeue();
            obj.gameObject.SetActive(true);
            return obj;
        }
        
        return Object.Instantiate(_prefab, _parent);
    }
    
    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        _pool.Enqueue(obj);
    }
}
```

## 6. 错误处理规范

### 6.1 异常处理
```csharp
/// <summary>
/// 加载配置文件
/// </summary>
/// <param name="configPath">配置文件路径</param>
/// <returns>是否加载成功</returns>
public bool LoadConfig(string configPath)
{
    try
    {
        if (string.IsNullOrEmpty(configPath))
        {
            Debug.LogError("配置文件路径不能为空");
            return false;
        }
        
        // 加载逻辑
        return true;
    }
    catch (FileNotFoundException ex)
    {
        Debug.LogError($"配置文件未找到: {configPath}, 错误: {ex.Message}");
        return false;
    }
    catch (Exception ex)
    {
        Debug.LogError($"加载配置文件失败: {ex.Message}");
        return false;
    }
}
```

### 6.2 空值检查
```csharp
/// <summary>
/// 设置目标对象
/// </summary>
/// <param name="target">目标对象</param>
public void SetTarget(Transform target)
{
    if (target == null)
    {
        Debug.LogWarning("目标对象为空");
        return;
    }
    
    _target = target;
}
```

## 7. 调试和日志规范

### 7.1 日志使用
```csharp
// 正确的日志使用示例
public class GameManager : MonoBehaviour
{
    [SerializeField] private bool _enableDebugMode = false;
    
    private void LogDebug(string message)
    {
        if (_enableDebugMode)
        {
            Debug.Log($"[GameManager] {message}");
        }
    }
    
    private void LogError(string message)
    {
        Debug.LogError($"[GameManager] {message}");
    }
    
    public void Initialize()
    {
        LogDebug("开始初始化游戏管理器");
        
        try
        {
            // 初始化逻辑
            LogDebug("游戏管理器初始化完成");
        }
        catch (Exception ex)
        {
            LogError($"初始化失败: {ex.Message}");
        }
    }
}
```

### 7.2 条件编译
```csharp
public class DebugManager : MonoBehaviour
{
    [Conditional("UNITY_EDITOR")]
    public void ShowDebugInfo()
    {
        // 只在编辑器中执行的调试代码
        Debug.Log("调试信息");
    }
    
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 编辑器中的Gizmos绘制
    }
    #endif
}
```

## 8. Git提交规范

### 8.1 提交信息格式
```
<type>(<scope>): <subject>

<body>

<footer>
```

### 8.2 提交类型
- `feat`: 新功能
- `fix`: Bug修复
- `docs`: 文档更新
- `style`: 代码格式调整（不影响功能）
- `refactor`: 代码重构
- `test`: 测试相关
- `chore`: 构建过程或辅助工具变动

### 8.3 提交示例
```
feat(player): 添加双职业系统

- 实现商人和修士职业切换
- 添加职业技能树
- 完成职业数据保存

Closes #123
```

## 9. 代码审查清单

### 9.1 功能性检查
- [ ] 代码实现了预期功能
- [ ] 边界条件处理正确
- [ ] 错误处理完善
- [ ] 性能考虑合理

### 9.2 代码质量检查
- [ ] 命名规范符合要求
- [ ] 代码结构清晰
- [ ] 注释充分且准确
- [ ] 无重复代码

### 9.3 Unity特定检查
- [ ] 组件引用正确
- [ ] 生命周期方法使用恰当
- [ ] 内存泄漏风险评估
- [ ] 平台兼容性考虑

## 15. 编辑器工具开发规范

### 15.1 编辑器工具命名规范

#### 类命名
```csharp
// 编辑器工具类必须以Editor结尾
public class ConfigEditorWindow : FXEditorBase
public class AssetNamingValidator : FXEditorBase
public class BuildAutomation : FXEditorBase

// 基础框架类使用FX前缀
public abstract class FXEditorBase : EditorWindow
```

#### 菜单项命名
```csharp
// 使用统一的菜单根路径
private const string MENU_ROOT = "FanXing/";

// 菜单项按功能分组
[MenuItem(MENU_ROOT + "配置编辑器/策划配置工具", false, 1)]
[MenuItem(MENU_ROOT + "质量工具/资源命名检查", false, 10)]
[MenuItem(MENU_ROOT + "构建工具/构建管理器", false, 20)]
```

### 15.2 编辑器工具架构规范

#### 继承结构
```csharp
// 所有编辑器工具必须继承FXEditorBase
public class YourEditorTool : FXEditorBase
{
    // 必须实现的抽象方法
    protected override void LoadData() { }
    protected override void SaveData() { }
    protected override void OnGUI() { }
}
```

#### 数据管理
```csharp
// 使用统一的数据持久化方法
protected void SaveConfig<T>(T data, string fileName)
{
    ExportJsonConfig(data, fileName);
}

protected T LoadConfig<T>(string fileName) where T : class, new()
{
    return ImportJsonConfig<T>(fileName);
}
```

### 15.3 用户界面规范

#### 样式使用
```csharp
// 使用基类提供的统一样式
DrawTitle("工具标题");
DrawHeader("分组标题");
DrawButtonGroup(
    ("按钮1", Action1),
    ("按钮2", Action2)
);
```

#### 错误处理
```csharp
// 使用统一的错误处理方法
try
{
    // 工具操作
}
catch (Exception e)
{
    ShowErrorMessage($"操作失败: {e.Message}");
    LogError($"详细错误: {e}");
}
```

### 15.4 配置数据规范

#### 数据结构定义
```csharp
// 配置数据类必须标记为Serializable
[System.Serializable]
public class NPCConfigData
{
    public int npcId;
    public string npcName;
    public NPCType npcType;
    // 其他配置字段
}
```

#### 数据验证
```csharp
// 提供数据验证方法
private bool ValidateNPCData(NPCConfigData data)
{
    if (data.npcId <= 0)
    {
        ShowErrorMessage("NPC ID必须大于0");
        return false;
    }

    if (string.IsNullOrEmpty(data.npcName))
    {
        ShowErrorMessage("NPC名称不能为空");
        return false;
    }

    return true;
}
```

### 15.5 质量保证规范

#### 编译检查
- 所有编辑器工具必须零错误零警告编译
- 使用Unity 2022.3 LTS兼容的API
- 避免使用过时的API和方法

#### 功能测试
- 每个工具功能必须经过完整测试
- 提供错误情况的处理和恢复
- 确保工具的稳定性和可靠性

#### 文档要求
- 每个工具类必须有完整的XML注释
- 提供使用说明和示例代码
- 记录已知问题和解决方案

---

**注：本规范将根据项目发展需要持续更新和完善。**
