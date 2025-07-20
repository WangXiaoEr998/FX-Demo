using UnityEngine;
using System;
using FanXing.Data;

/// <summary>
/// 玩家属性类，包含玩家的所有属性数据和计算
/// 作者：黄畅修
/// 创建时间：2025-07-12
/// </summary>
[Serializable]
public class PlayerAttributes
{
    #region 基础属性

    [Header("基础属性")] public int baseStrength = 10; // 基础力量
    public int baseAgility = 10; // 基础敏捷
    public int baseIntelligence = 10; // 基础智力
    public int baseVitality = 10; // 基础体质
    public int baseLuck = 10; // 基础幸运

    #endregion

    #region 装备加成属性

    [Header("装备加成")] public int equipmentStrength = 0; // 装备力量加成
    public int equipmentAgility = 0; // 装备敏捷加成
    public int equipmentIntelligence = 0; // 装备智力加成
    public int equipmentVitality = 0; // 装备体质加成
    public int equipmentLuck = 0; // 装备幸运加成

    #endregion

    #region 技能加成属性

    [Header("技能加成")] public int skillStrength = 0; // 技能力量加成
    public int skillAgility = 0; // 技能敏捷加成
    public int skillIntelligence = 0; // 技能智力加成
    public int skillVitality = 0; // 技能体质加成
    public int skillLuck = 0; // 技能幸运加成

    #endregion

    #region 临时加成属性

    [Header("临时加成")] public int temporaryStrength = 0; // 临时力量加成
    public int temporaryAgility = 0; // 临时敏捷加成
    public int temporaryIntelligence = 0; // 临时智力加成
    public int temporaryVitality = 0; // 临时体质加成
    public int temporaryLuck = 0; // 临时幸运加成

    #endregion

    #region 计算属性

    /// <summary>
    /// 总力量值
    /// </summary>
    public int TotalStrength => baseStrength + equipmentStrength + skillStrength + temporaryStrength;

    /// <summary>
    /// 总敏捷值
    /// </summary>
    public int TotalAgility => baseAgility + equipmentAgility + skillAgility + temporaryAgility;

    /// <summary>
    /// 总智力值
    /// </summary>
    public int TotalIntelligence =>
        baseIntelligence + equipmentIntelligence + skillIntelligence + temporaryIntelligence;

    /// <summary>
    /// 总体质值
    /// </summary>
    public int TotalVitality => baseVitality + equipmentVitality + skillVitality + temporaryVitality;

    /// <summary>
    /// 总幸运值
    /// </summary>
    public int TotalLuck => baseLuck + equipmentLuck + skillLuck + temporaryLuck;

    /// <summary>
    /// 最大生命值（基于体质）
    /// </summary>
    public int MaxHealth => 100 + TotalVitality * 10;

    /// <summary>
    /// 最大法力值（基于智力）
    /// </summary>
    public int MaxMana => 50 + TotalIntelligence * 5;

    /// <summary>
    /// 物理攻击力（基于力量）
    /// </summary>
    public int PhysicalAttack => 10 + TotalStrength * 2;

    /// <summary>
    /// 魔法攻击力（基于智力）
    /// </summary>
    public int MagicalAttack => 5 + TotalIntelligence * 3;

    /// <summary>
    /// 物理防御力（基于体质）
    /// </summary>
    public int PhysicalDefense => 5 + TotalVitality * 1;

    /// <summary>
    /// 魔法防御力（基于智力和体质）
    /// </summary>
    public int MagicalDefense => 3 + (TotalIntelligence + TotalVitality) / 2;

    /// <summary>
    /// 移动速度（基于敏捷）
    /// </summary>
    public float MoveSpeed => 5.0f + TotalAgility * 0.1f;

    /// <summary>
    /// 攻击速度（基于敏捷）
    /// </summary>
    public float AttackSpeed => 1.0f + TotalAgility * 0.02f;

    /// <summary>
    /// 暴击率（基于敏捷和幸运）
    /// </summary>
    public float CriticalRate => Mathf.Clamp01((TotalAgility + TotalLuck) * 0.001f);

