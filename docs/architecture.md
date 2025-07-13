# 繁星Demo架构设计文档

## 文档信息
- **作者**：黄畅修（主程序）
- **创建时间**：2025-07-12
- **版本**：v3.0
- **更新时间**：2025-07-12
- **架构状态**：主体架构100%完成
- **代码统计**：18个核心文件，6200行代码

## 1. 架构概述

### 1.1 设计原则
- **分层架构**：管理器层 → 系统层 → 组件层，职责清晰
- **模块化设计**：各系统相对独立，便于开发和维护
- **单一职责**：每个类和模块只负责一个明确的功能
- **松耦合**：通过事件系统和接口降低模块间依赖
- **事件驱动**：使用EventManager实现系统间通信
- **数据驱动**：配置文件和数据分离，便于策划调整
- **接口导向**：所有系统实现IGameSystem接口
- **可扩展性**：为正式版网络功能预留接口
- **性能优先**：智能计算和缓存机制优化性能

### 1.2 技术栈
- **游戏引擎**：Unity 2022.3.12f1 LTS
- **编程语言**：C#（严格遵循编码规范）
- **数据管理**：ScriptableObject + JSON + 自定义序列化
- **UI框架**：Unity UGUI + 自定义UIManager
- **动画系统**：Unity Animation + DOTween
- **事件系统**：自定义EventManager
- **配置管理**：数据驱动的配置系统

## 2. 整体架构

### 2.1 架构层次
```
┌─────────────────────────────────────┐
│           表现层 (Presentation)      │
│  UI界面、特效、音效、输入处理        │
│  Player组件、动画控制、视觉效果      │
├─────────────────────────────────────┤
│           管理器层 (Manager)         │
│  8个核心管理器：游戏、UI、数据、     │
│  事件、系统、职业、技能、属性        │
├─────────────────────────────────────┤
│           系统层 (System)            │
│  游戏系统：玩家、NPC、种田、战斗、   │
│  经济、任务、商铺等业务逻辑          │
├─────────────────────────────────────┤
│           数据层 (Data)              │
│  数据结构、配置管理、序列化、        │
│  事件参数、枚举定义                  │
├─────────────────────────────────────┤
│           基础层 (Foundation)        │
│  工具类、扩展方法、第三方插件        │
└─────────────────────────────────────┘
```

### 2.2 核心管理器体系（已完成）
```
SystemManager (系统管理器) - 统一管理所有游戏系统
    ├── GameManager (游戏总控) - 游戏状态和生命周期
    ├── UIManager (UI管理) - 界面显示和切换
    ├── DataManager (数据管理) - 存档和配置文件
    ├── EventManager (事件管理) - 全局事件通信
    ├── ProfessionManager (职业管理) - 双职业系统
    ├── SkillManager (技能管理) - 技能学习和使用
    └── AttributeManager (属性管理) - 角色属性计算
    └── SystemManager (系统管理)
            ├── PlayerSystem (玩家系统)
            ├── NPCSystem (NPC系统)
            ├── FarmingSystem (种田系统)
            ├── CombatSystem (战斗系统)
            ├── EconomySystem (经济系统)
            ├── QuestSystem (任务系统)
            └── ShopSystem (商铺系统)
```

## 3. 核心管理器设计

### 3.1 GameManager (游戏管理器)
**职责**：
- 游戏状态管理（主菜单、游戏中、暂停等）
- 核心系统初始化和生命周期管理
- 全局事件处理
- 游戏流程控制

**主要接口**：
```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; }
    public GameState CurrentState { get; }
    public bool IsGamePaused { get; }
    
    public void ChangeGameState(GameState newState);
    public void PauseGame();
    public void ResumeGame();
    public void QuitGame();
}
```

### 3.2 UIManager (UI管理器)
**职责**：
- UI界面的显示、隐藏、切换
- UI堆栈管理
- UI事件处理
- UI资源管理

**主要接口**：
```csharp
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; }
    public Canvas MainCanvas { get; }
    public string CurrentUI { get; }
    
    public void ShowUI(string uiName, bool hideOthers = true);
    public void HideUI(string uiName);
    public void HideAllUI();
    public void GoBack();
}
```

### 3.3 DataManager (数据管理器)
**职责**：
- 游戏数据的保存和加载
- 配置文件管理
- 数据加密和验证
- 数据缓存管理

**主要接口**：
```csharp
public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; }
    public GameData CurrentGameData { get; }
    public bool HasSaveData { get; }
    
    public void SaveGameData();
    public void LoadGameData();
    public T GetConfig<T>(string configName) where T : ScriptableObject;
}
```

### 3.4 EventManager (事件管理器)
**职责**：
- 全局事件的注册、触发、注销
- 模块间通信
- 事件参数传递
- 事件优先级管理

**主要接口**：
```csharp
public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; }
    
    public void RegisterEvent(string eventName, System.Action<object> callback);
    public void UnregisterEvent(string eventName, System.Action<object> callback);
    public void TriggerEvent(string eventName, object eventArgs = null);
}
```

