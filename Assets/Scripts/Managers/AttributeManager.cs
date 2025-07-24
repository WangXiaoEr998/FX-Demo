using UnityEngine;
using System.Collections.Generic;
using FanXing.Data;

/// <summary>
/// 属性管理器，负责玩家属性系统的管理
/// 作者：黄畅修，容泳森，徐锵
/// 修改时间：2025-07-24
/// </summary>
public class AttributeManager : SingletonMono<AttributeManager>
{
    #region 字段定义
    [Header("属性配置")]
    [SerializeField] private bool _enableDebugMode = false;

    // 属性数据
    private PlayerAttributes _currentAttributes;
    private PlayerAttributes _baseAttributes;
    private PlayerAttributes _equipmentBonus;
    private PlayerAttributes _skillBonus;
    private PlayerAttributes _temporaryBonus;

    // 属性修改器列表
    private List<AttributeModifier> _attributeModifiers = new List<AttributeModifier>();

    // 管理器状态
    private bool _isInitialized = false;
    private bool _needsUpdate = false;
    #endregion

    #region 属性
    /// <summary>
    /// 是否已初始化
    /// </summary>
    public bool IsInitialized => _isInitialized;
    
    /// <summary>
    /// 当前属性
    /// </summary>
    public PlayerAttributes CurrentAttributes => _currentAttributes;
    
    /// <summary>
    /// 基础属性
    /// </summary>
    public PlayerAttributes BaseAttributes => _baseAttributes;
    
    /// <summary>
    /// 装备加成
    /// </summary>
    public PlayerAttributes EquipmentBonus => _equipmentBonus;
    
    /// <summary>
    /// 技能加成
    /// </summary>
    public PlayerAttributes SkillBonus => _skillBonus;
    
    /// <summary>
    /// 临时加成
    /// </summary>
    public PlayerAttributes TemporaryBonus => _temporaryBonus;
    #endregion

    #region Unity生命周期
    private void OnDestroy()
    {
        UnregisterEvents();
    }
    #endregion

