using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 系统管理器，负责游戏各个子系统的初始化、更新和管理
/// 作者：黄畅修
/// 创建时间：2025-07-12
/// </summary>
public class SystemManager : MonoBehaviour
{
    #region 单例模式
    private static SystemManager _instance;
    public static SystemManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SystemManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SystemManager");
                    _instance = go.AddComponent<SystemManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    #endregion

    #region 字段定义
    [Header("系统配置")]
    [SerializeField] private bool _enableDebugMode = false;
    [SerializeField] private bool _autoInitializeSystems = true;
    
    [Header("系统引用")]
    [SerializeField] private PlayerSystem _playerSystem;
    [SerializeField] private NPCSystem _npcSystem;
    [SerializeField] private FarmingSystem _farmingSystem;
    [SerializeField] private CombatSystem _combatSystem;
    [SerializeField] private EconomySystem _economySystem;
    [SerializeField] private QuestSystem _questSystem;
    [SerializeField] private ShopSystem _shopSystem;
    
    // 系统列表
    private List<IGameSystem> _gameSystems = new List<IGameSystem>();
    private Dictionary<System.Type, IGameSystem> _systemDictionary = new Dictionary<System.Type, IGameSystem>();
    
    // 系统状态
    private bool _systemsInitialized = false;
    private bool _systemsStarted = false;
    #endregion

    #region 属性
    /// <summary>
    /// 系统是否已初始化
    /// </summary>
    public bool SystemsInitialized => _systemsInitialized;
    
    /// <summary>
    /// 系统是否已启动
    /// </summary>
    public bool SystemsStarted => _systemsStarted;
    
    /// <summary>
    /// 已注册的系统数量
    /// </summary>
    public int SystemCount => _gameSystems.Count;
    #endregion

    #region Unity生命周期
    private void Awake()
    {
        // 确保单例唯一性
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        if (_autoInitializeSystems)
        {
            InitializeSystems();
        }
    }

    private void Start()
    {
        if (_autoInitializeSystems)
        {
            StartSystems();
        }
    }

