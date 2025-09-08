using System;
using System.Collections.Generic;
using System.Linq;
using FanXing.Data;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ũ��UI����������������ѡ�����ؿ��ء��������ƹ���
/// </summary>
public class FarmUIManager : MonoBehaviour
{
    [Header("UI���� - �������")]
    [SerializeField] private GameObject _cropSelectionPanel; // ����ѡ�����
    [SerializeField] private GameObject _landExpansionPanel; // ���ؿ������
    [SerializeField] private GameObject _weatherControlPanel; // ����������������壨�뿪�����ͬ����

    [Header("UI���� - ��פ��ť��ͬ�����У�")]
    [SerializeField] private Button _openCropSelectionBtn;    // ������ѡ��İ�ť
    [SerializeField] private Button _openExpansionBtn;        // �򿪿������İ�ť
    [SerializeField] private Button _openWeatherControlBtn;   // ���������������İ�ť���뿪�ذ�ťͬ����

    [Header("UI���� - ����ѡ��������")]
    [SerializeField] private Button _closeCropBtn;            // �ر�����ѡ��İ�ť
    [SerializeField] private TextMeshProUGUI _currentCropText;   // ��ǰѡ�������ı�
    [SerializeField] private Transform _cropButtonContainer;   // ���ﰴť����
    [SerializeField] private GameObject _cropButtonPrefab;     // ���ﰴťԤ����

    [Header("UI���� - ���ؿ���������")]
    [SerializeField] private Button _closeExpansionBtn;        // �رտ������İ�ť
    [SerializeField] private Button _expandLandBtn;           // �������صİ�ť
    [SerializeField] private TextMeshProUGUI _expansionCostText; // ���سɱ��ı�

    [Header("UI���� - ��������������������+Ǩ�ƣ�")]
    [SerializeField] private Button _closeWeatherBtn;         // �������ر��������İ�ť
    [SerializeField] private TextMeshProUGUI _currentWeatherText; // Ǩ�ƣ���ǰ������ʾ�ı���ԭ������ʾ������ڣ�
    [SerializeField] private Transform _weatherSwitchContainer; // �����������л���ť����
    [SerializeField] private GameObject _weatherSwitchPrefab;   // �����������л���ťԤ����

    [Header("����")]
    [SerializeField] private int _landExpansionCost = 50;     // ÿ�ο������صĳɱ�
    [SerializeField] private int _maxExpandablePlots = 5;      // ÿ�ο��ؿɽ��������ؿ���
    //[SerializeField] private int _weatherSwitchCost = 30;      // �������ֶ��л����������ģ���ң�

    private WeatherSystem _weatherSystem;
    private EconomySystem _economySystem;
    private Dictionary<WeatherSystem.WeatherType, string> _weatherNameTranslations = new Dictionary<WeatherSystem.WeatherType, string>();
    private List<Button> _weatherSwitchButtons = new List<Button>(); // �����������л���ť�б�
    private PlotInteractionManager _plotManager;
    private FarmingSystem _farmingSystem;
    private CropType _selectedCropType;
    private List<Button> _cropButtons = new List<Button>();
    private int _availableFunds; // ʾ���ʽ�ʵ����Ŀ��Ӧ�Ӿ���ϵͳ��ȡ

    private void Start()
    {
        // ��ȡϵͳ����
        _plotManager = FindObjectOfType<PlotInteractionManager>();
        _farmingSystem = Global.Farming;
        _economySystem = Global.Economy;

        if (_plotManager == null || _farmingSystem == null)
        {
            Debug.LogError("[FarmUIManager] �Ҳ���PlotInteractionManager��FarmingSystem�����");
            enabled = false;
            return;
        }

        // ��ȡ����ϵͳ����
        _weatherSystem = FindObjectOfType<WeatherSystem>();
        if (_weatherSystem == null)
        {
            Debug.LogWarning("[FarmUIManager] �Ҳ���WeatherSystem������������ܽ��޷�ʹ��");
            _openWeatherControlBtn.interactable = false; // ����������ť
        }
        else
        {
            // ��ʼ���������� + �л���ť
            InitWeatherTranslations();
            InitWeatherSwitchButtons();
            // ��ʼ��ʾ�������ı���Ǩ�Ƶ�����ڣ�
            UpdateWeatherDisplay();
            // ���������仯�¼�
            WeatherSystem.OnWeatherChanged += OnWeatherChanged;
        }

        // ��ʼ��ѡ�е�����ΪĬ������
        _selectedCropType = _plotManager.DefaultPlantCrop;

        // ע�����а�ť�¼���������������ť��
        RegisterButtonEvents();

        // ��ʼ������ѡ��ť
        InitCropSelectionButtons();

        // ����UI��ʾ
        UpdateCurrentCropText();
        UpdateExpansionCostText();

        // ��ʼ�������й������
        _cropSelectionPanel.SetActive(false);
        _landExpansionPanel.SetActive(false);
        _weatherControlPanel.SetActive(false); // ������Ĭ�������������
    }

