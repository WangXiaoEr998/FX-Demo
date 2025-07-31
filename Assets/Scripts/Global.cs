using UnityEngine;
using System;
using FanXing.Data;

/// <summary>
/// 全局统一调用入口，提供所有系统和管理器的统一访问接口
/// 解决模块间相互依赖问题，作为唯一的系统间通信入口
/// 事件系统可支持4个参数泛型（做了容错避免反复注册事件）
/// 作者：黄畅修,容泳森
/// 创建时间：2025-07-13，修改时间：2025-07-22
/// </summary>
public static class Global
{
    #region 管理器访问接口
    /// <summary>
    /// 游戏管理器
    /// </summary>
    public static GameManager Game => GameManager.Instance;
    
    /// <summary>
    /// 事件管理器
    /// </summary>
    public static EventManager Event => EventManager.Instance;
    
    /// <summary>
    /// 数据管理器
    /// </summary>
    public static DataManager Data => DataManager.Instance;
    
    /// <summary>
    /// UI管理器
    /// </summary>
    public static UIManager UI => UIManager.Instance;
    
    /// <summary>
    /// 系统管理器
    /// </summary>
    public static SystemManager System => SystemManager.Instance;
    
    /// <summary>
    /// 职业管理器
    /// </summary>
    public static ProfessionManager Profession => ProfessionManager.Instance;
    
    /// <summary>
    /// 技能管理器
    /// </summary>
    public static SkillManager Skill => SkillManager.Instance;
    
    /// <summary>
    /// 属性管理器
    /// </summary>
    public static AttributeManager Attribute => AttributeManager.Instance;
    #endregion

    #region 系统访问接口
    /// <summary>
    /// 获取指定类型的游戏系统
    /// </summary>
    /// <typeparam name="T">系统类型</typeparam>
    /// <returns>系统实例</returns>
    public static T GetSystem<T>() where T : class, IGameSystem
    {
        return System?.GetSystem<T>();
    }
    
    /// <summary>
    /// 玩家系统
    /// </summary>
    public static PlayerSystem Player => GetSystem<PlayerSystem>();
    
    /// <summary>
    /// NPC系统
    /// </summary>
    public static NPCSystem NPC => GetSystem<NPCSystem>();
    
    /// <summary>
    /// 种田系统
    /// </summary>
    public static FarmingSystem Farming => GetSystem<FarmingSystem>();
    
    /// <summary>
    /// 战斗系统
    /// </summary>
    public static CombatSystem Combat => GetSystem<CombatSystem>();
    
    /// <summary>
    /// 经济系统
    /// </summary>
    public static EconomySystem Economy => GetSystem<EconomySystem>();
    
    /// <summary>
    /// 任务系统
    /// </summary>
    public static QuestSystem Quest => GetSystem<QuestSystem>();
    
    /// <summary>
    /// 商店系统
    /// </summary>
    public static ShopSystem Shop => GetSystem<ShopSystem>();
    #endregion

    #region 事件常量快捷访问
    /// <summary>
    /// 事件常量快捷访问，引用EventConstants
    /// </summary>
    public static class Events
    {
        /// <summary>
        /// 游戏状态事件
        /// </summary>
        public static class Game
        {
            public const string STATE_CHANGED = EventConstants.GameState.STATE_CHANGED;
            public const string STARTED = EventConstants.GameState.STARTED;
            public const string PAUSED = EventConstants.GameState.PAUSED;
            public const string RESUMED = EventConstants.GameState.RESUMED;
            public const string ENDED = EventConstants.GameState.ENDED;
            public const string RESTARTED = EventConstants.GameState.RESTARTED;
        }

        /// <summary>
        /// 玩家事件
        /// </summary>
        public static class Player
        {
            public const string CREATED = EventConstants.Player.CREATED;
            public const string LOADED = EventConstants.Player.LOADED;
            public const string LEVEL_UP = EventConstants.Player.LEVEL_UP;
            public const string PROFESSION_CHANGED = EventConstants.Player.PROFESSION_CHANGED;
            public const string POSITION_CHANGED = EventConstants.Player.POSITION_CHANGED;
            public const string HEALTH_CHANGED = EventConstants.Player.HEALTH_CHANGED;
            public const string MANA_CHANGED = EventConstants.Player.MANA_CHANGED;
            public const string EXPERIENCE_GAINED = EventConstants.Player.EXPERIENCE_GAINED;
            public const string DEATH = EventConstants.Player.DEATH;
            public const string RESPAWN = EventConstants.Player.RESPAWN;
        }

