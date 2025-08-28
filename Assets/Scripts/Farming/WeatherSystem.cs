using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ����ϵͳ���������������л������ʼ��㡢״̬�������� BaseGameSystem ���ࣩ
/// </summary>
public class WeatherSystem : BaseGameSystem
{
    #region �����ò�����Inspector���ӻ�������
    [Header("=== ����ϵͳ���� ===")]
    [Tooltip("�����л�����С��������ӣ���ֹƵ���л���")]
    [SerializeField] private float _minWeatherDuration = 5f;
    [Tooltip("�����л�������������ӣ�")]
    [SerializeField] private float _maxWeatherDuration = 15f;
    [Tooltip("�Ƿ���ϵͳ����ʱ��������һ������")]
    [SerializeField] private bool _triggerWeatherOnStart = true;

    [Header("=== �����������ã�������Ȩ������===")]
    [SerializeField] private List<WeatherConfig> _weatherConfigs; // ���������б������ʣ�
    #endregion

    #region �ڲ�״̬����
    private WeatherType _currentWeatherType; // ��ǰ��������
    private float _currentWeatherEndTime;    // ��ǰ�����Ľ���ʱ�䣨Time.time��
    private List<WeatherWeightData> _weightedWeathers; // ��Ȩ�����б����ڸ��ʼ��㣩
    private float _totalWeight;              // ����������Ȩ���ܺͣ����ʼ����ã�
    #endregion

    #region �¼����壨���ⲿϵͳ����������������
    /// <summary>
    /// �����仯�¼������������������͡��������ã�
    /// </summary>
    public static event Action<WeatherType, WeatherConfig> OnWeatherChanged;
    #endregion

    #region ���ݽṹ���壨�������������ͣ�
    /// <summary>
    /// ��������ö�٣�����չ��ѩ����ɳ�����ȣ�
    /// </summary>
    public enum WeatherType
    {
        Sunny,    // ���죨Ĭ�ϣ�
        HeavyRain,// ����
        Drought   // �ɺ�
    }

    /// <summary>
    /// �������ã� Inspector �����ã�������Ȩ�أ�
    /// </summary>
    [Serializable]
    public class WeatherConfig
    {
        public WeatherType WeatherType;       // ��������
        [Range(1, 100)] public int ProbabilityWeight; // ����Ȩ�أ�ֵԽ�󣬳��ָ���Խ�ߣ�
        public string WeatherDesc;            // �����������硰������㣬�ʺ�������������
        [Tooltip("�������Ķ�������������Ľ�ˮ�����ɺ��ĸ���ȣ�Ԥ����չ��")]
        public float WeatherParam;            // Ԥ������������ʹ�ã�
    }

    /// <summary>
    /// ��Ȩ�������ݣ��ڲ����ʼ����ã�
    /// </summary>
    private class WeatherWeightData
    {
        public WeatherType WeatherType; // ��������
        public WeatherConfig Config;    // ��Ӧ����
        public float MinWeight;         // Ȩ��������Сֵ
        public float MaxWeight;         // Ȩ���������ֵ
    }
    #endregion

    #region ϵͳ�������ԣ����䵱ǰ������ѯ��
    public WeatherType CurrentWeather => _currentWeatherType; // �ⲿ��ȡ��ǰ����
    public WeatherConfig CurrentWeatherConfig { get; private set; } // �ⲿ��ȡ��ǰ��������
    #endregion

    #region ��������������д���ؼ�����ѭ Initialize �� Start �� Update ���̣�
    /// <summary>
    /// ϵͳ��ʼ����������� Initialize() ʱ�����������ֶ����ã�
    /// </summary>
    protected override void OnInitialize()
    {
        LogDebug("=== ����ϵͳ��ʼ����ʼ��OnInitialize��===");

        // 1. У�����ã�������б�Ȩ��Ϊ0��ȱ��Ĭ��������
        ValidateWeatherConfig();
        // 2. ��ʼ����Ȩ�����б����ʼ�����ģ�
        InitWeightedWeatherList();
        // 3. ��ʼ����ǰ����״̬��Ĭ�����죩
        _currentWeatherType = WeatherType.HeavyRain;
        CurrentWeatherConfig = GetWeatherConfigByType(WeatherType.HeavyRain);

        LogDebug($"��ʼ����� | ��ǰ������{_currentWeatherType} | �л������{_minWeatherDuration}~{_maxWeatherDuration}����");
    }

