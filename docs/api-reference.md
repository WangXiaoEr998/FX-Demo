# 繁星Demo API参考文档

## 文档信息
- **作者**：黄畅修（主程序）
- **创建时间**：2025-07-12
- **版本**：v3.0
- **更新时间**：2025-07-12
- **架构状态**：主体架构100%完成
- **API覆盖**：18个核心文件，100%API文档覆盖

## API概述

本文档提供繁星Demo项目的完整API参考，包括8个核心管理器、完整的系统架构、数据结构和组件系统的详细API说明。

### 架构层次
- **管理器层**：8个核心管理器
- **系统层**：游戏系统架构
- **数据层**：完整的数据结构
- **组件层**：玩家组件系统

## 1. 核心管理器API（8个管理器）

### 1.1 GameManager

#### 基本信息
- **文件位置**：Assets/Scripts/Managers/GameManager.cs
- **继承**：MonoBehaviour
- **模式**：单例模式
- **描述**：游戏总控制器，负责游戏状态管理和核心系统初始化
- **代码行数**：363行
- **完成状态**：已完成

### 1.2 EventManager

#### 基本信息
- **文件位置**：Assets/Scripts/Managers/EventManager.cs
- **继承**：MonoBehaviour
- **模式**：单例模式
- **描述**：事件管理器，提供全局事件通信机制
- **代码行数**：300行
- **完成状态**：已完成

#### 核心功能
- 事件注册和注销
- 即时和延迟事件触发
- 事件统计和调试支持
- 异常处理和错误恢复

#### 主要方法

##### RegisterEvent
```csharp
public void RegisterEvent(string eventName, Action<object> callback)
public void RegisterEvent(string eventName, Action callback)
```
- **描述**：注册事件监听器
- **参数**：
  - `eventName`：事件名称
  - `callback`：回调函数
- **示例**：
```csharp
EventManager.Instance.RegisterEvent("OnPlayerLevelUp", OnPlayerLevelUpHandler);
```

##### TriggerEvent
```csharp
public void TriggerEvent(string eventName, object eventArgs = null)
```
- **描述**：触发事件
- **参数**：
  - `eventName`：事件名称
  - `eventArgs`：事件参数（可选）
- **示例**：
```csharp
EventManager.Instance.TriggerEvent("OnPlayerLevelUp", new PlayerLevelUpEventArgs(oldLevel, newLevel));
```

##### TriggerEventDelayed
```csharp
public void TriggerEventDelayed(string eventName, float delay, object eventArgs = null)
```
- **描述**：延迟触发事件
- **参数**：
  - `eventName`：事件名称
  - `delay`：延迟时间（秒）
  - `eventArgs`：事件参数（可选）

### 1.3 SystemManager

#### 基本信息
- **文件位置**：Assets/Scripts/Managers/SystemManager.cs
- **继承**：MonoBehaviour
- **模式**：单例模式
- **描述**：系统管理器，统一管理所有游戏系统的生命周期
- **代码行数**：300行
- **完成状态**：已完成

#### 管理的系统
- PlayerSystem：玩家系统
- NPCSystem：NPC系统（预留）
- FarmingSystem：种田系统（预留）
- CombatSystem：战斗系统（预留）
- EconomySystem：经济系统（预留）
- QuestSystem：任务系统（预留）
- ShopSystem：商铺系统（预留）

#### 主要方法

##### GetSystem<T>
```csharp
public T GetSystem<T>() where T : class, IGameSystem
```
- **描述**：获取指定类型的游戏系统
- **返回值**：系统实例，如果未找到返回null
- **示例**：
```csharp
PlayerSystem playerSystem = SystemManager.Instance.GetSystem<PlayerSystem>();
```

##### RegisterSystem
```csharp
public void RegisterSystem(IGameSystem system)
```
- **描述**：注册游戏系统
- **参数**：
  - `system`：要注册的系统实例