    /// <summary>
    /// 暴击伤害倍率（基于力量和幸运）
    /// </summary>
    public float CriticalDamage => 1.5f + (TotalStrength + TotalLuck) * 0.01f;

    /// <summary>
    /// 闪避率（基于敏捷）
    /// </summary>
    public float DodgeRate => Mathf.Clamp01(TotalAgility * 0.002f);

    /// <summary>
    /// 格挡率（基于力量和体质）
    /// </summary>
    public float BlockRate => Mathf.Clamp01((TotalStrength + TotalVitality) * 0.001f);

    /// <summary>
    /// 经验获取倍率（基于幸运）
    /// </summary>
    public float ExperienceMultiplier => 1.0f + TotalLuck * 0.01f;

    /// <summary>
    /// 金币获取倍率（基于幸运）
    /// </summary>
    public float GoldMultiplier => 1.0f + TotalLuck * 0.015f;

    /// <summary>
    /// 掉落率加成（基于幸运）
    /// </summary>
    public float DropRateBonus => TotalLuck * 0.02f;

    /// <summary>
    /// 种田效率（基于智力和幸运）
    /// </summary>
    public float FarmingEfficiency => 1.0f + (TotalIntelligence + TotalLuck) * 0.01f;

    /// <summary>
    /// 交易效率（基于智力和幸运）
    /// </summary>
    public float TradingEfficiency => 1.0f + (TotalIntelligence + TotalLuck) * 0.012f;

    #endregion

    #region 构造函数

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public PlayerAttributes()
    {
        Initialize();
    }

    /// <summary>
    /// 带参数构造函数
    /// </summary>
    /// <param name="str">力量</param>
    /// <param name="agi">敏捷</param>
    /// <param name="intel">智力</param>
    /// <param name="vit">体质</param>
    /// <param name="luck">幸运</param>
    public PlayerAttributes(int str, int agi, int intel, int vit, int luck)
    {
        baseStrength = str;
        baseAgility = agi;
        baseIntelligence = intel;
        baseVitality = vit;
        baseLuck = luck;

        Initialize();
    }

    #endregion

    #region 初始化方法

    /// <summary>
    /// 初始化属性
    /// </summary>
    public void Initialize()
    {
        // 重置所有加成属性
        ResetBonusAttributes();
    }

    /// <summary>
    /// 重置加成属性
    /// </summary>
    public void ResetBonusAttributes()
    {
        equipmentStrength = 0;
        equipmentAgility = 0;
        equipmentIntelligence = 0;
        equipmentVitality = 0;
        equipmentLuck = 0;

        skillStrength = 0;
        skillAgility = 0;
        skillIntelligence = 0;
        skillVitality = 0;
        skillLuck = 0;

        temporaryStrength = 0;
        temporaryAgility = 0;
        temporaryIntelligence = 0;
        temporaryVitality = 0;
        temporaryLuck = 0;
    }

    #endregion

    #region 更新方法

    /// <summary>
    /// 从玩家数据更新基础属性
    /// </summary>
    /// <param name="playerData">玩家数据</param>
    public void UpdateFromPlayerData(PlayerData playerData)
    {
        if (playerData == null)
            return;

        baseStrength = playerData.strength;
        baseAgility = playerData.agility;
        baseIntelligence = playerData.intelligence;
        baseVitality = playerData.vitality;
        baseLuck = playerData.luck;
    }

    /// <summary>
    /// 更新装备加成
    /// </summary>
    /// <param name="equipment">装备列表</param>
    public void UpdateEquipmentBonus(System.Collections.Generic.List<ItemData> equipment)
    {
        // 重置装备加成
        equipmentStrength = 0;
        equipmentAgility = 0;
        equipmentIntelligence = 0;
        equipmentVitality = 0;
        equipmentLuck = 0;

        if (equipment == null)
            return;

        // 计算装备加成
        foreach (var item in equipment)
        {
            if (item != null && item.itemType == ItemType.Equipment)
            {
                equipmentStrength += item.strengthBonus;
                equipmentAgility += item.agilityBonus;
                equipmentIntelligence += item.intelligenceBonus;
                equipmentVitality += item.vitalityBonus;
                equipmentLuck += item.luckBonus;
            }
        }
    }

