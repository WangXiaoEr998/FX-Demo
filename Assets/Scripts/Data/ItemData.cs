using UnityEngine;
using System;
using FanXing.Data;
/// <summary>
/// 物品数据类，包含物品的所有属性和信息
/// 作者：黄畅修
/// 创建时间：2025-07-12
/// </summary>
[Serializable]
public class ItemData
{
    #region 基础信息
    [Header("基础信息")]
    public string itemId = "";              // 物品唯一ID
    public string itemName = "";            // 物品名称
    public string description = "";         // 物品描述
    public ItemType itemType = ItemType.Consumable;  // 物品类型
    public ItemQuality quality = ItemQuality.Common; // 物品品质
    public Sprite icon;                     // 物品图标
    public GameObject prefab;               // 物品预制体
    #endregion

    #region 数值属性
    [Header("数值属性")]
    public int stackSize = 1;               // 堆叠数量
    public int currentStack = 1;            // 当前堆叠
    public int buyPrice = 0;                // 购买价格
    public int sellPrice = 0;               // 出售价格
    public int durability = 100;            // 耐久度
    public int maxDurability = 100;         // 最大耐久度
    public float weight = 1.0f;             // 重量
    #endregion

    #region 装备属性
    [Header("装备属性")]
    public int strengthBonus = 0;           // 力量加成
    public int agilityBonus = 0;            // 敏捷加成
    public int intelligenceBonus = 0;       // 智力加成
    public int vitalityBonus = 0;           // 体质加成
    public int luckBonus = 0;               // 幸运加成
    public int attackPower = 0;             // 攻击力
    public int defensePower = 0;            // 防御力
    public int magicPower = 0;              // 魔法力
    public int magicDefense = 0;            // 魔法防御
    #endregion

    #region 消耗品属性
    [Header("消耗品属性")]
    public int healthRestore = 0;           // 恢复生命值
    public int manaRestore = 0;             // 恢复法力值
    public float effectDuration = 0f;       // 效果持续时间
    public string[] buffEffects;            // Buff效果列表
    public string[] debuffEffects;          // Debuff效果列表
    #endregion

    #region 工具属性
    [Header("工具属性")]
    public int toolLevel = 1;               // 工具等级
    public float efficiency = 1.0f;         // 工具效率
    public string[] applicableTargets;      // 适用目标
    public int usageCount = 0;              // 使用次数
    public int maxUsageCount = -1;          // 最大使用次数（-1表示无限）
    #endregion

    #region 种子/作物属性
    [Header("种子/作物属性")]
    public CropType cropType = CropType.Wheat;  // 作物类型
    public int growthTime = 60;             // 生长时间（秒）
    public int harvestYield = 1;            // 收获数量
    public float qualityMultiplier = 1.0f;  // 品质倍率
    public string[] requiredConditions;     // 种植条件
    #endregion

    #region 任务物品属性
    [Header("任务物品属性")]
    public string relatedQuestId = "";      // 相关任务ID
    public bool isQuestItem = false;        // 是否为任务物品
    public bool canDrop = true;             // 是否可丢弃
    public bool canTrade = true;            // 是否可交易
    public bool canSell = true;             // 是否可出售
    #endregion

    #region 特殊属性
    [Header("特殊属性")]
    public string[] specialEffects;         // 特殊效果
    public string[] requirements;           // 使用要求
    public int requiredLevel = 1;           // 需求等级
    public ProfessionType requiredProfession = ProfessionType.None; // 需求职业
    public bool isUnique = false;           // 是否唯一
    public bool isBound = false;            // 是否绑定
    #endregion

    #region 计算属性
    /// <summary>
    /// 是否已损坏
    /// </summary>
    public bool IsBroken => durability <= 0;

    /// <summary>
    /// 耐久度百分比
    /// </summary>
    public float DurabilityPercentage => maxDurability > 0 ? (float)durability / maxDurability : 1f;

    /// <summary>
    /// 是否可堆叠
    /// </summary>
    public bool IsStackable => stackSize > 1;

    /// <summary>
    /// 是否已满堆叠
    /// </summary>
    public bool IsFullStack => currentStack >= stackSize;

    /// <summary>
    /// 剩余堆叠空间
    /// </summary>
    public int RemainingStackSpace => stackSize - currentStack;

    /// <summary>
    /// 总价值（考虑堆叠）
    /// </summary>
    public int TotalValue => sellPrice * currentStack;

    /// <summary>
    /// 总重量（考虑堆叠）
    /// </summary>
    public float TotalWeight => weight * currentStack;