### 1.4 ProfessionManager

#### 基本信息
- **文件位置**：Assets/Scripts/Managers/ProfessionManager.cs
- **继承**：MonoBehaviour
- **描述**：职业管理器，负责双职业系统的管理
- **代码行数**：300行
- **完成状态**：已完成

#### 核心功能
- 双职业切换管理
- 职业经验和等级计算
- 职业效果应用
- 职业配置管理

#### 主要方法

##### SwitchProfession
```csharp
public bool SwitchProfession(ProfessionType profession)
```
- **描述**：切换职业
- **参数**：
  - `profession`：目标职业类型
- **返回值**：是否切换成功

##### AddExperience
```csharp
public void AddExperience(ProfessionType profession, int experience)
```
- **描述**：增加职业经验
- **参数**：
  - `profession`：职业类型
  - `experience`：经验值

### 1.5 SkillManager

#### 基本信息
- **文件位置**：Assets/Scripts/Managers/SkillManager.cs
- **继承**：MonoBehaviour
- **描述**：技能管理器，负责技能学习、升级和使用
- **代码行数**：600行
- **完成状态**：已完成

#### 核心功能
- 技能学习和升级
- 技能使用和冷却管理
- 技能效果应用
- 技能配置管理

#### 主要方法

##### LearnSkill
```csharp
public bool LearnSkill(string skillId)
```
- **描述**：学习技能
- **参数**：
  - `skillId`：技能ID
- **返回值**：是否学习成功

##### UseSkill
```csharp
public bool UseSkill(string skillId, GameObject target = null)
```
- **描述**：使用技能
- **参数**：
  - `skillId`：技能ID
  - `target`：目标对象（可选）
- **返回值**：是否使用成功

### 1.6 AttributeManager

#### 基本信息
- **文件位置**：Assets/Scripts/Managers/AttributeManager.cs
- **继承**：MonoBehaviour
- **描述**：属性管理器，负责角色属性计算和管理
- **代码行数**：500行
- **完成状态**：已完成

#### 核心功能
- 属性计算和缓存
- 装备加成管理
- 技能加成管理
- 临时属性修改器

#### 主要方法

##### GetCurrentAttributes
```csharp
public PlayerAttributes GetCurrentAttributes()
```
- **描述**：获取当前计算后的属性
- **返回值**：当前属性对象

##### AddTemporaryBonus
```csharp
public void AddTemporaryBonus(int str, int agi, int intel, int vit, int luck, float duration = -1f)
```
- **描述**：添加临时属性加成
- **参数**：
  - `str`：力量加成
  - `agi`：敏捷加成
  - `intel`：智力加成
  - `vit`：体质加成
  - `luck`：幸运加成
  - `duration`：持续时间（-1表示永久）

### 1.7 UIManager

#### 基本信息
- **文件位置**：Assets/Scripts/Managers/UIManager.cs
- **继承**：MonoBehaviour
- **模式**：单例模式
- **描述**：UI界面管理器，负责界面显示和切换
- **代码行数**：363行
- **完成状态**：已完成

### 1.8 DataManager

#### 基本信息
- **文件位置**：Assets/Scripts/Managers/DataManager.cs
- **继承**：MonoBehaviour
- **模式**：单例模式
- **描述**：数据管理器，负责存档和配置文件管理
- **代码行数**：395行
- **完成状态**：已完成

## 2. 游戏系统架构API

### 2.1 IGameSystem接口

#### 基本信息
- **文件位置**：Assets/Scripts/Systems/IGameSystem.cs
- **类型**：接口
- **描述**：定义所有游戏系统必须实现的标准接口
- **代码行数**：50行
- **完成状态**：已完成

#### 接口方法
```csharp
public interface IGameSystem
{
    string SystemName { get; }
    bool IsInitialized { get; }
    bool IsRunning { get; }
    int Priority { get; }

    void Initialize();
    void Start();
    void Update(float deltaTime);
    void FixedUpdate(float fixedDeltaTime);
    void Shutdown();
    void Reset();
    void Pause();
    void Resume();
}
```

