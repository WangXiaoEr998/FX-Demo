using System;
using System.Collections.Generic;
using System.Linq;
using FanXing.Data;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 农场UI管理器：处理作物选择、土地开拓、天气控制功能
/// </summary>
public class FarmUIManager : MonoBehaviour
{
    [Header("UI引用 - 功能面板")]
    [SerializeField] private GameObject _cropSelectionPanel; // 作物选择面板
    [SerializeField] private GameObject _landExpansionPanel; // 土地开拓面板
    [SerializeField] private GameObject _weatherControlPanel; // 新增：天气控制面板（与开拓面板同级）

    [Header("UI引用 - 常驻按钮（同级排列）")]
    [SerializeField] private Button _openCropSelectionBtn;    // 打开作物选择的按钮
    [SerializeField] private Button _openExpansionBtn;        // 打开开拓面板的按钮
    [SerializeField] private Button _openWeatherControlBtn;   // 新增：打开天气面板的按钮（与开拓按钮同级）

    [Header("UI引用 - 作物选择面板组件")]
    [SerializeField] private Button _closeCropBtn;            // 关闭作物选择的按钮
    [SerializeField] private TextMeshProUGUI _currentCropText;   // 当前选中作物文本
    [SerializeField] private Transform _cropButtonContainer;   // 作物按钮容器
    [SerializeField] private GameObject _cropButtonPrefab;     // 作物按钮预制体

    [Header("UI引用 - 土地开拓面板组件")]
    [SerializeField] private Button _closeExpansionBtn;        // 关闭开拓面板的按钮
    [SerializeField] private Button _expandLandBtn;           // 开拓土地的按钮
    [SerializeField] private TextMeshProUGUI _expansionCostText; // 开拓成本文本

    [Header("UI引用 - 天气控制面板组件（新增+迁移）")]
    [SerializeField] private Button _closeWeatherBtn;         // 新增：关闭天气面板的按钮
    [SerializeField] private TextMeshProUGUI _currentWeatherText; // 迁移：当前天气显示文本（原单独显示→面板内）
    [SerializeField] private Transform _weatherSwitchContainer; // 新增：天气切换按钮容器
    [SerializeField] private GameObject _weatherSwitchPrefab;   // 新增：天气切换按钮预制体

    [Header("配置")]
    [SerializeField] private int _landExpansionCost = 50;     // 每次开拓土地的成本
    [SerializeField] private int _maxExpandablePlots = 5;      // 每次开拓可解锁的最大地块数
    //[SerializeField] private int _weatherSwitchCost = 30;      // 新增：手动切换天气的消耗（金币）

    private WeatherSystem _weatherSystem;
    private EconomySystem _economySystem;
    private Dictionary<WeatherSystem.WeatherType, string> _weatherNameTranslations = new Dictionary<WeatherSystem.WeatherType, string>();
    private List<Button> _weatherSwitchButtons = new List<Button>(); // 新增：天气切换按钮列表
    private PlotInteractionManager _plotManager;
    private FarmingSystem _farmingSystem;
    private CropType _selectedCropType;
    private List<Button> _cropButtons = new List<Button>();
    private int _availableFunds; // 示例资金，实际项目中应从经济系统获取

    private void Start()
    {
        // 获取系统引用
        _plotManager = FindObjectOfType<PlotInteractionManager>();
        _farmingSystem = Global.Farming;
        _economySystem = Global.Economy;

        if (_plotManager == null || _farmingSystem == null)
        {
            Debug.LogError("[FarmUIManager] 找不到PlotInteractionManager或FarmingSystem组件！");
            enabled = false;
            return;
        }

        // 获取天气系统引用
        _weatherSystem = FindObjectOfType<WeatherSystem>();
        if (_weatherSystem == null)
        {
            Debug.LogWarning("[FarmUIManager] 找不到WeatherSystem组件，天气功能将无法使用");
            _openWeatherControlBtn.interactable = false; // 禁用天气按钮
        }
        else
        {
            // 初始化天气翻译 + 切换按钮
            InitWeatherTranslations();
            InitWeatherSwitchButtons();
            // 初始显示天气（文本已迁移到面板内）
            UpdateWeatherDisplay();
            // 监听天气变化事件
            WeatherSystem.OnWeatherChanged += OnWeatherChanged;
        }

        // 初始化选中的作物为默认作物
        _selectedCropType = _plotManager.DefaultPlantCrop;

        // 注册所有按钮事件（含新增天气按钮）
        RegisterButtonEvents();

        // 初始化作物选择按钮
        InitCropSelectionButtons();

        // 更新UI显示
        UpdateCurrentCropText();
        UpdateExpansionCostText();

        // 初始隐藏所有功能面板
        _cropSelectionPanel.SetActive(false);
        _landExpansionPanel.SetActive(false);
        _weatherControlPanel.SetActive(false); // 新增：默认隐藏天气面板
    }

