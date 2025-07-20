using UnityEngine;
using System.Collections.Generic;
using FanXing.Data;

/// <summary>
/// 战斗系统，负责战斗逻辑、伤害计算、技能释放等功能
/// 作者：黄畅修
/// 创建时间：2025-07-13
/// </summary>
public class CombatSystem : BaseGameSystem
{
    #region 字段定义
    [Header("战斗系统配置")]
    #pragma warning disable CS0414 // 字段已赋值但从未使用 - Demo版本中暂未实现相关功能
    [SerializeField] private float _combatRange = 5.0f;
    #pragma warning restore CS0414
    [SerializeField] private float _combatUpdateInterval = 0.1f;
    [SerializeField] private int _maxCombatants = 20;
    
    [Header("伤害配置")]
    [SerializeField] private float _baseDamageMultiplier = 1.0f;
    [SerializeField] private float _criticalChance = 0.1f;
    [SerializeField] private float _criticalMultiplier = 2.0f;
    
    // 战斗管理
    private List<GameObject> _activeCombatants = new List<GameObject>();
    private Dictionary<GameObject, CombatData> _combatDataDictionary = new Dictionary<GameObject, CombatData>();
    private List<CombatAction> _pendingActions = new List<CombatAction>();
    
    // 更新计时器
    private float _updateTimer = 0f;
    
    // 战斗状态
    private bool _isInCombat = false;
    private GameObject _currentTarget;
    #endregion

    #region 内部类
    /// <summary>
    /// 战斗数据
    /// </summary>
    [System.Serializable]
    public class CombatData
    {
        public GameObject combatant;
        public float currentHealth;
        public float maxHealth;
        public float attackPower;
        public float defense;
        public float attackSpeed;
        public float lastAttackTime;
        public bool isAlive;
        public GameObject currentTarget;
        
        public CombatData(GameObject obj, float health, float attack, float def, float speed)
        {
            combatant = obj;
            currentHealth = health;
            maxHealth = health;
            attackPower = attack;
            defense = def;
            attackSpeed = speed;
            lastAttackTime = 0f;
            isAlive = true;
            currentTarget = null;
        }
    }

    /// <summary>
    /// 战斗行动
    /// </summary>
    [System.Serializable]
    public class CombatAction
    {
        public GameObject attacker;
        public GameObject target;
        public SkillType skillType;
        public float damage;
        public bool isCritical;
        public float executeTime;
        
        public CombatAction(GameObject att, GameObject tar, SkillType skill, float dmg, bool crit)
        {
            attacker = att;
            target = tar;
            skillType = skill;
            damage = dmg;
            isCritical = crit;
            executeTime = Time.time;
        }
    }
    #endregion

    #region 属性
    /// <summary>
    /// 系统名称
    /// </summary>
    public override string SystemName => "战斗系统";
    
    /// <summary>
    /// 当前战斗者数量
    /// </summary>
    public int ActiveCombatantCount => _activeCombatants.Count;
    
    /// <summary>
    /// 是否正在战斗中
    /// </summary>
    public bool IsInCombat => _isInCombat;
    
    /// <summary>
    /// 当前目标
    /// </summary>
    public GameObject CurrentTarget => _currentTarget;
    #endregion

    #region BaseGameSystem抽象方法实现
    /// <summary>
    /// 系统初始化时调用
    /// </summary>
    protected override void OnInitialize()
    {
        LogDebug("战斗系统初始化开始");

        // 注册事件监听器
        RegisterEventListeners();

        LogDebug("战斗系统初始化完成");
    }

    /// <summary>
    /// 系统启动时调用
    /// </summary>
    protected override void OnStart()
    {
        LogDebug("战斗系统启动");

        // 开始更新计时器
        _updateTimer = 0f;

        LogDebug("战斗系统启动完成");
    }

    /// <summary>
    /// 系统更新时调用
    /// </summary>
    /// <param name="deltaTime">帧时间</param>
    protected override void OnUpdate(float deltaTime)
    {
        if (_isPaused)
            return;

        // 更新计时器
        _updateTimer += deltaTime;

        // 定期更新战斗逻辑
        if (_updateTimer >= _combatUpdateInterval)
        {
            UpdateCombat();
            ProcessPendingActions();
            _updateTimer = 0f;
        }
    }
    #endregion