### 2.2 BaseGameSystem基类

#### 基本信息
- **文件位置**：Assets/Scripts/Systems/BaseGameSystem.cs
- **继承**：MonoBehaviour, IGameSystem
- **类型**：抽象基类
- **描述**：提供游戏系统的基础实现和工具方法
- **代码行数**：300行
- **完成状态**：已完成

#### 核心功能
- 标准的系统生命周期管理
- 模板方法模式实现
- 事件系统集成
- 调试和日志支持

#### 抽象方法
```csharp
protected abstract void OnInitialize();
protected abstract void OnStart();
protected abstract void OnUpdate(float deltaTime);
```

#### 工具方法
```csharp
protected void LogDebug(string message);
protected void LogWarning(string message);
protected void LogError(string message);
protected void TriggerEvent(string eventName, object eventArgs = null);
protected void RegisterEvent(string eventName, System.Action<object> callback);
```

### 2.3 PlayerSystem玩家系统

#### 基本信息
- **文件位置**：Assets/Scripts/Systems/PlayerSystem.cs
- **继承**：BaseGameSystem
- **描述**：完整的玩家系统，管理角色、双职业、技能和属性
- **代码行数**：1100行
- **完成状态**：已完成

#### 子管理器
- **ProfessionManager**：职业管理
- **SkillManager**：技能管理
- **AttributeManager**：属性管理

#### 核心功能
- 玩家创建和加载
- 双职业系统管理
- 技能学习和使用
- 属性计算和管理
- 玩家状态机

#### 主要方法

##### CreateNewPlayer
```csharp
public bool CreateNewPlayer(string playerName, ProfessionType initialProfession)
```
- **描述**：创建新玩家
- **参数**：
  - `playerName`：玩家名称
  - `initialProfession`：初始职业
- **返回值**：是否创建成功

##### SwitchProfession
```csharp
public bool SwitchProfession(ProfessionType profession)
```
- **描述**：切换职业
- **参数**：
  - `profession`：目标职业
- **返回值**：是否切换成功

##### AddExperience
```csharp
public void AddExperience(int amount)
```
- **描述**：增加经验值
- **参数**：
  - `amount`：经验值数量

## 3. 数据结构API

### 3.1 枚举定义

#### 基本信息
- **文件位置**：Assets/Scripts/Data/Enums.cs
- **描述**：包含25个枚举定义，覆盖所有游戏功能
- **代码行数**：300行
- **完成状态**：已完成

#### 核心枚举
- **GameState**：游戏状态（MainMenu、Playing、Paused、Loading、GameOver）
- **ProfessionType**：职业类型（None、Merchant、Cultivator）
- **PlayerState**：玩家状态（Idle、Moving、Fighting、Farming、Trading等）
- **ItemType**：物品类型（11种类型）
- **ItemQuality**：物品品质（5个等级）
- **SkillType**：技能类型（5种类型）
- **QuestStatus**：任务状态（7种状态）

### 3.2 PlayerData玩家数据

#### 基本信息
- **文件位置**：Assets/Scripts/Data/PlayerData.cs
- **描述**：完整的玩家数据类，包含13个功能模块
- **代码行数**：300行
- **完成状态**：已完成

#### 数据模块
- 基础信息、属性数据、职业数据
- 技能数据、背包数据、任务数据
- 种田数据、商铺数据、关系数据
- 统计数据、设置数据、时间数据

#### 主要方法
```csharp
public void Initialize(string name, ProfessionType profession);
public bool IsValid();
public void FixInvalidData();
public PlayerData Clone();
```

### 3.3 PlayerAttributes属性系统

#### 基本信息
- **文件位置**：Assets/Scripts/Data/PlayerAttributes.cs
- **描述**：智能属性系统，提供20+个计算属性
- **代码行数**：300行
- **完成状态**：已完成

