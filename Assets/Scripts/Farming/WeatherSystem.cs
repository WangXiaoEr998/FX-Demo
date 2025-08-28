using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 天气系统：负责天气类型切换、概率计算、状态管理（适配 BaseGameSystem 基类）
/// </summary>
public class WeatherSystem : BaseGameSystem
{
    #region 可配置参数（Inspector可视化调整）
    [Header("=== 天气系统配置 ===")]
    [Tooltip("天气切换的最小间隔（分钟，防止频繁切换）")]
    [SerializeField] private float _minWeatherDuration = 5f;
    [Tooltip("天气切换的最大间隔（分钟）")]
    [SerializeField] private float _maxWeatherDuration = 15f;
    [Tooltip("是否在系统启动时立即触发一次天气")]
    [SerializeField] private bool _triggerWeatherOnStart = true;

    [Header("=== 天气类型配置（按概率权重排序）===")]
    [SerializeField] private List<WeatherConfig> _weatherConfigs; // 天气配置列表（含概率）
    #endregion

    #region 内部状态管理
    private WeatherType _currentWeatherType; // 当前天气类型
    private float _currentWeatherEndTime;    // 当前天气的结束时间（Time.time）
    private List<WeatherWeightData> _weightedWeathers; // 加权天气列表（用于概率计算）
    private float _totalWeight;              // 所有天气的权重总和（概率计算用）
    #endregion

    #region 事件定义（供外部系统监听，解耦依赖）
    /// <summary>
    /// 天气变化事件（参数：新天气类型、天气配置）
    /// </summary>
    public static event Action<WeatherType, WeatherConfig> OnWeatherChanged;
    #endregion

    #region 数据结构定义（天气配置与类型）
    /// <summary>
    /// 天气类型枚举（可扩展：雪、雾、沙尘暴等）
    /// </summary>
    public enum WeatherType
    {
        Sunny,    // 晴天（默认）
        HeavyRain,// 大雨
        Drought   // 干旱
    }

    /// <summary>
    /// 天气配置（ Inspector 可配置，含概率权重）
    /// </summary>
    [Serializable]
    public class WeatherConfig
    {
        public WeatherType WeatherType;       // 天气类型
        [Range(1, 100)] public int ProbabilityWeight; // 概率权重（值越大，出现概率越高）
        public string WeatherDesc;            // 天气描述（如“阳光充足，适合作物生长”）
        [Tooltip("该天气的额外参数（如大雨的降水量，干旱的干燥度，预留扩展）")]
        public float WeatherParam;            // 预留参数（按需使用）
    }

    /// <summary>
    /// 加权天气数据（内部概率计算用）
    /// </summary>
    private class WeatherWeightData
    {
        public WeatherType WeatherType; // 天气类型
        public WeatherConfig Config;    // 对应配置
        public float MinWeight;         // 权重区间最小值
        public float MaxWeight;         // 权重区间最大值
    }
    #endregion

    #region 系统基础属性（补充当前天气查询）
    public WeatherType CurrentWeather => _currentWeatherType; // 外部获取当前天气
    public WeatherConfig CurrentWeatherConfig { get; private set; } // 外部获取当前天气配置
    #endregion

    #region 基类生命周期重写（关键：遵循 Initialize → Start → Update 流程）
    /// <summary>
    /// 系统初始化（基类调用 Initialize() 时触发，无需手动调用）
    /// </summary>
    protected override void OnInitialize()
    {
        LogDebug("=== 天气系统初始化开始（OnInitialize）===");

        // 1. 校验配置（避免空列表、权重为0、缺少默认天气）
        ValidateWeatherConfig();
        // 2. 初始化加权天气列表（概率计算核心）
        InitWeightedWeatherList();
        // 3. 初始化当前天气状态（默认雨天）
        _currentWeatherType = WeatherType.HeavyRain;
        CurrentWeatherConfig = GetWeatherConfigByType(WeatherType.HeavyRain);

        LogDebug($"初始化完成 | 当前天气：{_currentWeatherType} | 切换间隔：{_minWeatherDuration}~{_maxWeatherDuration}分钟");
    }

