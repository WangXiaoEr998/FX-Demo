using UnityEngine;
using System;
using FanXing.Data;
/// <summary>
/// 游戏事件参数类集合，包含所有游戏事件的参数定义
/// 作者：黄畅修
/// 创建时间：2025-07-12
/// </summary>

#region 游戏状态事件参数
/// <summary>
/// 游戏状态改变事件参数
/// </summary>
[Serializable]
public class GameStateEventArgs
{
    public GameState PreviousState { get; private set; }
    public GameState NewState { get; private set; }
    public float StateTime { get; private set; }

    public GameStateEventArgs(GameState previousState, GameState newState, float stateTime = 0f)
    {
        PreviousState = previousState;
        NewState = newState;
        StateTime = stateTime;
    }
}
#endregion

#region 玩家事件参数
/// <summary>
/// 玩家创建事件参数
/// </summary>
[Serializable]
public class PlayerCreatedEventArgs
{
    public PlayerData PlayerData { get; private set; }
    public DateTime CreationTime { get; private set; }

    public PlayerCreatedEventArgs(PlayerData playerData)
    {
        PlayerData = playerData;
        CreationTime = DateTime.Now;
    }
}

/// <summary>
/// 玩家加载事件参数
/// </summary>
[Serializable]
public class PlayerLoadedEventArgs
{
    public PlayerData PlayerData { get; private set; }
    public DateTime LoadTime { get; private set; }

    public PlayerLoadedEventArgs(PlayerData playerData)
    {
        PlayerData = playerData;
        LoadTime = DateTime.Now;
    }
}

/// <summary>
/// 玩家数据保存事件参数
/// </summary>
[Serializable]
public class PlayerDataSavedEventArgs
{
    public PlayerData PlayerData { get; private set; }
    public DateTime SaveTime { get; private set; }

    public PlayerDataSavedEventArgs(PlayerData playerData)
    {
        PlayerData = playerData;
        SaveTime = DateTime.Now;
    }
}

/// <summary>
/// 玩家重生事件参数
/// </summary>
[Serializable]
public class PlayerRespawnedEventArgs
{
    public PlayerData PlayerData { get; private set; }
    public Vector3 RespawnPosition { get; private set; }
    public DateTime RespawnTime { get; private set; }

    public PlayerRespawnedEventArgs(PlayerData playerData)
    {
        PlayerData = playerData;
        RespawnPosition = playerData.position;
        RespawnTime = DateTime.Now;
    }
}

/// <summary>
/// 玩家状态改变事件参数
/// </summary>
[Serializable]
public class PlayerStateChangedEventArgs
{
    public PlayerState PreviousState { get; private set; }
    public PlayerState NewState { get; private set; }
    public DateTime ChangeTime { get; private set; }

    public PlayerStateChangedEventArgs(PlayerState previousState, PlayerState newState)
    {
        PreviousState = previousState;
        NewState = newState;
        ChangeTime = DateTime.Now;
    }
}

/// <summary>
/// 玩家死亡事件参数
/// </summary>
[Serializable]
public class PlayerDeathEventArgs
{
    public PlayerData PlayerData { get; private set; }
    public Vector3 DeathPosition { get; private set; }
    public string DeathCause { get; private set; }
    public DateTime DeathTime { get; private set; }

    public PlayerDeathEventArgs(PlayerData playerData, string deathCause = "Unknown")
    {
        PlayerData = playerData;
        DeathPosition = playerData.position;
        DeathCause = deathCause;
        DeathTime = DateTime.Now;
    }
}

/// <summary>
/// 玩家升级事件参数
/// </summary>
[Serializable]
public class PlayerLevelUpEventArgs
{
    public int PreviousLevel { get; private set; }
    public int NewLevel { get; private set; }
    public int LevelGain { get; private set; }
    public DateTime LevelUpTime { get; private set; }

    public PlayerLevelUpEventArgs(int previousLevel, int newLevel)
    {
        PreviousLevel = previousLevel;
        NewLevel = newLevel;
        LevelGain = newLevel - previousLevel;
        LevelUpTime = DateTime.Now;
    }
}
#endregion

