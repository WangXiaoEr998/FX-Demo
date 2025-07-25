using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// UI管理器，负责UI界面的显示、隐藏、切换和管理
/// 作者：黄畅修，徐锵
/// 创建时间：2025-07-24
/// </summary>
public class UIManager : SingletonMono<UIManager>
{
    #region 字段定义
    [Header("UI根节点")]
    [SerializeField] private Canvas _mainCanvas;
    [SerializeField] private Transform _uiRoot;
    
    [Header("UI预制体")]
    [SerializeField] private GameObject _mainMenuPrefab;
    [SerializeField] private GameObject _gameHUDPrefab;
    [SerializeField] private GameObject _pauseMenuPrefab;
    [SerializeField] private GameObject _loadingScreenPrefab;
    
    [Header("调试设置")]
    [SerializeField] private bool _enableDebugMode = false;
    
    // UI界面字典
    private Dictionary<string, GameObject> _uiPanels = new Dictionary<string, GameObject>();
    private Stack<string> _uiStack = new Stack<string>();
    #endregion

    #region 属性
    /// <summary>
    /// 主画布
    /// </summary>
    public Canvas MainCanvas => _mainCanvas;
    
    /// <summary>
    /// UI根节点
    /// </summary>
    public Transform UIRoot => _uiRoot;
    
    /// <summary>
    /// 当前显示的UI界面
    /// </summary>
    public string CurrentUI => _uiStack.Count > 0 ? _uiStack.Peek() : null;
    #endregion

    #region Unity生命周期
    protected override void Awake()
    {
        base.Awake();
        InitializeUI();
    }

