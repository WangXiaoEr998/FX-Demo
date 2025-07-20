using UnityEngine;

namespace FanXing.Data
{
    /// <summary>
    /// 事件常量定义，统一管理所有游戏事件名称
    /// 避免硬编码字符串，提供编译时检查和智能提示
    /// 作者：黄畅修
    /// 创建时间：2025-07-13
    /// </summary>
    public static class EventConstants
    {
        #region 游戏状态事件
        /// <summary>
        /// 游戏状态相关事件
        /// </summary>
        public static class GameState
        {
            /// <summary>
            /// 游戏状态改变事件
            /// 参数类型：GameStateEventArgs
            /// </summary>
            public const string STATE_CHANGED = "Game.StateChanged";
            
            /// <summary>
            /// 游戏开始事件
            /// 参数类型：GameStartedEventArgs
            /// </summary>
            public const string STARTED = "Game.Started";
            
            /// <summary>
            /// 游戏暂停事件
            /// 参数类型：GamePausedEventArgs
            /// </summary>
            public const string PAUSED = "Game.Paused";
            
            /// <summary>
            /// 游戏恢复事件
            /// 参数类型：GameResumedEventArgs
            /// </summary>
            public const string RESUMED = "Game.Resumed";
            
            /// <summary>
            /// 游戏结束事件
            /// 参数类型：GameEndedEventArgs
            /// </summary>
            public const string ENDED = "Game.Ended";
            
            /// <summary>
            /// 游戏重启事件
            /// 参数类型：null
            /// </summary>
            public const string RESTARTED = "Game.Restarted";
        }
        #endregion

        #region 玩家事件
        /// <summary>
        /// 玩家相关事件
        /// </summary>
        public static class Player
        {
            /// <summary>
            /// 玩家创建事件
            /// 参数类型：PlayerCreatedEventArgs
            /// </summary>
            public const string CREATED = "Player.Created";
            
            /// <summary>
            /// 玩家加载事件
            /// 参数类型：PlayerLoadedEventArgs
            /// </summary>
            public const string LOADED = "Player.Loaded";
            
            /// <summary>
            /// 玩家升级事件
            /// 参数类型：PlayerLevelUpEventArgs
            /// </summary>
            public const string LEVEL_UP = "Player.LevelUp";
            
            /// <summary>
            /// 玩家职业改变事件
            /// 参数类型：PlayerProfessionChangedEventArgs
            /// </summary>
            public const string PROFESSION_CHANGED = "Player.ProfessionChanged";
            
            /// <summary>
            /// 玩家位置改变事件
            /// 参数类型：PlayerPositionChangedEventArgs
            /// </summary>
            public const string POSITION_CHANGED = "Player.PositionChanged";
            
            /// <summary>
            /// 玩家生命值改变事件
            /// 参数类型：PlayerHealthChangedEventArgs
            /// </summary>
            public const string HEALTH_CHANGED = "Player.HealthChanged";
            
            /// <summary>
            /// 玩家法力值改变事件
            /// 参数类型：PlayerManaChangedEventArgs
            /// </summary>
            public const string MANA_CHANGED = "Player.ManaChanged";
            
            /// <summary>
            /// 玩家获得经验事件
            /// 参数类型：PlayerExperienceGainedEventArgs
            /// </summary>
            public const string EXPERIENCE_GAINED = "Player.ExperienceGained";
            
            /// <summary>
            /// 玩家死亡事件
            /// 参数类型：PlayerDeathEventArgs
            /// </summary>
            public const string DEATH = "Player.Death";
            
            /// <summary>
            /// 玩家复活事件
            /// 参数类型：PlayerRespawnEventArgs
            /// </summary>
            public const string RESPAWN = "Player.Respawn";
        }
        #endregion

        #region 技能事件
        /// <summary>
        /// 技能相关事件
        /// </summary>
        public static class Skill
        {
            /// <summary>
            /// 技能学习事件
            /// 参数类型：SkillLearnedEventArgs
            /// </summary>
            public const string LEARNED = "Skill.Learned";
            
            /// <summary>
            /// 技能升级事件
            /// 参数类型：SkillUpgradedEventArgs
            /// </summary>
            public const string UPGRADED = "Skill.Upgraded";
            