#region 属性事件参数
/// <summary>
/// 经验值改变事件参数
/// </summary>
[Serializable]
public class ExperienceChangedEventArgs
{
    public int ExperienceGain { get; private set; }
    public int TotalExperience { get; private set; }
    public string Source { get; private set; }
    public DateTime ChangeTime { get; private set; }

    public ExperienceChangedEventArgs(int experienceGain, int totalExperience, string source = "Unknown")
    {
        ExperienceGain = experienceGain;
        TotalExperience = totalExperience;
        Source = source;
        ChangeTime = DateTime.Now;
    }
}

/// <summary>
/// 金币改变事件参数
/// </summary>
[Serializable]
public class GoldChangedEventArgs
{
    public int GoldChange { get; private set; }
    public int TotalGold { get; private set; }
    public string Source { get; private set; }
    public DateTime ChangeTime { get; private set; }

    public GoldChangedEventArgs(int goldChange, int totalGold, string source = "Unknown")
    {
        GoldChange = goldChange;
        TotalGold = totalGold;
        Source = source;
        ChangeTime = DateTime.Now;
    }
}

/// <summary>
/// 生命值改变事件参数
/// </summary>
[Serializable]
public class HealthChangedEventArgs
{
    public int HealthChange { get; private set; }
    public int CurrentHealth { get; private set; }
    public int MaxHealth { get; private set; }
    public string Source { get; private set; }
    public DateTime ChangeTime { get; private set; }

    public HealthChangedEventArgs(int healthChange, int currentHealth, int maxHealth, string source = "Unknown")
    {
        HealthChange = healthChange;
        CurrentHealth = currentHealth;
        MaxHealth = maxHealth;
        Source = source;
        ChangeTime = DateTime.Now;
    }
}
#endregion

#region 职业事件参数
/// <summary>
/// 职业改变事件参数
/// </summary>
[Serializable]
public class ProfessionChangedEventArgs
{
    public ProfessionType PreviousProfession { get; private set; }
    public ProfessionType NewProfession { get; private set; }
    public DateTime ChangeTime { get; private set; }

    public ProfessionChangedEventArgs(ProfessionType previousProfession, ProfessionType newProfession)
    {
        PreviousProfession = previousProfession;
        NewProfession = newProfession;
        ChangeTime = DateTime.Now;
    }
}

/// <summary>
/// 职业升级事件参数
/// </summary>
[Serializable]
public class ProfessionLevelUpEventArgs
{
    public ProfessionType Profession { get; private set; }
    public int PreviousLevel { get; private set; }
    public int NewLevel { get; private set; }
    public DateTime LevelUpTime { get; private set; }

    public ProfessionLevelUpEventArgs(ProfessionType profession, int previousLevel, int newLevel)
    {
        Profession = profession;
        PreviousLevel = previousLevel;
        NewLevel = newLevel;
        LevelUpTime = DateTime.Now;
    }
}
#endregion

#region 技能事件参数
/// <summary>
/// 技能学习事件参数
/// </summary>
[Serializable]
public class SkillLearnedEventArgs
{
    public string SkillId { get; private set; }
    public string SkillName { get; private set; }
    public int SkillLevel { get; private set; }
    public DateTime LearnTime { get; private set; }

    public SkillLearnedEventArgs(string skillId, string skillName = "", int skillLevel = 1)
    {
        SkillId = skillId;
        SkillName = skillName;
        SkillLevel = skillLevel;
        LearnTime = DateTime.Now;
    }
}

/// <summary>
/// 技能升级事件参数
/// </summary>
[Serializable]
public class SkillUpgradedEventArgs
{
    public string SkillId { get; private set; }
    public int PreviousLevel { get; private set; }
    public int NewLevel { get; private set; }
    public DateTime UpgradeTime { get; private set; }

