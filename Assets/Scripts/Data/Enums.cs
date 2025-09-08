/// <summary>
/// 游戏枚举定义，包含所有游戏中使用的枚举类型
/// 作者：黄畅修
/// 创建时间：2025-07-12
/// </summary>

namespace FanXing.Data
{
    /// <summary>
    /// 游戏状态枚举
    /// </summary>
    public enum GameState
    {
        MainMenu, // 主菜单
        Playing, // 游戏进行中
        Paused, // 暂停
        Loading, // 加载中
        GameOver // 游戏结束
    }

    /// <summary>
    /// 职业类型枚举
    /// </summary>
    public enum ProfessionType
    {
        None, // 无职业
        Merchant, // 商人
        Cultivator // 修士
    }

    /// <summary>
    /// NPC类型枚举（对应Demo草案中的7个NPC）
    /// </summary>
    public enum NPCType
    {
        Innkeeper, // 驿站老板
        Bartender, // 酒楼老板
        Shopkeeper, // 杂货店老板
        Blacksmith, // 铁匠
        Official, // 官员
        Merchant, // 商人
        Librarian // 藏经阁管理员
    }

    /// <summary>
    /// 作物类型枚举
    /// </summary>
    public enum CropType
    {
        None, // 无作物
        Wheat, // 小麦
        Rice, // 水稻
        Corn, // 玉米
        Potato, // 土豆
        Cabbage, // 白菜
        Carrot, // 胡萝卜
        Tomato, // 番茄
        Cucumber, // 黄瓜
        Apple, //苹果
        SunFlower, // 向日葵
    }

    /// <summary>
    /// 作物生长阶段枚举
    /// </summary>
    public enum GrowthStage
    {
        Seed, // 种子
        Sprout, // 发芽
        Growing, // 生长中
        Mature, // 成熟
        ReadyToHarvest // 可收获
    }

    /// <summary>
    /// 土壤状态枚举（与FarmPlotView视觉状态一一对应）
    /// </summary>
    public enum PlotState
    {
        Locked,          // 未解锁（灰色/带锁）
        Unlocked_Empty,  // 已解锁未种植（普通土壤）
        Unlocked_Planted // 已解锁已种植（翻耕土壤）
    }


    /// <summary>
    /// 玩家状态枚举
    /// </summary>
    public enum PlayerState
    {
        Idle, // 空闲
        Moving, // 移动中
        Fighting, // 战斗中
        Farming, // 种田中
        Trading, // 交易中
        Talking, // 对话中
        Dead, // 死亡
        Respawning // 重生中
    }

    /// <summary>
    /// 物品类型枚举
    /// </summary>
    public enum ItemType
    {
        Consumable, // 消耗品
        Equipment, // 装备
        Material, // 材料
        Crop, // 作物
        Seed, // 种子
        Tool, // 工具
        Weapon, // 武器
        Armor, // 护甲
        Accessory, // 饰品
        QuestItem, // 任务物品
        Valuable, // 贵重物品
        Currency // 货币
    }

    /// <summary>
    /// 物品品质枚举
    /// </summary>
    public enum ItemQuality
    {
        Common, // 普通（白色）
        Uncommon, // 不常见（绿色）
        Rare, // 稀有（蓝色）
        Epic, // 史诗（紫色）
        Legendary // 传说（橙色）
    }

    /// <summary>
    /// 技能类型枚举
    /// </summary>
    public enum SkillType
    {
        BasicAttack, // 基础攻击
        Passive, // 被动技能
        Active, // 主动技能
        Toggle, // 切换技能
        Channeled, // 引导技能
        Instant // 瞬发技能
    }

    /// <summary>
    /// 技能目标类型枚举
    /// </summary>
    public enum SkillTargetType
    {
        Self, // 自身
        Enemy, // 敌人
        Ally, // 盟友
        Ground, // 地面
        Item, // 物品
        None // 无目标
    }

    /// <summary>
    /// 任务状态枚举
    /// </summary>
    public enum QuestStatus
    {
        NotStarted, // 未开始
        Available, // 可接取
        InProgress, // 进行中
        Completed, // 已完成
        Failed, // 已失败
        Turned, // 已交付
        Expired // 已过期
    }

    /// <summary>
    /// 任务类型枚举
    /// </summary>
    public enum QuestType
    {
        Main, // 主线任务
        Side, // 支线任务
        Daily, // 日常任务
        Repeatable, // 可重复任务
        Chain, // 连锁任务
        Collection, // 收集任务
        Delivery, // 运送任务
        Kill, // 击杀任务
        Combat, // 战斗任务
        Exploration, // 探索任务
        Crafting // 制作任务
    }