    private void Update()
    {
        if (_systemsStarted)
        {
            UpdateSystems(Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (_systemsStarted)
        {
            FixedUpdateSystems(Time.fixedDeltaTime);
        }
    }

    private void OnDestroy()
    {
        ShutdownSystems();
    }
    #endregion

    #region 公共方法 - 系统管理
    /// <summary>
    /// 初始化所有系统
    /// </summary>
    public void InitializeSystems()
    {
        if (_systemsInitialized)
        {
            Debug.LogWarning("系统已经初始化过了");
            return;
        }

        if (_enableDebugMode)
        {
            Debug.Log("开始初始化游戏系统...");
        }

        // 注册所有系统
        RegisterSystems();
        
        // 初始化系统
        foreach (var system in _gameSystems)
        {
            try
            {
                system.Initialize();
                
                if (_enableDebugMode)
                {
                    Debug.Log($"系统初始化完成: {system.GetType().Name}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"系统初始化失败: {system.GetType().Name}, 错误: {ex.Message}");
            }
        }

        _systemsInitialized = true;
        
        // 触发系统初始化完成事件
        Global.Event.TriggerEvent(Global.Events.System.INITIALIZED);
        
        if (_enableDebugMode)
        {
            Debug.Log($"所有游戏系统初始化完成，共 {_gameSystems.Count} 个系统");
        }
    }

    /// <summary>
    /// 启动所有系统
    /// </summary>
    public void StartSystems()
    {
        if (!_systemsInitialized)
        {
            Debug.LogError("系统尚未初始化，无法启动");
            return;
        }

        if (_systemsStarted)
        {
            Debug.LogWarning("系统已经启动过了");
            return;
        }

        if (_enableDebugMode)
        {
            Debug.Log("开始启动游戏系统...");
        }

        foreach (var system in _gameSystems)
        {
            try
            {
                system.Start();
                
                if (_enableDebugMode)
                {
                    Debug.Log($"系统启动完成: {system.GetType().Name}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"系统启动失败: {system.GetType().Name}, 错误: {ex.Message}");
            }
        }

        _systemsStarted = true;
        
        // 触发系统启动完成事件
        Global.Event.TriggerEvent(Global.Events.System.STARTED);
        
        if (_enableDebugMode)
        {
            Debug.Log("所有游戏系统启动完成");
        }
    }

    /// <summary>
    /// 关闭所有系统
    /// </summary>
    public void ShutdownSystems()
    {
        if (_enableDebugMode)
        {
            Debug.Log("开始关闭游戏系统...");
        }

        // 反向关闭系统
        for (int i = _gameSystems.Count - 1; i >= 0; i--)
        {
            try
            {
                _gameSystems[i].Shutdown();
                
                if (_enableDebugMode)
                {
                    Debug.Log($"系统关闭完成: {_gameSystems[i].GetType().Name}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"系统关闭失败: {_gameSystems[i].GetType().Name}, 错误: {ex.Message}");
            }
        }

        _systemsStarted = false;
        _systemsInitialized = false;
        
        // 触发系统关闭完成事件
        Global.Event.TriggerEvent(Global.Events.System.SHUTDOWN);
        
        if (_enableDebugMode)
        {
            Debug.Log("所有游戏系统关闭完成");
        }
    }

    /// <summary>
    /// 获取指定类型的系统
    /// </summary>
    /// <typeparam name="T">系统类型</typeparam>
    /// <returns>系统实例</returns>
    public T GetSystem<T>() where T : class, IGameSystem
    {
        System.Type systemType = typeof(T);
        
        if (_systemDictionary.ContainsKey(systemType))
        {
            return _systemDictionary[systemType] as T;
        }
        
        Debug.LogWarning($"系统未找到: {systemType.Name}");
        return null;
    }

    /// <summary>
    /// 注册系统
    /// </summary>
    /// <param name="system">系统实例</param>
    public void RegisterSystem(IGameSystem system)
    {
        if (system == null)
        {
            Debug.LogError("系统实例不能为空");
            return;
        }

        System.Type systemType = system.GetType();
        
        if (_systemDictionary.ContainsKey(systemType))
        {
            Debug.LogWarning($"系统已存在: {systemType.Name}");
            return;
        }

        _gameSystems.Add(system);
        _systemDictionary[systemType] = system;
        
        if (_enableDebugMode)
        {
            Debug.Log($"系统注册成功: {systemType.Name}");
        }
    }

    /// <summary>
    /// 注销系统
    /// </summary>
    /// <typeparam name="T">系统类型</typeparam>
    public void UnregisterSystem<T>() where T : class, IGameSystem
    {
        System.Type systemType = typeof(T);
        
        if (_systemDictionary.ContainsKey(systemType))
        {
            IGameSystem system = _systemDictionary[systemType];
            _gameSystems.Remove(system);
            _systemDictionary.Remove(systemType);
            
            if (_enableDebugMode)
            {
                Debug.Log($"系统注销成功: {systemType.Name}");
            }
        }
    }
    #endregion

    #region 私有方法
    /// <summary>
    /// 注册所有系统
    /// </summary>
    private void RegisterSystems()
    {
        // 创建并注册各个系统
        if (_playerSystem == null)
            _playerSystem = gameObject.AddComponent<PlayerSystem>();
        RegisterSystem(_playerSystem);

        if (_npcSystem == null)
            _npcSystem = gameObject.AddComponent<NPCSystem>();
        RegisterSystem(_npcSystem);

        if (_farmingSystem == null)
            _farmingSystem = gameObject.AddComponent<FarmingSystem>();
        RegisterSystem(_farmingSystem);

        if (_combatSystem == null)
            _combatSystem = gameObject.AddComponent<CombatSystem>();
        RegisterSystem(_combatSystem);

        if (_economySystem == null)
            _economySystem = gameObject.AddComponent<EconomySystem>();
        RegisterSystem(_economySystem);

        if (_questSystem == null)
            _questSystem = gameObject.AddComponent<QuestSystem>();
        RegisterSystem(_questSystem);

        if (_shopSystem == null)
            _shopSystem = gameObject.AddComponent<ShopSystem>();
        RegisterSystem(_shopSystem);
    }

    /// <summary>
    /// 更新所有系统
    /// </summary>
    /// <param name="deltaTime">帧时间</param>
    private void UpdateSystems(float deltaTime)
    {
        foreach (var system in _gameSystems)
        {
            try
            {
                system.UpdateSystem(deltaTime);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"系统更新异常: {system.GetType().Name}, 错误: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 固定更新所有系统
    /// </summary>
    /// <param name="fixedDeltaTime">固定帧时间</param>
    private void FixedUpdateSystems(float fixedDeltaTime)
    {
        foreach (var system in _gameSystems)
        {
            try
            {
                system.FixedUpdateSystem(fixedDeltaTime);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"系统固定更新异常: {system.GetType().Name}, 错误: {ex.Message}");
            }
        }
    }
    #endregion
}