    public SkillUpgradedEventArgs(string skillId, int previousLevel, int newLevel)
    {
        SkillId = skillId;
        PreviousLevel = previousLevel;
        NewLevel = newLevel;
        UpgradeTime = DateTime.Now;
    }
}

/// <summary>
/// 技能使用事件参数
/// </summary>
[Serializable]
public class SkillUsedEventArgs
{
    public string SkillId { get; private set; }
    public GameObject Target { get; private set; }
    public Vector3 Position { get; private set; }
    public bool Success { get; private set; }
    public DateTime UseTime { get; private set; }

    public SkillUsedEventArgs(string skillId, GameObject target, Vector3 position, bool success)
    {
        SkillId = skillId;
        Target = target;
        Position = position;
        Success = success;
        UseTime = DateTime.Now;
    }
}
#endregion

#region 物品事件参数
/// <summary>
/// 物品获得事件参数
/// </summary>
[Serializable]
public class ItemObtainedEventArgs
{
    public ItemData Item { get; private set; }
    public int Quantity { get; private set; }
    public string Source { get; private set; }
    public DateTime ObtainTime { get; private set; }

    public ItemObtainedEventArgs(ItemData item, int quantity = 1, string source = "Unknown")
    {
        Item = item;
        Quantity = quantity;
        Source = source;
        ObtainTime = DateTime.Now;
    }
}

/// <summary>
/// 物品使用事件参数
/// </summary>
[Serializable]
public class ItemUsedEventArgs
{
    public ItemData Item { get; private set; }
    public int Quantity { get; private set; }
    public GameObject Target { get; private set; }
    public bool Success { get; private set; }
    public DateTime UseTime { get; private set; }

    public ItemUsedEventArgs(ItemData item, int quantity, GameObject target, bool success)
    {
        Item = item;
        Quantity = quantity;
        Target = target;
        Success = success;
        UseTime = DateTime.Now;
    }
}
#endregion

#region 任务事件参数
/// <summary>
/// 任务接受事件参数
/// </summary>
[Serializable]
public class QuestAcceptedEventArgs
{
    public string QuestId { get; private set; }
    public string QuestName { get; private set; }
    public QuestType QuestType { get; private set; }
    public DateTime AcceptTime { get; private set; }

    public QuestAcceptedEventArgs(string questId, string questName, QuestType questType)
    {
        QuestId = questId;
        QuestName = questName;
        QuestType = questType;
        AcceptTime = DateTime.Now;
    }
}

/// <summary>
/// 任务完成事件参数
/// </summary>
[Serializable]
public class QuestCompletedEventArgs
{
    public string QuestId { get; private set; }
    public string QuestName { get; private set; }
    public int ExperienceReward { get; private set; }
    public int GoldReward { get; private set; }
    public DateTime CompletionTime { get; private set; }

    public QuestCompletedEventArgs(string questId, string questName, int experienceReward, int goldReward)
    {
        QuestId = questId;
        QuestName = questName;
        ExperienceReward = experienceReward;
        GoldReward = goldReward;
        CompletionTime = DateTime.Now;
    }
}
#endregion

#region NPC事件参数
/// <summary>
/// NPC对话事件参数
/// </summary>
[Serializable]
public class NPCDialogueEventArgs
{
    public string NPCId { get; private set; }
    public string NPCName { get; private set; }
    public NPCType NPCType { get; private set; }
    public string DialogueId { get; private set; }
    public DateTime DialogueTime { get; private set; }

    public NPCDialogueEventArgs(string npcId, string npcName, NPCType npcType, string dialogueId)
    {
        NPCId = npcId;
        NPCName = npcName;
        NPCType = npcType;
        DialogueId = dialogueId;
        DialogueTime = DateTime.Now;
    }
}

/// <summary>
/// NPC交易事件参数
/// </summary>
[Serializable]
public class NPCTradeEventArgs
{
    public string NPCId { get; private set; }
    public ItemData Item { get; private set; }
    public int Quantity { get; private set; }
    public int Price { get; private set; }
    public bool IsBuying { get; private set; }
    public DateTime TradeTime { get; private set; }