#### 属性分层
- **基础属性**：baseStrength、baseAgility等
- **装备加成**：equipmentStrength、equipmentAgility等
- **技能加成**：skillStrength、skillAgility等
- **临时加成**：temporaryStrength、temporaryAgility等

#### 计算属性
```csharp
public int TotalStrength { get; }
public int MaxHealth { get; }
public int PhysicalAttack { get; }
public float MoveSpeed { get; }
public float CriticalRate { get; }
```

### 3.4 ItemData物品数据

#### 基本信息
- **文件位置**：Assets/Scripts/Data/ItemData.cs
- **描述**：完整的物品数据类，支持11种物品类型
- **代码行数**：500行
- **完成状态**：已完成

#### 核心功能
- 11种物品类型支持
- 品质系统和耐久度
- 堆叠系统和使用条件
- 数据验证和修复

#### 主要方法
```csharp
public bool CanUse(PlayerData playerData);
public int AddStack(int amount);
public bool ReduceDurability(int amount);
public ItemData Clone();
```

## 4. 组件系统API

### 4.1 Player玩家组件

#### 基本信息
- **文件位置**：Assets/Scripts/Player/Player.cs
- **继承**：MonoBehaviour
- **描述**：玩家组件，处理角色移动、交互和表现
- **代码行数**：300行
- **完成状态**：已完成

#### 核心功能
- 角色移动控制
- 输入处理
- 动画管理
- 交互处理

#### 主要方法
```csharp
public void Initialize(PlayerData playerData, PlayerSystem playerSystem);
public void SetMoveInput(float horizontal, float vertical);
public void TeleportTo(Vector3 position);
public Vector3 GetPosition();
```

## 5. 事件系统API

### 5.1 事件参数类

#### 基本信息
- **文件位置**：Assets/Scripts/Data/EventArgs.cs
- **描述**：完整的事件参数类集合
- **代码行数**：400行
- **完成状态**：已完成

#### 主要事件参数
- **GameStateEventArgs**：游戏状态改变
- **PlayerCreatedEventArgs**：玩家创建
- **PlayerLevelUpEventArgs**：玩家升级
- **ExperienceChangedEventArgs**：经验值变化
- **SkillLearnedEventArgs**：技能学习
- **ItemObtainedEventArgs**：物品获得

## 6. 配置系统API

### 6.1 配置类集合

#### 基本信息
- **文件位置**：Assets/Scripts/Data/ConfigClasses.cs
- **描述**：配置类集合，支持数据驱动设计
- **代码行数**：600行
- **完成状态**：已完成

#### 主要配置类
- **PlayerConfig**：玩家配置
- **ProfessionConfig**：职业配置
- **SkillConfig**：技能配置
- **ItemConfig**：物品配置
- **NPCConfig**：NPC配置
- **QuestConfig**：任务配置

#### 属性

##### Instance
```csharp
public static GameManager Instance { get; }
```
- **描述**：获取GameManager单例实例
- **返回值**：GameManager实例

##### CurrentState
```csharp
public GameState CurrentState { get; }
```
- **描述**：获取当前游戏状态
- **返回值**：当前游戏状态枚举值

##### IsGamePaused
```csharp
public bool IsGamePaused { get; }
```
- **描述**：获取游戏是否处于暂停状态
- **返回值**：true表示暂停，false表示运行

#### 方法

##### ChangeGameState
```csharp
public void ChangeGameState(GameState newState)
```
- **描述**：改变游戏状态
- **参数**：
  - `newState`：新的游戏状态
- **示例**：
```csharp
GameManager.Instance.ChangeGameState(GameState.Playing);
```

##### PauseGame
```csharp
public void PauseGame()
```
- **描述**：暂停游戏
- **效果**：设置Time.timeScale = 0，触发暂停事件

