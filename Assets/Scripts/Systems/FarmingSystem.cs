using UnityEngine;
using System.Collections.Generic;
using FanXing.Data;

/// <summary>
/// 种田系统，负责农作物种植、生长、收获等功能
/// 作者：黄畅修
/// 创建时间：2025-07-13
/// </summary>
public class FarmingSystem : BaseGameSystem
{
    #region 字段定义
    [Header("种田系统配置")]
    [SerializeField] private int _maxFarmPlots = 100;
    [SerializeField] private float _growthUpdateInterval = 1.0f;
    #pragma warning disable CS0414 // 字段已赋值但从未使用 - Demo版本中暂未实现相关功能
    [SerializeField] private float _harvestRange = 2.0f;
    #pragma warning restore CS0414
    
    [Header("农作物配置")]
    [SerializeField] private GameObject[] _cropPrefabs;
    [SerializeField] private float[] _growthTimes = { 60f, 120f, 180f }; // 不同作物的生长时间
    
    // 农田管理
    private List<FarmPlot> _activePlots = new List<FarmPlot>();
    private Dictionary<Vector3Int, FarmPlot> _plotDictionary = new Dictionary<Vector3Int, FarmPlot>();
    
    // 更新计时器
    private float _updateTimer = 0f;
    
    // 农作物数据
    private Dictionary<CropType, CropData> _cropDataDictionary = new Dictionary<CropType, CropData>();
    #endregion

    #region 内部类
    /// <summary>
    /// 农田地块数据
    /// </summary>
    [System.Serializable]
    public class FarmPlot
    {
        public Vector3Int position;
        public CropType cropType;
        public float plantTime;
        public float growthProgress;
        public bool isPlanted;
        public bool isGrown;
        public GameObject cropObject;
        
        public FarmPlot(Vector3Int pos)
        {
            position = pos;
            cropType = CropType.None;
            plantTime = 0f;
            growthProgress = 0f;
            isPlanted = false;
            isGrown = false;
            cropObject = null;
        }
    }

    /// <summary>
    /// 农作物数据
    /// </summary>
    [System.Serializable]
    public class CropData
    {
        public CropType type;
        public string name;
        public float growthTime;
        public int baseYield;
        public int sellPrice;
        public GameObject prefab;
    }
    #endregion

    #region 属性
    /// <summary>
    /// 系统名称
    /// </summary>
    public override string SystemName => "种田系统";
    
    /// <summary>
    /// 当前农田数量
    /// </summary>
    public int ActivePlotCount => _activePlots.Count;
    
    /// <summary>
    /// 可收获的农田数量
    /// </summary>
    public int ReadyToHarvestCount
    {
        get
        {
            int count = 0;
            foreach (var plot in _activePlots)
            {
                if (plot.isGrown)
                    count++;
            }
            return count;
        }
    }
    #endregion

    #region BaseGameSystem抽象方法实现
    /// <summary>
    /// 系统初始化时调用
    /// </summary>
    protected override void OnInitialize()
    {
        LogDebug("种田系统初始化开始");

        // 初始化农作物数据
        InitializeCropData();

        // 注册事件监听器
        RegisterEventListeners();

        LogDebug("种田系统初始化完成");
    }

    /// <summary>
    /// 系统启动时调用
    /// </summary>
    protected override void OnStart()
    {
        LogDebug("种田系统启动");

        // 开始更新计时器
        _updateTimer = 0f;

        LogDebug("种田系统启动完成");
    }

    /// <summary>
    /// 系统更新时调用
    /// </summary>
    /// <param name="deltaTime">帧时间</param>
    protected override void OnUpdate(float deltaTime)
    {
        if (_isPaused)
            return;

        // 更新计时器
        _updateTimer += deltaTime;

        // 定期更新农作物生长
        if (_updateTimer >= _growthUpdateInterval)
        {
            UpdateCropGrowth();
            _updateTimer = 0f;
        }
    }
    #endregion

    #region 私有方法
    /// <summary>
    /// 初始化农作物数据
    /// </summary>
    private void InitializeCropData()
    {
        // 初始化基础农作物数据
        _cropDataDictionary.Clear();
        
        // 添加默认农作物数据
        if (_cropPrefabs != null && _cropPrefabs.Length > 0)
        {
            for (int i = 0; i < _cropPrefabs.Length && i < _growthTimes.Length; i++)
            {
                var cropData = new CropData
                {
                    type = (CropType)(i + 1),
                    name = $"作物{i + 1}",
                    growthTime = _growthTimes[i],
                    baseYield = 1 + i,
                    sellPrice = 10 * (i + 1),
                    prefab = _cropPrefabs[i]
                };
                
                _cropDataDictionary[cropData.type] = cropData;
            }
        }
    }

