using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using FanXing.Data;
/// <summary>
/// 技能管理器，负责玩家技能系统的管理
/// 作者：黄畅修
/// 创建时间：2025-07-12
/// </summary>
public class SkillManager : MonoBehaviour
{
    #region 单例模式
    private static SkillManager _instance;

    /// <summary>
    /// 单例实例
    /// </summary>
    public static SkillManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SkillManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SkillManager");
                    _instance = go.AddComponent<SkillManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    #endregion

    #region 字段定义
    [Header("技能配置")]
    [SerializeField] private SkillConfig _skillConfig;
    [SerializeField] private bool _enableDebugMode = false;

    // 技能状态
    private Dictionary<string, SkillInfo> _allSkills = new Dictionary<string, SkillInfo>();
    private Dictionary<string, SkillInfo> _learnedSkills = new Dictionary<string, SkillInfo>();
    private Dictionary<string, float> _skillCooldowns = new Dictionary<string, float>();
    private List<string> _activeSkills = new List<string>();

    // 管理器状态
    private bool _isInitialized = false;
    #endregion

    #region 属性
    /// <summary>
    /// 是否已初始化
    /// </summary>
    public bool IsInitialized => _isInitialized;

    /// <summary>
    /// 已学习的技能数量
    /// </summary>
    public int LearnedSkillCount => _learnedSkills.Count;

    /// <summary>
    /// 激活的技能数量
    /// </summary>
    public int ActiveSkillCount => _activeSkills.Count;

    /// <summary>
    /// 最大激活技能数量
    /// </summary>
    public int MaxActiveSkills => _skillConfig?.maxActiveSkills ?? 10;
    #endregion