    #region 私有方法
    /// <summary>
    /// 更新战斗逻辑
    /// </summary>
    private void UpdateCombat()
    {
        // 清理无效的战斗者
        for (int i = _activeCombatants.Count - 1; i >= 0; i--)
        {
            if (_activeCombatants[i] == null)
            {
                _activeCombatants.RemoveAt(i);
                continue;
            }

            UpdateCombatantBehavior(_activeCombatants[i]);
        }

        // 检查战斗状态
        CheckCombatState();
    }

    /// <summary>
    /// 更新战斗者行为
    /// </summary>
    /// <param name="combatant">战斗者</param>
    private void UpdateCombatantBehavior(GameObject combatant)
    {
        if (!_combatDataDictionary.TryGetValue(combatant, out CombatData data))
            return;

        if (!data.isAlive)
            return;

        // 检查是否可以攻击
        if (data.currentTarget != null && CanAttack(data))
        {
            PerformAttack(data);
        }
    }

    /// <summary>
    /// 检查是否可以攻击
    /// </summary>
    /// <param name="data">战斗数据</param>
    /// <returns>是否可以攻击</returns>
    private bool CanAttack(CombatData data)
    {
        float timeSinceLastAttack = Time.time - data.lastAttackTime;
        float attackCooldown = 1.0f / data.attackSpeed;
        
        return timeSinceLastAttack >= attackCooldown;
    }

    /// <summary>
    /// 执行攻击
    /// </summary>
    /// <param name="attackerData">攻击者数据</param>
    private void PerformAttack(CombatData attackerData)
    {
        if (attackerData.currentTarget == null)
            return;

        // 计算伤害
        float damage = CalculateDamage(attackerData, attackerData.currentTarget);
        bool isCritical = Random.value < _criticalChance;
        
        if (isCritical)
        {
            damage *= _criticalMultiplier;
        }

        // 创建战斗行动
        var action = new CombatAction(attackerData.combatant, attackerData.currentTarget, SkillType.BasicAttack, damage, isCritical);
        _pendingActions.Add(action);

        attackerData.lastAttackTime = Time.time;
    }

    /// <summary>
    /// 计算伤害
    /// </summary>
    /// <param name="attackerData">攻击者数据</param>
    /// <param name="target">目标</param>
    /// <returns>伤害值</returns>
    private float CalculateDamage(CombatData attackerData, GameObject target)
    {
        float baseDamage = attackerData.attackPower * _baseDamageMultiplier;
        
        if (_combatDataDictionary.TryGetValue(target, out CombatData targetData))
        {
            // 考虑防御力
            float finalDamage = Mathf.Max(1, baseDamage - targetData.defense);
            return finalDamage;
        }

        return baseDamage;
    }

    /// <summary>
    /// 处理待执行的行动
    /// </summary>
    private void ProcessPendingActions()
    {
        for (int i = _pendingActions.Count - 1; i >= 0; i--)
        {
            var action = _pendingActions[i];
            ExecuteAction(action);
            _pendingActions.RemoveAt(i);
        }
    }

    /// <summary>
    /// 执行战斗行动
    /// </summary>
    /// <param name="action">战斗行动</param>
    private void ExecuteAction(CombatAction action)
    {
        if (action.target == null)
            return;

        // 应用伤害
        ApplyDamage(action.target, action.damage);

        if (_enableDebugMode)
        {
            string critText = action.isCritical ? " (暴击)" : "";
            Debug.Log($"{action.attacker.name} 对 {action.target.name} 造成 {action.damage:F1} 伤害{critText}");
        }
    }

    /// <summary>
    /// 应用伤害
    /// </summary>
    /// <param name="target">目标</param>
    /// <param name="damage">伤害值</param>
    private void ApplyDamage(GameObject target, float damage)
    {
        if (!_combatDataDictionary.TryGetValue(target, out CombatData data))
            return;

        data.currentHealth -= damage;
        
        if (data.currentHealth <= 0)
        {
            data.currentHealth = 0;
            data.isAlive = false;
            
            if (_enableDebugMode)
            {
                Debug.Log($"{target.name} 被击败");
            }
        }
    }