    /// <summary>
    /// ע������UI��ť�¼�������������ť����¼���
    /// </summary>
    private void RegisterButtonEvents()
    {
        // ԭ�а�ť�¼�
        _openCropSelectionBtn.onClick.AddListener(ShowCropSelectionPanel);
        _closeCropBtn.onClick.AddListener(HideCropSelectionPanel);
        _openExpansionBtn.onClick.AddListener(ShowLandExpansionPanel);
        _closeExpansionBtn.onClick.AddListener(HideLandExpansionPanel);
        _expandLandBtn.onClick.AddListener(OnExpandLandClicked);

        // ������������尴ť�¼�
        _openWeatherControlBtn.onClick.AddListener(ShowWeatherControlPanel);
        _closeWeatherBtn.onClick.AddListener(HideWeatherControlPanel);
    }

    /// <summary>
    /// ��������ʼ�������л���ť��������ֶ��л��ã�
    /// </summary>
    private void InitWeatherSwitchButtons()
    {
        // ������а�ť�������ظ�����
        foreach (var btn in _weatherSwitchButtons)
        {
            Destroy(btn.gameObject);
        }
        _weatherSwitchButtons.Clear();

        // ��ȡ�����������ͣ��ų���Ч���ͣ�
        var allWeatherTypes = Enum.GetValues(typeof(WeatherSystem.WeatherType))
            .Cast<WeatherSystem.WeatherType>()
            .Where(type => type != WeatherSystem.WeatherType.Sunny ? type != 0 : true) // ������None��ö��
            .ToList();

        // Ϊÿ�����������л���ť
        foreach (var weatherType in allWeatherTypes)
        {
            // ��ȡ��������������
            string translatedName = _weatherNameTranslations.TryGetValue(weatherType, out var name)
                ? name
                : weatherType.ToString();

            // ʵ������ť������Ԥ���壩
            GameObject btnObj = Instantiate(_weatherSwitchPrefab, _weatherSwitchContainer);
            Button switchBtn = btnObj.GetComponent<Button>();
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();

            // ���ð�ť�ı�����ʾ����+���ģ�
            if (btnText != null)
            {
                btnText.text = $"{translatedName}";
            }

            // ���浱ǰ�������ͣ��հ��貶��
            WeatherSystem.WeatherType targetType = weatherType;

            // ע���л��¼�
            switchBtn.onClick.AddListener(() => OnSwitchWeatherClicked(targetType));

            // ��ӵ���ť�б�
            _weatherSwitchButtons.Add(switchBtn);

            // ���õ�ǰ�����İ�ť�������ظ��л���
            if (targetType == _weatherSystem.CurrentWeather)
            {
                switchBtn.interactable = false;
                btnText.text += "\n(��ǰ����)";
            }
        }
    }

    /// <summary>
    /// �������ֶ��л������ĵ���߼�
    /// </summary>
    private void OnSwitchWeatherClicked(WeatherSystem.WeatherType targetWeather)
    {
        // 1. У��ϵͳ״̬
        if (_weatherSystem == null || !_weatherSystem.IsRunning)
        {
            ShowNotification("����ϵͳδ���У��޷��л���");
            return;
        }

        // 2. У���Ƿ�Ϊ��ǰ����
        if (targetWeather == _weatherSystem.CurrentWeather)
        {
            ShowNotification($"��ǰ����{_weatherNameTranslations[targetWeather]}�������л���");
            return;
        }


        _weatherSystem.ForceSwitchWeather(targetWeather); // ǿ���л�����

        // 5. ����UI����
        RefreshWeatherSwitchButtons();
        ShowNotification($"�ɹ��л�Ϊ{_weatherNameTranslations[targetWeather]}");
    }