    private void Start()
    {
        SetupUI();
    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 显示UI界面
    /// </summary>
    /// <param name="uiName">UI界面名称</param>
    /// <param name="hideOthers">是否隐藏其他界面</param>
    public void ShowUI(string uiName, bool hideOthers = true)
    {
        if (string.IsNullOrEmpty(uiName))
        {
            Debug.LogError("UI名称不能为空");
            return;
        }

        // 如果需要隐藏其他界面
        if (hideOthers)
        {
            HideAllUI();
        }

        // 显示指定界面
        if (_uiPanels.ContainsKey(uiName))
        {
            _uiPanels[uiName].SetActive(true);
            _uiStack.Push(uiName);
            
            if (_enableDebugMode)
            {
                Debug.Log($"显示UI界面: {uiName}");
            }
        }
        else
        {
            Debug.LogError($"UI界面不存在: {uiName}");
        }
    }

    /// <summary>
    /// 隐藏UI界面
    /// </summary>
    /// <param name="uiName">UI界面名称</param>
    public void HideUI(string uiName)
    {
        if (string.IsNullOrEmpty(uiName))
        {
            Debug.LogError("UI名称不能为空");
            return;
        }

        if (_uiPanels.ContainsKey(uiName))
        {
            _uiPanels[uiName].SetActive(false);
            
            // 从堆栈中移除
            if (_uiStack.Count > 0 && _uiStack.Peek() == uiName)
            {
                _uiStack.Pop();
            }
            
            if (_enableDebugMode)
            {
                Debug.Log($"隐藏UI界面: {uiName}");
            }
        }
        else
        {
            Debug.LogError($"UI界面不存在: {uiName}");
        }
    }

    /// <summary>
    /// 隐藏所有UI界面
    /// </summary>
    public void HideAllUI()
    {
        foreach (var panel in _uiPanels.Values)
        {
            panel.SetActive(false);
        }
        
        _uiStack.Clear();
        
        if (_enableDebugMode)
        {
            Debug.Log("隐藏所有UI界面");
        }
    }

    /// <summary>
    /// 返回上一个UI界面
    /// </summary>
    public void GoBack()
    {
        if (_uiStack.Count > 1)
        {
            string currentUI = _uiStack.Pop();
            string previousUI = _uiStack.Peek();
            
            HideUI(currentUI);
            ShowUI(previousUI, false);
            
            if (_enableDebugMode)
            {
                Debug.Log($"返回上一界面: {currentUI} -> {previousUI}");
            }
        }
    }

    /// <summary>
    /// 注册UI界面
    /// </summary>
    /// <param name="uiName">UI名称</param>
    /// <param name="uiObject">UI对象</param>
    public void RegisterUI(string uiName, GameObject uiObject)
    {
        if (string.IsNullOrEmpty(uiName) || uiObject == null)
        {
            Debug.LogError("注册UI参数无效");
            return;
        }

        if (_uiPanels.ContainsKey(uiName))
        {
            Debug.LogWarning($"UI界面已存在，将被覆盖: {uiName}");
        }

        _uiPanels[uiName] = uiObject;
        uiObject.SetActive(false);
        
        if (_enableDebugMode)
        {
            Debug.Log($"注册UI界面: {uiName}");
        }
    }

    /// <summary>
    /// 注销UI界面
    /// </summary>
    /// <param name="uiName">UI名称</param>
    public void UnregisterUI(string uiName)
    {
        if (_uiPanels.ContainsKey(uiName))
        {
            _uiPanels.Remove(uiName);
            
            if (_enableDebugMode)
            {
                Debug.Log($"注销UI界面: {uiName}");
            }
        }
    }
    #endregion

    #region 私有方法
    /// <summary>
    /// 初始化UI系统
    /// </summary>
    private void InitializeUI()
    {
        // 查找或创建主画布
        if (_mainCanvas == null)
        {
            _mainCanvas = FindObjectOfType<Canvas>();
            if (_mainCanvas == null)
            {
                CreateMainCanvas();
            }
        }

        // 设置UI根节点
        if (_uiRoot == null)
        {
            _uiRoot = _mainCanvas.transform;
        }

        if (_enableDebugMode)
        {
            Debug.Log("UI系统初始化完成");
        }
    }

    /// <summary>
    /// 创建主画布
    /// </summary>
    private void CreateMainCanvas()
    {
        GameObject canvasGO = new GameObject("MainCanvas");
        _mainCanvas = canvasGO.AddComponent<Canvas>();
        _mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _mainCanvas.sortingOrder = 0;

        // 尝试添加UI组件（如果可用）
        try
        {
            // 使用反射添加CanvasScaler组件
            var scalerType = System.Type.GetType("UnityEngine.UI.CanvasScaler, UnityEngine.UI");
            if (scalerType != null)
            {
                var scaler = canvasGO.AddComponent(scalerType);
                // 设置基本属性
                var uiScaleModeField = scalerType.GetField("m_UiScaleMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (uiScaleModeField != null)
                {
                    uiScaleModeField.SetValue(scaler, 1); // ScaleWithScreenSize = 1
                }
            }

            // 使用反射添加GraphicRaycaster组件
            var raycasterType = System.Type.GetType("UnityEngine.UI.GraphicRaycaster, UnityEngine.UI");
            if (raycasterType != null)
            {
                canvasGO.AddComponent(raycasterType);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"无法添加UI组件: {ex.Message}");
        }

        DontDestroyOnLoad(canvasGO);
    }

    /// <summary>
    /// 设置UI界面
    /// </summary>
    private void SetupUI()
    {
        // 实例化并注册UI预制体
        if (_mainMenuPrefab != null)
        {
            GameObject mainMenu = Instantiate(_mainMenuPrefab, _uiRoot);
            RegisterUI(UIConst.MainMenu, mainMenu);
        }

        if (_gameHUDPrefab != null)
        {
            GameObject gameHUD = Instantiate(_gameHUDPrefab, _uiRoot);
            RegisterUI(UIConst.GameHUD, gameHUD);
        }

        if (_pauseMenuPrefab != null)
        {
            GameObject pauseMenu = Instantiate(_pauseMenuPrefab, _uiRoot);
            RegisterUI(UIConst.PauseMenu, pauseMenu);
        }

        if (_loadingScreenPrefab != null)
        {
            GameObject loadingScreen = Instantiate(_loadingScreenPrefab, _uiRoot);
            RegisterUI(UIConst.LoadingScreen, loadingScreen);
        }

        if (_enableDebugMode)
        {
            Debug.Log("UI界面设置完成");
        }
    }
    #endregion
}

public static class UIConst
{
    public const string MainMenu = "MainMenu";
    public const string GameHUD = "GameHUD";
    public const string PauseMenu = "PauseMenu";
    public const string LoadingScreen = "LoadingScreen";
}
