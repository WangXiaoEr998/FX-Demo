using UnityEngine;
using FanXing.Data;

/// <summary>
/// 玩家组件，负责玩家角色的具体行为和表现
/// 作者：黄畅修,容泳森
/// 修改时间：2025-07-23
/// </summary>
public class Player : MonoBehaviour
{
    #region 字段定义
    [Header("玩家配置")]
    [SerializeField] private float _moveSpeed = 5.0f;
    [SerializeField] private float _runSpeedMultiplier = 1.5f;
    [SerializeField] private bool _enableDebugMode = false;
    
    [Header("组件引用")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Animator _animator;
    [SerializeField] private AudioSource _audioSource;
    
    // 玩家数据和系统引用
    private PlayerData _playerData;
    private PlayerSystem _playerSystem;
    
    // 移动相关
    private Vector3 _moveDirection = Vector3.zero;
    private bool _isRunning = false;
    private bool _isPaused = false;
    
    // 动画相关
    private int _animMoveSpeed;
    private int _animIsRunning;
    private int _animIsIdle;
    
    // 输入相关
    private float _horizontalInput;
    private float _verticalInput;
    #endregion

    #region 属性
    /// <summary>
    /// 玩家数据
    /// </summary>
    public PlayerData PlayerData => _playerData;
    
    /// <summary>
    /// 是否正在移动
    /// </summary>
    public bool IsMoving => _moveDirection.magnitude > 0.1f;
    
    /// <summary>
    /// 是否正在跑步
    /// </summary>
    public bool IsRunning => _isRunning;
    
    /// <summary>
    /// 是否暂停
    /// </summary>
    public bool IsPaused => _isPaused;
    
    /// <summary>
    /// 当前移动速度
    /// </summary>
    public float CurrentMoveSpeed => _moveSpeed * (_isRunning ? _runSpeedMultiplier : 1.0f);
    #endregion

    #region Unity生命周期
    private void Awake()
    {
        // 获取组件引用
        InitializeComponents();
        
        // 初始化动画参数
        InitializeAnimationParameters();
    }

    private void Start()
    {
        LogDebug("玩家组件启动");
    }

    private void Update()
    {
        if (_isPaused || _playerData == null)
            return;

        // 处理输入
        HandleInput();
        
        // 更新移动
        UpdateMovement();
        
        // 更新动画
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (_isPaused || _playerData == null)
            return;

        // 物理更新
        UpdatePhysics();
    }
    #endregion

    #region 公共方法 - 初始化
    /// <summary>
    /// 初始化玩家组件
    /// </summary>
    /// <param name="playerData">玩家数据</param>
    /// <param name="playerSystem">玩家系统</param>
    public void Initialize(PlayerData playerData, PlayerSystem playerSystem)
    {
        if (playerData == null)
        {
            LogError("玩家数据不能为空");
            return;
        }

        if (playerSystem == null)
        {
            LogError("玩家系统不能为空");
            return;
        }

        LogDebug("初始化玩家组件");
        
        _playerData = playerData;
        _playerSystem = playerSystem;
        
        // 设置初始位置和旋转
        transform.position = _playerData.position;
        transform.rotation = Quaternion.Euler(0, _playerData.rotation, 0);
        
        // 更新移动速度
        UpdateMoveSpeed();
        
        LogDebug("玩家组件初始化完成");
    }

    /// <summary>
    /// 设置暂停状态
    /// </summary>
    /// <param name="paused">是否暂停</param>
    public void SetPaused(bool paused)
    {
        _isPaused = paused;
        
        if (_isPaused)
        {
            // 暂停时停止移动
            _moveDirection = Vector3.zero;
            _horizontalInput = 0f;
            _verticalInput = 0f;
        }
        
        LogDebug($"设置暂停状态: {paused}");
    }
    #endregion

    #region 公共方法 - 移动控制
    /// <summary>
    /// 设置移动输入
    /// </summary>
    /// <param name="horizontal">水平输入</param>
    /// <param name="vertical">垂直输入</param>
    public void SetMoveInput(float horizontal, float vertical)
    {
        _horizontalInput = horizontal;
        _verticalInput = vertical;
    }

    /// <summary>
    /// 设置跑步状态
    /// </summary>
    /// <param name="running">是否跑步</param>
    public void SetRunning(bool running)
    {
        _isRunning = running;
    }

    /// <summary>
    /// 传送到指定位置
    /// </summary>
    /// <param name="position">目标位置</param>
    public void TeleportTo(Vector3 position)
    {
        LogDebug($"传送到位置: {position}");
        
        transform.position = position;
        _playerData.position = position;
        
        // 重置移动状态
        _moveDirection = Vector3.zero;
    }

    /// <summary>
    /// 传送到指定位置和旋转
    /// </summary>
    /// <param name="position">目标位置</param>
    /// <param name="rotation">目标旋转</param>
    public void TeleportTo(Vector3 position, Quaternion rotation)
    {
        LogDebug($"传送到位置: {position}, 旋转: {rotation.eulerAngles}");
        
        transform.position = position;
        transform.rotation = rotation;
        
        _playerData.position = position;
        _playerData.rotation = rotation.eulerAngles.y;
        
        // 重置移动状态
        _moveDirection = Vector3.zero;
    }
    #endregion

    #region 公共方法 - 状态查询
    /// <summary>
    /// 获取当前位置
    /// </summary>
    /// <returns>当前位置</returns>
    public Vector3 GetPosition()
    {
        return transform.position;
    }

    /// <summary>
    /// 获取当前旋转
    /// </summary>
    /// <returns>当前旋转</returns>
    public Quaternion GetRotation()
    {
        return transform.rotation;
    }

    /// <summary>
    /// 获取移动方向
    /// </summary>
    /// <returns>移动方向</returns>
    public Vector3 GetMoveDirection()
    {
        return _moveDirection;
    }
    #endregion

    #region 私有方法 - 初始化
    /// <summary>
    /// 初始化组件引用
    /// </summary>
    private void InitializeComponents()
    {
        // 获取CharacterController组件
        if (_characterController == null)
        {
            _characterController = GetComponent<CharacterController>();
            if (_characterController == null)
            {
                _characterController = gameObject.AddComponent<CharacterController>();
            }
        }
        
        // 获取Animator组件
        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }
        
        // 获取AudioSource组件
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    /// <summary>
    /// 初始化动画参数
    /// </summary>
    private void InitializeAnimationParameters()
    {
        if (_animator != null)
        {
            _animMoveSpeed = Animator.StringToHash("MoveSpeed");
            _animIsRunning = Animator.StringToHash("IsRunning");
            _animIsIdle = Animator.StringToHash("IsIdle");
        }
    }

    /// <summary>
    /// 更新移动速度
    /// </summary>
    private void UpdateMoveSpeed()
    {
        if (_playerSystem != null && _playerSystem.AttributeManager != null)
        {
            PlayerAttributes attributes = _playerSystem.AttributeManager.GetCurrentAttributes();
            _moveSpeed = attributes.MoveSpeed;
        }
    }
    #endregion

    #region 私有方法 - 输入处理
    /// <summary>
    /// 处理输入
    /// </summary>
    private void HandleInput()
    {
        // 获取移动输入
        _horizontalInput = Input.GetAxis("Horizontal");
        _verticalInput = Input.GetAxis("Vertical");
        
        // 获取跑步输入
        _isRunning = Input.GetKey(KeyCode.LeftShift);
        
        // 处理其他输入
        HandleOtherInput();
    }

    /// <summary>
    /// 处理其他输入
    /// </summary>
    private void HandleOtherInput()
    {
        // 交互输入
        if (Input.GetKeyDown(KeyCode.E))
        {
            HandleInteraction();
        }
        
        // 背包输入
        if (Input.GetKeyDown(KeyCode.I))
        {
            HandleInventoryToggle();
        }
        
        // 角色面板输入
        if (Input.GetKeyDown(KeyCode.C))
        {
            HandleCharacterPanelToggle();
        }
    }

    /// <summary>
    /// 处理交互
    /// </summary>
    private void HandleInteraction()
    {
        LogDebug("处理交互输入");
        
        // 触发交互事件
        Global.Event.TriggerEvent(Global.Events.NPC.INTERACTION_STARTED, transform.position);
    }

    /// <summary>
    /// 处理背包切换
    /// </summary>
    private void HandleInventoryToggle()
    {
        LogDebug("切换背包界面");
        
        // 触发UI事件
        Global.Event.TriggerEvent(Global.Events.UI.PANEL_OPENED);
    }

    /// <summary>
    /// 处理角色面板切换
    /// </summary>
    private void HandleCharacterPanelToggle()
    {
        LogDebug("切换角色面板");
        
        // 触发UI事件
        Global.Event.TriggerEvent(Global.Events.UI.PANEL_OPENED);
    }
    #endregion

    #region 私有方法 - 移动更新
    /// <summary>
    /// 更新移动
    /// </summary>
    private void UpdateMovement()
    {
        // 计算移动方向
        Vector3 inputDirection = new Vector3(_horizontalInput, 0f, _verticalInput);
        inputDirection = inputDirection.normalized;
        
        // 转换为世界坐标系
        _moveDirection = transform.TransformDirection(inputDirection);
        
        // 应用移动速度
        _moveDirection *= CurrentMoveSpeed;
        
        // 应用重力
        if (_characterController != null && !_characterController.isGrounded)
        {
            _moveDirection.y -= 9.81f * Time.deltaTime;
        }
        
        // 旋转角色
        if (inputDirection.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Lerp(transform.rotation, 
                Quaternion.Euler(0f, targetAngle, 0f), Time.deltaTime * 10f);
        }
    }

    /// <summary>
    /// 更新物理
    /// </summary>
    private void UpdatePhysics()
    {
        if (_characterController != null)
        {
            // 移动角色
            _characterController.Move(_moveDirection * Time.fixedDeltaTime);
            
            // 更新玩家数据中的位置
            if (_playerData != null)
            {
                _playerData.position = transform.position;
                _playerData.rotation = transform.eulerAngles.y;
            }
        }
    }
    #endregion

    #region 私有方法 - 动画更新
    /// <summary>
    /// 更新动画
    /// </summary>
    private void UpdateAnimation()
    {
        if (_animator == null)
            return;

        // 计算移动速度
        float moveSpeed = new Vector3(_horizontalInput, 0f, _verticalInput).magnitude;
        
        // 设置动画参数
        _animator.SetFloat(_animMoveSpeed, moveSpeed);
        _animator.SetBool(_animIsRunning, _isRunning && moveSpeed > 0.1f);
        _animator.SetBool(_animIsIdle, moveSpeed < 0.1f);
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
            Debug.Log($"[Player] {message}");
        }
    }

    /// <summary>
    /// 输出警告日志
    /// </summary>
    /// <param name="message">警告信息</param>
    private void LogWarning(string message)
    {
        Debug.LogWarning($"[Player] {message}");
    }

    /// <summary>
    /// 输出错误日志
    /// </summary>
    /// <param name="message">错误信息</param>
    private void LogError(string message)
    {
        Debug.LogError($"[Player] {message}");
    }
    #endregion
}
