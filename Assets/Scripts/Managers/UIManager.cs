using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using System.Reflection;
using static UnityEditor.Experimental.GraphView.GraphView;
using UnityEngine.UI;
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
    public const string MainMenu = "MainMenu";
    public const string GameHUD = "GameHUD";
    public const string PauseMenu = "PauseMenu";
    public const string LoadingScreen = "LoadingScreen";
    // UI界面字典
    private Dictionary<string, UIBase> _uiPanels = new Dictionary<string, UIBase>();
    private Dictionary<UIConst, int> _layerMap = new Dictionary<UIConst, int>()
    {
        [UIConst.MainMenu] = 0,
        [UIConst.GameHUD] = 100,
        [UIConst.PauseMenu] = 200,
        [UIConst.LoadingScreen] = 300,

    };
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
        ShowUI("UI_HUD");
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
    public void ShowUI(string uiName, bool hideOthers = true, object param = null)
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
            _uiPanels[uiName].gameObject.SetActive(true);
            if (_uiStack.Count > 0)
            {
                _uiPanels[_uiStack.Peek()].LoseFocus();
            }
            _uiStack.Push(uiName);
            
            if (_enableDebugMode)
            {
                GameObject go = _uiPanels[uiName].gameObject;
                UIBase uiNode = go.GetComponent<UIBase>();
                go.SetActive(true);
                uiNode.OnOpen(param);
                uiNode.uiName = uiName;

                Debug.Log($"显示UI界面: {uiName}");
            }
        }
        else
        {
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/UIPrefab/{uiName}.prefab");
            if (go != null)
            {
                if (_uiStack.Count > 0)
                {
                    _uiPanels[_uiStack.Peek()].LoseFocus();
                }
                _uiStack.Push(uiName);
                GameObject insGo = Instantiate(go);
                insGo.transform.SetParent(gameObject.transform);
               
                UIBase uiNode = insGo.GetComponent<UIBase>();
                uiNode.uiName = uiName;
                RegisterUI(uiName, uiNode);
                insGo.SetActive(true);
                uiNode.OnOpen(param);
                LayerDstribute(uiNode);
            }
            Debug.LogError($"UI界面不存在: {uiName}");
        }
    }
  
    public void LayerDstribute(UIBase uIBase)
    {
        GameObject go = uIBase.gameObject;
        Canvas canvas = go.GetComponent<Canvas>();
        if (canvas == null)
        {

            canvas = go.AddComponent<Canvas>();

        }
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        // 尝试添加UI组件（如果可用）
        try
        {
            // 使用反射添加CanvasScaler组件
            var scalerType = System.Type.GetType("UnityEngine.UI.CanvasScaler, UnityEngine.UI");
            if (scalerType != null)
            {   
                var scaler = go.GetComponent(scalerType);
                if (scaler == null)
                {
                     scaler = go.AddComponent(scalerType);
                }
                // 设置基本属性
                var uiScaleModeField = scalerType.GetField("m_UiScaleMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (uiScaleModeField != null)
                {
                    uiScaleModeField.SetValue(scaler, 1); // ScaleWithScreenSize = 1

                }
                PropertyInfo referenceResolutionProp = scalerType.GetProperty("referenceResolution");

                if (referenceResolutionProp != null)
                {
                    referenceResolutionProp.SetValue(scaler, new Vector2(Screen.width, Screen.height), null);
                }
                else
                {
                    Debug.LogError("未找到 referenceResolution 属性！");
                }
            }

            // 使用反射添加GraphicRaycaster组件
            var raycasterType = System.Type.GetType("UnityEngine.UI.GraphicRaycaster, UnityEngine.UI");
            if (raycasterType != null)
            {
                GraphicRaycaster graphic = go.GetComponent<GraphicRaycaster>();
                if (graphic == null)
                {
                    go.AddComponent(raycasterType);
                }
           
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"无法添加UI组件: {ex.Message}");
        }
        canvas.sortingOrder = _layerMap[uIBase.layer];
        _layerMap[uIBase.layer]++;
    }
    /// <summary>
    /// 隐藏UI界面
    /// </summary>
    /// <param name="uiName">UI界面名称</param>
    public void HideUI(string uiName,bool isDestroy = true)
    {
        if (string.IsNullOrEmpty(uiName))
        {
            Debug.LogError("UI名称不能为空");
            return;
        }

        if (_uiPanels.ContainsKey(uiName))
        {
            
            // 从堆栈中移除
            if (_uiStack.Count > 0 && _uiStack.Peek() == uiName)
            {
                _uiStack.Pop();
            }
            else if (_uiStack.Count > 0)//没有按顺序关闭UI
            {
                RemoveMiddleElement(_uiStack, uiName);
            }
            if (_uiStack.Count > 0)
            {
                string fName = _uiStack.Peek();

                _uiPanels[fName].gameObject.SetActive(true);
            }
            if (_uiStack.Count>0)
            {
                _uiPanels[_uiStack.Peek()].OnFocus();
            }
            if (_enableDebugMode)
            {
                Debug.Log($"隐藏UI界面: {uiName}");
            }
            _uiPanels[uiName].OnClose();
            _layerMap[_uiPanels[uiName].layer]--;
            if (isDestroy)
            {
                GameObject.Destroy(_uiPanels[uiName].gameObject);
                _uiPanels.Remove(uiName);

            }
            else
            {
                _uiPanels[uiName].gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogError($"UI界面不存在: {uiName}");
        }
    }
    void RemoveMiddleElement(Stack<string> stack, string target)
    {
        Stack<string> tempStack = new Stack<string>();

        // 把元素一个一个弹出，放到临时栈中，直到找到目标
        while (stack.Count > 0)
        {
            string top = stack.Pop();
            if (top == target)
                break;

            tempStack.Push(top);
        }

        // 再把临时栈中的元素恢复回去
        while (tempStack.Count > 0)
        {
            stack.Push(tempStack.Pop());
        }
    }
    /// <summary>
    /// 隐藏所有UI界面
    /// </summary>
    public void HideAllUI()
    {
        foreach (var panel in _uiPanels.Values)
        {
            panel.gameObject.SetActive(false);
        }
        
      //  _uiStack.Clear();
        
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
    public void RegisterUI(string uiName, UIBase uiObject)
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
        canvasGO.transform.SetParent(gameObject.transform, false);

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
                PropertyInfo referenceResolutionProp = scalerType.GetProperty("referenceResolution");

                if (referenceResolutionProp != null)
                {
                    referenceResolutionProp.SetValue(scaler, new Vector2(Screen.width, Screen.height), null);
                }
                else
                {
                    Debug.LogError("未找到 referenceResolution 属性！");
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
        //if (_mainMenuPrefab != null)
        //{
        //    GameObject mainMenu = Instantiate(_mainMenuPrefab, _uiRoot);
        //    RegisterUI(UIConst.MainMenu, mainMenu);
        //}

        //if (_gameHUDPrefab != null)
        //{
        //    GameObject gameHUD = Instantiate(_gameHUDPrefab, _uiRoot);
        //    RegisterUI(UIConst.GameHUD, gameHUD);
        //}

        //if (_pauseMenuPrefab != null)
        //{
        //    GameObject pauseMenu = Instantiate(_pauseMenuPrefab, _uiRoot);
        //    RegisterUI(UIConst.PauseMenu, pauseMenu);
        //}

        //if (_loadingScreenPrefab != null)
        //{
        //    GameObject loadingScreen = Instantiate(_loadingScreenPrefab, _uiRoot);
        //    RegisterUI(UIConst.LoadingScreen, loadingScreen);
        //}

        if (_enableDebugMode)
        {
            Debug.Log("UI界面设置完成");
        }
    }
    #endregion
}

public enum UIConst
{
     MainMenu = 1,
     GameHUD = 2,
     PauseMenu = 3,
     LoadingScreen = 4,
}
