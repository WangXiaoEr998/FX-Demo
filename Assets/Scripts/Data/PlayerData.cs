using UnityEngine;
using System;
using System.Collections.Generic;
using FanXing.Data;
/// <summary>
/// 玩家数据类，包含玩家的所有持久化数据
/// 作者：黄畅修
/// 创建时间：2025-07-12
/// </summary>
[Serializable]
public class PlayerData
{
    #region 基础信息
    [Header("基础信息")]
    public string playerName = "";
    public int level = 1;
    public int experience = 0;
    public int gold = 100;
    public Vector3 position = Vector3.zero;
    public float rotation = 0f;
    #endregion

    #region 属性数据
    [Header("属性数据")]
    public int currentHealth = 100;
    public int maxHealth = 100;
    public int currentMana = 50;
    public int maxMana = 50;
    public int strength = 10;
    public int agility = 10;
    public int intelligence = 10;
    public int vitality = 10;
    public int luck = 10;
    #endregion

    #region 职业数据
    [Header("职业数据")]
    public ProfessionType currentProfession = ProfessionType.Merchant;
    public Dictionary<ProfessionType, int> professionLevels = new Dictionary<ProfessionType, int>();
    public Dictionary<ProfessionType, int> professionExperience = new Dictionary<ProfessionType, int>();
    #endregion

    #region 技能数据
    [Header("技能数据")]
    public List<string> learnedSkills = new List<string>();
    public Dictionary<string, int> skillLevels = new Dictionary<string, int>();
    public Dictionary<string, float> skillCooldowns = new Dictionary<string, float>();
    #endregion

    #region 背包数据
    [Header("背包数据")]
    public List<ItemData> inventory = new List<ItemData>();
    public List<ItemData> equipment = new List<ItemData>();
    public int inventorySize = 30;
    #endregion

    #region 任务数据
    [Header("任务数据")]
    public List<QuestData> activeQuests = new List<QuestData>();
    public List<string> completedQuests = new List<string>();
    public List<string> failedQuests = new List<string>();
    #endregion

    #region 种田数据
    [Header("种田数据")]
    public Dictionary<Vector2Int, CropData> farmPlots = new Dictionary<Vector2Int, CropData>();
    public List<string> ownedSeeds = new List<string>();
    public int farmingLevel = 1;
    public int farmingExperience = 0;
    #endregion

    #region 商铺数据
    [Header("商铺数据")]
    public List<ShopData> rentedShops = new List<ShopData>();
    public Dictionary<string, int> shopReputation = new Dictionary<string, int>();
    public int tradingLevel = 1;
    public int tradingExperience = 0;
    #endregion

    #region 关系数据
    [Header("关系数据")]
    public Dictionary<string, int> npcRelationships = new Dictionary<string, int>();
    public Dictionary<string, bool> npcMeetFlags = new Dictionary<string, bool>();
    #endregion

    #region 统计数据
    [Header("统计数据")]
    public float totalPlayTime = 0f;
    public int totalDeaths = 0;
    public int totalKills = 0;
    public int totalGoldEarned = 0;
    public int totalGoldSpent = 0;
    public int totalQuestsCompleted = 0;
    public int totalCropsHarvested = 0;
    public int totalItemsCrafted = 0;
    #endregion

    #region 设置数据
    [Header("设置数据")]
    public PlayerSettings settings = new PlayerSettings();
    #endregion

    #region 时间数据
    [Header("时间数据")]
    public long creationTime = 0;
    public long lastSaveTime = 0;
    public long lastLoginTime = 0;
    #endregion

    #region 自定义属性
    /// <summary>
    /// 玩家总属性（包含装备加成）
    /// </summary>
    public PlayerAttributes attributes = new PlayerAttributes();
    
    /// <summary>
    /// 玩家是否存活
    /// </summary>
    public bool IsAlive => currentHealth > 0;
    
    /// <summary>
    /// 升级所需经验
    /// </summary>
    public int ExperienceToNextLevel => CalculateExperienceToNextLevel();
    
    /// <summary>
    /// 当前经验百分比
    /// </summary>
    public float ExperiencePercentage => CalculateExperiencePercentage();
    
    /// <summary>
    /// 背包剩余空间
    /// </summary>
    public int InventorySpace => inventorySize - inventory.Count;
    
    /// <summary>
    /// 是否背包已满
    /// </summary>
    public bool IsInventoryFull => inventory.Count >= inventorySize;
    #endregion

    #region 构造函数
    /// <summary>
    /// 默认构造函数
    /// </summary>
    public PlayerData()
    {
        // 初始化集合
        InitializeCollections();
    }

    /// <summary>
    /// 带参数构造函数
    /// </summary>
    /// <param name="name">玩家名称</param>
    /// <param name="profession">初始职业</param>
    public PlayerData(string name, ProfessionType profession)
    {
        playerName = name;
        currentProfession = profession;
        InitializeCollections();
        InitializeDefaultValues();
    }
    #endregion

    #region 初始化方法
    /// <summary>
    /// 初始化玩家数据
    /// </summary>
    /// <param name="name">玩家名称</param>
    /// <param name="profession">初始职业</param>
    public void Initialize(string name, ProfessionType profession)
    {
        playerName = name;
        currentProfession = profession;
        
        InitializeCollections();
        InitializeDefaultValues();
        InitializeProfessions();
        InitializeAttributes();
        InitializeTimestamps();
        
        Debug.Log($"玩家数据初始化完成: {playerName}, 职业: {profession}");
    }