    /// <summary>
    /// ϵͳ������������� Start() ʱ���������ڳ�ʼ����ִ�У�
    /// </summary>
    protected override void OnStart()
    {
        LogDebug("=== ����ϵͳ������OnStart��===");

        if (_triggerWeatherOnStart)
        {
            // ����ʱ��������һ�������������Ĭ�ϣ�
            TriggerRandomWeather(immediate: true);
        }
        else
        {
            // �����״���������ʱ�䣨����ת�룬Time.time ��λΪ�룩
            _currentWeatherEndTime = Time.time + GetRandomWeatherDuration();
            LogDebug($"�״������л���{_currentWeatherEndTime - Time.time:F1}���");
        }

        LogDebug("ϵͳ�������");
    }

    /// <summary>
    /// ϵͳ���£�������� UpdateSystem() ʱ�������Զ�������ͣ�߼���
    /// </summary>
    protected override void OnUpdate(float deltaTime)
    {
        // ��鵱ǰ�����Ƿ��ڣ��������л�������
        if (Time.time >= _currentWeatherEndTime)
        {
            TriggerRandomWeather(immediate: false);
        }
    }

    /// <summary>
    /// ϵͳ�ر�ʱ������д���෽���������¼��ڴ�й©��
    /// </summary>
    protected override void OnShutdown()
    {
        base.OnShutdown();
        // ȡ�������¼����ģ������ⲿ���õ����ڴ�й©��
        OnWeatherChanged = null;
        LogDebug("����ϵͳ�رգ��¼�������");
    }
    #endregion

    #region �����߼������ʼ���������л����¼�֪ͨ��
    /// <summary>
    /// ����������������ķ�����������ѡ�񣬱���������ͬ������
    /// </summary>
    /// <param name="immediate">�Ƿ�������Ч��true���޹��ɣ�false�����������̣�</param>
    private void TriggerRandomWeather(bool immediate)
    {
        // 1. ���������ѡ���������ų���ǰ���������������ظ���
        WeatherType newWeatherType = GetRandomWeatherExcludeCurrent();
        // 2. ��ȡ�����������ã����ף�δ�ҵ��������죩
        WeatherConfig newWeatherConfig = GetWeatherConfigByType(newWeatherType) ?? GetWeatherConfigByType(WeatherType.Sunny);

        // 3. ���µ�ǰ����״̬
        _currentWeatherType = newWeatherType;
        CurrentWeatherConfig = newWeatherConfig;

        // ���޸����ġ������Ƿ�������Ч�������������ĳ���ʱ��
        // ������Ч�������л���������ʱ��������5~15���ӣ�
        // ��������Ч�������������л�������ʱ������
        float newDuration = GetRandomWeatherDuration();
        _currentWeatherEndTime = Time.time + newDuration;

        // 5. ��־���
        string triggerDesc = immediate ? "������Ч" : "���ƻ��л�";
        LogDebug($"�����л���{triggerDesc}��| ��������{_currentWeatherType} | ������{newWeatherConfig.WeatherDesc} | �´��л���{newDuration:F1}���");

        // 6. ���������仯�¼�
        OnWeatherChanged?.Invoke(_currentWeatherType, newWeatherConfig);
    }


    /// <summary>
    /// ���������ѡ���������ų���ǰ���������������ظ���
    /// </summary>
    private WeatherType GetRandomWeatherExcludeCurrent()
    {
        WeatherType selectedType = _currentWeatherType;
        int maxRetry = 10; // ������Դ�������ֹ���������ѭ����
        int retryCount = 0;

        while (selectedType == _currentWeatherType && retryCount < maxRetry)
        {
            // ���� 0~��Ȩ�� ֮��������
            float randomValue = UnityEngine.Random.Range(0, _totalWeight);
            // ������Ȩ�б��ҵ���������ڵ�����
            foreach (var weightedWeather in _weightedWeathers)
            {
                if (randomValue >= weightedWeather.MinWeight && randomValue < weightedWeather.MaxWeight)
                {
                    selectedType = weightedWeather.WeatherType;
                    break;
                }
            }
            retryCount++;
        }

        // ���Գ�������ʱ��ǿ���л�Ϊ���죨�����߼���
        if (selectedType == _currentWeatherType)
        {

            LogWarning($"����ѡ�����Գ������ޣ�{maxRetry}�Σ���ǿ���л�Ϊ����");
            selectedType = WeatherType.Sunny;
        }

        return selectedType;
    }

