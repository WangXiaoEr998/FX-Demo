using UnityEngine;
using System.Collections.Generic;
using FanXing.Data;
/// <summary>
/// 职业管理器，负责玩家双职业系统的管理
/// 作者：黄畅修
/// 创建时间：2025-07-12
/// </summary>
public class ProfessionManager : MonoBehaviour
{
    #region 单例模式
    private static ProfessionManager _instance;

    /// <summary>
    /// 单例实例
    /// </summary>
    public static ProfessionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ProfessionManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("ProfessionManager");
                    _instance = go.AddComponent<ProfessionManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    #endregion

    #region 字段定义
    [Header("职业配置")]
    [SerializeField] private ProfessionConfig _merchantConfig;
    [SerializeField] private ProfessionConfig _cultivatorConfig;
    [SerializeField] private bool _enableDebugMode = false;

    // 当前职业状态
    private ProfessionType _currentProfession = ProfessionType.None;
    private Dictionary<ProfessionType, int> _professionLevels = new Dictionary<ProfessionType, int>();
    private Dictionary<ProfessionType, int> _professionExperience = new Dictionary<ProfessionType, int>();
    private Dictionary<ProfessionType, ProfessionConfig> _professionConfigs = new Dictionary<ProfessionType, ProfessionConfig>();

    // 管理器状态
    private bool _isInitialized = false;
    #endregion

    #region 属性
    /// <summary>
    /// 当前职业
    /// </summary>
    public ProfessionType CurrentProfession => _currentProfession;
    
    /// <summary>
    /// 是否已初始化
    /// </summary>
    public bool IsInitialized => _isInitialized;
    
    /// <summary>
    /// 商人等级
    /// </summary>
    public int MerchantLevel => GetProfessionLevel(ProfessionType.Merchant);
    
    /// <summary>
    /// 修士等级
    /// </summary>
    public int CultivatorLevel => GetProfessionLevel(ProfessionType.Cultivator);
    
    /// <summary>
    /// 商人经验
    /// </summary>
    public int MerchantExperience => GetProfessionExperience(ProfessionType.Merchant);
    
    /// <summary>
    /// 修士经验
    /// </summary>
    public int CultivatorExperience => GetProfessionExperience(ProfessionType.Cultivator);
    #endregion