    #region 公共方法 - 初始化
    /// <summary>
    /// 初始化技能管理器
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized)
        {
            LogWarning("技能管理器已经初始化过了");
            return;
        }

        LogDebug("初始化技能管理器");

        // 加载技能配置
        LoadSkillConfig();

        // 初始化技能数据
        InitializeSkillData();

        // 注册事件
        RegisterEvents();

        _isInitialized = true;

        LogDebug("技能管理器初始化完成");
    }

    /// <summary>
    /// 更新技能管理器
    /// </summary>
    /// <param name="deltaTime">帧时间</param>
    public void UpdateManager(float deltaTime)
    {
        if (!_isInitialized)
            return;

        // 更新技能冷却
        UpdateSkillCooldowns(deltaTime);

        // 更新激活技能效果
        UpdateActiveSkillEffects(deltaTime);
    }
    #endregion

    #region 公共方法 - 技能学习
    /// <summary>
    /// 学习技能
    /// </summary>
    /// <param name="skillId">技能ID</param>
    /// <returns>是否学习成功</returns>
    public bool LearnSkill(string skillId)
    {
        if (string.IsNullOrEmpty(skillId))
        {
            LogError("技能ID不能为空");
            return false;
        }

        if (_learnedSkills.ContainsKey(skillId))
        {
            LogWarning($"技能{skillId}已经学习过了");
            return false;
        }

        if (!_allSkills.ContainsKey(skillId))
        {
            LogError($"技能{skillId}不存在");
            return false;
        }

        SkillInfo skill = _allSkills[skillId];

        // 检查学习条件
        if (!CanLearnSkill(skill))
        {
            LogWarning($"不满足学习技能{skillId}的条件");
            return false;
        }

        LogDebug($"学习技能: {skillId}");

        // 创建技能副本并添加到已学习技能
        SkillInfo learnedSkill = CloneSkillInfo(skill);
        learnedSkill.isLearned = true;
        learnedSkill.currentLevel = 1;

        _learnedSkills[skillId] = learnedSkill;

        // 触发技能学习事件
        Global.Event.TriggerEvent(Global.Events.Skill.LEARNED,
            new SkillLearnedEventArgs(skillId, skill.skillName, 1));

        return true;
    }

    /// <summary>
    /// 升级技能
    /// </summary>
    /// <param name="skillId">技能ID</param>
    /// <returns>是否升级成功</returns>
    public bool UpgradeSkill(string skillId)
    {
        if (!_learnedSkills.ContainsKey(skillId))
        {
            LogError($"技能{skillId}尚未学习");
            return false;
        }

        SkillInfo skill = _learnedSkills[skillId];

        if (skill.currentLevel >= skill.maxLevel)
        {
            LogWarning($"技能{skillId}已达到最大等级");
            return false;
        }

        // 检查升级条件
        if (!CanUpgradeSkill(skill))
        {
            LogWarning($"不满足升级技能{skillId}的条件");
            return false;
        }

        LogDebug($"升级技能: {skillId} ({skill.currentLevel} -> {skill.currentLevel + 1})");

        int oldLevel = skill.currentLevel;
        skill.currentLevel++;

        // 触发技能升级事件
        Global.Event.TriggerEvent(Global.Events.Skill.UPGRADED,
            new SkillUpgradedEventArgs(skillId, oldLevel, skill.currentLevel));

        return true;
    }

    /// <summary>
    /// 使用技能
    /// </summary>
    /// <param name="skillId">技能ID</param>
    /// <param name="target">目标对象</param>
    /// <returns>是否使用成功</returns>
    public bool UseSkill(string skillId, GameObject target = null)
    {
        if (!_learnedSkills.ContainsKey(skillId))
        {
            LogError($"技能{skillId}尚未学习");
            return false;
        }

        SkillInfo skill = _learnedSkills[skillId];

        // 检查使用条件
        if (!CanUseSkill(skill, target))
        {
            LogWarning($"不能使用技能{skillId}");
            return false;
        }

        LogDebug($"使用技能: {skillId}");

        // 执行技能效果
        bool success = ExecuteSkillEffect(skill, target);

        if (success)
        {
            // 设置冷却时间
            _skillCooldowns[skillId] = skill.cooldown;

            // 触发技能使用事件
            Vector3 position = target != null ? target.transform.position : Vector3.zero;
            Global.Event.TriggerEvent(Global.Events.Skill.USED,
                new SkillUsedEventArgs(skillId, target, position, success));
        }

        return success;
    }

    /// <summary>
    /// 激活技能
    /// </summary>
    /// <param name="skillId">技能ID</param>
    /// <returns>是否激活成功</returns>
    public bool ActivateSkill(string skillId)
    {
        if (!_learnedSkills.ContainsKey(skillId))
        {
            LogError($"技能{skillId}尚未学习");
            return false;
        }

        if (_activeSkills.Contains(skillId))
        {
            LogWarning($"技能{skillId}已经激活");
            return false;
        }

        if (_activeSkills.Count >= MaxActiveSkills)
        {
            LogWarning("激活技能数量已达上限");
            return false;
        }

        SkillInfo skill = _learnedSkills[skillId];

        // 只有被动技能和切换技能可以激活
        if (skill.skillType != SkillType.Passive && skill.skillType != SkillType.Toggle)
        {
            LogWarning($"技能{skillId}不能激活");
            return false;
        }

        LogDebug($"激活技能: {skillId}");

        _activeSkills.Add(skillId);
        skill.isActive = true;

        // 应用技能效果
        ApplySkillEffect(skill);

        return true;
    }

    /// <summary>
    /// 取消激活技能
    /// </summary>
    /// <param name="skillId">技能ID</param>
    /// <returns>是否取消成功</returns>
    public bool DeactivateSkill(string skillId)
    {
        if (!_activeSkills.Contains(skillId))
        {
            LogWarning($"技能{skillId}未激活");
            return false;
        }

        LogDebug($"取消激活技能: {skillId}");

        _activeSkills.Remove(skillId);

        if (_learnedSkills.ContainsKey(skillId))
        {
            _learnedSkills[skillId].isActive = false;

            // 移除技能效果
            RemoveSkillEffect(_learnedSkills[skillId]);
        }

        return true;
    }
    #endregion

    #region 公共方法 - 技能查询
    /// <summary>
    /// 获取技能信息
    /// </summary>
    /// <param name="skillId">技能ID</param>
    /// <returns>技能信息</returns>
    public SkillInfo GetSkillInfo(string skillId)
    {
        if (_learnedSkills.ContainsKey(skillId))
        {
            return _learnedSkills[skillId];
        }

        if (_allSkills.ContainsKey(skillId))
        {
            return _allSkills[skillId];
        }

        return null;
    }

    /// <summary>
    /// 检查技能是否已学习
    /// </summary>
    /// <param name="skillId">技能ID</param>
    /// <returns>是否已学习</returns>
    public bool IsSkillLearned(string skillId)
    {
        return _learnedSkills.ContainsKey(skillId);
    }

    /// <summary>
    /// 检查技能是否在冷却中
    /// </summary>
    /// <param name="skillId">技能ID</param>
    /// <returns>是否在冷却中</returns>
    public bool IsSkillOnCooldown(string skillId)
    {
        return _skillCooldowns.ContainsKey(skillId) && _skillCooldowns[skillId] > 0f;
    }

    /// <summary>
    /// 获取技能剩余冷却时间
    /// </summary>
    /// <param name="skillId">技能ID</param>
    /// <returns>剩余冷却时间</returns>
    public float GetSkillCooldownRemaining(string skillId)
    {
        if (_skillCooldowns.ContainsKey(skillId))
        {
            return Mathf.Max(0f, _skillCooldowns[skillId]);
        }
        return 0f;
    }

    /// <summary>
    /// 获取已学习的技能列表
    /// </summary>
    /// <returns>技能ID列表</returns>
    public List<string> GetLearnedSkills()
    {
        return new List<string>(_learnedSkills.Keys);
    }

    /// <summary>
    /// 获取激活的技能列表
    /// </summary>
    /// <returns>技能ID列表</returns>
    public List<string> GetActiveSkills()
    {
        return new List<string>(_activeSkills);
    }

    /// <summary>
    /// 获取可学习的技能列表
    /// </summary>
    /// <param name="profession">职业类型</param>
    /// <returns>技能信息列表</returns>
    public List<SkillInfo> GetAvailableSkills(ProfessionType profession)
    {
        List<SkillInfo> availableSkills = new List<SkillInfo>();

        foreach (var skill in _allSkills.Values)
        {
            if (skill.requiredProfession == profession || skill.requiredProfession == ProfessionType.None)
            {
                if (!_learnedSkills.ContainsKey(skill.skillId) && CanLearnSkill(skill))
                {
                    availableSkills.Add(skill);
                }
            }
        }

        return availableSkills;
    }
    #endregion

    #region 公共方法 - 数据管理
    /// <summary>
    /// 恢复技能数据
    /// </summary>
    /// <param name="learnedSkills">已学习的技能列表</param>
    public void RestoreSkills(List<string> learnedSkills)
    {
        if (learnedSkills == null)
            return;

        LogDebug($"恢复技能数据，共{learnedSkills.Count}个技能");

        _learnedSkills.Clear();

        foreach (string skillId in learnedSkills)
        {
            if (_allSkills.ContainsKey(skillId))
            {
                SkillInfo skill = CloneSkillInfo(_allSkills[skillId]);
                skill.isLearned = true;
                skill.currentLevel = 1; // 这里应该从保存数据中读取实际等级

                _learnedSkills[skillId] = skill;
            }
        }
    }

    /// <summary>
    /// 获取技能保存数据
    /// </summary>
    /// <returns>技能保存数据</returns>
    public Dictionary<string, int> GetSkillSaveData()
    {
        Dictionary<string, int> saveData = new Dictionary<string, int>();

        foreach (var kvp in _learnedSkills)
        {
            saveData[kvp.Key] = kvp.Value.currentLevel;
        }

        return saveData;
    }

    /// <summary>
    /// 加载技能保存数据
    /// </summary>
    /// <param name="saveData">技能保存数据</param>
    public void LoadSkillSaveData(Dictionary<string, int> saveData)
    {
        if (saveData == null)
            return;

        LogDebug($"加载技能保存数据，共{saveData.Count}个技能");

        foreach (var kvp in saveData)
        {
            string skillId = kvp.Key;
            int level = kvp.Value;

            if (_allSkills.ContainsKey(skillId))
            {
                SkillInfo skill = CloneSkillInfo(_allSkills[skillId]);
                skill.isLearned = true;
                skill.currentLevel = level;

                _learnedSkills[skillId] = skill;
            }
        }
    }
    #endregion

    #region 私有方法 - 初始化
    /// <summary>
    /// 加载技能配置
    /// </summary>
    private void LoadSkillConfig()
    {
        LogDebug("加载技能配置");

        // 从DataManager加载配置（如果本地配置为空）
        if (_skillConfig == null)
        {
            _skillConfig = DataManager.Instance?.GetConfig<SkillConfig>("SkillConfig");
        }

        if (_skillConfig == null)
        {
            LogError("技能配置未找到");
            return;
        }

        // 加载所有技能
        if (_skillConfig.allSkills != null)
        {
            foreach (var skill in _skillConfig.allSkills)
            {
                if (!string.IsNullOrEmpty(skill.skillId))
                {
                    _allSkills[skill.skillId] = skill;
                }
            }
        }

        LogDebug($"加载了{_allSkills.Count}个技能");
    }

    /// <summary>
    /// 初始化技能数据
    /// </summary>
    private void InitializeSkillData()
    {
        LogDebug("初始化技能数据");

        // 初始化冷却时间字典
        _skillCooldowns.Clear();

        // 初始化激活技能列表
        _activeSkills.Clear();

        // 初始化已学习技能字典
        _learnedSkills.Clear();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvents()
    {
        LogDebug("注册技能管理器事件");

        Global.Event.Register<ProfessionLevelUpEventArgs>(Global.Events.Player.LEVEL_UP, OnPlayerLevelUp);
        Global.Event.Register<ProfessionChangedEventArgs>(Global.Events.Player.PROFESSION_CHANGED, OnProfessionChanged);
    }

    /// <summary>
    /// 注销事件
    /// </summary>
    private void UnregisterEvents()
    {
        LogDebug("注销技能管理器事件");

        Global.Event.UnRegister<ProfessionLevelUpEventArgs>(Global.Events.Player.LEVEL_UP, OnPlayerLevelUp);
        Global.Event.UnRegister<ProfessionChangedEventArgs>(Global.Events.Player.PROFESSION_CHANGED, OnProfessionChanged);
    }
    #endregion

    #region 私有方法 - 技能逻辑
    /// <summary>
    /// 更新技能冷却
    /// </summary>
    /// <param name="deltaTime">帧时间</param>
    private void UpdateSkillCooldowns(float deltaTime)
    {
        List<string> expiredCooldowns = new List<string>();

        foreach (var kvp in _skillCooldowns.ToList())
        {
            string skillId = kvp.Key;
            float cooldown = kvp.Value;

            cooldown -= deltaTime;

            if (cooldown <= 0f)
            {
                expiredCooldowns.Add(skillId);
            }
            else
            {
                _skillCooldowns[skillId] = cooldown;
            }
        }

        // 移除已过期的冷却
        foreach (string skillId in expiredCooldowns)
        {
            _skillCooldowns.Remove(skillId);
        }
    }

    /// <summary>
    /// 更新激活技能效果
    /// </summary>
    /// <param name="deltaTime">帧时间</param>
    private void UpdateActiveSkillEffects(float deltaTime)
    {
        foreach (string skillId in _activeSkills)
        {
            if (_learnedSkills.ContainsKey(skillId))
            {
                UpdateSkillEffect(_learnedSkills[skillId], deltaTime);
            }
        }
    }

    /// <summary>
    /// 检查是否可以学习技能
    /// </summary>
    /// <param name="skill">技能信息</param>
    /// <returns>是否可以学习</returns>
    private bool CanLearnSkill(SkillInfo skill)
    {
        // 检查职业要求
        if (skill.requiredProfession != ProfessionType.None)
        {
            // 这里应该从PlayerSystem获取当前职业
            // 暂时返回true
        }

        // 检查等级要求
        if (skill.requiredLevel > 1)
        {
            // 这里应该从PlayerSystem获取玩家等级
            // 暂时返回true
        }

        // 检查前置技能
        if (skill.requiredSkills != null && skill.requiredSkills.Length > 0)
        {
            foreach (string requiredSkillId in skill.requiredSkills)
            {
                if (!_learnedSkills.ContainsKey(requiredSkillId))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// 检查是否可以升级技能
    /// </summary>
    /// <param name="skill">技能信息</param>
    /// <returns>是否可以升级</returns>
    private bool CanUpgradeSkill(SkillInfo skill)
    {
        // 检查等级限制
        if (skill.currentLevel >= skill.maxLevel)
            return false;

        // 检查升级要求（技能点、金币等）
        // 这里应该添加具体的升级条件检查

        return true;
    }

    /// <summary>
    /// 检查是否可以使用技能
    /// </summary>
    /// <param name="skill">技能信息</param>
    /// <param name="target">目标对象</param>
    /// <returns>是否可以使用</returns>
    private bool CanUseSkill(SkillInfo skill, GameObject target)
    {
        // 检查冷却时间
        if (IsSkillOnCooldown(skill.skillId))
            return false;

        // 检查法力消耗
        // 这里应该从PlayerSystem检查玩家当前法力值

        // 检查目标要求
        if (skill.targetType == SkillTargetType.Enemy && target == null)
            return false;

        if (skill.targetType == SkillTargetType.Ally && target == null)
            return false;

        return true;
    }

    /// <summary>
    /// 执行技能效果
    /// </summary>
    /// <param name="skill">技能信息</param>
    /// <param name="target">目标对象</param>
    /// <returns>是否执行成功</returns>
    private bool ExecuteSkillEffect(SkillInfo skill, GameObject target)
    {
        LogDebug($"执行技能效果: {skill.skillId}");

        // 根据技能类型执行不同的效果
        switch (skill.skillType)
        {
            case SkillType.Active:
                return ExecuteActiveSkill(skill, target);
            case SkillType.Instant:
                return ExecuteInstantSkill(skill, target);
            case SkillType.Channeled:
                return ExecuteChanneledSkill(skill, target);
            default:
                LogWarning($"未处理的技能类型: {skill.skillType}");
                return false;
        }
    }

    /// <summary>
    /// 执行主动技能
    /// </summary>
    /// <param name="skill">技能信息</param>
    /// <param name="target">目标对象</param>
    /// <returns>是否执行成功</returns>
    private bool ExecuteActiveSkill(SkillInfo skill, GameObject target)
    {
        // 主动技能的具体实现
        return true;
    }

    /// <summary>
    /// 执行瞬发技能
    /// </summary>
    /// <param name="skill">技能信息</param>
    /// <param name="target">目标对象</param>
    /// <returns>是否执行成功</returns>
    private bool ExecuteInstantSkill(SkillInfo skill, GameObject target)
    {
        // 瞬发技能的具体实现
        return true;
    }

    /// <summary>
    /// 执行引导技能
    /// </summary>
    /// <param name="skill">技能信息</param>
    /// <param name="target">目标对象</param>
    /// <returns>是否执行成功</returns>
    private bool ExecuteChanneledSkill(SkillInfo skill, GameObject target)
    {
        // 引导技能的具体实现
        return true;
    }

    /// <summary>
    /// 应用技能效果
    /// </summary>
    /// <param name="skill">技能信息</param>
    private void ApplySkillEffect(SkillInfo skill)
    {
        LogDebug($"应用技能效果: {skill.skillId}");

        // 应用被动技能效果
        if (skill.skillType == SkillType.Passive)
        {
            ApplyPassiveSkillEffect(skill);
        }
    }

    /// <summary>
    /// 移除技能效果
    /// </summary>
    /// <param name="skill">技能信息</param>
    private void RemoveSkillEffect(SkillInfo skill)
    {
        LogDebug($"移除技能效果: {skill.skillId}");

        // 移除被动技能效果
        if (skill.skillType == SkillType.Passive)
        {
            RemovePassiveSkillEffect(skill);
        }
    }

    /// <summary>
    /// 应用被动技能效果
    /// </summary>
    /// <param name="skill">技能信息</param>
    private void ApplyPassiveSkillEffect(SkillInfo skill)
    {
        // 被动技能效果的具体实现
        // 例如：属性加成、特殊能力等
    }

    /// <summary>
    /// 移除被动技能效果
    /// </summary>
    /// <param name="skill">技能信息</param>
    private void RemovePassiveSkillEffect(SkillInfo skill)
    {
        // 移除被动技能效果的具体实现
    }

    /// <summary>
    /// 更新技能效果
    /// </summary>
    /// <param name="skill">技能信息</param>
    /// <param name="deltaTime">帧时间</param>
    private void UpdateSkillEffect(SkillInfo skill, float deltaTime)
    {
        // 更新持续性技能效果
        if (skill.skillType == SkillType.Toggle)
        {
            UpdateToggleSkillEffect(skill, deltaTime);
        }
    }

    /// <summary>
    /// 更新切换技能效果
    /// </summary>
    /// <param name="skill">技能信息</param>
    /// <param name="deltaTime">帧时间</param>
    private void UpdateToggleSkillEffect(SkillInfo skill, float deltaTime)
    {
        // 切换技能效果的具体实现
    }

    /// <summary>
    /// 克隆技能信息
    /// </summary>
    /// <param name="original">原始技能信息</param>
    /// <returns>克隆的技能信息</returns>
    private SkillInfo CloneSkillInfo(SkillInfo original)
    {
        SkillInfo clone = new SkillInfo();

        clone.skillId = original.skillId;
        clone.skillName = original.skillName;
        clone.description = original.description;
        clone.icon = original.icon;
        clone.skillType = original.skillType;
        clone.targetType = original.targetType;
        clone.maxLevel = original.maxLevel;
        clone.currentLevel = original.currentLevel;
        clone.cooldown = original.cooldown;
        clone.currentCooldown = original.currentCooldown;
        clone.manaCost = original.manaCost;
        clone.requiredProfession = original.requiredProfession;
        clone.requiredLevel = original.requiredLevel;
        clone.requiredSkills = original.requiredSkills != null ? (string[])original.requiredSkills.Clone() : null;
        clone.effects = original.effects != null ? (string[])original.effects.Clone() : null;
        clone.effectValues = original.effectValues != null ? (float[])original.effectValues.Clone() : null;
        clone.isLearned = original.isLearned;
        clone.isActive = original.isActive;

        return clone;
    }
    #endregion

    #region 事件处理
    /// <summary>
    /// 玩家升级事件处理
    /// </summary>
    private void OnPlayerLevelUp(ProfessionLevelUpEventArgs args)
    {
        LogDebug("处理玩家升级事件");

        // 玩家升级时可能解锁新技能
    }

    /// <summary>
    /// 职业改变事件处理
    /// </summary>
    private void OnProfessionChanged(ProfessionChangedEventArgs args)
    {
        LogDebug("处理职业改变事件");

        // 职业改变时可能需要重新计算可用技能
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
            Debug.Log($"[SkillManager] {message}");
        }
    }

    /// <summary>
    /// 输出警告日志
    /// </summary>
    /// <param name="message">警告信息</param>
    private void LogWarning(string message)
    {
        Debug.LogWarning($"[SkillManager] {message}");
    }

    /// <summary>
    /// 输出错误日志
    /// </summary>
    /// <param name="message">错误信息</param>
    private void LogError(string message)
    {
        Debug.LogError($"[SkillManager] {message}");
    }
    #endregion

    #region Unity生命周期
    private void OnDestroy()
    {
        UnregisterEvents();
    }
    #endregion
}