    public NPCTradeEventArgs(string npcId, ItemData item, int quantity, int price, bool isBuying)
    {
        NPCId = npcId;
        Item = item;
        Quantity = quantity;
        Price = price;
        IsBuying = isBuying;
        TradeTime = DateTime.Now;
    }
}
#endregion

#region 商店事件参数
/// <summary>
/// 商店创建事件参数
/// </summary>
[Serializable]
public class ShopCreatedEventArgs
{
    public int ShopId { get; private set; }
    public string ShopName { get; private set; }
    public string OwnerName { get; private set; }
    public DateTime CreationTime { get; private set; }

    public ShopCreatedEventArgs(int shopId, string shopName, string ownerName)
    {
        ShopId = shopId;
        ShopName = shopName;
        OwnerName = ownerName;
        CreationTime = DateTime.Now;
    }
}

/// <summary>
/// 商店关闭事件参数
/// </summary>
[Serializable]
public class ShopClosedEventArgs
{
    public int ShopId { get; private set; }
    public string ShopName { get; private set; }
    public DateTime ClosureTime { get; private set; }

    public ShopClosedEventArgs(int shopId, string shopName)
    {
        ShopId = shopId;
        ShopName = shopName;
        ClosureTime = DateTime.Now;
    }
}

/// <summary>
/// 商店物品添加事件参数
/// </summary>
[Serializable]
public class ShopItemAddedEventArgs
{
    public int ShopId { get; private set; }
    public int ItemId { get; private set; }
    public int Quantity { get; private set; }
    public DateTime AddTime { get; private set; }

    public ShopItemAddedEventArgs(int shopId, int itemId, int quantity)
    {
        ShopId = shopId;
        ItemId = itemId;
        Quantity = quantity;
        AddTime = DateTime.Now;
    }
}

/// <summary>
/// 商店物品移除事件参数
/// </summary>
[Serializable]
public class ShopItemRemovedEventArgs
{
    public int ShopId { get; private set; }
    public int ItemId { get; private set; }
    public int Quantity { get; private set; }
    public DateTime RemoveTime { get; private set; }

    public ShopItemRemovedEventArgs(int shopId, int itemId, int quantity)
    {
        ShopId = shopId;
        ItemId = itemId;
        Quantity = quantity;
        RemoveTime = DateTime.Now;
    }
}

/// <summary>
/// 商店租赁到期事件参数
/// </summary>
[Serializable]
public class ShopRentExpiredEventArgs
{
    public int ShopId { get; private set; }
    public string ShopName { get; private set; }
    public DateTime ExpirationTime { get; private set; }

    public ShopRentExpiredEventArgs(int shopId, string shopName)
    {
        ShopId = shopId;
        ShopName = shopName;
        ExpirationTime = DateTime.Now;
    }
}

/// <summary>
/// 商店库存更新事件参数
/// </summary>
[Serializable]
public class ShopInventoryUpdatedEventArgs
{
    public int ShopId { get; private set; }
    public int TotalItems { get; private set; }
    public DateTime UpdateTime { get; private set; }

    public ShopInventoryUpdatedEventArgs(int shopId, int totalItems)
    {
        ShopId = shopId;
        TotalItems = totalItems;
        UpdateTime = DateTime.Now;
    }
}
#endregion

#region 扩展事件参数
/// <summary>
/// 玩家生命值改变事件参数
/// </summary>
[Serializable]
public class PlayerHealthChangedEventArgs
{
    public int OldHealth { get; private set; }
    public int NewHealth { get; private set; }
    public int HealthChange { get; private set; }
    public string Source { get; private set; }
    public DateTime ChangeTime { get; private set; }

    public PlayerHealthChangedEventArgs(int oldHealth, int newHealth, string source = "Unknown")
    {
        OldHealth = oldHealth;
        NewHealth = newHealth;
        HealthChange = newHealth - oldHealth;
        Source = source;
        ChangeTime = DateTime.Now;
    }
}