## 4. 游戏系统设计

### 4.1 玩家系统 (PlayerSystem)
**功能**：
- 玩家角色管理
- 双职业系统（商人、修士）
- 属性和等级管理
- 技能系统

**核心类**：
- `Player`：玩家主类
- `PlayerData`：玩家数据
- `ProfessionManager`：职业管理
- `SkillManager`：技能管理

### 4.2 NPC系统 (NPCSystem)
**功能**：
- 7个功能NPC管理
- NPC对话系统
- NPC交互逻辑
- NPC状态管理

**核心类**：
- `NPC`：NPC基类
- `NPCManager`：NPC管理器
- `DialogueSystem`：对话系统
- `InteractionSystem`：交互系统

### 4.3 种田系统 (FarmingSystem)
**功能**：
- 作物种植和收获
- 土地管理
- 作物生长周期
- 收益计算

**核心类**：
- `FarmManager`：种田管理器
- `Crop`：作物类
- `FarmLand`：土地类
- `GrowthSystem`：生长系统

### 4.4 战斗系统 (CombatSystem)
**功能**：
- 野外敌对生物
- 战斗逻辑
- AI行为
- 伤害计算

**核心类**：
- `CombatManager`：战斗管理器
- `Enemy`：敌人类
- `CombatAI`：战斗AI
- `DamageSystem`：伤害系统

## 5. 数据架构

### 5.1 数据分层
```
配置数据 (ScriptableObject)
    ├── GameConfig (游戏配置)
    ├── NPCConfig (NPC配置)
    ├── ItemConfig (物品配置)
    ├── SkillConfig (技能配置)
    └── CropConfig (作物配置)

运行时数据 (Class)
    ├── GameData (游戏数据)
    ├── PlayerData (玩家数据)
    ├── WorldData (世界数据)
    └── GameSettings (游戏设置)

持久化数据 (JSON)
    ├── gamedata.json (存档文件)
    ├── settings.json (设置文件)
    └── cache.json (缓存文件)
```

### 5.2 数据流向
```
配置文件 → DataManager → 游戏系统 → UI显示
    ↓
存档文件 ← DataManager ← 游戏系统 ← 用户操作
```

## 6. UI架构

### 6.1 UI层级结构
```
MainCanvas (主画布)
    ├── Background (背景层) - SortingOrder: 0
    ├── Game (游戏层) - SortingOrder: 100
    ├── UI (界面层) - SortingOrder: 200
    ├── Popup (弹窗层) - SortingOrder: 300
    └── System (系统层) - SortingOrder: 400
```

### 6.2 UI界面管理
- **界面堆栈**：管理界面的显示顺序和返回逻辑
- **界面缓存**：常用界面保持在内存中，提高响应速度
- **界面预加载**：关键界面提前加载，减少等待时间
- **界面池化**：相似界面使用对象池，优化内存使用

## 7. 性能优化策略

### 7.1 内存优化
- **对象池**：频繁创建销毁的对象使用对象池
- **资源管理**：及时释放不用的资源
- **纹理压缩**：合理设置纹理格式和压缩
- **模型优化**：控制模型面数和贴图大小

### 7.2 渲染优化
- **批处理**：合并相同材质的渲染调用
- **遮挡剔除**：隐藏不可见的物体
- **LOD系统**：根据距离使用不同精度的模型
- **光照优化**：合理使用实时光照和烘焙光照

### 7.3 逻辑优化
- **帧率分摊**：将耗时操作分摊到多帧执行
- **事件驱动**：减少不必要的Update调用
- **数据缓存**：缓存计算结果，避免重复计算
- **异步加载**：使用协程进行异步操作

## 8. 扩展性设计

### 8.1 网络功能预留
- **数据同步接口**：为多人游戏预留数据同步接口
- **网络管理器**：预留网络管理器框架
- **服务器通信**：预留客户端-服务器通信接口
- **状态同步**：设计支持网络同步的状态管理

### 8.2 模块化扩展
- **插件系统**：支持功能模块的热插拔
- **配置驱动**：通过配置文件控制功能开关
- **接口抽象**：使用接口定义系统间的交互
- **事件解耦**：通过事件系统降低模块依赖

## 9. 开发规范

### 9.1 代码组织
- **命名空间**：使用FanXing命名空间
- **文件夹结构**：按功能模块组织代码文件
- **命名规范**：遵循C#命名约定
- **注释规范**：重要方法和类添加XML注释

### 9.2 设计模式
- **单例模式**：管理器类使用单例模式
- **观察者模式**：事件系统使用观察者模式
- **工厂模式**：对象创建使用工厂模式
- **状态模式**：游戏状态使用状态模式

## 10. 部署和构建

### 10.1 构建配置
- **开发版本**：包含调试信息和日志
- **测试版本**：优化性能，保留部分调试功能
- **发布版本**：完全优化，移除调试代码

### 10.2 平台适配
- **Windows**：主要目标平台
- **macOS**：次要目标平台
- **Linux**：可选目标平台

---

**注：本文档将随着开发进度持续更新和完善。**