    /// <summary>
    /// ������ˢ�������л���ť״̬�����õ�ǰ������ť��
    /// </summary>
    private void RefreshWeatherSwitchButtons()
    {
        if (_weatherSystem == null || _weatherSwitchButtons.Count == 0) return;

        var currentWeather = _weatherSystem.CurrentWeather;
        var allWeatherTypes = Enum.GetValues(typeof(WeatherSystem.WeatherType))
            .Cast<WeatherSystem.WeatherType>()
            .Where(type => type != WeatherSystem.WeatherType.Sunny ? type != 0 : true)
            .ToList();

        // ������ť����״̬
        for (int i = 0; i < _weatherSwitchButtons.Count; i++)
        {
            var btn = _weatherSwitchButtons[i];
            var btnText = btn.GetComponentInChildren<TextMeshProUGUI>();
            var weatherType = allWeatherTypes[i];

            if (weatherType == currentWeather)
            {
                btn.interactable = false;
                btnText.text = $"{_weatherNameTranslations[weatherType]}\n(��ǰ����)";
            }
            else
            {
                btn.interactable = true;
                btnText.text = $"{_weatherNameTranslations[weatherType]}";
            }
        }
    }

    /// <summary>
    /// �����仯ʱ���£�������ͬ��ˢ�°�ť״̬��
    /// </summary>
    private void OnWeatherChanged(WeatherSystem.WeatherType newWeather, WeatherSystem.WeatherConfig config)
    {
        UpdateWeatherDisplay();
        RefreshWeatherSwitchButtons(); // �����Զ��仯ʱ��ͬ�����°�ť״̬
    }

    private void InitCropSelectionButtons()
    {
        // ����ɰ�ť
        foreach (var btn in _cropButtons) Destroy(btn.gameObject);
        _cropButtons.Clear();

        // �ؼ��޸ģ�ֱ�Ӵ� FarmingSystem ��ȡ������Ч�������ͣ������� _cropConfigMap �ֵ䣩
        // _cropConfigMap �� <CropType, CropData> �ֵ䣬���������������õ���������
        var validCropTypes = _farmingSystem.GetAllValidCropTypes();
        if (validCropTypes.Count == 0)
        {
            Debug.LogWarning("[FarmUIManager] FarmingSystem ��δ�����κ���Ч����");
            return;
        }

        // ����������Ч�����������ɰ�ť
        foreach (var cropType in validCropTypes)
        {
            // �� FarmingSystem ��ȡ�������ã�ȷ�����ô��ڣ�
            var cropData = _farmingSystem.GetCropConfig(cropType);
            if (cropData == null) continue;

            // ���ɰ�ť
            GameObject btnObj = Instantiate(_cropButtonPrefab, _cropButtonContainer);
            Button cropBtn = btnObj.GetComponent<Button>();
            TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();

            // ���ð�ť�ı�����ʾ�������ƺ�����ʱ�䣩
            if (btnText != null)
            {
                btnText.text = $"{cropData.Name}\n({cropData.GrowthTime}s)";
            }

            // ���浱ǰ�������ͣ��հ����񣬱���ѭ���������⣩
            CropType currentType = cropType;

            // ע�����¼�
            cropBtn.onClick.AddListener(() => OnCropSelected(currentType));

            // ��ӵ���ť�б�
            _cropButtons.Add(cropBtn);

            // ��ʼ��ʱ������ѡ�е����ﰴť
            if (currentType == _selectedCropType)
            {
                SetButtonAsSelected(cropBtn);
            }
        }
    }