    /// <summary>
    /// ��ȡ�������������ʱ�䣨����ת�룩
    /// </summary>
    private float GetRandomWeatherDuration()
    {
        float minutes = UnityEngine.Random.Range(_minWeatherDuration, _maxWeatherDuration);
        return minutes * 60; // ת��Ϊ�루Time.time ��λΪ�룩
    }
    #endregion

    #region ���߷���������У���Ȩ�ؼ�������ò�ѯ��
    /// <summary>
    /// У���������ã�������б�Ȩ��Ϊ0��ȱ��Ĭ��������
    /// </summary>
    private void ValidateWeatherConfig()
    {
        // 1. ��������б��Ƿ�Ϊ��
        if (_weatherConfigs == null || _weatherConfigs.Count == 0)
        {
            throw new Exception("���������б���Ϊ�գ�����Inspector���������һ�������������죩");
        }

        // 2. ����Ƿ�����������ã�����������������ڣ�
        bool hasSunny = _weatherConfigs.Exists(cfg => cfg.WeatherType == WeatherType.Sunny);
        if (!hasSunny)
        {
            throw new Exception("���������б������������죨Sunny���������������������Ϊ����");
        }

        // 3. ���Ȩ���Ƿ�Ϸ�������Ȩ�ء�0���¸��ʼ������
        foreach (var cfg in _weatherConfigs)
        {
            if (cfg.ProbabilityWeight <= 0)
            {
                throw new Exception($"������{cfg.WeatherType}����Ȩ�ر������0����ǰȨ�أ�{cfg.ProbabilityWeight}");
            }
        }
    }

    /// <summary>
    /// ��ʼ����Ȩ�����б���Ȩ��ת��Ϊ���䣬���ڸ��������
    /// </summary>
    private void InitWeightedWeatherList()
    {
        _weightedWeathers = new List<WeatherWeightData>();
        _totalWeight = 0;

        // ���������������ã�����Ȩ������
        foreach (var cfg in _weatherConfigs)
        {
            float minWeight = _totalWeight;
            float maxWeight = _totalWeight + cfg.ProbabilityWeight;
            _totalWeight = maxWeight;

            // ��ӵ���Ȩ�б�
            _weightedWeathers.Add(new WeatherWeightData
            {
                WeatherType = cfg.WeatherType,
                Config = cfg,
                MinWeight = minWeight,
                MaxWeight = maxWeight
            });

            // ���Ȩ��������־������ģʽ�ɼ���
            LogDebug($"��Ȩ���� | ������{cfg.WeatherType} | Ȩ�أ�{cfg.ProbabilityWeight} | ���䣺[{minWeight:F1}, {maxWeight:F1})");
        }

        LogDebug($"��Ȩ�б��ʼ����� | ��Ȩ�أ�{_totalWeight:F1}");
    }

    /// <summary>
    /// �����������ͻ�ȡ���ã��ⲿ�ɵ��ã�����nullʱ����־��ʾ��
    /// </summary>
    public WeatherConfig GetWeatherConfigByType(WeatherType type)
    {
        WeatherConfig config = _weatherConfigs.Find(cfg => cfg.WeatherType == type);
        if (config == null)
        {
            LogWarning($"δ�ҵ�������{type}�������ã�������Inspector�в���");
        }
        return config;
    }

    /// <summary>
    /// �ⲿǿ���л���������GM���ߡ����鴥������ѡ���ܣ�
    /// </summary>
    public void ForceSwitchWeather(WeatherType targetType)
    {
        // У��ϵͳ״̬�������ʼ���������У�
        if (!_isInitialized || !_isRunning)
        {
            LogError($"ǿ���л�����ʧ�ܣ�ϵͳδ��ʼ����δ���У���ʼ����{_isInitialized} | �����У�{_isRunning}��");
            return;
        }

        // ��ȡĿ���������ã����������죩
        WeatherConfig targetConfig = GetWeatherConfigByType(targetType) ?? GetWeatherConfigByType(WeatherType.Sunny);
        // ����״̬
        _currentWeatherType = targetType;
        CurrentWeatherConfig = targetConfig;
        _currentWeatherEndTime = Time.time + GetRandomWeatherDuration();
        // �����¼�
        OnWeatherChanged?.Invoke(_currentWeatherType, targetConfig);

        LogDebug($"ǿ���л����� | Ŀ�꣺{targetType} | �´��л���{_currentWeatherEndTime - Time.time:F1}���");
    }
    #endregion
}