    /// <summary>
    /// 是否为装备
    /// </summary>
    public bool IsEquipment => itemType == ItemType.Equipment || itemType == ItemType.Weapon || 
                              itemType == ItemType.Armor || itemType == ItemType.Accessory;

    /// <summary>
    /// 是否为消耗品
    /// </summary>
    public bool IsConsumable => itemType == ItemType.Consumable;

    /// <summary>
    /// 是否为工具
    /// </summary>
    public bool IsTool => itemType == ItemType.Tool;

    /// <summary>
    /// 是否为种子
    /// </summary>
    public bool IsSeed => itemType == ItemType.Seed;

    /// <summary>
    /// 是否为作物
    /// </summary>
    public bool IsCrop => itemType == ItemType.Crop;
    #endregion

    #region 构造函数
    /// <summary>
    /// 默认构造函数
    /// </summary>
    public ItemData()
    {
        InitializeArrays();
    }

    /// <summary>
    /// 带参数构造函数
    /// </summary>
    /// <param name="id">物品ID</param>
    /// <param name="name">物品名称</param>
    /// <param name="type">物品类型</param>
    public ItemData(string id, string name, ItemType type)
    {
        itemId = id;
        itemName = name;
        itemType = type;
        InitializeArrays();
        SetDefaultValues();
    }

    /// <summary>
    /// 复制构造函数
    /// </summary>
    /// <param name="other">要复制的物品数据</param>
    public ItemData(ItemData other)
    {
        if (other != null)
        {
            CopyFrom(other);
        }
        else
        {
            InitializeArrays();
        }
    }
    #endregion

    #region 初始化方法
    /// <summary>
    /// 初始化数组
    /// </summary>
    private void InitializeArrays()
    {
        if (buffEffects == null)
            buffEffects = new string[0];
        
        if (debuffEffects == null)
            debuffEffects = new string[0];
        
        if (applicableTargets == null)
            applicableTargets = new string[0];
        
        if (requiredConditions == null)
            requiredConditions = new string[0];
        
        if (specialEffects == null)
            specialEffects = new string[0];
        
        if (requirements == null)
            requirements = new string[0];
    }