    /// <summary>
    /// 注册所有UI按钮事件（新增天气按钮相关事件）
    /// </summary>
    private void RegisterButtonEvents()
    {
        // 原有按钮事件
        _openCropSelectionBtn.onClick.AddListener(ShowCropSelectionPanel);
        _closeCropBtn.onClick.AddListener(HideCropSelectionPanel);
        _openExpansionBtn.onClick.AddListener(ShowLandExpansionPanel);
        _closeExpansionBtn.onClick.AddListener(HideLandExpansionPanel);
        _expandLandBtn.onClick.AddListener(OnExpandLandClicked);

        // 新增：天气面板按钮事件
        _openWeatherControlBtn.onClick.AddListener(ShowWeatherControlPanel);
        _closeWeatherBtn.onClick.AddListener(HideWeatherControlPanel);
    }

    /// <summary>
    /// 新增：初始化天气切换按钮（面板内手动切换用）
    /// </summary>
    private void InitWeatherSwitchButtons()
    {
        // 清除现有按钮，避免重复生成
        foreach (var btn in _weatherSwitchButtons)
        {
            Destroy(btn.gameObject);
        }
        _weatherSwitchButtons.Clear();

        // 获取所有天气类型（排除无效类型）
        var allWeatherTypes = Enum.GetValues(typeof(WeatherSystem.WeatherType))
            .Cast<WeatherSystem.WeatherType>()
            .Where(type => type != WeatherSystem.WeatherType.Sunny ? type != 0 : true) // 兼容无None的枚举
            .ToList();

        // 为每种天气生成切换按钮
        foreach (var weatherType in allWeatherTypes)
        {
            // 获取翻译后的天气名称
            string translatedName = _weatherNameTranslations.TryGetValue(weatherType, out var name)
                ? name
                : weatherType.ToString();

            // 实例化按钮（复用预制体）
            GameObject btnObj = Instantiate(_weatherSwitchPrefab, _weatherSwitchContainer);
            Button switchBtn = btnObj.GetComponent<Button>();
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();

            // 设置按钮文本（显示名称+消耗）
            if (btnText != null)
            {
                btnText.text = $"{translatedName}";
            }

            // 保存当前天气类型（闭包需捕获）
            WeatherSystem.WeatherType targetType = weatherType;

            // 注册切换事件
            switchBtn.onClick.AddListener(() => OnSwitchWeatherClicked(targetType));

            // 添加到按钮列表
            _weatherSwitchButtons.Add(switchBtn);

            // 禁用当前天气的按钮（避免重复切换）
            if (targetType == _weatherSystem.CurrentWeather)
            {
                switchBtn.interactable = false;
                btnText.text += "\n(当前天气)";
            }
        }
    }

    /// <summary>
    /// 新增：手动切换天气的点击逻辑
    /// </summary>
    private void OnSwitchWeatherClicked(WeatherSystem.WeatherType targetWeather)
    {
        // 1. 校验系统状态
        if (_weatherSystem == null || !_weatherSystem.IsRunning)
        {
            ShowNotification("天气系统未运行，无法切换！");
            return;
        }

        // 2. 校验是否为当前天气
        if (targetWeather == _weatherSystem.CurrentWeather)
        {
            ShowNotification($"当前已是{_weatherNameTranslations[targetWeather]}，无需切换！");
            return;
        }


        _weatherSystem.ForceSwitchWeather(targetWeather); // 强制切换天气

        // 5. 更新UI反馈
        RefreshWeatherSwitchButtons();
        ShowNotification($"成功切换为{_weatherNameTranslations[targetWeather]}");
    }

