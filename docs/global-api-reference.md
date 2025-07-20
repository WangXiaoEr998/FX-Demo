# Global统一入口API参考文档

## 文档信息
- **作者**：黄畅修（主程序）
- **创建时间**：2025-07-13
- **版本**：v1.0
- **更新时间**：2025-07-13
- **API状态**：Global统一入口API已完成

## 概述

Global类是FX-Demo项目的统一访问入口，提供所有管理器和系统的访问接口，彻底解决模块间直接依赖问题，实现完全的解耦合架构。

### 核心特性
- **统一访问入口**：所有外部访问都通过Global类
- **类型安全**：编译时类型检查和智能代码提示
- **事件驱动**：基于事件常量的类型安全事件系统
- **完全解耦合**：消除所有模块间的直接依赖

## 1. Global类API

### 1.1 类定义
```csharp
public static class Global
```

### 1.2 管理器访问接口

#### 游戏管理器
```csharp
public static GameManager Game => GameManager.Instance;
```
**说明**：获取游戏管理器实例，用于游戏状态控制和生命周期管理。

#### 事件管理器
```csharp
public static EventManager Event => EventManager.Instance;
```
**说明**：获取事件管理器实例，用于事件注册、触发和注销。

#### 数据管理器
```csharp
public static DataManager Data => DataManager.Instance;
```
**说明**：获取数据管理器实例，用于游戏数据保存、加载和配置管理。

#### UI管理器
```csharp
public static UIManager UI => UIManager.Instance;
```
**说明**：获取UI管理器实例，用于界面显示、隐藏和切换。

#### 系统管理器
```csharp
public static SystemManager System => SystemManager.Instance;
```
**说明**：获取系统管理器实例，用于游戏系统的统一管理。

#### 职业管理器
```csharp
public static ProfessionManager Profession => ProfessionManager.Instance;
```
**说明**：获取职业管理器实例，用于双职业系统管理。

#### 技能管理器
```csharp
public static SkillManager Skill => SkillManager.Instance;
```
**说明**：获取技能管理器实例，用于技能学习、升级和使用。

#### 属性管理器
```csharp
public static AttributeManager Attribute => AttributeManager.Instance;
```
**说明**：获取属性管理器实例，用于角色属性计算和管理。

### 1.3 系统访问接口

#### 泛型系统获取
```csharp
public static T GetSystem<T>() where T : class, IGameSystem
```
**说明**：获取指定类型的游戏系统实例。
**参数**：
- `T`：系统类型，必须实现IGameSystem接口
**返回值**：系统实例，如果未找到则返回null

#### 玩家系统
```csharp
public static PlayerSystem Player => GetSystem<PlayerSystem>();
```
**说明**：获取玩家系统实例，用于玩家相关功能管理。

#### NPC系统
```csharp
public static NPCSystem NPC => GetSystem<NPCSystem>();
```
**说明**：获取NPC系统实例，用于NPC交互和对话管理。

#### 种田系统
```csharp
public static FarmingSystem Farming => GetSystem<FarmingSystem>();
```
**说明**：获取种田系统实例，用于作物种植和收获管理。

#### 战斗系统
```csharp
public static CombatSystem Combat => GetSystem<CombatSystem>();
```
**说明**：获取战斗系统实例，用于战斗逻辑和AI管理。

#### 经济系统
```csharp
public static EconomySystem Economy => GetSystem<EconomySystem>();
```
**说明**：获取经济系统实例，用于金钱和交易管理。

#### 任务系统
```csharp
public static QuestSystem Quest => GetSystem<QuestSystem>();
```
**说明**：获取任务系统实例，用于任务接取、完成和管理。

#### 商店系统
```csharp
public static ShopSystem Shop => GetSystem<ShopSystem>();
```
**说明**：获取商店系统实例，用于商店租赁和商品管理。

### 1.4 事件系统统一接口

#### 事件注册
```csharp
public static void RegisterEvent(string eventName, Action<object> listener)
```
**说明**：注册事件监听器。
**参数**：
- `eventName`：事件名称，建议使用EventConstants中的常量
- `listener`：事件监听器回调函数

#### 泛型事件注册
```csharp
public static void RegisterEvent<T>(string eventName, Action<T> listener) where T : class
```
**说明**：注册类型安全的事件监听器。
**参数**：
- `T`：事件参数类型
- `eventName`：事件名称
- `listener`：类型安全的事件监听器回调函数

#### 事件触发
```csharp
public static void TriggerEvent(string eventName, object eventArgs = null)
```
**说明**：触发事件。
**参数**：
- `eventName`：事件名称，建议使用EventConstants中的常量
- `eventArgs`：事件参数，可选

#### 事件注销
```csharp
public static void UnregisterEvent(string eventName, Action<object> listener)
```
**说明**：注销事件监听器。
**参数**：
- `eventName`：事件名称
- `listener`：要注销的事件监听器

#### 延迟事件触发
```csharp
public static void TriggerEventDelayed(string eventName, object eventArgs, float delay)
```
**说明**：延迟触发事件。
**参数**：
- `eventName`：事件名称
- `eventArgs`：事件参数
- `delay`：延迟时间（秒）