    /// <summary>
    /// 系统启动（基类调用 Start() 时触发，需在初始化后执行）
    /// </summary>
    protected override void OnStart()
    {
        LogDebug("=== 天气系统启动（OnStart）===");

        if (_triggerWeatherOnStart)
        {
            // 启动时立即触发一次天气（随机或默认）
            TriggerRandomWeather(immediate: true);
        }
        else
        {
            // 计算首次天气结束时间（分钟转秒，Time.time 单位为秒）
            _currentWeatherEndTime = Time.time + GetRandomWeatherDuration();
            LogDebug($"首次天气切换：{_currentWeatherEndTime - Time.time:F1}秒后");
        }

        LogDebug("系统启动完成");
    }

    /// <summary>
    /// 系统更新（基类调用 UpdateSystem() 时触发，自动处理暂停逻辑）
    /// </summary>
    protected override void OnUpdate(float deltaTime)
    {
        // 检查当前天气是否到期，到期则切换新天气
        if (Time.time >= _currentWeatherEndTime)
        {
            TriggerRandomWeather(immediate: false);
        }
    }

    /// <summary>
    /// 系统关闭时清理（重写基类方法，避免事件内存泄漏）
    /// </summary>
    protected override void OnShutdown()
    {
        base.OnShutdown();
        // 取消所有事件订阅（避免外部引用导致内存泄漏）
        OnWeatherChanged = null;
        LogDebug("天气系统关闭，事件已清理");
    }
    #endregion

    #region 核心逻辑（概率计算→天气切换→事件通知）
    /// <summary>
    /// 触发随机天气（核心方法：按概率选择，避免连续相同天气）
    /// </summary>
    /// <param name="immediate">是否立即生效（true：无过渡，false：按正常流程）</param>
    private void TriggerRandomWeather(bool immediate)
    {
        // 1. 按概率随机选择天气（排除当前天气，避免连续重复）
        WeatherType newWeatherType = GetRandomWeatherExcludeCurrent();
        // 2. 获取新天气的配置（保底：未找到则用晴天）
        WeatherConfig newWeatherConfig = GetWeatherConfigByType(newWeatherType) ?? GetWeatherConfigByType(WeatherType.Sunny);

        // 3. 更新当前天气状态
        _currentWeatherType = newWeatherType;
        CurrentWeatherConfig = newWeatherConfig;

        // 【修复核心】无论是否立即生效，都设置正常的持续时间
        // 立即生效：立刻切换，但持续时间正常（5~15分钟）
        // 非立即生效：按正常流程切换，持续时间正常
        float newDuration = GetRandomWeatherDuration();
        _currentWeatherEndTime = Time.time + newDuration;

        // 5. 日志输出
        string triggerDesc = immediate ? "立即生效" : "按计划切换";
        LogDebug($"天气切换（{triggerDesc}）| 新天气：{_currentWeatherType} | 描述：{newWeatherConfig.WeatherDesc} | 下次切换：{newDuration:F1}秒后");

        // 6. 发送天气变化事件
        OnWeatherChanged?.Invoke(_currentWeatherType, newWeatherConfig);
    }