        /// <summary>
        /// 技能事件
        /// </summary>
        public static class Skill
        {
            public const string LEARNED = EventConstants.Skill.LEARNED;
            public const string UPGRADED = EventConstants.Skill.UPGRADED;
            public const string USED = EventConstants.Skill.USED;
            public const string COOLDOWN_STARTED = EventConstants.Skill.COOLDOWN_STARTED;
            public const string COOLDOWN_FINISHED = EventConstants.Skill.COOLDOWN_FINISHED;
            public const string POINTS_GAINED = EventConstants.Skill.POINTS_GAINED;
        }

        /// <summary>
        /// 物品事件
        /// </summary>
        public static class Item
        {
            public const string ACQUIRED = EventConstants.Item.ACQUIRED;
            public const string USED = EventConstants.Item.USED;
            public const string DROPPED = EventConstants.Item.DROPPED;
            public const string EQUIPPED = EventConstants.Item.EQUIPPED;
            public const string UNEQUIPPED = EventConstants.Item.UNEQUIPPED;
            public const string CRAFTED = EventConstants.Item.CRAFTED;
        }

        /// <summary>
        /// NPC事件
        /// </summary>
        public static class NPC
        {
            public const string DIALOGUE_STARTED = EventConstants.NPC.DIALOGUE_STARTED;
            public const string DIALOGUE_ENDED = EventConstants.NPC.DIALOGUE_ENDED;
            public const string INTERACTION_STARTED = EventConstants.NPC.INTERACTION_STARTED;
            public const string INTERACTION_ENDED = EventConstants.NPC.INTERACTION_ENDED;
            public const string CREATED = EventConstants.NPC.CREATED;
            public const string DESTROYED = EventConstants.NPC.DESTROYED;
            public const string INITIALIZE = EventConstants.NPC.INITIALIZE;
        }

        /// <summary>
        /// 任务事件
        /// </summary>
        public static class Quest
        {
            public const string ACCEPTED = EventConstants.Quest.ACCEPTED;
            public const string COMPLETED = EventConstants.Quest.COMPLETED;
            public const string FAILED = EventConstants.Quest.FAILED;
            public const string PROGRESS_UPDATED = EventConstants.Quest.PROGRESS_UPDATED;
            public const string OBJECTIVE_COMPLETED = EventConstants.Quest.OBJECTIVE_COMPLETED;
            public const string ABANDONED = EventConstants.Quest.ABANDONED;
        }

        /// <summary>
        /// 战斗事件
        /// </summary>
        public static class Combat
        {
            public const string STARTED = EventConstants.Combat.STARTED;
            public const string ENDED = EventConstants.Combat.ENDED;
            public const string DAMAGE_DEALT = EventConstants.Combat.DAMAGE_DEALT;
            public const string DAMAGE_RECEIVED = EventConstants.Combat.DAMAGE_RECEIVED;
            public const string ENEMY_DEFEATED = EventConstants.Combat.ENEMY_DEFEATED;
            public const string CRITICAL_HIT = EventConstants.Combat.CRITICAL_HIT;
        }

        /// <summary>
        /// 经济事件
        /// </summary>
        public static class Economy
        {
            public const string GOLD_GAINED = EventConstants.Economy.GOLD_GAINED;
            public const string GOLD_SPENT = EventConstants.Economy.GOLD_SPENT;
            public const string ITEM_BOUGHT = EventConstants.Economy.ITEM_BOUGHT;
            public const string ITEM_SOLD = EventConstants.Economy.ITEM_SOLD;
            public const string SHOP_OPENED = EventConstants.Economy.SHOP_OPENED;
            public const string SHOP_CLOSED = EventConstants.Economy.SHOP_CLOSED;
        }

        /// <summary>
        /// 种田事件
        /// </summary>
        public static class Farming
        {
            public const string CROP_PLANTED = EventConstants.Farming.CROP_PLANTED;
            public const string CROP_GROWN = EventConstants.Farming.CROP_GROWN;
            public const string CROP_HARVESTED = EventConstants.Farming.CROP_HARVESTED;
            public const string PLOT_CREATED = EventConstants.Farming.PLOT_CREATED;
            public const string PLOT_DESTROYED = EventConstants.Farming.PLOT_DESTROYED;
        }

        /// <summary>
        /// UI事件
        /// </summary>
        public static class UI
        {
            public const string PANEL_OPENED = EventConstants.UI.PANEL_OPENED;
            public const string PANEL_CLOSED = EventConstants.UI.PANEL_CLOSED;
            public const string BUTTON_CLICKED = EventConstants.UI.BUTTON_CLICKED;
            public const string INPUT_CHANGED = EventConstants.UI.INPUT_CHANGED;
        }

