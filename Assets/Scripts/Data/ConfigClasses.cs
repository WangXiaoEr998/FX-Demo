using UnityEngine;
using System;
using System.Collections.Generic;
using FanXing.Data;
/// <summary>
/// 游戏配置类集合，包含所有游戏配置数据结构
/// 作者：黄畅修
/// 创建时间：2025-07-12
/// </summary>

#region 玩家配置
/// <summary>
/// 玩家配置类
/// </summary>
[Serializable]
public class PlayerConfig : ScriptableObject
{
    [Header("基础配置")]
    public int startingLevel = 1;
    public int startingGold = 100;
    public int startingHealth = 100;
    public int startingMana = 50;
    
    [Header("属性配置")]
    public int baseStrength = 10;
    public int baseAgility = 10;
    public int baseIntelligence = 10;
    public int baseVitality = 10;
    public int baseLuck = 10;
    
    [Header("经验配置")]
    public int baseExperiencePerLevel = 100;
    public float experienceMultiplier = 1.5f;
    public int maxLevel = 100;
    
    [Header("背包配置")]
    public int defaultInventorySize = 30;
    public int maxInventorySize = 100;
    
    [Header("移动配置")]
    public float baseMoveSpeed = 5.0f;
    public float runSpeedMultiplier = 1.5f;
    
    [Header("重生配置")]
    public float respawnTime = 3.0f;
    public Vector3 defaultRespawnPosition = Vector3.zero;
}

/// <summary>
/// 玩家设置类
/// </summary>
[Serializable]
public class PlayerSettings
{
    [Header("音频设置")]
    public float masterVolume = 1.0f;
    public float musicVolume = 0.8f;
    public float sfxVolume = 1.0f;
    public float voiceVolume = 1.0f;
    public bool muteAudio = false;
    
    [Header("图形设置")]
    public int resolutionWidth = 1920;
    public int resolutionHeight = 1080;
    public bool fullscreen = true;
    public int qualityLevel = 2;
    public bool vsync = true;
    public int targetFrameRate = 60;
    
    [Header("控制设置")]
    public float mouseSensitivity = 1.0f;
    public bool invertMouseY = false;
    public KeyCode[] keyBindings = new KeyCode[20];
    
    [Header("游戏设置")]
    public bool showDamageNumbers = true;
    public bool showHealthBars = true;
    public bool autoSave = true;
    public float autoSaveInterval = 300f; // 5分钟
    public string language = "Chinese";
    
    [Header("UI设置")]
    public float uiScale = 1.0f;
    public bool showTooltips = true;
    public float tooltipDelay = 0.5f;
    
    public PlayerSettings()
    {
        InitializeDefaultKeyBindings();
    }
    
    private void InitializeDefaultKeyBindings()
    {
        keyBindings = new KeyCode[20];
        keyBindings[0] = KeyCode.W;      // 前进
        keyBindings[1] = KeyCode.S;      // 后退
        keyBindings[2] = KeyCode.A;      // 左移
        keyBindings[3] = KeyCode.D;      // 右移
        keyBindings[4] = KeyCode.Space;  // 跳跃
        keyBindings[5] = KeyCode.LeftShift; // 跑步
        keyBindings[6] = KeyCode.E;      // 交互
        keyBindings[7] = KeyCode.I;      // 背包
        keyBindings[8] = KeyCode.C;      // 角色面板
        keyBindings[9] = KeyCode.K;      // 技能面板
        keyBindings[10] = KeyCode.Q;     // 任务面板
        keyBindings[11] = KeyCode.Escape; // 菜单
        keyBindings[12] = KeyCode.Tab;   // 地图
        keyBindings[13] = KeyCode.Alpha1; // 快捷栏1
        keyBindings[14] = KeyCode.Alpha2; // 快捷栏2
        keyBindings[15] = KeyCode.Alpha3; // 快捷栏3
        keyBindings[16] = KeyCode.Alpha4; // 快捷栏4
        keyBindings[17] = KeyCode.Alpha5; // 快捷栏5
        keyBindings[18] = KeyCode.F5;    // 快速保存
        keyBindings[19] = KeyCode.F9;    // 快速加载
    }
}
#endregion

#region 职业配置
/// <summary>
/// 职业配置类
/// </summary>
[Serializable]
public class ProfessionConfig : ScriptableObject
{
    [Header("基础信息")]
    public ProfessionType professionType;
    public string professionName;
    public string description;
    public Sprite icon;
    
    [Header("属性加成")]
    public int strengthBonus = 0;
    public int agilityBonus = 0;
    public int intelligenceBonus = 0;
    public int vitalityBonus = 0;
    public int luckBonus = 0;
    
    [Header("技能配置")]
    public string[] availableSkills;
    public string[] startingSkills;
    
    [Header("经验配置")]
    public int baseExperiencePerLevel = 100;
    public float experienceMultiplier = 1.0f;
    public int maxLevel = 50;
    
    [Header("特殊能力")]
    public string[] specialAbilities;
    public float[] abilityEffectiveness;
}
#endregion