##### ResumeGame
```csharp
public void ResumeGame()
```
- **描述**：恢复游戏
- **效果**：设置Time.timeScale = 1，触发恢复事件

##### QuitGame
```csharp
public void QuitGame()
```
- **描述**：退出游戏
- **效果**：保存数据并退出应用程序

#### 事件

##### OnGameStateChanged
- **触发时机**：游戏状态改变时
- **参数**：GameStateEventArgs（包含前一状态和新状态）

### 1.2 UIManager

#### 基本信息
- **命名空间**：FanXing.UI
- **继承**：MonoBehaviour
- **描述**：UI界面管理器，负责UI的显示、隐藏和切换

#### 属性

##### Instance
```csharp
public static UIManager Instance { get; }
```
- **描述**：获取UIManager单例实例

##### MainCanvas
```csharp
public Canvas MainCanvas { get; }
```
- **描述**：获取主画布组件

##### CurrentUI
```csharp
public string CurrentUI { get; }
```
- **描述**：获取当前显示的UI界面名称

#### 方法

##### ShowUI
```csharp
public void ShowUI(string uiName, bool hideOthers = true)
```
- **描述**：显示指定的UI界面
- **参数**：
  - `uiName`：UI界面名称
  - `hideOthers`：是否隐藏其他界面（默认true）
- **示例**：
```csharp
UIManager.Instance.ShowUI("MainMenu");
UIManager.Instance.ShowUI("PauseMenu", false); // 不隐藏其他界面
```

##### HideUI
```csharp
public void HideUI(string uiName)
```
- **描述**：隐藏指定的UI界面
- **参数**：
  - `uiName`：UI界面名称

##### HideAllUI
```csharp
public void HideAllUI()
```
- **描述**：隐藏所有UI界面

##### GoBack
```csharp
public void GoBack()
```
- **描述**：返回上一个UI界面

##### RegisterUI
```csharp
public void RegisterUI(string uiName, GameObject uiObject)
```
- **描述**：注册UI界面到管理器
- **参数**：
  - `uiName`：UI界面名称
  - `uiObject`：UI界面GameObject

### 1.3 DataManager

#### 基本信息
- **命名空间**：FanXing.Data
- **继承**：MonoBehaviour
- **描述**：数据管理器，负责游戏数据的保存、加载和配置管理

#### 属性

##### Instance
```csharp
public static DataManager Instance { get; }
```
- **描述**：获取DataManager单例实例

##### CurrentGameData
```csharp
public GameData CurrentGameData { get; }
```
- **描述**：获取当前游戏数据

##### HasSaveData
```csharp
public bool HasSaveData { get; }
```
- **描述**：检查是否存在存档文件

#### 方法

##### SaveGameData
```csharp
public void SaveGameData()
```
- **描述**：保存当前游戏数据到文件
- **异常**：可能抛出IO相关异常

##### LoadGameData
```csharp
public void LoadGameData()
```
- **描述**：从文件加载游戏数据
- **行为**：如果文件不存在，创建新的游戏数据

##### CreateNewGameData
```csharp
public void CreateNewGameData()
```
- **描述**：创建新的游戏数据

##### DeleteSaveData
```csharp
public void DeleteSaveData()
```
- **描述**：删除存档文件

##### GetConfig<T>
```csharp
public T GetConfig<T>(string configName) where T : ScriptableObject
```
- **描述**：获取指定类型的配置对象
- **参数**：
  - `configName`：配置名称
- **返回值**：配置对象，如果不存在返回null
- **示例**：
```csharp
NPCConfig npcConfig = DataManager.Instance.GetConfig<NPCConfig>("NPCConfig");
```

##### GetPlayerData
```csharp
public PlayerData GetPlayerData()
```
- **描述**：获取玩家数据

##### UpdatePlayerData
```csharp
public void UpdatePlayerData(PlayerData playerData)
```
- **描述**：更新玩家数据
- **参数**：
  - `playerData`：新的玩家数据