            /// <summary>
            /// 技能使用事件
            /// 参数类型：SkillUsedEventArgs
            /// </summary>
            public const string USED = "Skill.Used";
            
            /// <summary>
            /// 技能冷却开始事件
            /// 参数类型：SkillCooldownEventArgs
            /// </summary>
            public const string COOLDOWN_STARTED = "Skill.CooldownStarted";
            
            /// <summary>
            /// 技能冷却结束事件
            /// 参数类型：SkillCooldownEventArgs
            /// </summary>
            public const string COOLDOWN_FINISHED = "Skill.CooldownFinished";
            
            /// <summary>
            /// 技能点获得事件
            /// 参数类型：SkillPointGainedEventArgs
            /// </summary>
            public const string POINTS_GAINED = "Skill.PointsGained";
        }
        #endregion

        #region 物品事件
        /// <summary>
        /// 物品相关事件
        /// </summary>
        public static class Item
        {
            /// <summary>
            /// 物品获得事件
            /// 参数类型：ItemAcquiredEventArgs
            /// </summary>
            public const string ACQUIRED = "Item.Acquired";
            
            /// <summary>
            /// 物品使用事件
            /// 参数类型：ItemUsedEventArgs
            /// </summary>
            public const string USED = "Item.Used";
            
            /// <summary>
            /// 物品丢弃事件
            /// 参数类型：ItemDroppedEventArgs
            /// </summary>
            public const string DROPPED = "Item.Dropped";
            
            /// <summary>
            /// 物品装备事件
            /// 参数类型：ItemEquippedEventArgs
            /// </summary>
            public const string EQUIPPED = "Item.Equipped";
            
            /// <summary>
            /// 物品卸下事件
            /// 参数类型：ItemUnequippedEventArgs
            /// </summary>
            public const string UNEQUIPPED = "Item.Unequipped";
            
            /// <summary>
            /// 物品合成事件
            /// 参数类型：ItemCraftedEventArgs
            /// </summary>
            public const string CRAFTED = "Item.Crafted";
        }
        #endregion

        #region NPC事件
        /// <summary>
        /// NPC相关事件
        /// </summary>
        public static class NPC
        {
            /// <summary>
            /// NPC对话开始事件
            /// 参数类型：NPCDialogueEventArgs
            /// </summary>
            public const string DIALOGUE_STARTED = "NPC.DialogueStarted";
            
            /// <summary>
            /// NPC对话结束事件
            /// 参数类型：NPCDialogueEventArgs
            /// </summary>
            public const string DIALOGUE_ENDED = "NPC.DialogueEnded";
            
            /// <summary>
            /// NPC交互开始事件
            /// 参数类型：NPCInteractionEventArgs
            /// </summary>
            public const string INTERACTION_STARTED = "NPC.InteractionStarted";
            
            /// <summary>
            /// NPC交互结束事件
            /// 参数类型：NPCInteractionEventArgs
            /// </summary>
            public const string INTERACTION_ENDED = "NPC.InteractionEnded";
            
            /// <summary>
            /// NPC创建事件
            /// 参数类型：NPCCreatedEventArgs
            /// </summary>
            public const string CREATED = "NPC.Created";
            
            /// <summary>
            /// NPC销毁事件
            /// 参数类型：NPCDestroyedEventArgs
            /// </summary>
            public const string DESTROYED = "NPC.Destroyed";
        }
        #endregion

        #region 任务事件
        /// <summary>
        /// 任务相关事件
        /// </summary>
        public static class Quest
        {
            /// <summary>
            /// 任务接受事件
            /// 参数类型：QuestAcceptedEventArgs
            /// </summary>
            public const string ACCEPTED = "Quest.Accepted";
            
            /// <summary>
            /// 任务完成事件
            /// 参数类型：QuestCompletedEventArgs
            /// </summary>
            public const string COMPLETED = "Quest.Completed";
            
            /// <summary>
            /// 任务失败事件
            /// 参数类型：QuestFailedEventArgs
            /// </summary>
            public const string FAILED = "Quest.Failed";
            
            /// <summary>
            /// 任务进度更新事件
            /// 参数类型：QuestProgressEventArgs
            /// </summary>
            public const string PROGRESS_UPDATED = "Quest.ProgressUpdated";
            