#region 技能配置
/// <summary>
/// 技能信息类
/// </summary>
[Serializable]
public class SkillInfo
{
    public string skillId;
    public string skillName;
    public string description;
    public Sprite icon;
    public SkillType skillType;
    public SkillTargetType targetType;
    public int maxLevel;
    public int currentLevel;
    public float cooldown;
    public float currentCooldown;
    public int manaCost;
    public ProfessionType requiredProfession;
    public int requiredLevel;
    public string[] requiredSkills;
    public string[] effects;
    public float[] effectValues;
    public bool isLearned;
    public bool isActive;
}

/// <summary>
/// 技能配置类
/// </summary>
[Serializable]
public class SkillConfig : ScriptableObject
{
    [Header("技能列表")]
    public SkillInfo[] allSkills;
    
    [Header("技能树配置")]
    public SkillTreeNode[] skillTrees;
    
    [Header("全局设置")]
    public float globalCooldownReduction = 0f;
    public float globalManaCostReduction = 0f;
    public int maxActiveSkills = 10;
    public int maxPassiveSkills = 20;
}

/// <summary>
/// 技能树节点
/// </summary>
[Serializable]
public class SkillTreeNode
{
    public string skillId;
    public Vector2 position;
    public string[] prerequisites;
    public string[] unlocks;
    public ProfessionType profession;
    public int tier;
}
#endregion

#region NPC配置
/// <summary>
/// NPC数据类
/// </summary>
[Serializable]
public class NPCData
{
    [Header("基础信息")]
    public string npcId;
    public string npcName;
    public NPCType npcType;
    public string description;
    public Sprite portrait;
    public GameObject prefab;
    
    [Header("位置信息")]
    public Vector3 position;
    public float rotation;
    public string sceneName;
    
    [Header("对话配置")]
    public string[] dialogueIds;
    public string defaultDialogueId;
    public Dictionary<string, int> relationshipRequirements;
    
    [Header("交易配置")]
    public bool canTrade;
    public ItemData[] sellItems;
    public ItemData[] buyItems;
    public float priceMultiplier = 1.0f;
    
    [Header("任务配置")]
    public string[] availableQuests;
    public string[] completableQuests;
    
    [Header("特殊功能")]
    public string[] specialFunctions;
    public bool isEssential;
    public bool respawns;
    public float respawnTime = 86400f; // 24小时
}

/// <summary>
/// NPC配置类
/// </summary>
[Serializable]
public class NPCConfig : ScriptableObject
{
    [Header("NPC列表")]
    public NPCData[] allNPCs;
    
    [Header("全局设置")]
    public float defaultInteractionRange = 3.0f;
    public float defaultDialogueRange = 5.0f;
    public int maxSimultaneousDialogues = 1;
}
#endregion

#region 物品配置
/// <summary>
/// 物品配置类
/// </summary>
[Serializable]
public class ItemConfig : ScriptableObject
{
    [Header("物品列表")]
    public ItemData[] allItems;
    
    [Header("品质配置")]
    public Color[] qualityColors = new Color[5];
    public string[] qualityNames = new string[5];
    
    [Header("掉落配置")]
    public DropTable[] dropTables;
    
    [Header("制作配置")]
    public CraftingRecipe[] craftingRecipes;
    
    [Header("全局设置")]
    public int maxStackSize = 999;
    public float defaultWeight = 1.0f;
    public int defaultDurability = 100;
}

/// <summary>
/// 掉落表
/// </summary>
[Serializable]
public class DropTable
{
    public string tableId;
    public DropEntry[] entries;
    public int minDrops = 1;
    public int maxDrops = 3;
}

/// <summary>
/// 掉落条目
/// </summary>
[Serializable]
public class DropEntry
{
    public string itemId;
    public float dropChance;
    public int minQuantity = 1;
    public int maxQuantity = 1;
    public ItemQuality minQuality = ItemQuality.Common;
    public ItemQuality maxQuality = ItemQuality.Common;
}

/// <summary>
/// 制作配方
/// </summary>
[Serializable]
public class CraftingRecipe
{
    public string recipeId;
    public string resultItemId;
    public int resultQuantity = 1;
    public CraftingIngredient[] ingredients;
    public int requiredLevel = 1;
    public ProfessionType requiredProfession = ProfessionType.None;
    public string[] requiredTools;
    public float craftingTime = 1.0f;
    public int experienceGain = 10;
}

/// <summary>
/// 制作材料
/// </summary>
[Serializable]
public class CraftingIngredient
{
    public string itemId;
    public int quantity;
    public ItemQuality minQuality = ItemQuality.Common;
}
#endregion

#region 任务配置
/// <summary>
/// 任务数据类
/// </summary>
[Serializable]
public class QuestData
{
    [Header("基础信息")]
    public string questId;
    public string questName;
    public string description;
    public QuestType questType;
    public QuestStatus status;
    