    /// <summary>
    /// 设置默认值
    /// </summary>
    private void SetDefaultValues()
    {
        switch (itemType)
        {
            case ItemType.Consumable:
                stackSize = 10;
                canDrop = true;
                canTrade = true;
                canSell = true;
                break;
                
            case ItemType.Equipment:
            case ItemType.Weapon:
            case ItemType.Armor:
            case ItemType.Accessory:
                stackSize = 1;
                durability = 100;
                maxDurability = 100;
                break;
                
            case ItemType.Material:
                stackSize = 50;
                break;
                
            case ItemType.Seed:
                stackSize = 20;
                growthTime = 60;
                harvestYield = 1;
                break;
                
            case ItemType.Crop:
                stackSize = 30;
                break;
                
            case ItemType.Tool:
                stackSize = 1;
                durability = 100;
                maxDurability = 100;
                efficiency = 1.0f;
                break;
                
            case ItemType.QuestItem:
                stackSize = 1;
                isQuestItem = true;
                canDrop = false;
                canTrade = false;
                canSell = false;
                break;
        }
    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 从另一个物品数据复制
    /// </summary>
    /// <param name="other">源物品数据</param>
    public void CopyFrom(ItemData other)
    {
        if (other == null) return;

        // 复制基础信息
        itemId = other.itemId;
        itemName = other.itemName;
        description = other.description;
        itemType = other.itemType;
        quality = other.quality;
        icon = other.icon;
        prefab = other.prefab;

        // 复制数值属性
        stackSize = other.stackSize;
        currentStack = other.currentStack;
        buyPrice = other.buyPrice;
        sellPrice = other.sellPrice;
        durability = other.durability;
        maxDurability = other.maxDurability;
        weight = other.weight;

        // 复制装备属性
        strengthBonus = other.strengthBonus;
        agilityBonus = other.agilityBonus;
        intelligenceBonus = other.intelligenceBonus;
        vitalityBonus = other.vitalityBonus;
        luckBonus = other.luckBonus;
        attackPower = other.attackPower;
        defensePower = other.defensePower;
        magicPower = other.magicPower;
        magicDefense = other.magicDefense;

        // 复制其他属性
        healthRestore = other.healthRestore;
        manaRestore = other.manaRestore;
        effectDuration = other.effectDuration;
        toolLevel = other.toolLevel;
        efficiency = other.efficiency;
        cropType = other.cropType;
        growthTime = other.growthTime;
        harvestYield = other.harvestYield;
        qualityMultiplier = other.qualityMultiplier;
        relatedQuestId = other.relatedQuestId;
        isQuestItem = other.isQuestItem;
        canDrop = other.canDrop;
        canTrade = other.canTrade;
        canSell = other.canSell;
        requiredLevel = other.requiredLevel;
        requiredProfession = other.requiredProfession;
        isUnique = other.isUnique;
        isBound = other.isBound;

        // 复制数组
        buffEffects = other.buffEffects != null ? (string[])other.buffEffects.Clone() : new string[0];
        debuffEffects = other.debuffEffects != null ? (string[])other.debuffEffects.Clone() : new string[0];
        applicableTargets = other.applicableTargets != null ? (string[])other.applicableTargets.Clone() : new string[0];
        requiredConditions = other.requiredConditions != null ? (string[])other.requiredConditions.Clone() : new string[0];
        specialEffects = other.specialEffects != null ? (string[])other.specialEffects.Clone() : new string[0];
        requirements = other.requirements != null ? (string[])other.requirements.Clone() : new string[0];
    }

    /// <summary>
    /// 克隆物品数据
    /// </summary>
    /// <returns>克隆的物品数据</returns>
    public ItemData Clone()
    {
        return new ItemData(this);
    }

    /// <summary>
    /// 增加堆叠数量
    /// </summary>
    /// <param name="amount">增加的数量</param>
    /// <returns>实际增加的数量</returns>
    public int AddStack(int amount)
    {
        if (!IsStackable || amount <= 0)
            return 0;

        int canAdd = Mathf.Min(amount, RemainingStackSpace);
        currentStack += canAdd;
        return canAdd;
    }

    /// <summary>
    /// 减少堆叠数量
    /// </summary>
    /// <param name="amount">减少的数量</param>
    /// <returns>实际减少的数量</returns>
    public int RemoveStack(int amount)
    {
        if (amount <= 0)
            return 0;

        int canRemove = Mathf.Min(amount, currentStack);
        currentStack -= canRemove;
        return canRemove;
    }

    /// <summary>
    /// 修复耐久度
    /// </summary>
    /// <param name="amount">修复数量</param>
    /// <returns>实际修复的数量</returns>
    public int RepairDurability(int amount)
    {
        if (amount <= 0 || maxDurability <= 0)
            return 0;

        int canRepair = Mathf.Min(amount, maxDurability - durability);
        durability += canRepair;
        return canRepair;
    }

    /// <summary>
    /// 减少耐久度
    /// </summary>
    /// <param name="amount">减少数量</param>
    /// <returns>是否已损坏</returns>
    public bool ReduceDurability(int amount)
    {
        if (amount <= 0 || maxDurability <= 0)
            return false;

        durability = Mathf.Max(0, durability - amount);
        return IsBroken;
    }

    /// <summary>
    /// 检查是否满足使用要求
    /// </summary>
    /// <param name="playerData">玩家数据</param>
    /// <returns>是否满足要求</returns>
    public bool CanUse(PlayerData playerData)
    {
        if (playerData == null)
            return false;

        // 检查等级要求
        if (playerData.level < requiredLevel)
            return false;

        // 检查职业要求
        if (requiredProfession != ProfessionType.None && 
            playerData.currentProfession != requiredProfession)
            return false;

        // 检查耐久度
        if (IsBroken)
            return false;

        return true;
    }

    /// <summary>
    /// 验证数据有效性
    /// </summary>
    /// <returns>数据是否有效</returns>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(itemId))
            return false;

        if (string.IsNullOrEmpty(itemName))
            return false;

        if (stackSize <= 0)
            return false;

        if (currentStack < 0 || currentStack > stackSize)
            return false;

        if (durability < 0 || durability > maxDurability)
            return false;

        return true;
    }

    /// <summary>
    /// 修复无效数据
    /// </summary>
    public void FixInvalidData()
    {
        if (string.IsNullOrEmpty(itemId))
            itemId = "unknown_item";

        if (string.IsNullOrEmpty(itemName))
            itemName = "Unknown Item";

        stackSize = Mathf.Max(1, stackSize);
        currentStack = Mathf.Clamp(currentStack, 1, stackSize);
        durability = Mathf.Clamp(durability, 0, maxDurability);
        maxDurability = Mathf.Max(1, maxDurability);

        InitializeArrays();
    }

    /// <summary>
    /// 转换为字符串
    /// </summary>
    /// <returns>物品信息字符串</returns>
    public override string ToString()
    {
        string stackInfo = IsStackable ? $" x{currentStack}" : "";
        string durabilityInfo = IsEquipment || IsTool ? $" ({durability}/{maxDurability})" : "";
        return $"{itemName}{stackInfo}{durabilityInfo}";
    }
    #endregion
}