    /// <summary>
    /// 更新农作物生长
    /// </summary>
    private void UpdateCropGrowth()
    {
        float currentTime = Time.time;
        
        foreach (var plot in _activePlots)
        {
            if (plot.isPlanted && !plot.isGrown)
            {
                if (_cropDataDictionary.TryGetValue(plot.cropType, out CropData cropData))
                {
                    float elapsedTime = currentTime - plot.plantTime;
                    plot.growthProgress = elapsedTime / cropData.growthTime;
                    
                    if (plot.growthProgress >= 1.0f)
                    {
                        plot.isGrown = true;
                        plot.growthProgress = 1.0f;
                        
                        // 更新作物外观
                        UpdateCropAppearance(plot);
                        
                        if (_enableDebugMode)
                        {
                            Debug.Log($"农作物成熟: {plot.position}");
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 更新作物外观
    /// </summary>
    /// <param name="plot">农田地块</param>
    private void UpdateCropAppearance(FarmPlot plot)
    {
        if (plot.cropObject != null)
        {
            // TODO: 更新作物的视觉表现
            // 例如：改变材质、播放动画等
        }
    }

    /// <summary>
    /// 注册事件监听
    /// </summary>
    private void RegisterEventListeners()
    {
        // TODO: 注册相关事件监听
    }

    /// <summary>
    /// 注销事件监听
    /// </summary>
    private void UnregisterEventListeners()
    {
        // TODO: 注销相关事件监听
    }

    /// <summary>
    /// 清理所有农田
    /// </summary>
    private void ClearAllPlots()
    {
        foreach (var plot in _activePlots)
        {
            if (plot.cropObject != null)
            {
                DestroyImmediate(plot.cropObject);
            }
        }
        
        _activePlots.Clear();
        _plotDictionary.Clear();
    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 种植农作物
    /// </summary>
    /// <param name="position">种植位置</param>
    /// <param name="cropType">农作物类型</param>
    /// <returns>是否种植成功</returns>
    public bool PlantCrop(Vector3Int position, CropType cropType)
    {
        if (_activePlots.Count >= _maxFarmPlots)
        {
            Debug.LogWarning("已达到最大农田数量限制");
            return false;
        }

        if (_plotDictionary.ContainsKey(position))
        {
            Debug.LogWarning($"位置 {position} 已经有农作物了");
            return false;
        }

        if (!_cropDataDictionary.TryGetValue(cropType, out CropData cropData))
        {
            Debug.LogWarning($"未找到农作物数据: {cropType}");
            return false;
        }

        // 创建新的农田地块
        var plot = new FarmPlot(position)
        {
            cropType = cropType,
            plantTime = Time.time,
            isPlanted = true
        };

        // 创建作物对象
        if (cropData.prefab != null)
        {
            plot.cropObject = Instantiate(cropData.prefab, new Vector3(position.x, position.y, position.z), Quaternion.identity);
        }

        _activePlots.Add(plot);
        _plotDictionary[position] = plot;

        if (_enableDebugMode)
        {
            Debug.Log($"种植农作物: {cropType} 在位置 {position}");
        }

        return true;
    }

    /// <summary>
    /// 收获农作物
    /// </summary>
    /// <param name="position">收获位置</param>
    /// <returns>收获的物品数据</returns>
    public ItemData HarvestCrop(Vector3Int position)
    {
        if (!_plotDictionary.TryGetValue(position, out FarmPlot plot))
        {
            Debug.LogWarning($"位置 {position} 没有农作物");
            return null;
        }

        if (!plot.isGrown)
        {
            Debug.LogWarning($"位置 {position} 的农作物还未成熟");
            return null;
        }

        if (!_cropDataDictionary.TryGetValue(plot.cropType, out CropData cropData))
        {
            Debug.LogWarning($"未找到农作物数据: {plot.cropType}");
            return null;
        }

        // 创建收获物品
        var harvestedItem = new ItemData
        {
            itemName = cropData.name,
            itemType = ItemType.Consumable,
            currentStack = cropData.baseYield,
            sellPrice = cropData.sellPrice
        };

        // 清理农田
        if (plot.cropObject != null)
        {
            DestroyImmediate(plot.cropObject);
        }

        _activePlots.Remove(plot);
        _plotDictionary.Remove(position);

        if (_enableDebugMode)
        {
            Debug.Log($"收获农作物: {cropData.name} 数量: {harvestedItem.currentStack}");
        }

        return harvestedItem;
    }

    /// <summary>
    /// 获取指定位置的农田信息
    /// </summary>
    /// <param name="position">位置</param>
    /// <returns>农田地块信息</returns>
    public FarmPlot GetPlotInfo(Vector3Int position)
    {
        _plotDictionary.TryGetValue(position, out FarmPlot plot);
        return plot;
    }

    /// <summary>
    /// 检查位置是否可以种植
    /// </summary>
    /// <param name="position">位置</param>
    /// <returns>是否可以种植</returns>
    public bool CanPlantAt(Vector3Int position)
    {
        return !_plotDictionary.ContainsKey(position) && _activePlots.Count < _maxFarmPlots;
    }
    #endregion
}