    [Header("任务目标")]
    public QuestObjective[] objectives;
    public int currentObjectiveIndex;
    
    [Header("奖励")]
    public int experienceReward;
    public int goldReward;
    public ItemData[] itemRewards;
    
    [Header("要求")]
    public int requiredLevel = 1;
    public ProfessionType requiredProfession = ProfessionType.None;
    public string[] prerequisiteQuests;
    
    [Header("时间限制")]
    public bool hasTimeLimit;
    public float timeLimit;
    public float remainingTime;
    
    [Header("任务给予者")]
    public string questGiverId;
    public string questCompleterId;
    
    [Header("进度")]
    public DateTime acceptTime;
    public DateTime completeTime;
    public bool isRepeatable;
    public int completionCount;
}

/// <summary>
/// 任务目标
/// </summary>
[Serializable]
public class QuestObjective
{
    public string objectiveId;
    public string description;
    public QuestObjectiveType type;
    public string targetId;
    public int requiredAmount;
    public int currentAmount;
    public bool isCompleted;
    public bool isOptional;
}

/// <summary>
/// 任务目标类型
/// </summary>
public enum QuestObjectiveType
{
    Kill,           // 击杀
    Collect,        // 收集
    Deliver,        // 运送
    Talk,           // 对话
    Reach,          // 到达
    Use,            // 使用
    Craft,          // 制作
    Farm,           // 种植
    Trade,          // 交易
    Wait            // 等待
}

/// <summary>
/// 任务配置类
/// </summary>
[Serializable]
public class QuestConfig : ScriptableObject
{
    [Header("任务列表")]
    public QuestData[] allQuests;
    
    [Header("任务链配置")]
    public QuestChain[] questChains;
    
    [Header("全局设置")]
    public int maxActiveQuests = 10;
    public int maxCompletedQuests = 100;
    public bool allowQuestSharing = false;
}

/// <summary>
/// 任务链
/// </summary>
[Serializable]
public class QuestChain
{
    public string chainId;
    public string chainName;
    public string[] questIds;
    public bool isMainChain;
    public int currentQuestIndex;
}
#endregion

#region 作物配置
/// <summary>
/// 作物数据类
/// </summary>
[Serializable]
public class CropData
{
    public Vector2Int position;
    public CropType cropType;
    public GrowthStage growthStage;
    public float growthProgress;
    public float growthTime;
    public float waterLevel;
    public float fertilityLevel;
    public DateTime plantTime;
    public DateTime lastWaterTime;
    public bool isWithered;
    public bool isReady;
    public ItemQuality expectedQuality;
    public int expectedYield;
}

/// <summary>
/// 作物配置类
/// </summary>
[Serializable]
public class CropConfig : ScriptableObject
{
    [Header("作物信息")]
    public CropInfo[] allCrops;
    
    [Header("生长配置")]
    public float baseGrowthTime = 60f;
    public float waterDecayRate = 0.1f;
    public float fertilityDecayRate = 0.05f;
    
    [Header("天气影响")]
    public float rainyDayGrowthBonus = 1.2f;
    public float sunnyDayGrowthBonus = 1.1f;
    public float stormyDayGrowthPenalty = 0.8f;
}

/// <summary>
/// 作物信息
/// </summary>
[Serializable]
public class CropInfo
{
    public CropType cropType;
    public string cropName;
    public string description;
    public Sprite icon;
    public GameObject[] growthStagePrefabs;
    public float baseGrowthTime;
    public int baseYield;
    public ItemQuality baseQuality;
    public string seedItemId;
    public string harvestItemId;
    public float waterRequirement;
    public float fertilityRequirement;
    public WeatherType[] preferredWeather;
    public int sellPrice;
    public int experienceGain;
}
#endregion

#region 商铺配置
/// <summary>
/// 商铺数据类
/// </summary>
[Serializable]
public class ShopData
{
    public string shopId;
    public string shopName;
    public Vector3 position;
    public ShopStatus status;
    public string ownerId;
    public float rentPrice;
    public DateTime rentStartTime;
    public DateTime rentEndTime;
    public ItemData[] inventory;
    public float reputation;
    public int totalSales;
    public int totalProfit;
}

/// <summary>
/// 商铺配置类
/// </summary>
[Serializable]
public class ShopConfig : ScriptableObject
{
    [Header("商铺列表")]
    public ShopInfo[] availableShops;
    
    [Header("租赁配置")]
    public float baseRentPrice = 100f;
    public float rentDuration = 86400f; // 24小时
    public float reputationDecayRate = 0.01f;
    
    [Header("交易配置")]
    public float baseTaxRate = 0.1f;
    public float reputationBonusRate = 0.05f;
    public int maxInventorySize = 50;
}

/// <summary>
/// 商铺信息
/// </summary>
[Serializable]
public class ShopInfo
{
    public string shopId;
    public string shopName;
    public string description;
    public Vector3 position;
    public float baseRent;
    public int maxInventorySize;
    public string[] allowedItemTypes;
    public bool isAvailable;
}
#endregion