/// <summary>
/// 物品获取事件参数（别名）
/// </summary>
[Serializable]
public class ItemAcquiredEventArgs
{
    public ItemData ItemData { get; private set; }
    public int Quantity { get; private set; }
    public string Source { get; private set; }
    public DateTime AcquiredTime { get; private set; }

    public ItemAcquiredEventArgs(ItemData itemData, int quantity = 1, string source = "Unknown")
    {
        ItemData = itemData;
        Quantity = quantity;
        Source = source;
        AcquiredTime = DateTime.Now;
    }
}

/// <summary>
/// 伤害造成事件参数
/// </summary>
[Serializable]
public class DamageDealtEventArgs
{
    public int Damage { get; private set; }
    public GameObject Attacker { get; private set; }
    public GameObject Target { get; private set; }
    public string DamageType { get; private set; }
    public DateTime DamageTime { get; private set; }

    public DamageDealtEventArgs(int damage, GameObject attacker, GameObject target, string damageType = "Physical")
    {
        Damage = damage;
        Attacker = attacker;
        Target = target;
        DamageType = damageType;
        DamageTime = DateTime.Now;
    }
}

/// <summary>
/// 金币获得事件参数
/// </summary>
[Serializable]
public class GoldGainedEventArgs
{
    public int Amount { get; private set; }
    public string Source { get; private set; }
    public DateTime GainTime { get; private set; }

    public GoldGainedEventArgs(int amount, string source = "Unknown")
    {
        Amount = amount;
        Source = source;
        GainTime = DateTime.Now;
    }
}

/// <summary>
/// 物品购买事件参数
/// </summary>
[Serializable]
public class ItemBoughtEventArgs
{
    public string ItemName { get; private set; }
    public int Quantity { get; private set; }
    public int Price { get; private set; }
    public string ShopName { get; private set; }
    public DateTime PurchaseTime { get; private set; }

    public ItemBoughtEventArgs(string itemName, int quantity, int price, string shopName = "Unknown")
    {
        ItemName = itemName;
        Quantity = quantity;
        Price = price;
        ShopName = shopName;
        PurchaseTime = DateTime.Now;
    }
}

/// <summary>
/// 作物种植事件参数
/// </summary>
[Serializable]
public class CropPlantedEventArgs
{
    public CropType CropType { get; private set; }
    public Vector3Int Position { get; private set; }
    public string PlayerName { get; private set; }
    public DateTime PlantTime { get; private set; }

    public CropPlantedEventArgs(CropType cropType, Vector3Int position, string playerName)
    {
        CropType = cropType;
        Position = position;
        PlayerName = playerName;
        PlantTime = DateTime.Now;
    }
}

/// <summary>
/// 作物收获事件参数
/// </summary>
[Serializable]
public class CropHarvestedEventArgs
{
    public CropType CropType { get; private set; }
    public int Quantity { get; private set; }
    public Vector3Int Position { get; private set; }
    public string PlayerName { get; private set; }
    public DateTime HarvestTime { get; private set; }

    public CropHarvestedEventArgs(CropType cropType, int quantity, Vector3Int position, string playerName)
    {
        CropType = cropType;
        Quantity = quantity;
        Position = position;
        PlayerName = playerName;
        HarvestTime = DateTime.Now;
    }
}
/// <summary>
/// 作物成熟事件参数
/// 当作物从种植状态达到成熟状态时触发
/// </summary>
[Serializable]
public class CropGrownEventArgs
{
    /// <summary>
    /// 成熟的作物类型
    /// </summary>
    public CropType CropType { get; private set; }
    
    /// <summary>
    /// 作物所在的农田位置
    /// </summary>
    public Vector3Int PlotPosition { get; private set; }
    
    /// <summary>
    /// 作物从种植到成熟的生长时间（秒）
    /// </summary>
    public float GrowthDuration { get; private set; }
    
    /// <summary>
    /// 作物成熟时的产量（预估值）
    /// </summary>
    public int EstimatedYield { get; private set; }
    
    /// <summary>
    /// 事件触发的时间戳
    /// </summary>
    public DateTime GrowTime { get; private set; }