### 1.4 EventManager

#### 基本信息
- **命名空间**：FanXing.Events
- **继承**：MonoBehaviour
- **描述**：事件管理器，负责全局事件的注册、触发和注销

#### 属性

##### Instance
```csharp
public static EventManager Instance { get; }
```
- **描述**：获取EventManager单例实例

#### 方法

##### RegisterEvent
```csharp
public void RegisterEvent(string eventName, System.Action<object> callback)
```
- **描述**：注册事件监听器
- **参数**：
  - `eventName`：事件名称
  - `callback`：回调函数
- **示例**：
```csharp
EventManager.Instance.RegisterEvent("OnPlayerDeath", OnPlayerDeathHandler);
```

##### UnregisterEvent
```csharp
public void UnregisterEvent(string eventName, System.Action<object> callback)
```
- **描述**：注销事件监听器
- **参数**：
  - `eventName`：事件名称
  - `callback`：回调函数

##### TriggerEvent
```csharp
public void TriggerEvent(string eventName, object eventArgs = null)
```
- **描述**：触发事件
- **参数**：
  - `eventName`：事件名称
  - `eventArgs`：事件参数（可选）
- **示例**：
```csharp
EventManager.Instance.TriggerEvent("OnPlayerLevelUp", new LevelUpEventArgs(newLevel));
```

## 2. 游戏系统API

### 2.1 PlayerSystem

#### Player类

##### 属性
```csharp
public int Level { get; }                    // 玩家等级
public int Experience { get; }               // 经验值
public int Gold { get; }                     // 金币
public ProfessionType CurrentProfession { get; } // 当前职业
public PlayerStats Stats { get; }           // 玩家属性
```

##### 方法
```csharp
public void AddExperience(int amount)        // 增加经验值
public void AddGold(int amount)              // 增加金币
public bool SpendGold(int amount)            // 消费金币
public void SwitchProfession(ProfessionType profession) // 切换职业
public void LevelUp()                        // 升级
```

### 2.2 NPCSystem

#### NPC类

##### 属性
```csharp
public string NPCName { get; }               // NPC名称
public NPCType Type { get; }                 // NPC类型
public bool IsInteractable { get; }          // 是否可交互
public Vector3 Position { get; }             // NPC位置
```

##### 方法
```csharp
public void StartDialogue(Player player)     // 开始对话
public void EndDialogue()                    // 结束对话
public bool CanInteract(Player player)       // 检查是否可交互
public void OnPlayerApproach(Player player)  // 玩家接近时调用
```

### 2.3 FarmingSystem

#### FarmManager类

##### 方法
```csharp
public bool PlantCrop(Vector2Int position, CropType cropType) // 种植作物
public bool HarvestCrop(Vector2Int position)                 // 收获作物
public CropInfo GetCropInfo(Vector2Int position)             // 获取作物信息
public void UpdateCrops(float deltaTime)                     // 更新作物生长
```

#### Crop类

##### 属性
```csharp
public CropType Type { get; }                // 作物类型
public GrowthStage Stage { get; }            // 生长阶段
public float GrowthProgress { get; }         // 生长进度
public bool IsReadyToHarvest { get; }        // 是否可收获
```

## 3. 数据结构API

### 3.1 GameData

#### 属性
```csharp
public PlayerData playerData;                // 玩家数据
public WorldData worldData;                  // 世界数据
public GameSettings gameSettings;            // 游戏设置
public long lastSaveTime;                    // 最后保存时间
```

#### 方法
```csharp
public void Initialize()                     // 初始化数据
public bool IsValid()                        // 验证数据有效性
```

### 3.2 PlayerData

#### 属性
```csharp
public string playerName;                    // 玩家名称
public int level;                            // 等级
public int experience;                       // 经验值
public int gold;                             // 金币
public ProfessionType currentProfession;     // 当前职业
public Vector3 position;                     // 位置
public PlayerStats stats;                    // 属性
public List<ItemData> inventory;             // 背包物品
```