    /// <summary>
    /// 初始化集合
    /// </summary>
    private void InitializeCollections()
    {
        if (professionLevels == null)
            professionLevels = new Dictionary<ProfessionType, int>();
        
        if (professionExperience == null)
            professionExperience = new Dictionary<ProfessionType, int>();
        
        if (learnedSkills == null)
            learnedSkills = new List<string>();
        
        if (skillLevels == null)
            skillLevels = new Dictionary<string, int>();
        
        if (skillCooldowns == null)
            skillCooldowns = new Dictionary<string, float>();
        
        if (inventory == null)
            inventory = new List<ItemData>();
        
        if (equipment == null)
            equipment = new List<ItemData>();
        
        if (activeQuests == null)
            activeQuests = new List<QuestData>();
        
        if (completedQuests == null)
            completedQuests = new List<string>();
        
        if (failedQuests == null)
            failedQuests = new List<string>();
        
        if (farmPlots == null)
            farmPlots = new Dictionary<Vector2Int, CropData>();
        
        if (ownedSeeds == null)
            ownedSeeds = new List<string>();
        
        if (rentedShops == null)
            rentedShops = new List<ShopData>();
        
        if (shopReputation == null)
            shopReputation = new Dictionary<string, int>();
        
        if (npcRelationships == null)
            npcRelationships = new Dictionary<string, int>();
        
        if (npcMeetFlags == null)
            npcMeetFlags = new Dictionary<string, bool>();
        
        if (attributes == null)
            attributes = new PlayerAttributes();
        
        if (settings == null)
            settings = new PlayerSettings();
    }

    /// <summary>
    /// 初始化默认值
    /// </summary>
    private void InitializeDefaultValues()
    {
        level = 1;
        experience = 0;
        gold = 100;
        
        currentHealth = maxHealth;
        currentMana = maxMana;
        
        inventorySize = 30;
        
        farmingLevel = 1;
        farmingExperience = 0;
        tradingLevel = 1;
        tradingExperience = 0;
    }

    /// <summary>
    /// 初始化职业数据
    /// </summary>
    private void InitializeProfessions()
    {
        // 初始化所有职业等级为1
        professionLevels[ProfessionType.Merchant] = 1;
        professionLevels[ProfessionType.Cultivator] = 1;
        
        // 初始化职业经验为0
        professionExperience[ProfessionType.Merchant] = 0;
        professionExperience[ProfessionType.Cultivator] = 0;
    }

    /// <summary>
    /// 初始化属性
    /// </summary>
    private void InitializeAttributes()
    {
        attributes.Initialize();
        attributes.UpdateFromPlayerData(this);
    }

    /// <summary>
    /// 初始化时间戳
    /// </summary>
    private void InitializeTimestamps()
    {
        long currentTime = DateTime.Now.ToBinary();
        creationTime = currentTime;
        lastSaveTime = currentTime;
        lastLoginTime = currentTime;
    }
    #endregion

    #region 计算方法
    /// <summary>
    /// 计算升级所需经验
    /// </summary>
    /// <returns>升级所需经验值</returns>
    private int CalculateExperienceToNextLevel()
    {
        // 经验公式：下一级所需经验 = 当前等级 * 100 + (当前等级 - 1) * 50
        int nextLevelExp = level * 100 + (level - 1) * 50;
        int currentLevelExp = (level - 1) * 100 + (level - 2) * 50;
        if (currentLevelExp < 0) currentLevelExp = 0;
        
        return nextLevelExp - experience;
    }

    /// <summary>
    /// 计算当前经验百分比
    /// </summary>
    /// <returns>经验百分比（0-1）</returns>
    private float CalculateExperiencePercentage()
    {
        int currentLevelExp = (level - 1) * 100 + (level - 2) * 50;
        if (currentLevelExp < 0) currentLevelExp = 0;
        
        int nextLevelExp = level * 100 + (level - 1) * 50;
        int expInCurrentLevel = experience - currentLevelExp;
        int expNeededForLevel = nextLevelExp - currentLevelExp;
        
        if (expNeededForLevel <= 0) return 1f;
        
        return Mathf.Clamp01((float)expInCurrentLevel / expNeededForLevel);
    }
    #endregion

    #region 验证方法
    /// <summary>
    /// 验证数据有效性
    /// </summary>
    /// <returns>数据是否有效</returns>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(playerName))
            return false;
        
        if (level < 1 || level > 100)
            return false;
        
        if (experience < 0)
            return false;
        
        if (currentHealth < 0 || currentHealth > maxHealth)
            return false;
        
        if (currentMana < 0 || currentMana > maxMana)
            return false;
        
        return true;
    }

    /// <summary>
    /// 修复无效数据
    /// </summary>
    public void FixInvalidData()
    {
        if (string.IsNullOrEmpty(playerName))
            playerName = "DefaultPlayer";
        
        level = Mathf.Clamp(level, 1, 100);
        experience = Mathf.Max(0, experience);
        gold = Mathf.Max(0, gold);
        
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        currentMana = Mathf.Clamp(currentMana, 0, maxMana);
        
        // 确保集合不为null
        InitializeCollections();
    }
    #endregion

    #region 克隆方法
    /// <summary>
    /// 深度克隆玩家数据
    /// </summary>
    /// <returns>克隆的玩家数据</returns>
    public PlayerData Clone()
    {
        string json = JsonUtility.ToJson(this);
        return JsonUtility.FromJson<PlayerData>(json);
    }
    #endregion
}