    private void OnCropSelected(CropType cropType)
    {
        // ����ѡ�е���������
        _selectedCropType = cropType;
        _plotManager.SetSelectedCrop(cropType);

        // 1. �Ƚ����а�ť��Ϊ��δѡ��+�ɽ�����
        foreach (var btn in _cropButtons)
        {
            SetButtonAsDeselected(btn);
        }

        // 2. �ؼ��޸ģ�ͨ�� FarmingSystem �����б��밴ť�б�ġ�������Ӧ���ҵ�Ŀ�갴ť
        // �� FarmingSystem ��ȡ��Ч���������б��� Init ʱ˳����ȫһ�£�
        var validCropTypes = _farmingSystem.GetAllValidCropTypes();
        // �ҵ���ǰѡ�������������б��е�����
        int targetIndex = validCropTypes.IndexOf(cropType);

        // �����Ϸ�����ö�Ӧ��ť�����ԭ�߼�������λ���⣩
        if (targetIndex >= 0 && targetIndex < _cropButtons.Count)
        {
            SetButtonAsSelected(_cropButtons[targetIndex]);
        }

        // ���µ�ǰ������ʾ�ı�
        UpdateCurrentCropText();
    }



    private void OnExpandLandClicked()
    {
        if (!_economySystem.HasEnoughGold(_landExpansionCost)) { ShowNotification("�ʽ��㣬�޷��������أ�"); return; }

        var allPositions = _farmingSystem.GetAllPlotPositions();
        var activePositions = _farmingSystem.GetAllActivePlots().Select(p => p.Position).ToList();
        var lockedPositions = allPositions.Where(p => !activePositions.Contains(p)).ToList();

        if (lockedPositions.Count == 0) { ShowNotification("û�пɿ��ص������ˣ�"); return; }

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
        ShowNotification($"�ɹ����� {plotsToUnlock} �����أ�");
        UpdateExpansionCostText();
    }

    private void UpdateCurrentCropText()
    {
        var cropData = _farmingSystem.GetCropConfig(_selectedCropType);
        _currentCropText.text = $"��ǰ����: {cropData?.Name ?? "δ֪����"}";
    }

    private void UpdateExpansionCostText()
    {
        _expansionCostText.text = $"���سɱ�: {_landExpansionCost} ���";
    }

    private void ShowNotification(string message)
    {
        Debug.Log($"[֪ͨ] {message}");
    }

    private void SetButtonAsSelected(Button button)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.5f, 1f, 0.5f);
        colors.highlightedColor = new Color(0.6f, 1f, 0.6f);
        button.colors = colors;
        button.interactable = false;
        //btnText.text += "\n(��ǰ����)";
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
        _weatherNameTranslations.Add(WeatherSystem.WeatherType.Sunny, "����");
        _weatherNameTranslations.Add(WeatherSystem.WeatherType.HeavyRain, "����");
        _weatherNameTranslations.Add(WeatherSystem.WeatherType.Drought, "�ɺ�");
    }

    private void UpdateWeatherDisplay()
    {
        if (_weatherSystem == null || _currentWeatherText == null) return;

        var currentWeather = _weatherSystem.CurrentWeather;
        string weatherName = _weatherNameTranslations.TryGetValue(currentWeather, out var name) ? name : currentWeather.ToString();
        var weatherConfig = _weatherSystem.GetWeatherConfigByType(currentWeather);

        _currentWeatherText.text = weatherConfig != null
            ? $"��ǰ����: {weatherName}\n{weatherConfig.WeatherDesc}"
            : $"��ǰ����: {weatherName}";
    }

    // ---------------------- �����ʾ���ƣ�����������巽���� ----------------------
    public void ShowCropSelectionPanel() => _cropSelectionPanel.SetActive(true);
    public void HideCropSelectionPanel() => _cropSelectionPanel.SetActive(false);
    public void ShowLandExpansionPanel() => _landExpansionPanel.SetActive(true);
    public void HideLandExpansionPanel() => _landExpansionPanel.SetActive(false);
    public void ShowWeatherControlPanel() => _weatherControlPanel.SetActive(true); // ����
    public void HideWeatherControlPanel() => _weatherControlPanel.SetActive(false); // ����

    private void OnDestroy()
    {
        WeatherSystem.OnWeatherChanged -= OnWeatherChanged;
    }
}