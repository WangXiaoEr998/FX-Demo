/// <summary>
/// 游戏系统接口，定义所有游戏系统必须实现的基本方法
/// 作者：黄畅修
/// 创建时间：2025-07-12
/// </summary>
public interface IGameSystem
{
    /// <summary>
    /// 系统名称
    /// </summary>
    string SystemName { get; }
    
    /// <summary>
    /// 系统是否已初始化
    /// </summary>
    bool IsInitialized { get; }
    
    /// <summary>
    /// 系统是否正在运行
    /// </summary>
    bool IsRunning { get; }
    
    /// <summary>
    /// 系统优先级（数值越小优先级越高）
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// 初始化系统
    /// </summary>
    void Initialize();
    
    /// <summary>
    /// 启动系统
    /// </summary>
    void Start();
    
    /// <summary>
    /// 更新系统（每帧调用）
    /// </summary>
    /// <param name="deltaTime">帧时间</param>
    void UpdateSystem(float deltaTime);

    /// <summary>
    /// 固定更新系统（固定时间间隔调用）
    /// </summary>
    /// <param name="fixedDeltaTime">固定帧时间</param>
    void FixedUpdateSystem(float fixedDeltaTime);
    
    /// <summary>
    /// 关闭系统
    /// </summary>
    void Shutdown();
    
    /// <summary>
    /// 重置系统
    /// </summary>
    void Reset();
    
    /// <summary>
    /// 暂停系统
    /// </summary>
    void Pause();
    
    /// <summary>
    /// 恢复系统
    /// </summary>
    void Resume();
}