            /// <summary>
            /// 任务目标完成事件
            /// 参数类型：QuestObjectiveEventArgs
            /// </summary>
            public const string OBJECTIVE_COMPLETED = "Quest.ObjectiveCompleted";
            
            /// <summary>
            /// 任务放弃事件
            /// 参数类型：QuestAbandonedEventArgs
            /// </summary>
            public const string ABANDONED = "Quest.Abandoned";
        }
        #endregion

        #region 战斗事件
        /// <summary>
        /// 战斗相关事件
        /// </summary>
        public static class Combat
        {
            /// <summary>
            /// 战斗开始事件
            /// 参数类型：CombatStartedEventArgs
            /// </summary>
            public const string STARTED = "Combat.Started";
            
            /// <summary>
            /// 战斗结束事件
            /// 参数类型：CombatEndedEventArgs
            /// </summary>
            public const string ENDED = "Combat.Ended";
            
            /// <summary>
            /// 造成伤害事件
            /// 参数类型：DamageDealtEventArgs
            /// </summary>
            public const string DAMAGE_DEALT = "Combat.DamageDealt";
            
            /// <summary>
            /// 受到伤害事件
            /// 参数类型：DamageReceivedEventArgs
            /// </summary>
            public const string DAMAGE_RECEIVED = "Combat.DamageReceived";
            
            /// <summary>
            /// 敌人击败事件
            /// 参数类型：EnemyDefeatedEventArgs
            /// </summary>
            public const string ENEMY_DEFEATED = "Combat.EnemyDefeated";
            
            /// <summary>
            /// 暴击事件
            /// 参数类型：CriticalHitEventArgs
            /// </summary>
            public const string CRITICAL_HIT = "Combat.CriticalHit";
        }
        #endregion

        #region 经济事件
        /// <summary>
        /// 经济相关事件
        /// </summary>
        public static class Economy
        {
            /// <summary>
            /// 金钱获得事件
            /// 参数类型：GoldGainedEventArgs
            /// </summary>
            public const string GOLD_GAINED = "Economy.GoldGained";
            
            /// <summary>
            /// 金钱消费事件
            /// 参数类型：GoldSpentEventArgs
            /// </summary>
            public const string GOLD_SPENT = "Economy.GoldSpent";
            
            /// <summary>
            /// 物品购买事件
            /// 参数类型：ItemBoughtEventArgs
            /// </summary>
            public const string ITEM_BOUGHT = "Economy.ItemBought";
            
            /// <summary>
            /// 物品出售事件
            /// 参数类型：ItemSoldEventArgs
            /// </summary>
            public const string ITEM_SOLD = "Economy.ItemSold";
            
            /// <summary>
            /// 商店打开事件
            /// 参数类型：ShopOpenedEventArgs
            /// </summary>
            public const string SHOP_OPENED = "Economy.ShopOpened";
            
            /// <summary>
            /// 商店关闭事件
            /// 参数类型：ShopClosedEventArgs
            /// </summary>
            public const string SHOP_CLOSED = "Economy.ShopClosed";
        }
        #endregion

        #region 种田事件
        /// <summary>
        /// 种田相关事件
        /// </summary>
        public static class Farming
        {
            /// <summary>
            /// 作物种植事件
            /// 参数类型：CropPlantedEventArgs
            /// </summary>
            public const string CROP_PLANTED = "Farming.CropPlanted";
            
            /// <summary>
            /// 作物成熟事件
            /// 参数类型：CropGrownEventArgs
            /// </summary>
            public const string CROP_GROWN = "Farming.CropGrown";
            
            /// <summary>
            /// 作物收获事件
            /// 参数类型：CropHarvestedEventArgs
            /// </summary>
            public const string CROP_HARVESTED = "Farming.CropHarvested";
            
            /// <summary>
            /// 农田创建事件
            /// 参数类型：FarmPlotCreatedEventArgs
            /// </summary>
            public const string PLOT_CREATED = "Farming.PlotCreated";
            
            /// <summary>
            /// 农田销毁事件
            /// 参数类型：FarmPlotDestroyedEventArgs
            /// </summary>
            public const string PLOT_DESTROYED = "Farming.PlotDestroyed";
        }
        #endregion