    #region 公共方法 - 初始化
    /// <summary>
    /// 初始化职业管理器
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized)
        {
            LogWarning("职业管理器已经初始化过了");
            return;
        }

        LogDebug("初始化职业管理器");
        
        // 初始化职业数据
        InitializeProfessionData();
        
        // 加载职业配置
        LoadProfessionConfigs();
        
        // 注册事件
        RegisterEvents();
        
        _isInitialized = true;
        
        LogDebug("职业管理器初始化完成");
    }

    /// <summary>
    /// 更新职业管理器
    /// </summary>
    /// <param name="deltaTime">帧时间</param>
    public void UpdateManager(float deltaTime)
    {
        if (!_isInitialized)
            return;

        // 更新职业相关逻辑
        UpdateProfessionEffects(deltaTime);
    }
    #endregion

    #region 公共方法 - 职业管理
    /// <summary>
    /// 设置职业
    /// </summary>
    /// <param name="profession">职业类型</param>
    public void SetProfession(ProfessionType profession)
    {
        if (profession == ProfessionType.None)
        {
            LogWarning("不能设置为无职业");
            return;
        }

        ProfessionType oldProfession = _currentProfession;
        _currentProfession = profession;
        
        LogDebug($"设置职业: {profession}");
        
        // 触发职业改变事件
        Global.Event.TriggerEvent(Global.Events.Player.PROFESSION_CHANGED,
            new ProfessionChangedEventArgs(oldProfession, profession));
    }

    /// <summary>
    /// 切换职业
    /// </summary>
    /// <param name="profession">目标职业</param>
    /// <returns>是否切换成功</returns>
    public bool SwitchProfession(ProfessionType profession)
    {
        if (profession == ProfessionType.None)
        {
            LogError("不能切换到无职业");
            return false;
        }

        if (profession == _currentProfession)
        {
            LogWarning($"已经是{profession}职业了");
            return false;
        }

        // 检查是否已解锁该职业
        if (!IsProfessionUnlocked(profession))
        {
            LogError($"职业{profession}尚未解锁");
            return false;
        }

        LogDebug($"切换职业: {_currentProfession} -> {profession}");
        
        ProfessionType oldProfession = _currentProfession;
        _currentProfession = profession;
        
        // 应用职业效果
        ApplyProfessionEffects(profession);
        
        // 触发职业改变事件
        Global.Event.TriggerEvent(Global.Events.Player.PROFESSION_CHANGED,
            new ProfessionChangedEventArgs(oldProfession, profession));
        
        return true;
    }

    /// <summary>
    /// 获取职业等级
    /// </summary>
    /// <param name="profession">职业类型</param>
    /// <returns>职业等级</returns>
    public int GetProfessionLevel(ProfessionType profession)
    {
        if (_professionLevels.ContainsKey(profession))
        {
            return _professionLevels[profession];
        }
        return 1;
    }

    /// <summary>
    /// 获取职业经验
    /// </summary>
    /// <param name="profession">职业类型</param>
    /// <returns>职业经验</returns>
    public int GetProfessionExperience(ProfessionType profession)
    {
        if (_professionExperience.ContainsKey(profession))
        {
            return _professionExperience[profession];
        }
        return 0;
    }

    /// <summary>
    /// 增加职业经验
    /// </summary>
    /// <param name="profession">职业类型</param>
    /// <param name="experience">经验值</param>
    public void AddExperience(ProfessionType profession, int experience)
    {
        if (profession == ProfessionType.None || experience <= 0)
            return;

        LogDebug($"增加{profession}经验: {experience}");
        
        int oldLevel = GetProfessionLevel(profession);
        int oldExperience = GetProfessionExperience(profession);
        
        // 增加经验
        if (!_professionExperience.ContainsKey(profession))
        {
            _professionExperience[profession] = 0;
        }
        _professionExperience[profession] += experience;
        
        // 检查升级
        CheckProfessionLevelUp(profession, oldLevel);
        
        // 触发经验变化事件
        Global.Event.TriggerEvent(Global.Events.Player.EXPERIENCE_GAINED,
            new { profession, experienceGain = experience, totalExperience = _professionExperience[profession] });
    }

    /// <summary>
    /// 检查职业是否已解锁
    /// </summary>
    /// <param name="profession">职业类型</param>
    /// <returns>是否已解锁</returns>
    public bool IsProfessionUnlocked(ProfessionType profession)
    {
        // 商人和修士默认都解锁
        return profession == ProfessionType.Merchant || profession == ProfessionType.Cultivator;
    }

    /// <summary>
    /// 获取职业配置
    /// </summary>
    /// <param name="profession">职业类型</param>
    /// <returns>职业配置</returns>
    public ProfessionConfig GetProfessionConfig(ProfessionType profession)
    {
        if (_professionConfigs.ContainsKey(profession))
        {
            return _professionConfigs[profession];
        }
        return null;
    }

    /// <summary>
    /// 获取当前职业的属性加成
    /// </summary>
    /// <returns>属性加成</returns>
    public PlayerAttributes GetCurrentProfessionBonus()
    {
        PlayerAttributes bonus = new PlayerAttributes();
        
        ProfessionConfig config = GetProfessionConfig(_currentProfession);
        if (config != null)
        {
            int level = GetProfessionLevel(_currentProfession);
            
            // 根据等级计算属性加成
            bonus.skillStrength = config.strengthBonus * level;
            bonus.skillAgility = config.agilityBonus * level;
            bonus.skillIntelligence = config.intelligenceBonus * level;
            bonus.skillVitality = config.vitalityBonus * level;
            bonus.skillLuck = config.luckBonus * level;
        }
        
        return bonus;
    }
    #endregion

    #region 私有方法 - 初始化
    /// <summary>
    /// 初始化职业数据
    /// </summary>
    private void InitializeProfessionData()
    {
        LogDebug("初始化职业数据");
        
        // 初始化职业等级
        if (!_professionLevels.ContainsKey(ProfessionType.Merchant))
            _professionLevels[ProfessionType.Merchant] = 1;
        
        if (!_professionLevels.ContainsKey(ProfessionType.Cultivator))
            _professionLevels[ProfessionType.Cultivator] = 1;
        
        // 初始化职业经验
        if (!_professionExperience.ContainsKey(ProfessionType.Merchant))
            _professionExperience[ProfessionType.Merchant] = 0;
        
        if (!_professionExperience.ContainsKey(ProfessionType.Cultivator))
            _professionExperience[ProfessionType.Cultivator] = 0;
        
        // 设置默认职业
        if (_currentProfession == ProfessionType.None)
            _currentProfession = ProfessionType.Merchant;
    }

    /// <summary>
    /// 加载职业配置
    /// </summary>
    private void LoadProfessionConfigs()
    {
        LogDebug("加载职业配置");
        
        // 加载商人配置
        if (_merchantConfig != null)
        {
            _professionConfigs[ProfessionType.Merchant] = _merchantConfig;
        }
        
        // 加载修士配置
        if (_cultivatorConfig != null)
        {
            _professionConfigs[ProfessionType.Cultivator] = _cultivatorConfig;
        }
        
        // 从DataManager加载配置（如果本地配置为空）
        if (_merchantConfig == null)
        {
            _merchantConfig = Global.Data?.GetConfig<ProfessionConfig>("MerchantConfig");
            if (_merchantConfig != null)
                _professionConfigs[ProfessionType.Merchant] = _merchantConfig;
        }
        
        if (_cultivatorConfig == null)
        {
            _cultivatorConfig = Global.Data?.GetConfig<ProfessionConfig>("CultivatorConfig");
            if (_cultivatorConfig != null)
                _professionConfigs[ProfessionType.Cultivator] = _cultivatorConfig;
        }
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvents()
    {
        LogDebug("注册职业管理器事件");
        
        Global.Event.Register<ProfessionLevelUpEventArgs>(Global.Events.Player.LEVEL_UP, OnPlayerLevelUp);
        Global.Event.Register<SkillLearnedEventArgs>(Global.Events.Skill.LEARNED, OnSkillLearned);
    }

    /// <summary>
    /// 注销事件
    /// </summary>
    private void UnregisterEvents()
    {
        LogDebug("注销职业管理器事件");
        
        Global.Event.UnRegister<ProfessionLevelUpEventArgs>(Global.Events.Player.LEVEL_UP, OnPlayerLevelUp);
        Global.Event.UnRegister<SkillLearnedEventArgs>(Global.Events.Skill.LEARNED, OnSkillLearned);
    }
    #endregion

    #region 私有方法 - 职业逻辑
    /// <summary>
    /// 更新职业效果
    /// </summary>
    /// <param name="deltaTime">帧时间</param>
    private void UpdateProfessionEffects(float deltaTime)
    {
        // 根据当前职业更新相关效果
        switch (_currentProfession)
        {
            case ProfessionType.Merchant:
                UpdateMerchantEffects(deltaTime);
                break;
            case ProfessionType.Cultivator:
                UpdateCultivatorEffects(deltaTime);
                break;
        }
    }

    /// <summary>
    /// 更新商人效果
    /// </summary>
    /// <param name="deltaTime">帧时间</param>
    private void UpdateMerchantEffects(float deltaTime)
    {
        // 商人职业的特殊效果更新
    }

    /// <summary>
    /// 更新修士效果
    /// </summary>
    /// <param name="deltaTime">帧时间</param>
    private void UpdateCultivatorEffects(float deltaTime)
    {
        // 修士职业的特殊效果更新
    }

    /// <summary>
    /// 应用职业效果
    /// </summary>
    /// <param name="profession">职业类型</param>
    private void ApplyProfessionEffects(ProfessionType profession)
    {
        LogDebug($"应用{profession}职业效果");
        
        // 应用职业特有的效果
        switch (profession)
        {
            case ProfessionType.Merchant:
                ApplyMerchantEffects();
                break;
            case ProfessionType.Cultivator:
                ApplyCultivatorEffects();
                break;
        }
    }

    /// <summary>
    /// 应用商人效果
    /// </summary>
    private void ApplyMerchantEffects()
    {
        // 商人职业的特殊效果
        // 例如：交易价格优惠、金币获取加成等
    }

    /// <summary>
    /// 应用修士效果
    /// </summary>
    private void ApplyCultivatorEffects()
    {
        // 修士职业的特殊效果
        // 例如：法力恢复加成、技能冷却减少等
    }

    /// <summary>
    /// 检查职业升级
    /// </summary>
    /// <param name="profession">职业类型</param>
    /// <param name="oldLevel">旧等级</param>
    private void CheckProfessionLevelUp(ProfessionType profession, int oldLevel)
    {
        int currentExperience = GetProfessionExperience(profession);
        int newLevel = CalculateProfessionLevel(profession, currentExperience);
        
        if (newLevel > oldLevel)
        {
            _professionLevels[profession] = newLevel;
            
            LogDebug($"{profession}职业升级: {oldLevel} -> {newLevel}");
            
            // 触发职业升级事件
            Global.Event.TriggerEvent(Global.Events.Player.LEVEL_UP,
                new ProfessionLevelUpEventArgs(profession, oldLevel, newLevel));
        }
    }

    /// <summary>
    /// 计算职业等级
    /// </summary>
    /// <param name="profession">职业类型</param>
    /// <param name="experience">经验值</param>
    /// <returns>等级</returns>
    private int CalculateProfessionLevel(ProfessionType profession, int experience)
    {
        ProfessionConfig config = GetProfessionConfig(profession);
        if (config == null)
            return 1;

        int level = 1;
        int requiredExp = 0;
        
        while (requiredExp <= experience && level < config.maxLevel)
        {
            level++;
            requiredExp += Mathf.RoundToInt(config.baseExperiencePerLevel * Mathf.Pow(config.experienceMultiplier, level - 1));
        }
        
        return level - 1;
    }
    #endregion

    #region 事件处理
    /// <summary>
    /// 玩家升级事件处理
    /// </summary>
    private void OnPlayerLevelUp(ProfessionLevelUpEventArgs args)
    {
        LogDebug("处理玩家升级事件");
        
        // 玩家升级时可能解锁新的职业能力
    }

    /// <summary>
    /// 技能学习事件处理
    /// </summary>
    private void OnSkillLearned(SkillLearnedEventArgs args)
    {
        LogDebug("处理技能学习事件");
        
        // 学习技能时可能影响职业经验
    }
    #endregion

    #region 工具方法
    /// <summary>
    /// 输出调试日志
    /// </summary>
    /// <param name="message">日志信息</param>
    private void LogDebug(string message)
    {
        if (_enableDebugMode)
        {
            Debug.Log($"[ProfessionManager] {message}");
        }
    }

    /// <summary>
    /// 输出警告日志
    /// </summary>
    /// <param name="message">警告信息</param>
    private void LogWarning(string message)
    {
        Debug.LogWarning($"[ProfessionManager] {message}");
    }

    /// <summary>
    /// 输出错误日志
    /// </summary>
    /// <param name="message">错误信息</param>
    private void LogError(string message)
    {
        Debug.LogError($"[ProfessionManager] {message}");
    }
    #endregion

    #region Unity生命周期
    private void OnDestroy()
    {
        UnregisterEvents();
    }
    #endregion
}