    /// <summary>
    /// 新增：刷新天气切换按钮状态（禁用当前天气按钮）
    /// </summary>
    private void RefreshWeatherSwitchButtons()
    {
        if (_weatherSystem == null || _weatherSwitchButtons.Count == 0) return;

        var currentWeather = _weatherSystem.CurrentWeather;
        var allWeatherTypes = Enum.GetValues(typeof(WeatherSystem.WeatherType))
            .Cast<WeatherSystem.WeatherType>()
            .Where(type => type != WeatherSystem.WeatherType.Sunny ? type != 0 : true)
            .ToList();

        // 遍历按钮更新状态
        for (int i = 0; i < _weatherSwitchButtons.Count; i++)
        {
            var btn = _weatherSwitchButtons[i];
            var btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
            var weatherType = allWeatherTypes[i];

            if (weatherType == currentWeather)
            {
                btn.interactable = false;
                btnText.text = $"{_weatherNameTranslations[weatherType]}\n(当前天气)";
            }
            else
            {
                btn.interactable = true;
                btnText.text = $"{_weatherNameTranslations[weatherType]}";
            }
        }
    }

    /// <summary>
    /// 天气变化时更新（新增：同步刷新按钮状态）
    /// </summary>
    private void OnWeatherChanged(WeatherSystem.WeatherType newWeather, WeatherSystem.WeatherConfig config)
    {
        UpdateWeatherDisplay();
        RefreshWeatherSwitchButtons(); // 天气自动变化时，同步更新按钮状态
    }

    private void InitCropSelectionButtons()
    {
        // 清除旧按钮
        foreach (var btn in _cropButtons) Destroy(btn.gameObject);
        _cropButtons.Clear();

        // 关键修改：直接从 FarmingSystem 获取所有有效作物类型（复用其 _cropConfigMap 字典）
        // _cropConfigMap 是 <CropType, CropData> 字典，键就是所有已配置的作物类型
        var validCropTypes = _farmingSystem.GetAllValidCropTypes();
        if (validCropTypes.Count == 0)
        {
            Debug.LogWarning("[FarmUIManager] FarmingSystem 中未配置任何有效作物");
            return;
        }

        // 遍历所有有效作物类型生成按钮
        foreach (var cropType in validCropTypes)
        {
            // 从 FarmingSystem 获取作物配置（确保配置存在）
            var cropData = _farmingSystem.GetCropConfig(cropType);
            if (cropData == null) continue;

            // 生成按钮
            GameObject btnObj = Instantiate(_cropButtonPrefab, _cropButtonContainer);
            Button cropBtn = btnObj.GetComponent<Button>();
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();

            // 设置按钮文本（显示作物名称和生长时间）
            if (btnText != null)
            {
                btnText.text = $"{cropData.Name}\n({cropData.GrowthTime}s)";
            }

            // 保存当前作物类型（闭包捕获，避免循环变量问题）
            CropType currentType = cropType;

            // 注册点击事件
            cropBtn.onClick.AddListener(() => OnCropSelected(currentType));

            // 添加到按钮列表
            _cropButtons.Add(cropBtn);

            // 初始化时禁用已选中的作物按钮
            if (currentType == _selectedCropType)
            {
                SetButtonAsSelected(cropBtn);
            }
        }
    }

    private void OnCropSelected(CropType cropType)
    {
        // 更新选中的作物类型
        _selectedCropType = cropType;
        _plotManager.SetSelectedCrop(cropType);

        // 1. 先将所有按钮设为“未选中+可交互”
        foreach (var btn in _cropButtons)
        {
            SetButtonAsDeselected(btn);
        }

        // 2. 关键修改：通过 FarmingSystem 作物列表与按钮列表的“索引对应”找到目标按钮
        // 从 FarmingSystem 获取有效作物类型列表（与 Init 时顺序完全一致）
        var validCropTypes = _farmingSystem.GetAllValidCropTypes();
        // 找到当前选中作物类型在列表中的索引
        int targetIndex = validCropTypes.IndexOf(cropType);

        // 索引合法则禁用对应按钮（解决原逻辑索引错位问题）
        if (targetIndex >= 0 && targetIndex < _cropButtons.Count)
        {
            SetButtonAsSelected(_cropButtons[targetIndex]);
        }

        // 更新当前作物显示文本
        UpdateCurrentCropText();
    }