    /// <summary>
    /// 任务目标类型枚举
    /// </summary>
    public enum ObjectiveType
    {
        MoveTo, // 移动到指定位置
        Collect, // 收集物品
        Kill, // 击杀敌人
        Talk, // 与NPC对话
        Deliver, // 运送物品
        Craft, // 制作物品
        Use, // 使用物品
        Explore, // 探索区域
        Survive, // 生存时间
        Protect // 保护目标
    }

    /// <summary>
    /// 奖励类型枚举
    /// </summary>
    public enum RewardType
    {
        Gold, // 金钱奖励
        Experience, // 经验奖励
        Item, // 物品奖励
        Skill, // 技能奖励
        Title, // 称号奖励
        Reputation // 声望奖励
    }

    /// <summary>
    /// 敌人类型枚举
    /// </summary>
    public enum EnemyType
    {
        WildAnimal, // 野生动物
        Monster, // 怪物
        Bandit, // 强盗
        Guard, // 守卫
        Boss, // Boss
        Elite // 精英
    }

    /// <summary>
    /// 敌人状态枚举
    /// </summary>
    public enum EnemyState
    {
        Idle, // 空闲
        Patrol, // 巡逻
        Alert, // 警戒
        Chasing, // 追击
        Fighting, // 战斗
        Returning, // 返回
        Dead // 死亡
    }

    /// <summary>
    /// 商铺状态枚举
    /// </summary>
    public enum ShopStatus
    {
        Available, // 可租赁
        Rented, // 已租赁
        Occupied, // 被占用
        Maintenance, // 维护中
        Closed // 关闭
    }

    /// <summary>
    /// 商店类型枚举
    /// </summary>
    public enum ShopType
    {
        General, // 杂货店
        Weapon, // 武器店
        Armor, // 护甲店
        Potion, // 药水店
        Food, // 食物店
        Material, // 材料店
        Tool, // 工具店
        Luxury, // 奢侈品店
        Book, // 书店
        Pet // 宠物店
    }

    /// <summary>
    /// 交易状态枚举
    /// </summary>
    public enum TradeStatus
    {
        Pending, // 等待中
        InProgress, // 进行中
        Completed, // 已完成
        Cancelled, // 已取消
        Failed // 失败
    }

    /// <summary>
    /// 天气类型枚举
    /// </summary>
    public enum WeatherType
    {
        Sunny, // 晴天
        Cloudy, // 多云
        Rainy, // 雨天
        Stormy, // 暴风雨
        Foggy, // 雾天
        Snowy // 雪天
    }

    /// <summary>
    /// 时间段枚举
    /// </summary>
    public enum TimeOfDay
    {
        Dawn, // 黎明
        Morning, // 上午
        Noon, // 中午
        Afternoon, // 下午
        Evening, // 傍晚
        Night, // 夜晚
        Midnight // 午夜
    }

    /// <summary>
    /// 音效类型枚举
    /// </summary>
    public enum SoundType
    {
        BGM, // 背景音乐
        SFX, // 音效
        Voice, // 语音
        UI, // UI音效
        Ambient // 环境音
    }

    /// <summary>
    /// UI面板类型枚举
    /// </summary>
    public enum UIPanelType
    {
        MainMenu, // 主菜单
        GameHUD, // 游戏HUD
        Inventory, // 背包界面
        Character, // 角色界面
        Skills, // 技能界面
        Quests, // 任务界面
        Shop, // 商店界面
        Dialogue, // 对话界面
        Settings, // 设置界面
        Pause, // 暂停界面
        Loading, // 加载界面
        GameOver // 游戏结束界面
    }

    /// <summary>
    /// 输入类型枚举
    /// </summary>
    public enum InputType
    {
        Keyboard, // 键盘
        Mouse, // 鼠标
        Gamepad, // 手柄
        Touch // 触摸
    }

    /// <summary>
    /// 保存类型枚举
    /// </summary>
    public enum SaveType
    {
        Auto, // 自动保存
        Manual, // 手动保存
        Checkpoint, // 检查点保存
        Quick, // 快速保存
        Exit // 退出保存
    }

    /// <summary>
    /// 日志级别枚举
    /// </summary>
    public enum LogLevel
    {
        Debug, // 调试
        Info, // 信息
        Warning, // 警告
        Error, // 错误
        Fatal // 致命错误
    }
}