### 1.5 便捷方法

#### 获取当前游戏状态
```csharp
public static GameState CurrentGameState => Game?.CurrentState ?? GameState.MainMenu;
```
**说明**：获取当前游戏状态。
**返回值**：当前游戏状态枚举值

#### 获取当前玩家数据
```csharp
public static PlayerData CurrentPlayerData => Data?.GetPlayerData();
```
**说明**：获取当前玩家数据。
**返回值**：玩家数据对象，如果未加载则返回null

#### 检查系统是否已初始化
```csharp
public static bool IsSystemInitialized<T>() where T : class, IGameSystem
```
**说明**：检查指定系统是否已初始化。
**参数**：
- `T`：系统类型
**返回值**：是否已初始化

#### 检查系统是否正在运行
```csharp
public static bool IsSystemRunning<T>() where T : class, IGameSystem
```
**说明**：检查指定系统是否正在运行。
**参数**：
- `T`：系统类型
**返回值**：是否正在运行

#### 统一日志输出
```csharp
public static void Log(string message, LogType logType = LogType.Log)
```
**说明**：统一的日志输出接口。
**参数**：
- `message`：日志消息
- `logType`：日志类型（Log、Warning、Error）

#### 检查全局系统就绪状态
```csharp
public static bool IsReady()
```
**说明**：检查全局系统是否就绪。
**返回值**：所有核心管理器是否都已初始化

#### 获取系统就绪状态报告
```csharp
public static string GetReadinessReport()
```
**说明**：获取详细的系统就绪状态报告。
**返回值**：包含所有管理器状态的报告字符串

## 2. 事件常量API

### 2.1 EventConstants类
```csharp
public static class EventConstants
```

### 2.2 游戏状态事件
```csharp
public static class GameState
{
    public const string STATE_CHANGED = "Game.StateChanged";
    public const string STARTED = "Game.Started";
    public const string PAUSED = "Game.Paused";
    public const string RESUMED = "Game.Resumed";
    public const string ENDED = "Game.Ended";
    public const string RESTARTED = "Game.Restarted";
}
```

### 2.3 玩家事件
```csharp
public static class Player
{
    public const string CREATED = "Player.Created";
    public const string LOADED = "Player.Loaded";
    public const string LEVEL_UP = "Player.LevelUp";
    public const string PROFESSION_CHANGED = "Player.ProfessionChanged";
    public const string POSITION_CHANGED = "Player.PositionChanged";
    public const string HEALTH_CHANGED = "Player.HealthChanged";
    public const string MANA_CHANGED = "Player.ManaChanged";
    public const string EXPERIENCE_GAINED = "Player.ExperienceGained";
    public const string DEATH = "Player.Death";
    public const string RESPAWN = "Player.Respawn";
}
```

### 2.4 技能事件
```csharp
public static class Skill
{
    public const string LEARNED = "Skill.Learned";
    public const string UPGRADED = "Skill.Upgraded";
    public const string USED = "Skill.Used";
    public const string COOLDOWN_STARTED = "Skill.CooldownStarted";
    public const string COOLDOWN_FINISHED = "Skill.CooldownFinished";
    public const string POINTS_GAINED = "Skill.PointsGained";
}
```

## 3. 使用示例

### 3.1 基本使用
```csharp
// 获取管理器
var gameManager = Global.Game;
var playerData = Global.CurrentPlayerData;

// 获取系统
var playerSystem = Global.Player;
var npcSystem = Global.NPC;

// 检查系统状态
if (Global.IsSystemRunning<PlayerSystem>())
{
    // 系统正在运行
}
```

### 3.2 事件使用
```csharp
// 注册事件监听器
Global.RegisterEvent<PlayerLevelUpEventArgs>(
    Global.Events.Player.LEVEL_UP, 
    OnPlayerLevelUp
);

// 触发事件
var args = new PlayerLevelUpEventArgs("Player1", 5, 6, 1000);
Global.TriggerEvent(Global.Events.Player.LEVEL_UP, args);

// 注销事件监听器
Global.UnregisterEvent(Global.Events.Player.LEVEL_UP, OnPlayerLevelUp);
```

### 3.3 系统交互
```csharp
// 通过Global访问系统功能
Global.Player.AddExperience(100);
Global.Data.SaveGameData();
Global.UI.ShowPanel(UIPanelType.Inventory);

// 检查系统就绪状态
if (Global.IsReady())
{
    Debug.Log("所有系统已就绪");
    Debug.Log(Global.GetReadinessReport());
}
```

## 4. 最佳实践

### 4.1 推荐用法
- 始终使用Global入口访问管理器和系统
- 使用事件常量而非硬编码字符串
- 利用类型安全的事件接口
- 在访问前检查系统就绪状态

### 4.2 避免的用法
- 直接访问Manager.Instance
- 使用硬编码的事件名称字符串
- 在系统间建立直接依赖关系
- 忽略空值检查和异常处理

---

**注：本文档将随着Global架构的完善持续更新。**