        #region UI事件
        /// <summary>
        /// UI相关事件
        /// </summary>
        public static class UI
        {
            /// <summary>
            /// UI面板打开事件
            /// 参数类型：UIPanelEventArgs
            /// </summary>
            public const string PANEL_OPENED = "UI.PanelOpened";
            
            /// <summary>
            /// UI面板关闭事件
            /// 参数类型：UIPanelEventArgs
            /// </summary>
            public const string PANEL_CLOSED = "UI.PanelClosed";
            
            /// <summary>
            /// UI按钮点击事件
            /// 参数类型：UIButtonEventArgs
            /// </summary>
            public const string BUTTON_CLICKED = "UI.ButtonClicked";
            
            /// <summary>
            /// UI输入改变事件
            /// 参数类型：UIInputEventArgs
            /// </summary>
            public const string INPUT_CHANGED = "UI.InputChanged";
        }
        #endregion

        #region 数据事件
        /// <summary>
        /// 数据相关事件
        /// </summary>
        public static class Data
        {
            /// <summary>
            /// 数据保存事件
            /// 参数类型：DataSavedEventArgs
            /// </summary>
            public const string SAVED = "Data.Saved";
            
            /// <summary>
            /// 数据加载事件
            /// 参数类型：DataLoadedEventArgs
            /// </summary>
            public const string LOADED = "Data.Loaded";
            
            /// <summary>
            /// 设置改变事件
            /// 参数类型：SettingsChangedEventArgs
            /// </summary>
            public const string SETTINGS_CHANGED = "Data.SettingsChanged";
            
            /// <summary>
            /// 配置重载事件
            /// 参数类型：ConfigReloadedEventArgs
            /// </summary>
            public const string CONFIG_RELOADED = "Data.ConfigReloaded";
        }
        #endregion

        #region 系统事件
        /// <summary>
        /// 系统相关事件
        /// </summary>
        public static class System
        {
            /// <summary>
            /// 系统初始化事件
            /// 参数类型：SystemInitializedEventArgs
            /// </summary>
            public const string INITIALIZED = "System.Initialized";
            
            /// <summary>
            /// 系统启动事件
            /// 参数类型：SystemStartedEventArgs
            /// </summary>
            public const string STARTED = "System.Started";
            
            /// <summary>
            /// 系统暂停事件
            /// 参数类型：SystemPausedEventArgs
            /// </summary>
            public const string PAUSED = "System.Paused";
            
            /// <summary>
            /// 系统恢复事件
            /// 参数类型：SystemResumedEventArgs
            /// </summary>
            public const string RESUMED = "System.Resumed";
            
            /// <summary>
            /// 系统关闭事件
            /// 参数类型：SystemShutdownEventArgs
            /// </summary>
            public const string SHUTDOWN = "System.Shutdown";
        }
        #endregion

        #region 商店事件
        /// <summary>
        /// 商店相关事件常量
        /// </summary>
        public static class Shop
        {
            /// <summary>
            /// 商店创建事件
            /// 参数类型：ShopCreatedEventArgs
            /// </summary>
            public const string CREATED = "Shop.Created";

            /// <summary>
            /// 商店关闭事件
            /// 参数类型：ShopClosedEventArgs
            /// </summary>
            public const string CLOSED = "Shop.Closed";

            /// <summary>
            /// 商店物品添加事件
            /// 参数类型：ShopItemAddedEventArgs
            /// </summary>
            public const string ITEM_ADDED = "Shop.ItemAdded";

            /// <summary>
            /// 商店物品移除事件
            /// 参数类型：ShopItemRemovedEventArgs
            /// </summary>
            public const string ITEM_REMOVED = "Shop.ItemRemoved";

            /// <summary>
            /// 商店租赁到期事件
            /// 参数类型：ShopRentExpiredEventArgs
            /// </summary>
            public const string RENT_EXPIRED = "Shop.RentExpired";

            /// <summary>
            /// 商店库存更新事件
            /// 参数类型：ShopInventoryUpdatedEventArgs
            /// </summary>
            public const string INVENTORY_UPDATED = "Shop.InventoryUpdated";
        }
        #endregion
    }
}
