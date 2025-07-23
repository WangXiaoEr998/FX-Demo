using UnityEngine;
using System.Collections.Generic;
using FanXing.Data;
/// <summary>
/// 玩家系统，负责玩家角色管理、双职业系统、属性和技能管理
/// 作者：黄畅修,容泳森
/// 创建时间：2025-07-23
/// </summary>
public class PlayerSystem : BaseGameSystem
{
    #region 字段定义
    [Header("玩家配置")]
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform _playerSpawnPoint;
    [SerializeField] private PlayerConfig _playerConfig;

    [Header("职业配置")]
    [SerializeField] private ProfessionConfig _merchantConfig;
    [SerializeField] private ProfessionConfig _cultivatorConfig;

    [Header("技能配置")]
    [SerializeField] private SkillConfig _skillConfig;

    // 玩家实例
    private Player _currentPlayer;
    private PlayerData _playerData;

    // 职业管理
    private ProfessionManager _professionManager;
    private SkillManager _skillManager;
    private AttributeManager _attributeManager;

    // 状态管理
    private PlayerState _currentState = PlayerState.Idle;
    private Vector3 _lastPosition;
    private float _stateTimer = 0f;
    #endregion

    #region 属性
    /// <summary>
    /// 当前玩家实例
    /// </summary>
    public Player CurrentPlayer => _currentPlayer;

    /// <summary>
    /// 玩家数据
    /// </summary>
    public PlayerData PlayerData => _playerData;

    /// <summary>
    /// 职业管理器
    /// </summary>
    public ProfessionManager ProfessionManager => _professionManager;

    /// <summary>
    /// 技能管理器
    /// </summary>
    public SkillManager SkillManager => _skillManager;

    /// <summary>
    /// 属性管理器
    /// </summary>
    public AttributeManager AttributeManager => _attributeManager;

    /// <summary>
    /// 当前玩家状态
    /// </summary>
    public PlayerState CurrentState => _currentState;

    /// <summary>
    /// 玩家是否存活
    /// </summary>
    public bool IsPlayerAlive => _currentPlayer != null && _playerData != null && _playerData.currentHealth > 0;

    /// <summary>
    /// 玩家当前位置
    /// </summary>
    public Vector3 PlayerPosition => _currentPlayer != null ? _currentPlayer.transform.position : Vector3.zero;

    /// <summary>
    /// 玩家当前等级
    /// </summary>
    public int PlayerLevel => _playerData?.level ?? 1;

    /// <summary>
    /// 玩家当前经验
    /// </summary>
    public int PlayerExperience => _playerData?.experience ?? 0;

    /// <summary>
    /// 玩家当前金币
    /// </summary>
    public int PlayerGold => _playerData?.gold ?? 0;
    #endregion

    #region 系统生命周期
    protected override void OnInitialize()
    {
        LogDebug("初始化玩家系统");

        // 初始化管理器
        InitializeManagers();

        // 加载配置
        LoadConfigurations();

        // 注册事件
        RegisterSystemEvents();

        LogDebug("玩家系统初始化完成");
    }

    protected override void OnStart()
    {
        LogDebug("启动玩家系统");

        // 加载玩家数据
        LoadPlayerData();

        // 创建玩家实例
        CreatePlayerInstance();

        // 初始化玩家状态
        InitializePlayerState();

        LogDebug("玩家系统启动完成");
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (_currentPlayer == null || _playerData == null)
            return;

        // 更新状态计时器
        _stateTimer += deltaTime;

        // 更新玩家状态
        UpdatePlayerState(deltaTime);

        // 更新管理器
        UpdateManagers(deltaTime);

        // 检查玩家状态变化
        CheckPlayerStateChanges();

        // 更新位置记录
        UpdatePositionTracking();
    }

    protected override void OnFixedUpdate(float fixedDeltaTime)
    {
        if (_currentPlayer == null)
            return;

        // 固定更新物理相关逻辑
        UpdatePhysics(fixedDeltaTime);
    }

