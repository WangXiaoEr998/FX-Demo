using UnityEngine;

/// <summary>
/// 游戏系统基类，提供系统的基础实现
/// 作者：黄畅修
/// 创建时间：2025-07-12
/// </summary>
public abstract class BaseGameSystem : MonoBehaviour, IGameSystem
{
    #region 字段定义
    [Header("系统基础配置")]
    [SerializeField] protected bool _enableDebugMode = false;
    [SerializeField] protected int _priority = 0;
    
    protected bool _isInitialized = false;
    protected bool _isRunning = false;
    protected bool _isPaused = false;
    #endregion

    #region 属性
    /// <summary>
    /// 系统名称
    /// </summary>
    public virtual string SystemName => GetType().Name;
    
    /// <summary>
    /// 系统是否已初始化
    /// </summary>
    public bool IsInitialized => _isInitialized;
    
    /// <summary>
    /// 系统是否正在运行
    /// </summary>
    public bool IsRunning => _isRunning;
    
    /// <summary>
    /// 系统是否暂停
    /// </summary>
    public bool IsPaused => _isPaused;
    
    /// <summary>
    /// 系统优先级
    /// </summary>
    public virtual int Priority => _priority;
    
    /// <summary>
    /// 是否启用调试模式
    /// </summary>
    public bool EnableDebugMode => _enableDebugMode;
    #endregion

    #region IGameSystem接口实现
    /// <summary>
    /// 初始化系统
    /// </summary>
    public virtual void Initialize()
    {
        if (_isInitialized)
        {
            LogWarning("系统已经初始化过了");
            return;
        }

        LogDebug("开始初始化系统");
        
        try
        {
            OnInitialize();
            _isInitialized = true;
            
            LogDebug("系统初始化完成");
        }
        catch (System.Exception ex)
        {
            LogError($"系统初始化失败: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 启动系统
    /// </summary>
    public virtual void Start()
    {
        if (!_isInitialized)
        {
            LogError("系统尚未初始化，无法启动");
            return;
        }

        if (_isRunning)
        {
            LogWarning("系统已经在运行中");
            return;
        }

        LogDebug("开始启动系统");
        
        try
        {
            OnStart();
            _isRunning = true;
            
            LogDebug("系统启动完成");
        }
        catch (System.Exception ex)
        {
            LogError($"系统启动失败: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 更新系统
    /// </summary>
    /// <param name="deltaTime">帧时间</param>
    public virtual void UpdateSystem(float deltaTime)
    {
        if (!_isRunning || _isPaused)
            return;

        try
        {
            OnUpdate(deltaTime);
        }
        catch (System.Exception ex)
        {
            LogError($"系统更新异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 固定更新系统
    /// </summary>
    /// <param name="fixedDeltaTime">固定帧时间</param>
    public virtual void FixedUpdateSystem(float fixedDeltaTime)
    {
        if (!_isRunning || _isPaused)
            return;

        try
        {
            OnFixedUpdate(fixedDeltaTime);
        }
        catch (System.Exception ex)
        {
            LogError($"系统固定更新异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 关闭系统
    /// </summary>
    public virtual void Shutdown()
    {
        LogDebug("开始关闭系统");
        
        try
        {
            OnShutdown();
            _isRunning = false;
            _isInitialized = false;
            _isPaused = false;
            
            LogDebug("系统关闭完成");
        }
        catch (System.Exception ex)
        {
            LogError($"系统关闭异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 重置系统
    /// </summary>
    public virtual void Reset()
    {
        LogDebug("开始重置系统");
        
        try
        {
            OnReset();
            LogDebug("系统重置完成");
        }
        catch (System.Exception ex)
        {
            LogError($"系统重置异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 暂停系统
    /// </summary>
    public virtual void Pause()
    {
        if (_isPaused)
        {
            LogWarning("系统已经暂停");
            return;
        }

        LogDebug("暂停系统");
        
        try
        {
            OnPause();
            _isPaused = true;
        }
        catch (System.Exception ex)
        {
            LogError($"系统暂停异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 恢复系统
    /// </summary>
    public virtual void Resume()
    {
        if (!_isPaused)
        {
            LogWarning("系统未暂停");
            return;
        }

        LogDebug("恢复系统");
        
        try
        {
            OnResume();
            _isPaused = false;
        }
        catch (System.Exception ex)
        {
            LogError($"系统恢复异常: {ex.Message}");
        }
    }
    #endregion

    #region 抽象方法 - 子类必须实现
    /// <summary>
    /// 系统初始化时调用
    /// </summary>
    protected abstract void OnInitialize();

    /// <summary>
    /// 系统启动时调用
    /// </summary>
    protected abstract void OnStart();

    /// <summary>
    /// 系统更新时调用
    /// </summary>
    /// <param name="deltaTime">帧时间</param>
    protected abstract void OnUpdate(float deltaTime);
    #endregion

    #region 虚方法 - 子类可选择重写
    /// <summary>
    /// 系统固定更新时调用
    /// </summary>
    /// <param name="fixedDeltaTime">固定帧时间</param>
    protected virtual void OnFixedUpdate(float fixedDeltaTime)
    {
        // 默认空实现
    }

    /// <summary>
    /// 系统关闭时调用
    /// </summary>
    protected virtual void OnShutdown()
    {
        // 默认空实现
    }

    /// <summary>
    /// 系统重置时调用
    /// </summary>
    protected virtual void OnReset()
    {
        // 默认空实现
    }

    /// <summary>
    /// 系统暂停时调用
    /// </summary>
    protected virtual void OnPause()
    {
        // 默认空实现
    }

    /// <summary>
    /// 系统恢复时调用
    /// </summary>
    protected virtual void OnResume()
    {
        // 默认空实现
    }
    #endregion

    #region 工具方法
    /// <summary>
    /// 输出调试日志
    /// </summary>
    /// <param name="message">日志信息</param>
    protected void LogDebug(string message)
    {
        if (_enableDebugMode)
        {
            Debug.Log($"[{SystemName}] {message}");
        }
    }

    /// <summary>
    /// 输出警告日志
    /// </summary>
    /// <param name="message">警告信息</param>
    protected void LogWarning(string message)
    {
        Debug.LogWarning($"[{SystemName}] {message}");
    }

    /// <summary>
    /// 输出错误日志
    /// </summary>
    /// <param name="message">错误信息</param>
    protected void LogError(string message)
    {
        Debug.LogError($"[{SystemName}] {message}");
    }

    /// <summary>
    /// 触发系统事件
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="eventArgs">事件参数</param>
    protected void TriggerEvent(string eventName, object eventArgs = null)
    {
        Global.TriggerEvent(eventName, eventArgs);
    }

    /// <summary>
    /// 注册系统事件
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="callback">回调函数</param>
    protected void RegisterEvent(string eventName, System.Action<object> callback)
    {
        Global.RegisterEvent(eventName, callback);
    }

    /// <summary>
    /// 注销系统事件
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="callback">回调函数</param>
    protected void UnregisterEvent(string eventName, System.Action<object> callback)
    {
        Global.UnregisterEvent(eventName, callback);
    }
    #endregion
}
