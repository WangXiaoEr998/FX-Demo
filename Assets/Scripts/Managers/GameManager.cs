using UnityEngine;
using FanXing.Data;
/// <summary>
/// 游戏总控制器，负责游戏状态管理、场景切换、核心系统初始化
/// 作者：黄畅修,容泳森，徐锵
/// 修改时间：2025-07-24
/// </summary>
public class GameManager : SingletonMono<GameManager>
{
    #region 字段定义
    [Header("游戏状态")]
    [SerializeField] private GameState _currentState = GameState.MainMenu;
    [SerializeField] private bool _isGamePaused = false;
    
    [Header("调试设置")]
    [SerializeField] private bool _enableDebugMode = false;
    [SerializeField] private bool _showFPS = false;
    #endregion

    #region 属性
    /// <summary>
    /// 当前游戏状态
    /// </summary>
    public GameState CurrentState => _currentState;
    
    /// <summary>
    /// 游戏是否暂停
    /// </summary>
    public bool IsGamePaused => _isGamePaused;
    
    /// <summary>
    /// 是否启用调试模式
    /// </summary>
    public bool EnableDebugMode => _enableDebugMode;
    #endregion

    #region Unity生命周期
    protected override void Awake()
    {
        base.Awake();
        InitializeGame();
    }

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        HandleInput();
        
        if (_showFPS)
        {
            ShowFPS();
        }
    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 改变游戏状态
    /// </summary>
    /// <param name="newState">新的游戏状态</param>
    public void ChangeGameState(GameState newState)
    {
        if (_currentState == newState) return;
        
        GameState previousState = _currentState;
        _currentState = newState;
        
        OnGameStateChanged(previousState, newState);
        
        if (_enableDebugMode)
        {
            Debug.Log($"游戏状态改变: {previousState} -> {newState}");
        }
    }

    /// <summary>
    /// 暂停游戏
    /// </summary>
    public void PauseGame()
    {
        if (_isGamePaused) return;
        
        _isGamePaused = true;
        Time.timeScale = 0f;
        
        // 触发暂停事件
        Global.Event.TriggerEvent(Global.Events.Game.PAUSED);
        
        if (_enableDebugMode)
        {
            Debug.Log("游戏已暂停");
        }
    }

    /// <summary>
    /// 恢复游戏
    /// </summary>
    public void ResumeGame()
    {
        if (!_isGamePaused) return;
        
        _isGamePaused = false;
        Time.timeScale = 1f;
        
        // 触发恢复事件
        Global.Event.TriggerEvent(Global.Events.Game.RESUMED);
        
        if (_enableDebugMode)
        {
            Debug.Log("游戏已恢复");
        }
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    public void QuitGame()
    {
        if (_enableDebugMode)
        {
            Debug.Log("退出游戏");
        }
        
        // 保存游戏数据
        Global.Data?.SaveGameData();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    #endregion

    #region 私有方法
    /// <summary>
    /// 初始化游戏
    /// </summary>
    private void InitializeGame()
    {
        // 初始化各个管理器
        InitializeManagers();
        
        // 设置游戏配置
        SetupGameSettings();
        
        if (_enableDebugMode)
        {
            Debug.Log("游戏初始化完成");
        }
    }

    /// <summary>
    /// 初始化管理器
    /// </summary>
    private void InitializeManagers()
    {
        // 确保关键管理器存在
        if (Global.UI == null)
        {
            Debug.LogWarning("UIManager未找到");
        }

        if (Global.Data == null)
        {
            Debug.LogWarning("DataManager未找到");
        }

        if (Global.Event == null)
        {
            Debug.LogWarning("EventManager未找到");
        }
    }

    /// <summary>
    /// 设置游戏配置
    /// </summary>
    private void SetupGameSettings()
    {
        // 设置帧率
        Application.targetFrameRate = 60;
        
        // 设置屏幕不休眠
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        
        // 设置质量等级
        QualitySettings.vSyncCount = 1;
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    private void StartGame()
    {
        ChangeGameState(GameState.MainMenu);
    }

    /// <summary>
    /// 处理输入
    /// </summary>
    private void HandleInput()
    {
        // ESC键暂停/恢复游戏
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_currentState == GameState.Playing)
            {
                if (_isGamePaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }
        
        // F1键切换调试模式
        if (Input.GetKeyDown(KeyCode.F1))
        {
            _enableDebugMode = !_enableDebugMode;
            Debug.Log($"调试模式: {(_enableDebugMode ? "开启" : "关闭")}");
        }
        
        // F2键切换FPS显示
        if (Input.GetKeyDown(KeyCode.F2))
        {
            _showFPS = !_showFPS;
        }
    }

    /// <summary>
    /// 游戏状态改变时的处理
    /// </summary>
    /// <param name="previousState">之前的状态</param>
    /// <param name="newState">新的状态</param>
    private void OnGameStateChanged(GameState previousState, GameState newState)
    {
        // 根据状态变化执行相应逻辑
        switch (newState)
        {
            case GameState.MainMenu:
                HandleMainMenuState();
                break;
            case GameState.Playing:
                HandlePlayingState();
                break;
            case GameState.Paused:
                HandlePausedState();
                break;
            case GameState.Loading:
                HandleLoadingState();
                break;
        }
        
        // 触发状态改变事件
        Global.Event.TriggerEvent(Global.Events.Game.STATE_CHANGED, new GameStateEventArgs(previousState, newState));
    }

    private void HandleMainMenuState()
    {
        // 主菜单状态处理
        Global.UI?.ShowMainMenu();
    }

    private void HandlePlayingState()
    {
        // 游戏进行状态处理
        Global.UI?.ShowGameHUD();
    }

    private void HandlePausedState()
    {
        // 暂停状态处理
        Global.UI?.ShowPauseMenu();
    }

    private void HandleLoadingState()
    {
        // 加载状态处理
        Global.UI?.ShowLoadingScreen();
    }

    /// <summary>
    /// 显示FPS
    /// </summary>
    private void ShowFPS()
    {
        // 简单的FPS显示实现
        // 实际项目中可以使用更复杂的FPS计算
    }
    #endregion
}