        /// <summary>
        /// 数据事件
        /// </summary>
        public static class Data
        {
            public const string SAVED = EventConstants.Data.SAVED;
            public const string LOADED = EventConstants.Data.LOADED;
            public const string SETTINGS_CHANGED = EventConstants.Data.SETTINGS_CHANGED;
            public const string CONFIG_RELOADED = EventConstants.Data.CONFIG_RELOADED;
        }

        /// <summary>
        /// 系统事件
        /// </summary>
        public static class System
        {
            public const string INITIALIZED = EventConstants.System.INITIALIZED;
            public const string STARTED = EventConstants.System.STARTED;
            public const string PAUSED = EventConstants.System.PAUSED;
            public const string RESUMED = EventConstants.System.RESUMED;
            public const string SHUTDOWN = EventConstants.System.SHUTDOWN;
        }

        /// <summary>
        /// 商店事件
        /// </summary>
        public static class Shop
        {
            public const string CREATED = EventConstants.Shop.CREATED;
            public const string CLOSED = EventConstants.Shop.CLOSED;
            public const string ITEM_ADDED = EventConstants.Shop.ITEM_ADDED;
            public const string ITEM_REMOVED = EventConstants.Shop.ITEM_REMOVED;
            public const string RENT_EXPIRED = EventConstants.Shop.RENT_EXPIRED;
            public const string INVENTORY_UPDATED = EventConstants.Shop.INVENTORY_UPDATED;
        }
    }
    #endregion

    #region 便捷方法
    /// <summary>
    /// 获取当前游戏状态
    /// </summary>
    public static GameState CurrentGameState => Game?.CurrentState ?? GameState.MainMenu;
    
    /// <summary>
    /// 获取当前玩家数据
    /// </summary>
    public static PlayerData CurrentPlayerData => Data?.GetPlayerData();
    
    /// <summary>
    /// 检查系统是否已初始化
    /// </summary>
    /// <typeparam name="T">系统类型</typeparam>
    /// <returns>是否已初始化</returns>
    public static bool IsSystemInitialized<T>() where T : class, IGameSystem
    {
        var system = GetSystem<T>();
        return system?.IsInitialized ?? false;
    }
    
    /// <summary>
    /// 检查系统是否正在运行
    /// </summary>
    /// <typeparam name="T">系统类型</typeparam>
    /// <returns>是否正在运行</returns>
    public static bool IsSystemRunning<T>() where T : class, IGameSystem
    {
        var system = GetSystem<T>();
        return system?.IsRunning ?? false;
    }
    
    /// <summary>
    /// 日志输出（统一日志接口）
    /// </summary>
    /// <param name="message">日志消息</param>
    /// <param name="logType">日志类型</param>
    public static void Log(string message, LogType logType = LogType.Log)
    {
        switch (logType)
        {
            case LogType.Error:
                Debug.LogError($"[Global] {message}");
                break;
            case LogType.Warning:
                Debug.LogWarning($"[Global] {message}");
                break;
            case LogType.Log:
            default:
                Debug.Log($"[Global] {message}");
                break;
        }
    }
    #endregion

    #region 初始化检查
    /// <summary>
    /// 检查全局系统是否就绪
    /// </summary>
    /// <returns>是否就绪</returns>
    public static bool IsReady()
    {
        return Game != null && 
               Event != null && 
               Data != null && 
               UI != null && 
               System != null;
    }
    
    /// <summary>
    /// 获取系统就绪状态报告
    /// </summary>
    /// <returns>就绪状态报告</returns>
    public static string GetReadinessReport()
    {
        var report = "=== Global System Readiness Report ===\n";
        report += $"GameManager: {(Game != null ? "✓" : "✗")}\n";
        report += $"EventManager: {(Event != null ? "✓" : "✗")}\n";
        report += $"DataManager: {(Data != null ? "✓" : "✗")}\n";
        report += $"UIManager: {(UI != null ? "✓" : "✗")}\n";
        report += $"SystemManager: {(System != null ? "✓" : "✗")}\n";
        report += $"ProfessionManager: {(Profession != null ? "✓" : "✗")}\n";
        report += $"SkillManager: {(Skill != null ? "✓" : "✗")}\n";
        report += $"AttributeManager: {(Attribute != null ? "✓" : "✗")}\n";
        report += $"Overall Ready: {(IsReady() ? "✓" : "✗")}";
        return report;
    }
    #endregion
}