    /// <summary>
    /// 构造函数：初始化作物成熟事件的所有必要数据
    /// </summary>
    /// <param name="cropType">作物类型</param>
    /// <param name="plotPosition">农田位置</param>
    /// <param name="growthDuration">生长时长（秒）</param>
    /// <param name="estimatedYield">预估产量</param>
    public CropGrownEventArgs(CropType cropType, Vector3Int plotPosition, float growthDuration, int estimatedYield)
    {
        CropType = cropType;
        PlotPosition = plotPosition;
        GrowthDuration = growthDuration;
        EstimatedYield = estimatedYield;
        GrowTime = DateTime.Now; // 自动记录成熟时间
    }
}
#endregion

#region 农田创建事件参数
/// <summary>
/// 农田创建事件参数
/// 当新的可种植农田地块被解锁或创建时触发
/// </summary>
[Serializable]
public class FarmPlotCreatedEventArgs
{
    /// <summary>
    /// 新创建的农田唯一ID
    /// </summary>
    public string PlotId { get; private set; }
    
    /// <summary>
    /// 农田在世界中的网格位置
    /// </summary>
    public Vector3Int PlotPosition { get; private set; }
    
    /// <summary>
    /// 农田的初始状态（如：未开垦/已开垦/湿润）
    /// </summary>
    public PlotState InitialState { get; private set; }
    
    /// <summary>
    /// 创建农田的玩家名称
    /// </summary>
    public string CreatorName { get; private set; }
    
    /// <summary>
    /// 事件触发的时间戳
    /// </summary>
    public DateTime CreationTime { get; private set; }

    /// <summary>
    /// 构造函数：初始化农田创建事件的所有必要数据
    /// </summary>
    /// <param name="plotId">农田ID</param>
    /// <param name="plotPosition">农田位置</param>
    /// <param name="initialState">初始状态</param>
    /// <param name="creatorName">创建者名称</param>
    public FarmPlotCreatedEventArgs(string plotId, Vector3Int plotPosition, PlotState initialState, string creatorName)
    {
        PlotId = plotId;
        PlotPosition = plotPosition;
        InitialState = initialState;
        CreatorName = creatorName;
        CreationTime = DateTime.Now; // 自动记录创建时间
    }
}
#endregion

#region 农田销毁事件参数
/// <summary>
/// 农田销毁事件参数
/// 当农田地块被移除或摧毁时触发（如：被破坏/回收）
/// </summary>
[Serializable]
public class FarmPlotDestroyedEventArgs
{
    /// <summary>
    /// 被销毁的农田唯一ID
    /// </summary>
    public string PlotId { get; private set; }
    
    /// <summary>
    /// 农田在世界中的网格位置
    /// </summary>
    public Vector3Int PlotPosition { get; private set; }
    
    /// <summary>
    /// 农田销毁的原因（如：自然损坏/玩家拆除/战斗破坏）
    /// </summary>
    public string DestroyReason { get; private set; }
    
    /// <summary>
    /// 农田销毁时是否有未收获的作物
    /// </summary>
    public bool HasUnharvestedCrop { get; private set; }
    
    /// <summary>
    /// 事件触发的时间戳
    /// </summary>
    public DateTime DestroyTime { get; private set; }

    /// <summary>
    /// 构造函数：初始化农田销毁事件的所有必要数据
    /// </summary>
    /// <param name="plotId">农田ID</param>
    /// <param name="plotPosition">农田位置</param>
    /// <param name="destroyReason">销毁原因</param>
    /// <param name="hasUn harvestedCrop">是否有未收获作物</param>
    public FarmPlotDestroyedEventArgs(string plotId, Vector3Int plotPosition, string destroyReason, bool hasUnharvestedCrop)
    {
        PlotId = plotId;
        PlotPosition = plotPosition;
        DestroyReason = destroyReason;
        HasUnharvestedCrop = hasUnharvestedCrop;
        DestroyTime = DateTime.Now; // 自动记录销毁时间
    }
}
#endregion