    #region 公共方法 - 初始化
    /// <summary>
    /// 初始化属性管理器
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized)
        {
            LogWarning("属性管理器已经初始化过了");
            return;
        }

        LogDebug("初始化属性管理器");
        
        // 初始化属性对象
        InitializeAttributes();
        
        // 注册事件
        RegisterEvents();
        
        _isInitialized = true;
        
        LogDebug("属性管理器初始化完成");
    }

    /// <summary>
    /// 更新属性管理器
    /// </summary>
    /// <param name="deltaTime">帧时间</param>
    public void UpdateManager(float deltaTime)
    {
        if (!_isInitialized)
            return;

        // 更新属性修改器
        UpdateAttributeModifiers(deltaTime);
        
        // 如果需要更新属性
        if (_needsUpdate)
        {
            RecalculateAttributes();
            _needsUpdate = false;
        }
    }
    #endregion

    #region 公共方法 - 属性管理
    /// <summary>
    /// 设置基础属性
    /// </summary>
    /// <param name="attributes">基础属性</param>
    public void SetBaseAttributes(PlayerAttributes attributes)
    {
        if (attributes == null)
        {
            LogError("基础属性不能为空");
            return;
        }

        LogDebug("设置基础属性");
        
        _baseAttributes.CopyFrom(attributes);
        _needsUpdate = true;
    }

    /// <summary>
    /// 更新装备加成
    /// </summary>
    /// <param name="equipment">装备列表</param>
    public void UpdateEquipmentBonus(List<ItemData> equipment)
    {
        LogDebug("更新装备加成");
        
        _equipmentBonus.ResetBonusAttributes();
        
        if (equipment != null)
        {
            foreach (var item in equipment)
            {
                if (item != null && item.IsEquipment)
                {
                    _equipmentBonus.equipmentStrength += item.strengthBonus;
                    _equipmentBonus.equipmentAgility += item.agilityBonus;
                    _equipmentBonus.equipmentIntelligence += item.intelligenceBonus;
                    _equipmentBonus.equipmentVitality += item.vitalityBonus;
                    _equipmentBonus.equipmentLuck += item.luckBonus;
                }
            }
        }
        
        _needsUpdate = true;
    }

    /// <summary>
    /// 更新技能加成
    /// </summary>
    /// <param name="skills">技能列表</param>
    public void UpdateSkillBonus(List<string> skills)
    {
        LogDebug("更新技能加成");
        
        _skillBonus.ResetBonusAttributes();
        
        if (skills != null)
        {
            foreach (string skillId in skills)
            {
                ApplySkillBonus(skillId);
            }
        }
        
        _needsUpdate = true;
    }

    /// <summary>
    /// 添加临时属性加成
    /// </summary>
    /// <param name="str">力量加成</param>
    /// <param name="agi">敏捷加成</param>
    /// <param name="intel">智力加成</param>
    /// <param name="vit">体质加成</param>
    /// <param name="luck">幸运加成</param>
    /// <param name="duration">持续时间（秒）</param>
    public void AddTemporaryBonus(int str, int agi, int intel, int vit, int luck, float duration = -1f)
    {
        LogDebug($"添加临时属性加成: STR+{str}, AGI+{agi}, INT+{intel}, VIT+{vit}, LUK+{luck}");
        
        if (duration > 0f)
        {
            // 创建临时属性修改器
            AttributeModifier modifier = new AttributeModifier
            {
                strengthBonus = str,
                agilityBonus = agi,
                intelligenceBonus = intel,
                vitalityBonus = vit,
                luckBonus = luck,
                duration = duration,
                remainingTime = duration,
                isTemporary = true
            };
            
            _attributeModifiers.Add(modifier);
        }
        else
        {
            // 永久加成
            _temporaryBonus.temporaryStrength += str;
            _temporaryBonus.temporaryAgility += agi;
            _temporaryBonus.temporaryIntelligence += intel;
            _temporaryBonus.temporaryVitality += vit;
            _temporaryBonus.temporaryLuck += luck;
        }
        
        _needsUpdate = true;
    }

    /// <summary>
    /// 移除临时属性加成
    /// </summary>
    /// <param name="str">力量减少</param>
    /// <param name="agi">敏捷减少</param>
    /// <param name="intel">智力减少</param>
    /// <param name="vit">体质减少</param>
    /// <param name="luck">幸运减少</param>
    public void RemoveTemporaryBonus(int str, int agi, int intel, int vit, int luck)
    {
        LogDebug($"移除临时属性加成: STR-{str}, AGI-{agi}, INT-{intel}, VIT-{vit}, LUK-{luck}");
        
        _temporaryBonus.temporaryStrength -= str;
        _temporaryBonus.temporaryAgility -= agi;
        _temporaryBonus.temporaryIntelligence -= intel;
        _temporaryBonus.temporaryVitality -= vit;
        _temporaryBonus.temporaryLuck -= luck;
        
        _needsUpdate = true;
    }

    /// <summary>
    /// 清除所有临时加成
    /// </summary>
    public void ClearTemporaryBonus()
    {
        LogDebug("清除所有临时属性加成");
        
        _temporaryBonus.ClearTemporaryBonus();
        _attributeModifiers.Clear();
        
        _needsUpdate = true;
    }

    /// <summary>
    /// 获取当前属性
    /// </summary>
    /// <returns>当前属性</returns>
    public PlayerAttributes GetCurrentAttributes()
    {
        if (_needsUpdate)
        {
            RecalculateAttributes();
            _needsUpdate = false;
        }
        
        return _currentAttributes.Clone();
    }

    /// <summary>
    /// 恢复属性数据
    /// </summary>
    /// <param name="attributes">属性数据</param>
    public void RestoreAttributes(PlayerAttributes attributes)
    {
        if (attributes == null)
        {
            LogWarning("恢复的属性数据为空，使用默认值");
            _baseAttributes = new PlayerAttributes();
            _baseAttributes.Initialize();
        }
        else
        {
            LogDebug("恢复属性数据");
            _baseAttributes = attributes.Clone();
        }
        
        _needsUpdate = true;
    }
    #endregion

    #region 公共方法 - 属性查询
    /// <summary>
    /// 获取总力量值
    /// </summary>
    /// <returns>总力量值</returns>
    public int GetTotalStrength()
    {
        return _currentAttributes.TotalStrength;
    }

    /// <summary>
    /// 获取总敏捷值
    /// </summary>
    /// <returns>总敏捷值</returns>
    public int GetTotalAgility()
    {
        return _currentAttributes.TotalAgility;
    }

    /// <summary>
    /// 获取总智力值
    /// </summary>
    /// <returns>总智力值</returns>
    public int GetTotalIntelligence()
    {
        return _currentAttributes.TotalIntelligence;
    }

    /// <summary>
    /// 获取总体质值
    /// </summary>
    /// <returns>总体质值</returns>
    public int GetTotalVitality()
    {
        return _currentAttributes.TotalVitality;
    }

    /// <summary>
    /// 获取总幸运值
    /// </summary>
    /// <returns>总幸运值</returns>
    public int GetTotalLuck()
    {
        return _currentAttributes.TotalLuck;
    }

    /// <summary>
    /// 获取战斗力
    /// </summary>
    /// <returns>战斗力数值</returns>
    public int GetCombatPower()
    {
        return _currentAttributes.GetCombatPower();
    }

    /// <summary>
    /// 获取属性总和
    /// </summary>
    /// <returns>所有属性的总和</returns>
    public int GetTotalAttributePoints()
    {
        return _currentAttributes.GetTotalAttributePoints();
    }
    #endregion

    #region 私有方法 - 初始化
    /// <summary>
    /// 初始化属性对象
    /// </summary>
    private void InitializeAttributes()
    {
        LogDebug("初始化属性对象");
        
        _currentAttributes = new PlayerAttributes();
        _baseAttributes = new PlayerAttributes();
        _equipmentBonus = new PlayerAttributes();
        _skillBonus = new PlayerAttributes();
        _temporaryBonus = new PlayerAttributes();
        
        // 初始化基础属性
        _baseAttributes.Initialize();
        _equipmentBonus.Initialize();
        _skillBonus.Initialize();
        _temporaryBonus.Initialize();
        
        // 初始化属性修改器列表
        _attributeModifiers.Clear();
        
        _needsUpdate = true;
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvents()
    {
        LogDebug("注册属性管理器事件");

        Global.Event.Register(Global.Events.Player.LEVEL_UP, OnPlayerLevelUp);
        Global.Event.Register<ProfessionChangedEventArgs>(Global.Events.Player.PROFESSION_CHANGED, OnProfessionChanged);
        Global.Event.Register<SkillLearnedEventArgs>(Global.Events.Skill.LEARNED, OnSkillLearned);
        Global.Event.Register(Global.Events.Item.EQUIPPED, OnItemEquipped);
        Global.Event.Register(Global.Events.Item.UNEQUIPPED, OnItemUnequipped);
    }

    /// <summary>
    /// 注销事件
    /// </summary>
    private void UnregisterEvents()
    {
        LogDebug("注销属性管理器事件");
        
        Global.Event.UnRegister(Global.Events.Player.LEVEL_UP, OnPlayerLevelUp);
        Global.Event.UnRegister<ProfessionChangedEventArgs>(Global.Events.Player.PROFESSION_CHANGED, OnProfessionChanged);
        Global.Event.UnRegister<SkillLearnedEventArgs>(Global.Events.Skill.LEARNED, OnSkillLearned);
        Global.Event.UnRegister(Global.Events.Item.EQUIPPED, OnItemEquipped);
        Global.Event.UnRegister(Global.Events.Item.UNEQUIPPED, OnItemUnequipped);
    }
    #endregion

    #region 私有方法 - 属性计算
    /// <summary>
    /// 重新计算属性
    /// </summary>
    private void RecalculateAttributes()
    {
        LogDebug("重新计算属性");
        
        // 复制基础属性
        _currentAttributes.CopyFrom(_baseAttributes);
        
        // 应用装备加成
        ApplyAttributeBonus(_currentAttributes, _equipmentBonus);
        
        // 应用技能加成
        ApplyAttributeBonus(_currentAttributes, _skillBonus);
        
        // 应用临时加成
        ApplyAttributeBonus(_currentAttributes, _temporaryBonus);
        
        // 应用属性修改器
        ApplyAttributeModifiers(_currentAttributes);
        
        // 触发属性更新事件
        Global.Event.TriggerEvent(Global.Events.Player.HEALTH_CHANGED, _currentAttributes);
    }

    /// <summary>
    /// 应用属性加成
    /// </summary>
    /// <param name="target">目标属性</param>
    /// <param name="bonus">加成属性</param>
    private void ApplyAttributeBonus(PlayerAttributes target, PlayerAttributes bonus)
    {
        target.equipmentStrength += bonus.equipmentStrength;
        target.equipmentAgility += bonus.equipmentAgility;
        target.equipmentIntelligence += bonus.equipmentIntelligence;
        target.equipmentVitality += bonus.equipmentVitality;
        target.equipmentLuck += bonus.equipmentLuck;
        
        target.skillStrength += bonus.skillStrength;
        target.skillAgility += bonus.skillAgility;
        target.skillIntelligence += bonus.skillIntelligence;
        target.skillVitality += bonus.skillVitality;
        target.skillLuck += bonus.skillLuck;
        
        target.temporaryStrength += bonus.temporaryStrength;
        target.temporaryAgility += bonus.temporaryAgility;
        target.temporaryIntelligence += bonus.temporaryIntelligence;
        target.temporaryVitality += bonus.temporaryVitality;
        target.temporaryLuck += bonus.temporaryLuck;
    }

    /// <summary>
    /// 应用属性修改器
    /// </summary>
    /// <param name="target">目标属性</param>
    private void ApplyAttributeModifiers(PlayerAttributes target)
    {
        foreach (var modifier in _attributeModifiers)
        {
            if (modifier.isActive)
            {
                target.temporaryStrength += modifier.strengthBonus;
                target.temporaryAgility += modifier.agilityBonus;
                target.temporaryIntelligence += modifier.intelligenceBonus;
                target.temporaryVitality += modifier.vitalityBonus;
                target.temporaryLuck += modifier.luckBonus;
            }
        }
    }

    /// <summary>
    /// 更新属性修改器
    /// </summary>
    /// <param name="deltaTime">帧时间</param>
    private void UpdateAttributeModifiers(float deltaTime)
    {
        bool hasExpiredModifiers = false;
        
        for (int i = _attributeModifiers.Count - 1; i >= 0; i--)
        {
            var modifier = _attributeModifiers[i];
            
            if (modifier.isTemporary && modifier.remainingTime > 0f)
            {
                modifier.remainingTime -= deltaTime;
                
                if (modifier.remainingTime <= 0f)
                {
                    modifier.isActive = false;
                    _attributeModifiers.RemoveAt(i);
                    hasExpiredModifiers = true;
                    
                    LogDebug($"属性修改器过期: STR+{modifier.strengthBonus}, AGI+{modifier.agilityBonus}");
                }
            }
        }
        
        if (hasExpiredModifiers)
        {
            _needsUpdate = true;
        }
    }

    /// <summary>
    /// 应用技能属性加成
    /// </summary>
    /// <param name="skillId">技能ID</param>
    private void ApplySkillBonus(string skillId)
    {
        // 这里应该根据技能配置来计算具体的属性加成
        // 暂时使用简单的示例实现
        switch (skillId)
        {
            case "strength_boost":
                _skillBonus.skillStrength += 5;
                break;
            case "agility_boost":
                _skillBonus.skillAgility += 5;
                break;
            case "intelligence_boost":
                _skillBonus.skillIntelligence += 5;
                break;
            case "vitality_boost":
                _skillBonus.skillVitality += 5;
                break;
            case "luck_boost":
                _skillBonus.skillLuck += 5;
                break;
        }
    }
    #endregion

    #region 事件处理
    /// <summary>
    /// 玩家升级事件处理
    /// </summary>
    private void OnPlayerLevelUp()
    {
        LogDebug("处理玩家升级事件");
        
        // 玩家升级时可能获得属性点
        _needsUpdate = true;
    }

    /// <summary>
    /// 职业改变事件处理
    /// </summary>
    private void OnProfessionChanged(ProfessionChangedEventArgs args)
    {
        LogDebug("处理职业改变事件");
        
        // 职业改变时重新计算属性
        _needsUpdate = true;
    }

    /// <summary>
    /// 技能学习事件处理
    /// </summary>
    private void OnSkillLearned(SkillLearnedEventArgs args)
    {
        LogDebug("处理技能学习事件");
        
        // 学习技能时可能获得属性加成
        _needsUpdate = true;
    }

    /// <summary>
    /// 装备穿戴事件处理
    /// </summary>
    private void OnItemEquipped()
    {
        LogDebug("处理装备穿戴事件");
        
        // 装备穿戴时重新计算属性
        _needsUpdate = true;
    }

    /// <summary>
    /// 装备卸下事件处理
    /// </summary>
    private void OnItemUnequipped()
    {
        LogDebug("处理装备卸下事件");
        
        // 装备卸下时重新计算属性
        _needsUpdate = true;
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
            Debug.Log($"[AttributeManager] {message}");
        }
    }

    /// <summary>
    /// 输出警告日志
    /// </summary>
    /// <param name="message">警告信息</param>
    private void LogWarning(string message)
    {
        Debug.LogWarning($"[AttributeManager] {message}");
    }

    /// <summary>
    /// 输出错误日志
    /// </summary>
    /// <param name="message">错误信息</param>
    private void LogError(string message)
    {
        Debug.LogError($"[AttributeManager] {message}");
    }
    #endregion
}

/// <summary>
/// 属性修改器类
/// </summary>
[System.Serializable]
public class AttributeModifier
{
    public int strengthBonus = 0;
    public int agilityBonus = 0;
    public int intelligenceBonus = 0;
    public int vitalityBonus = 0;
    public int luckBonus = 0;
    public float duration = -1f;
    public float remainingTime = 0f;
    public bool isTemporary = false;
    public bool isActive = true;
    public string source = "";
}