    /// <summary>
    /// 更新技能加成
    /// </summary>
    /// <param name="skills">技能列表</param>
    public void UpdateSkillBonus(System.Collections.Generic.List<string> skills)
    {
        // 重置技能加成
        skillStrength = 0;
        skillAgility = 0;
        skillIntelligence = 0;
        skillVitality = 0;
        skillLuck = 0;

        if (skills == null)
            return;

        // 这里应该根据具体的技能配置来计算加成
        // 暂时使用简单的计算方式
        foreach (var skillId in skills)
        {
            // 根据技能ID计算属性加成
            // 实际实现中应该从技能配置中读取
            CalculateSkillBonus(skillId);
        }
    }

    /// <summary>
    /// 添加临时属性加成
    /// </summary>
    /// <param name="str">力量加成</param>
    /// <param name="agi">敏捷加成</param>
    /// <param name="intel">智力加成</param>
    /// <param name="vit">体质加成</param>
    /// <param name="luck">幸运加成</param>
    public void AddTemporaryBonus(int str, int agi, int intel, int vit, int luck)
    {
        temporaryStrength += str;
        temporaryAgility += agi;
        temporaryIntelligence += intel;
        temporaryVitality += vit;
        temporaryLuck += luck;
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
        temporaryStrength -= str;
        temporaryAgility -= agi;
        temporaryIntelligence -= intel;
        temporaryVitality -= vit;
        temporaryLuck -= luck;
    }

    /// <summary>
    /// 清除所有临时加成
    /// </summary>
    public void ClearTemporaryBonus()
    {
        temporaryStrength = 0;
        temporaryAgility = 0;
        temporaryIntelligence = 0;
        temporaryVitality = 0;
        temporaryLuck = 0;
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 计算技能属性加成
    /// </summary>
    /// <param name="skillId">技能ID</param>
    private void CalculateSkillBonus(string skillId)
    {
        // 这里应该根据技能配置来计算具体的属性加成
        // 暂时使用简单的示例实现
        switch (skillId)
        {
            case "strength_boost":
                skillStrength += 5;
                break;
            case "agility_boost":
                skillAgility += 5;
                break;
            case "intelligence_boost":
                skillIntelligence += 5;
                break;
            case "vitality_boost":
                skillVitality += 5;
                break;
            case "luck_boost":
                skillLuck += 5;
                break;
        }
    }

    #endregion

    #region 工具方法

    /// <summary>
    /// 获取属性总和
    /// </summary>
    /// <returns>所有属性的总和</returns>
    public int GetTotalAttributePoints()
    {
        return TotalStrength + TotalAgility + TotalIntelligence + TotalVitality + TotalLuck;
    }

    /// <summary>
    /// 获取战斗力评估
    /// </summary>
    /// <returns>战斗力数值</returns>
    public int GetCombatPower()
    {
        return PhysicalAttack + MagicalAttack + PhysicalDefense + MagicalDefense +
               Mathf.RoundToInt(MoveSpeed * 10) + Mathf.RoundToInt(AttackSpeed * 50);
    }

    /// <summary>
    /// 克隆属性对象
    /// </summary>
    /// <returns>克隆的属性对象</returns>
    public PlayerAttributes Clone()
    {
        string json = JsonUtility.ToJson(this);
        return JsonUtility.FromJson<PlayerAttributes>(json);
    }

    /// <summary>
    /// 转换为字符串
    /// </summary>
    /// <returns>属性信息字符串</returns>
    public override string ToString()
    {
        return $"STR:{TotalStrength} AGI:{TotalAgility} INT:{TotalIntelligence} VIT:{TotalVitality} LUK:{TotalLuck}";
    }

    /// <summary>
    /// 复制属性方法
    /// </summary>
    /// <param name="source"></param>
    public void CopyFrom(PlayerAttributes source)
    {
        baseAgility = source.baseAgility;
        baseIntelligence = source.baseIntelligence;
        baseVitality = source.baseVitality;
        baseLuck = source.baseLuck;
        baseStrength = source.baseStrength;
    }

    #endregion
}