    /// <summary>
    /// 按概率随机选择天气（排除当前天气，避免连续重复）
    /// </summary>
    private WeatherType GetRandomWeatherExcludeCurrent()
    {
        WeatherType selectedType = _currentWeatherType;
        int maxRetry = 10; // 最大重试次数（防止极端情况死循环）
        int retryCount = 0;

        while (selectedType == _currentWeatherType && retryCount < maxRetry)
        {
            // 生成 0~总权重 之间的随机数
            float randomValue = UnityEngine.Random.Range(0, _totalWeight);
            // 遍历加权列表，找到随机数所在的区间
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

        // 重试超过上限时，强制切换为晴天（保底逻辑）
        if (selectedType == _currentWeatherType)
        {

            LogWarning($"天气选择重试超过上限（{maxRetry}次），强制切换为晴天");
            selectedType = WeatherType.Sunny;
        }

        return selectedType;
    }

    /// <summary>
    /// 获取随机的天气持续时间（分钟转秒）
    /// </summary>
    private float GetRandomWeatherDuration()
    {
        float minutes = UnityEngine.Random.Range(_minWeatherDuration, _maxWeatherDuration);
        return minutes * 60; // 转换为秒（Time.time 单位为秒）
    }
    #endregion

    #region 工具方法（配置校验→权重计算→配置查询）
    /// <summary>
    /// 校验天气配置（避免空列表、权重为0、缺少默认天气）
    /// </summary>
    private void ValidateWeatherConfig()
    {
        // 1. 检查配置列表是否为空
        if (_weatherConfigs == null || _weatherConfigs.Count == 0)
        {
            throw new Exception("天气配置列表不能为空！请在Inspector中添加至少一种天气（如晴天）");
        }

        // 2. 检查是否包含晴天配置（保底天气，必须存在）
        bool hasSunny = _weatherConfigs.Exists(cfg => cfg.WeatherType == WeatherType.Sunny);
        if (!hasSunny)
        {
            throw new Exception("天气配置列表必须包含「晴天（Sunny）」！请添加晴天配置作为保底");
        }

        // 3. 检查权重是否合法（避免权重≤0导致概率计算错误）
        foreach (var cfg in _weatherConfigs)
        {
            if (cfg.ProbabilityWeight <= 0)
            {
                throw new Exception($"天气「{cfg.WeatherType}」的权重必须大于0！当前权重：{cfg.ProbabilityWeight}");
            }
        }
    }

    /// <summary>
    /// 初始化加权天气列表（将权重转换为区间，用于概率随机）
    /// </summary>
    private void InitWeightedWeatherList()
    {
        _weightedWeathers = new List<WeatherWeightData>();
        _totalWeight = 0;

        // 遍历所有天气配置，计算权重区间
        foreach (var cfg in _weatherConfigs)
        {
            float minWeight = _totalWeight;
            float maxWeight = _totalWeight + cfg.ProbabilityWeight;
            _totalWeight = maxWeight;

            // 添加到加权列表
            _weightedWeathers.Add(new WeatherWeightData
            {
                WeatherType = cfg.WeatherType,
                Config = cfg,
                MinWeight = minWeight,
                MaxWeight = maxWeight
            });

            // 输出权重区间日志（调试模式可见）
            LogDebug($"加权配置 | 天气：{cfg.WeatherType} | 权重：{cfg.ProbabilityWeight} | 区间：[{minWeight:F1}, {maxWeight:F1})");
        }

        LogDebug($"加权列表初始化完成 | 总权重：{_totalWeight:F1}");
    }

    /// <summary>
    /// 根据天气类型获取配置（外部可调用，返回null时用日志提示）
    /// </summary>
    public WeatherConfig GetWeatherConfigByType(WeatherType type)
    {
        WeatherConfig config = _weatherConfigs.Find(cfg => cfg.WeatherType == type);
        if (config == null)
        {
            LogWarning($"未找到天气「{type}」的配置，建议在Inspector中补充");
        }
        return config;
    }

    /// <summary>
    /// 外部强制切换天气（如GM工具、剧情触发，可选功能）
    /// </summary>
    public void ForceSwitchWeather(WeatherType targetType)
    {
        // 校验系统状态（必须初始化且运行中）
        if (!_isInitialized || !_isRunning)
        {
            LogError($"强制切换天气失败：系统未初始化或未运行（初始化：{_isInitialized} | 运行中：{_isRunning}）");
            return;
        }

        // 获取目标天气配置（保底用晴天）
        WeatherConfig targetConfig = GetWeatherConfigByType(targetType) ?? GetWeatherConfigByType(WeatherType.Sunny);
        // 更新状态
        _currentWeatherType = targetType;
        CurrentWeatherConfig = targetConfig;
        _currentWeatherEndTime = Time.time + GetRandomWeatherDuration();
        // 触发事件
        OnWeatherChanged?.Invoke(_currentWeatherType, targetConfig);

        LogDebug($"强制切换天气 | 目标：{targetType} | 下次切换：{_currentWeatherEndTime - Time.time:F1}秒后");
    }
    #endregion
}