    private void OnExpandLandClicked()
    {
        if (!_economySystem.HasEnoughGold(_landExpansionCost)) { ShowNotification("资金不足，无法开拓土地！"); return; }

        var allPositions = _farmingSystem.GetAllPlotPositions();
        var activePositions = _farmingSystem.GetAllActivePlots().Select(p => p.Position).ToList();
        var lockedPositions = allPositions.Where(p => !activePositions.Contains(p)).ToList();

        if (lockedPositions.Count == 0) { ShowNotification("没有可开拓的土地了！"); return; }

        int plotsToUnlock = Mathf.Min(_maxExpandablePlots, lockedPositions.Count);
        for (int i = 0; i < plotsToUnlock; i++)
        {
            Vector3Int plotPos = lockedPositions[i];
            FarmingSystem.FarmPlot newPlot = new FarmingSystem.FarmPlot(plotPos) { SoilState = PlotState.Unlocked_Empty };
            _farmingSystem.AddNewPlot(newPlot);
            _farmingSystem.SyncSoilState(plotPos, PlotState.Unlocked_Empty);
        }

        //_availableFunds -= _landExpansionCost;
        _economySystem.SpendGold(_landExpansionCost);
        ShowNotification($"成功开拓 {plotsToUnlock} 块土地！");
        UpdateExpansionCostText();
    }

    private void UpdateCurrentCropText()
    {
        var cropData = _farmingSystem.GetCropConfig(_selectedCropType);
        _currentCropText.text = $"当前作物: {cropData?.Name ?? "未知作物"}";
    }

    private void UpdateExpansionCostText()
    {
        _expansionCostText.text = $"开拓成本: {_landExpansionCost} 金币";
    }

    private void ShowNotification(string message)
    {
        Debug.Log($"[通知] {message}");
    }

    private void SetButtonAsSelected(Button button)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.5f, 1f, 0.5f);
        colors.highlightedColor = new Color(0.6f, 1f, 0.6f);
        button.colors = colors;
        button.interactable = false;
        //btnText.text += "\n(当前天气)";
    }

    private void SetButtonAsDeselected(Button button)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(1f, 1f, 1f);
        colors.highlightedColor = new Color(0.8f, 0.8f, 0.8f);
        button.colors = colors;
        button.interactable = true;
    }

    private void InitWeatherTranslations()
    {
        _weatherNameTranslations.Clear();
        _weatherNameTranslations.Add(WeatherSystem.WeatherType.Sunny, "晴天");
        _weatherNameTranslations.Add(WeatherSystem.WeatherType.HeavyRain, "大雨");
        _weatherNameTranslations.Add(WeatherSystem.WeatherType.Drought, "干旱");
    }

    private void UpdateWeatherDisplay()
    {
        if (_weatherSystem == null || _currentWeatherText == null) return;

        var currentWeather = _weatherSystem.CurrentWeather;
        string weatherName = _weatherNameTranslations.TryGetValue(currentWeather, out var name) ? name : currentWeather.ToString();
        var weatherConfig = _weatherSystem.GetWeatherConfigByType(currentWeather);

        _currentWeatherText.text = weatherConfig != null
            ? $"当前天气: {weatherName}\n{weatherConfig.WeatherDesc}"
            : $"当前天气: {weatherName}";
    }

    // ---------------------- 面板显示控制（新增天气面板方法） ----------------------
    public void ShowCropSelectionPanel() => _cropSelectionPanel.SetActive(true);
    public void HideCropSelectionPanel() => _cropSelectionPanel.SetActive(false);
    public void ShowLandExpansionPanel() => _landExpansionPanel.SetActive(true);
    public void HideLandExpansionPanel() => _landExpansionPanel.SetActive(false);
    public void ShowWeatherControlPanel() => _weatherControlPanel.SetActive(true); // 新增
    public void HideWeatherControlPanel() => _weatherControlPanel.SetActive(false); // 新增

    private void OnDestroy()
    {
        WeatherSystem.OnWeatherChanged -= OnWeatherChanged;
    }
}