### 3.3 NPCData

#### 属性
```csharp
public string npcId;                         // NPC唯一ID
public string displayName;                   // 显示名称
public NPCType type;                         // NPC类型
public Vector3 position;                     // 位置
public string[] dialogues;                   // 对话内容
public ItemData[] tradeItems;                // 交易物品
public bool isActive;                        // 是否激活
```

## 4. 枚举定义

### 4.1 GameState
```csharp
public enum GameState
{
    MainMenu,        // 主菜单
    Playing,         // 游戏中
    Paused,          // 暂停
    Loading,         // 加载中
    GameOver         // 游戏结束
}
```

### 4.2 ProfessionType
```csharp
public enum ProfessionType
{
    None,            // 无职业
    Merchant,        // 商人
    Cultivator       // 修士
}
```

### 4.3 NPCType
```csharp
public enum NPCType
{
    Innkeeper,       // 驿站老板
    Bartender,       // 酒楼老板
    Shopkeeper,      // 杂货店老板
    Blacksmith,      // 铁匠
    Official,        // 官员
    Merchant,        // 商人
    Librarian        // 藏经阁管理员
}
```

### 4.4 CropType
```csharp
public enum CropType
{
    Wheat,           // 小麦
    Rice,            // 水稻
    Corn,            // 玉米
    Potato,          // 土豆
    Cabbage          // 白菜
}
```

### 4.5 GrowthStage
```csharp
public enum GrowthStage
{
    Seed,            // 种子
    Sprout,          // 发芽
    Growing,         // 生长中
    Mature,          // 成熟
    ReadyToHarvest   // 可收获
}
```

## 5. 事件系统API

### 5.1 常用事件名称

#### 游戏状态事件
- `OnGameStateChanged`：游戏状态改变
- `OnGamePaused`：游戏暂停
- `OnGameResumed`：游戏恢复

#### 玩家事件
- `OnPlayerLevelUp`：玩家升级
- `OnPlayerDeath`：玩家死亡
- `OnProfessionChanged`：职业改变
- `OnGoldChanged`：金币变化

#### UI事件
- `OnUIShown`：UI显示
- `OnUIHidden`：UI隐藏
- `OnButtonClicked`：按钮点击

#### 系统事件
- `OnCropPlanted`：作物种植
- `OnCropHarvested`：作物收获
- `OnNPCInteraction`：NPC交互
- `OnQuestCompleted`：任务完成

### 5.2 事件参数类

#### GameStateEventArgs
```csharp
public class GameStateEventArgs
{
    public GameState PreviousState { get; }
    public GameState NewState { get; }
}
```

#### LevelUpEventArgs
```csharp
public class LevelUpEventArgs
{
    public int OldLevel { get; }
    public int NewLevel { get; }
    public int ExperienceGained { get; }
}
```

## 6. 工具类API

### 6.1 MathUtils

#### 方法
```csharp
public static float CalculateDistance(Vector3 a, Vector3 b)     // 计算距离
public static bool IsInRange(Vector3 position, Vector3 target, float range) // 检查范围
public static Vector3 GetRandomPosition(Vector3 center, float radius)       // 获取随机位置
```

### 6.2 StringUtils

#### 方法
```csharp
public static string FormatTime(float seconds)                  // 格式化时间
public static string FormatNumber(int number)                   // 格式化数字
public static bool IsValidName(string name)                     // 验证名称有效性
```

### 6.3 FileUtils

#### 方法
```csharp
public static bool SaveToFile(string path, string content)      // 保存到文件
public static string LoadFromFile(string path)                  // 从文件加载
public static bool FileExists(string path)                      // 检查文件存在
public static void CreateDirectory(string path)                 // 创建目录
```

---

**注：本API文档将随着开发进度持续更新，添加新的API和修改现有API。**