    protected override void OnShutdown()
    {
        LogDebug("关闭玩家系统");

        // 保存玩家数据
        SavePlayerData();

        // 注销事件
        UnregisterSystemEvents();

        // 清理资源
        CleanupResources();

        LogDebug("玩家系统关闭完成");
    }

    protected override void OnPause()
    {
        LogDebug("暂停玩家系统");

        if (_currentPlayer != null)
        {
            _currentPlayer.SetPaused(true);
        }
    }

    protected override void OnResume()
    {
        LogDebug("恢复玩家系统");

        if (_currentPlayer != null)
        {
            _currentPlayer.SetPaused(false);
        }
    }
    #endregion

    #region 公共方法 - 玩家管理
    /// <summary>
    /// 创建新玩家
    /// </summary>
    /// <param name="playerName">玩家名称</param>
    /// <param name="initialProfession">初始职业</param>
    /// <returns>是否创建成功</returns>
    public bool CreateNewPlayer(string playerName, ProfessionType initialProfession)
    {
        if (string.IsNullOrEmpty(playerName))
        {
            LogError("玩家名称不能为空");
            return false;
        }

        LogDebug($"创建新玩家: {playerName}, 职业: {initialProfession}");

        try
        {
            // 创建新的玩家数据
            _playerData = new PlayerData();
            _playerData.Initialize(playerName, initialProfession);

            // 创建玩家实例
            CreatePlayerInstance();

            // 设置初始职业
            _professionManager.SetProfession(initialProfession);

            // 触发玩家创建事件
            Global.Event.TriggerEvent(Global.Events.Player.CREATED, new PlayerCreatedEventArgs(_playerData));

            LogDebug("新玩家创建成功");
            return true;
        }
        catch (System.Exception ex)
        {
            LogError($"创建新玩家失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 加载玩家
    /// </summary>
    /// <param name="playerData">玩家数据</param>
    /// <returns>是否加载成功</returns>
    public bool LoadPlayer(PlayerData playerData)
    {
        if (playerData == null)
        {
            LogError("玩家数据不能为空");
            return false;
        }

        LogDebug($"加载玩家: {playerData.playerName}");

        try
        {
            _playerData = playerData;

            // 创建玩家实例
            CreatePlayerInstance();

            // 恢复玩家状态
            RestorePlayerState();

            // 触发玩家加载事件
            Global.Event.TriggerEvent(Global.Events.Player.LOADED, new PlayerLoadedEventArgs(_playerData));

            LogDebug("玩家加载成功");
            return true;
        }
        catch (System.Exception ex)
        {
            LogError($"加载玩家失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 保存玩家数据
    /// </summary>
    public void SavePlayerData()
    {
        if (_playerData == null)
        {
            LogWarning("没有玩家数据需要保存");
            return;
        }

        LogDebug("保存玩家数据");

        try
        {
            // 更新当前状态到数据
            UpdatePlayerDataFromCurrentState();

            // 通过Global统一入口保存
            Global.Data?.UpdatePlayerData(_playerData);
            Global.Data?.SaveGameData();

            // 触发保存事件
            Global.Event.TriggerEvent(Global.Events.Data.SAVED, new PlayerDataSavedEventArgs(_playerData));

            LogDebug("玩家数据保存成功");
        }
        catch (System.Exception ex)
        {
            LogError($"保存玩家数据失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 重生玩家
    /// </summary>
    public void RespawnPlayer()
    {
        if (_playerData == null)
        {
            LogError("没有玩家数据，无法重生");
            return;
        }

        LogDebug("重生玩家");

        try
        {
            // 重置生命值
            _playerData.currentHealth = _playerData.maxHealth;

            // 重置位置
            if (_playerSpawnPoint != null)
            {
                _playerData.position = _playerSpawnPoint.position;
                if (_currentPlayer != null)
                {
                    _currentPlayer.transform.position = _playerSpawnPoint.position;
                }
            }

            // 重置状态
            ChangePlayerState(PlayerState.Idle);

            // 触发重生事件
            Global.Event.TriggerEvent(Global.Events.Player.RESPAWN, new PlayerRespawnedEventArgs(_playerData));

            LogDebug("玩家重生成功");
        }
        catch (System.Exception ex)
        {
            LogError($"玩家重生失败: {ex.Message}");
        }
    }
    #endregion

    #region 公共方法 - 属性管理
    /// <summary>
    /// 增加经验值
    /// </summary>
    /// <param name="amount">经验值数量</param>
    public void AddExperience(int amount)
    {
        if (_playerData == null || amount <= 0)
            return;

        LogDebug($"增加经验值: {amount}");

        int oldLevel = _playerData.level;
        _playerData.experience += amount;

        // 检查升级
        CheckLevelUp(oldLevel);

        // 触发经验值变化事件
        Global.Event.TriggerEvent(Global.Events.Player.EXPERIENCE_GAINED, new ExperienceChangedEventArgs(amount, _playerData.experience));
    }

    /// <summary>
    /// 增加金币
    /// </summary>
    /// <param name="amount">金币数量</param>
    public void AddGold(int amount)
    {
        if (_playerData == null || amount <= 0)
            return;

        LogDebug($"增加金币: {amount}");

        _playerData.gold += amount;

        // 触发金币变化事件
        Global.Event.TriggerEvent(Global.Events.Economy.GOLD_GAINED, new GoldChangedEventArgs(amount, _playerData.gold));
    }

    /// <summary>
    /// 消费金币
    /// </summary>
    /// <param name="amount">金币数量</param>
    /// <returns>是否消费成功</returns>
    public bool SpendGold(int amount)
    {
        if (_playerData == null || amount <= 0)
            return false;

        if (_playerData.gold < amount)
        {
            LogWarning($"金币不足，需要: {amount}, 拥有: {_playerData.gold}");
            return false;
        }

        LogDebug($"消费金币: {amount}");

        _playerData.gold -= amount;

        // 触发金币变化事件
        Global.Event.TriggerEvent(Global.Events.Economy.GOLD_SPENT, new GoldChangedEventArgs(-amount, _playerData.gold));

        return true;
    }

    /// <summary>
    /// 修改生命值
    /// </summary>
    /// <param name="amount">变化量（正数为治疗，负数为伤害）</param>
    public void ModifyHealth(int amount)
    {
        if (_playerData == null)
            return;

        int oldHealth = _playerData.currentHealth;
        _playerData.currentHealth = Mathf.Clamp(_playerData.currentHealth + amount, 0, _playerData.maxHealth);

        LogDebug($"生命值变化: {amount}, 当前: {_playerData.currentHealth}/{_playerData.maxHealth}");

        // 触发生命值变化事件
        Global.Event.TriggerEvent(Global.Events.Player.HEALTH_CHANGED, new HealthChangedEventArgs(amount, _playerData.currentHealth, _playerData.maxHealth));

        // 检查死亡
        if (oldHealth > 0 && _playerData.currentHealth <= 0)
        {
            HandlePlayerDeath();
        }
    }
    #endregion

    #region 公共方法 - 职业管理
    /// <summary>
    /// 切换职业
    /// </summary>
    /// <param name="profession">目标职业</param>
    /// <returns>是否切换成功</returns>
    public bool SwitchProfession(ProfessionType profession)
    {
        if (_professionManager == null)
        {
            LogError("职业管理器未初始化");
            return false;
        }

        LogDebug($"切换职业: {profession}");

        return _professionManager.SwitchProfession(profession);
    }

    /// <summary>
    /// 获取当前职业
    /// </summary>
    /// <returns>当前职业类型</returns>
    public ProfessionType GetCurrentProfession()
    {
        return _professionManager?.CurrentProfession ?? ProfessionType.None;
    }

    /// <summary>
    /// 获取职业等级
    /// </summary>
    /// <param name="profession">职业类型</param>
    /// <returns>职业等级</returns>
    public int GetProfessionLevel(ProfessionType profession)
    {
        return _professionManager?.GetProfessionLevel(profession) ?? 0;
    }

    /// <summary>
    /// 增加职业经验
    /// </summary>
    /// <param name="profession">职业类型</param>
    /// <param name="experience">经验值</param>
    public void AddProfessionExperience(ProfessionType profession, int experience)
    {
        _professionManager?.AddExperience(profession, experience);
    }
    #endregion

    #region 公共方法 - 技能管理
    /// <summary>
    /// 学习技能
    /// </summary>
    /// <param name="skillId">技能ID</param>
    /// <returns>是否学习成功</returns>
    public bool LearnSkill(string skillId)
    {
        if (_skillManager == null)
        {
            LogError("技能管理器未初始化");
            return false;
        }

        LogDebug($"学习技能: {skillId}");

        return _skillManager.LearnSkill(skillId);
    }

    /// <summary>
    /// 升级技能
    /// </summary>
    /// <param name="skillId">技能ID</param>
    /// <returns>是否升级成功</returns>
    public bool UpgradeSkill(string skillId)
    {
        if (_skillManager == null)
        {
            LogError("技能管理器未初始化");
            return false;
        }

        LogDebug($"升级技能: {skillId}");

        return _skillManager.UpgradeSkill(skillId);
    }

    /// <summary>
    /// 使用技能
    /// </summary>
    /// <param name="skillId">技能ID</param>
    /// <param name="target">目标对象</param>
    /// <returns>是否使用成功</returns>
    public bool UseSkill(string skillId, GameObject target = null)
    {
        if (_skillManager == null)
        {
            LogError("技能管理器未初始化");
            return false;
        }

        LogDebug($"使用技能: {skillId}");

        return _skillManager.UseSkill(skillId, target);
    }

    /// <summary>
    /// 获取技能信息
    /// </summary>
    /// <param name="skillId">技能ID</param>
    /// <returns>技能信息</returns>
    public SkillInfo GetSkillInfo(string skillId)
    {
        return _skillManager?.GetSkillInfo(skillId);
    }
    #endregion

    #region 私有方法 - 初始化
    /// <summary>
    /// 初始化管理器
    /// </summary>
    private void InitializeManagers()
    {
        LogDebug("初始化子管理器");

        // 创建职业管理器
        _professionManager = gameObject.AddComponent<ProfessionManager>();
        _professionManager.Initialize();

        // 创建技能管理器
        _skillManager = gameObject.AddComponent<SkillManager>();
        _skillManager.Initialize();

        // 创建属性管理器
        _attributeManager = gameObject.AddComponent<AttributeManager>();
        _attributeManager.Initialize();

        LogDebug("子管理器初始化完成");
    }

    /// <summary>
    /// 加载配置
    /// </summary>
    private void LoadConfigurations()
    {
        LogDebug("加载玩家系统配置");

        // 从Global统一入口加载配置
        if (_playerConfig == null)
        {
            _playerConfig = Global.Data?.GetConfig<PlayerConfig>("PlayerConfig");
        }

        if (_merchantConfig == null)
        {
            _merchantConfig = Global.Data?.GetConfig<ProfessionConfig>("MerchantConfig");
        }

        if (_cultivatorConfig == null)
        {
            _cultivatorConfig = Global.Data?.GetConfig<ProfessionConfig>("CultivatorConfig");
        }

        if (_skillConfig == null)
        {
            _skillConfig = Global.Data?.GetConfig<SkillConfig>("SkillConfig");
        }

        LogDebug("配置加载完成");
    }

    /// <summary>
    /// 注册系统事件
    /// </summary>
    private void RegisterSystemEvents()
    {
        LogDebug("注册玩家系统事件");

        RegisterEvent("OnGameStateChanged", OnGameStateChanged);
        RegisterEvent("OnPlayerDeath", OnPlayerDeath);
        RegisterEvent("OnPlayerLevelUp", OnPlayerLevelUp);
        RegisterEvent("OnProfessionChanged", OnProfessionChanged);
        RegisterEvent("OnSkillLearned", OnSkillLearned);

        LogDebug("事件注册完成");
    }

    /// <summary>
    /// 注销系统事件
    /// </summary>
    private void UnregisterSystemEvents()
    {
        LogDebug("注销玩家系统事件");

        UnregisterEvent("OnGameStateChanged", OnGameStateChanged);
        UnregisterEvent("OnPlayerDeath", OnPlayerDeath);
        UnregisterEvent("OnPlayerLevelUp", OnPlayerLevelUp);
        UnregisterEvent("OnProfessionChanged", OnProfessionChanged);
        UnregisterEvent("OnSkillLearned", OnSkillLearned);

        LogDebug("事件注销完成");
    }

    /// <summary>
    /// 创建玩家实例
    /// </summary>
    private void CreatePlayerInstance()
    {
        LogDebug("创建玩家实例");

        // 销毁现有实例
        if (_currentPlayer != null)
        {
            DestroyImmediate(_currentPlayer.gameObject);
        }

        // 创建新实例
        if (_playerPrefab != null)
        {
            Vector3 spawnPosition = _playerSpawnPoint != null ? _playerSpawnPoint.position : Vector3.zero;
            GameObject playerObject = Instantiate(_playerPrefab, spawnPosition, Quaternion.identity);
            _currentPlayer = playerObject.GetComponent<Player>();

            if (_currentPlayer == null)
            {
                _currentPlayer = playerObject.AddComponent<Player>();
            }

            // 初始化玩家组件
            _currentPlayer.Initialize(_playerData, this);

            LogDebug("玩家实例创建成功");
        }
        else
        {
            LogError("玩家预制体未设置");
        }
    }

    /// <summary>
    /// 加载玩家数据
    /// </summary>
    private void LoadPlayerData()
    {
        LogDebug("加载玩家数据");

        // 从Global统一入口获取玩家数据
        _playerData = Global.Data?.GetPlayerData();

        if (_playerData == null)
        {
            LogWarning("没有找到玩家数据，创建默认数据");
            _playerData = new PlayerData();
            _playerData.Initialize("DefaultPlayer", ProfessionType.Merchant);
        }

        LogDebug($"玩家数据加载完成: {_playerData.playerName}");
    }

    /// <summary>
    /// 初始化玩家状态
    /// </summary>
    private void InitializePlayerState()
    {
        LogDebug("初始化玩家状态");

        if (_playerData != null)
        {
            // 设置位置
            if (_currentPlayer != null)
            {
                _currentPlayer.transform.position = _playerData.position;
            }

            // 设置初始状态
            ChangePlayerState(PlayerState.Idle);

            // 更新位置记录
            _lastPosition = _playerData.position;
        }

        LogDebug("玩家状态初始化完成");
    }

    /// <summary>
    /// 恢复玩家状态
    /// </summary>
    private void RestorePlayerState()
    {
        LogDebug("恢复玩家状态");

        if (_playerData != null && _currentPlayer != null)
        {
            // 恢复位置
            _currentPlayer.transform.position = _playerData.position;

            // 恢复职业
            _professionManager.SetProfession(_playerData.currentProfession);

            // 恢复技能
            _skillManager.RestoreSkills(_playerData.learnedSkills);

            // 恢复属性
            _attributeManager.RestoreAttributes(_playerData.attributes);

            // 设置状态
            ChangePlayerState(PlayerState.Idle);
        }

        LogDebug("玩家状态恢复完成");
    }

    /// <summary>
    /// 更新玩家状态
    /// </summary>
    /// <param name="deltaTime">帧时间</param>
    private void UpdatePlayerState(float deltaTime)
    {
        // 根据当前状态执行相应逻辑
        switch (_currentState)
        {
            case PlayerState.Idle:
                HandleIdleState(deltaTime);
                break;
            case PlayerState.Moving:
                HandleMovingState(deltaTime);
                break;
            case PlayerState.Fighting:
                HandleFightingState(deltaTime);
                break;
            case PlayerState.Farming:
                HandleFarmingState(deltaTime);
                break;
            case PlayerState.Trading:
                HandleTradingState(deltaTime);
                break;
            case PlayerState.Talking:
                HandleTalkingState(deltaTime);
                break;
            case PlayerState.Dead:
                HandleDeadState(deltaTime);
                break;
            case PlayerState.Respawning:
                HandleRespawningState(deltaTime);
                break;
        }
    }

    /// <summary>
    /// 更新管理器
    /// </summary>
    /// <param name="deltaTime">帧时间</param>
    private void UpdateManagers(float deltaTime)
    {
        _professionManager?.UpdateManager(deltaTime);
        _skillManager?.UpdateManager(deltaTime);
        _attributeManager?.UpdateManager(deltaTime);
    }

    /// <summary>
    /// 检查玩家状态变化
    /// </summary>
    private void CheckPlayerStateChanges()
    {
        if (_currentPlayer == null || _playerData == null)
            return;

        // 检查位置变化
        Vector3 currentPosition = _currentPlayer.transform.position;
        if (Vector3.Distance(currentPosition, _lastPosition) > 0.1f)
        {
            if (_currentState == PlayerState.Idle)
            {
                ChangePlayerState(PlayerState.Moving);
            }
            _lastPosition = currentPosition;
            _playerData.position = currentPosition;
        }
        else if (_currentState == PlayerState.Moving)
        {
            ChangePlayerState(PlayerState.Idle);
        }
    }

    /// <summary>
    /// 更新位置记录
    /// </summary>
    private void UpdatePositionTracking()
    {
        if (_currentPlayer != null && _playerData != null)
        {
            _playerData.position = _currentPlayer.transform.position;
            _playerData.rotation = _currentPlayer.transform.eulerAngles.y;
        }
    }

    /// <summary>
    /// 更新物理相关逻辑
    /// </summary>
    /// <param name="fixedDeltaTime">固定帧时间</param>
    private void UpdatePhysics(float fixedDeltaTime)
    {
        // 物理更新逻辑
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    private void CleanupResources()
    {
        if (_currentPlayer != null)
        {
            DestroyImmediate(_currentPlayer.gameObject);
            _currentPlayer = null;
        }

        _playerData = null;
        _professionManager = null;
        _skillManager = null;
        _attributeManager = null;
    }

    /// <summary>
    /// 更新当前状态到数据
    /// </summary>
    private void UpdatePlayerDataFromCurrentState()
    {
        if (_playerData == null || _currentPlayer == null)
            return;

        // 更新位置和旋转
        _playerData.position = _currentPlayer.transform.position;
        _playerData.rotation = _currentPlayer.transform.eulerAngles.y;

        // 更新属性
        _playerData.attributes = _attributeManager?.GetCurrentAttributes() ?? _playerData.attributes;

        // 更新时间戳
        _playerData.lastSaveTime = System.DateTime.Now.ToBinary();
    }
    #endregion

    #region 私有方法 - 状态处理
    /// <summary>
    /// 改变玩家状态
    /// </summary>
    /// <param name="newState">新状态</param>
    private void ChangePlayerState(PlayerState newState)
    {
        if (_currentState == newState)
            return;

        PlayerState oldState = _currentState;
        _currentState = newState;
        _stateTimer = 0f;

        LogDebug($"玩家状态改变: {oldState} -> {newState}");

        // 触发状态改变事件
        Global.Event.TriggerEvent(Global.Events.Player.POSITION_CHANGED, new PlayerStateChangedEventArgs(oldState, newState));
    }

    /// <summary>
    /// 处理空闲状态
    /// </summary>
    /// <param name="deltaTime">帧时间</param>
    private void HandleIdleState(float deltaTime)
    {
        // 空闲状态逻辑
    }

    /// <summary>
    /// 处理移动状态
    /// </summary>
    /// <param name="deltaTime">帧时间</param>
    private void HandleMovingState(float deltaTime)
    {
        // 移动状态逻辑
    }

    /// <summary>
    /// 处理战斗状态
    /// </summary>
    /// <param name="deltaTime">帧时间</param>
    private void HandleFightingState(float deltaTime)
    {
        // 战斗状态逻辑
    }

    /// <summary>
    /// 处理种田状态
    /// </summary>
    /// <param name="deltaTime">帧时间</param>
    private void HandleFarmingState(float deltaTime)
    {
        // 种田状态逻辑
    }

    /// <summary>
    /// 处理交易状态
    /// </summary>
    /// <param name="deltaTime">帧时间</param>
    private void HandleTradingState(float deltaTime)
    {
        // 交易状态逻辑
    }

    /// <summary>
    /// 处理对话状态
    /// </summary>
    /// <param name="deltaTime">帧时间</param>
    private void HandleTalkingState(float deltaTime)
    {
        // 对话状态逻辑
    }

    /// <summary>
    /// 处理死亡状态
    /// </summary>
    /// <param name="deltaTime">帧时间</param>
    private void HandleDeadState(float deltaTime)
    {
        // 死亡状态逻辑
        if (_stateTimer > 3.0f) // 3秒后可以重生
        {
            ChangePlayerState(PlayerState.Respawning);
        }
    }

    /// <summary>
    /// 处理重生状态
    /// </summary>
    /// <param name="deltaTime">帧时间</param>
    private void HandleRespawningState(float deltaTime)
    {
        // 重生状态逻辑
        if (_stateTimer > 1.0f) // 1秒重生时间
        {
            RespawnPlayer();
            ChangePlayerState(PlayerState.Idle);
        }
    }

    /// <summary>
    /// 处理玩家死亡
    /// </summary>
    private void HandlePlayerDeath()
    {
        LogDebug("玩家死亡");

        ChangePlayerState(PlayerState.Dead);
        _playerData.totalDeaths++;

        // 触发死亡事件
        Global.Event.TriggerEvent(Global.Events.Player.DEATH, new PlayerDeathEventArgs(_playerData));
    }

    /// <summary>
    /// 检查升级
    /// </summary>
    /// <param name="oldLevel">旧等级</param>
    private void CheckLevelUp(int oldLevel)
    {
        int newLevel = CalculateLevel(_playerData.experience);

        if (newLevel > oldLevel)
        {
            _playerData.level = newLevel;

            // 升级时恢复生命值和法力值
            _playerData.currentHealth = _playerData.maxHealth;
            _playerData.currentMana = _playerData.maxMana;

            LogDebug($"玩家升级: {oldLevel} -> {newLevel}");

            // 触发升级事件
            Global.Event.TriggerEvent(Global.Events.Player.LEVEL_UP, new PlayerLevelUpEventArgs(oldLevel, newLevel));
        }
    }

    /// <summary>
    /// 计算等级
    /// </summary>
    /// <param name="experience">经验值</param>
    /// <returns>等级</returns>
    private int CalculateLevel(int experience)
    {
        // 简单的等级计算公式
        int level = 1;
        int requiredExp = 0;

        while (requiredExp <= experience && level < 100)
        {
            level++;
            requiredExp += level * 100 + (level - 1) * 50;
        }

        return level - 1;
    }
    #endregion

    #region 私有方法 - 事件处理
    /// <summary>
    /// 游戏状态改变事件处理
    /// </summary>
    /// <param name="eventArgs">事件参数</param>
    private void OnGameStateChanged(object eventArgs)
    {
        if (eventArgs is GameStateEventArgs args)
        {
            LogDebug($"响应游戏状态改变: {args.PreviousState} -> {args.NewState}");

            switch (args.NewState)
            {
                case GameState.Playing:
                    // 游戏开始时的处理
                    break;
                case GameState.Paused:
                    // 游戏暂停时的处理
                    break;
            }
        }
    }

    /// <summary>
    /// 玩家死亡事件处理
    /// </summary>
    /// <param name="eventArgs">事件参数</param>
    private void OnPlayerDeath(object eventArgs)
    {
        LogDebug("处理玩家死亡事件");
    }

    /// <summary>
    /// 玩家升级事件处理
    /// </summary>
    /// <param name="eventArgs">事件参数</param>
    private void OnPlayerLevelUp(object eventArgs)
    {
        LogDebug("处理玩家升级事件");
    }

    /// <summary>
    /// 职业改变事件处理
    /// </summary>
    /// <param name="eventArgs">事件参数</param>
    private void OnProfessionChanged(object eventArgs)
    {
        LogDebug("处理职业改变事件");
    }

    /// <summary>
    /// 技能学习事件处理
    /// </summary>
    /// <param name="eventArgs">事件参数</param>
    private void OnSkillLearned(object eventArgs)
    {
        LogDebug("处理技能学习事件");
    }
    #endregion
}