    /// <summary>
    /// 检查战斗状态
    /// </summary>
    private void CheckCombatState()
    {
        bool hasAliveCombatants = false;
        
        foreach (var data in _combatDataDictionary.Values)
        {
            if (data.isAlive)
            {
                hasAliveCombatants = true;
                break;
            }
        }

        if (!hasAliveCombatants && _isInCombat)
        {
            EndCombat();
        }
    }

    /// <summary>
    /// 更新战斗物理
    /// </summary>
    /// <param name="fixedDeltaTime">固定时间增量</param>
    private void UpdateCombatPhysics(float fixedDeltaTime)
    {
        // TODO: 实现战斗物理更新
        // 例如：弹道计算、碰撞检测等
    }

    /// <summary>
    /// 注册事件监听
    /// </summary>
    private void RegisterEventListeners()
    {
        // TODO: 注册相关事件监听
    }

    /// <summary>
    /// 注销事件监听
    /// </summary>
    private void UnregisterEventListeners()
    {
        // TODO: 注销相关事件监听
    }

    /// <summary>
    /// 清理战斗数据
    /// </summary>
    private void ClearCombatData()
    {
        _activeCombatants.Clear();
        _combatDataDictionary.Clear();
        _pendingActions.Clear();
        _isInCombat = false;
        _currentTarget = null;
    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 添加战斗者
    /// </summary>
    /// <param name="combatant">战斗者对象</param>
    /// <param name="health">生命值</param>
    /// <param name="attack">攻击力</param>
    /// <param name="defense">防御力</param>
    /// <param name="attackSpeed">攻击速度</param>
    public void AddCombatant(GameObject combatant, float health, float attack, float defense, float attackSpeed)
    {
        if (_activeCombatants.Count >= _maxCombatants)
        {
            Debug.LogWarning("已达到最大战斗者数量限制");
            return;
        }

        if (_combatDataDictionary.ContainsKey(combatant))
        {
            Debug.LogWarning($"战斗者 {combatant.name} 已经在战斗中");
            return;
        }

        var combatData = new CombatData(combatant, health, attack, defense, attackSpeed);
        _activeCombatants.Add(combatant);
        _combatDataDictionary[combatant] = combatData;

        if (_enableDebugMode)
        {
            Debug.Log($"添加战斗者: {combatant.name}");
        }
    }

    /// <summary>
    /// 移除战斗者
    /// </summary>
    /// <param name="combatant">战斗者对象</param>
    public void RemoveCombatant(GameObject combatant)
    {
        if (_activeCombatants.Remove(combatant))
        {
            _combatDataDictionary.Remove(combatant);
            
            if (_enableDebugMode)
            {
                Debug.Log($"移除战斗者: {combatant.name}");
            }
        }
    }

    /// <summary>
    /// 开始战斗
    /// </summary>
    public void StartCombat()
    {
        if (_isInCombat)
        {
            Debug.LogWarning("已经在战斗中");
            return;
        }

        _isInCombat = true;
        
        if (_enableDebugMode)
        {
            Debug.Log("战斗开始");
        }
    }

    /// <summary>
    /// 结束战斗
    /// </summary>
    public void EndCombat()
    {
        if (!_isInCombat)
            return;

        _isInCombat = false;
        _currentTarget = null;
        
        if (_enableDebugMode)
        {
            Debug.Log("战斗结束");
        }
    }

    /// <summary>
    /// 设置攻击目标
    /// </summary>
    /// <param name="attacker">攻击者</param>
    /// <param name="target">目标</param>
    public void SetTarget(GameObject attacker, GameObject target)
    {
        if (_combatDataDictionary.TryGetValue(attacker, out CombatData data))
        {
            data.currentTarget = target;
            _currentTarget = target;
        }
    }

    /// <summary>
    /// 获取战斗数据
    /// </summary>
    /// <param name="combatant">战斗者</param>
    /// <returns>战斗数据</returns>
    public CombatData GetCombatData(GameObject combatant)
    {
        _combatDataDictionary.TryGetValue(combatant, out CombatData data);
        return data;
    }
    #endregion
}
