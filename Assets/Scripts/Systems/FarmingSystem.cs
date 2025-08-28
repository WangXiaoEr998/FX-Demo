using FanXing.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 种田系统：负责农田网格管理、作物种植/生长/收获的业务逻辑（不直接处理土壤视觉，仅通过FarmPlotView间接控制）
/// 作者：黄畅修
/// 创建时间：2025-07-13
/// 优化点：1. 移除无效调用 2. 硬编码转配置 3. 容错强化 4. 代码结构规整 5. 内存管理优化
/// </summary>
public class FarmingSystem : BaseGameSystem
{
    #region 配置字段（按功能分组，新增关键配置项）
    [Header("=== 农田网格配置 ===")]
    [SerializeField] private int _gridRows = 5;                  // 农田行数
    [SerializeField] private int _gridCols = 5;                  // 农田列数
    [SerializeField] private Vector3Int _gridStartPos = Vector3Int.zero; // 网格起始世界坐标
    [SerializeField] private float _plotSpacing = 10f;           // 地块间距（改为float，支持小数精度）
    [SerializeField] private GameObject _farmPlotPrefab;         // 土壤预制体（仅包含土壤视觉）

    [Header("=== 系统核心配置 ===")]
    [SerializeField] private int _initialPlotCount = 6;          // 初始解锁地块数
    [SerializeField] private int _maxFarmPlots = 100;            // 最大地块数上限
    [SerializeField] private float _growthUpdateInterval = 1f;   // 生长更新间隔（秒）
    [SerializeField] private float _cropYOffset = 0.1f;          // 作物在土地上方的Y轴偏移（转配置，避免硬编码）

    [Header("=== 农作物基础配置 ===")]
    [SerializeField] private GameObject[] _cropPrefabs;          // 作物预制体数组（默认小麦三阶段）
    [SerializeField] private List<CropData> _cropDefinitions;    // 自定义作物配置列表（优先级高于默认）
#pragma warning disable CS0414 
    [SerializeField] private float _harvestRange = 2f;           // 预留：收获距离检测（暂未实现）
#pragma warning restore CS0414 
    #endregion

    #region 数据管理字段（按职责分组，命名规范化）
    // 土壤视觉脚本映射（数据坐标→视觉脚本）
    private Dictionary<Vector3Int, FarmPlotView> _soilViewMap = new Dictionary<Vector3Int, FarmPlotView>();
    // 地块数据映射（数据坐标→地块业务数据）
    private Dictionary<Vector3Int, FarmPlot> _plotDataMap = new Dictionary<Vector3Int, FarmPlot>();
    // 所有可生成的地块坐标（含未解锁）
    private List<Vector3Int> _allPlotPositions = new List<Vector3Int>();
    // 活跃地块列表（已解锁的地块）
    private List<FarmPlot> _activePlots = new List<FarmPlot>();
    // 作物数据字典（作物类型→作物配置）
    private Dictionary<CropType, CropData> _cropConfigMap = new Dictionary<CropType, CropData>();

    // 生长更新计时器
    private float _growthTimer = 0f;
    #endregion

    #region 内部数据结构（优化字段命名，补充注释）
    /// <summary>
    /// 地块业务数据（仅存储逻辑数据，无视觉引用）
    /// </summary>
    [System.Serializable]
    public class FarmPlot
    {
        public Vector3Int Position;          // 地块世界坐标
        public CropType CropType;            // 当前种植的作物类型
        public float PlantTime;              // 种植时间（Time.time）
        public float GrowthProgress;         // 生长进度（0~1）
        public bool IsPlanted;               // 是否已种植
        public bool IsGrown;                 // 是否成熟
        public GameObject CropInstance;      // 作物实例（场景中的GameObject）
        public GameObject PestInstance;      // 预留：害虫实例（暂未实现）
        public PlotState SoilState;          // 土壤状态（用于同步土壤视觉）

        public FarmPlot(Vector3Int pos)
        {
            Position = pos;
            CropType = CropType.None;
            PlantTime = 0f;
            GrowthProgress = 0f;
            IsPlanted = false;
            IsGrown = false;
            CropInstance = null;
            PestInstance = null;
            SoilState = PlotState.Unlocked_Empty;
        }
    }

    /// <summary>
    /// 作物配置数据（可在Inspector可视化配置）
    /// </summary>
    [System.Serializable]
    public class CropData
    {
        public CropType Type;                // 作物类型（唯一标识）
        public string Name;                  // 作物名称（用于UI/日志）
        public float GrowthTime;             // 总生长时间（秒）
        public int BaseYield;                // 基础产量
        public int SellPrice;                // 单株售价
        public List<GrowthStage> GrowthStages; // 生长阶段列表（按进度排序）
    }

    /// <summary>
    /// 作物生长阶段配置
    /// </summary>
    [System.Serializable]
    public class GrowthStage
    {
        [Range(0f, 1f)] public float ProgressThreshold; // 进入该阶段的进度阈值（0~1）
        public GameObject Prefab;                      // 该阶段的作物预制体
        public AnimationClip Animation;                // 该阶段的动画（预留）
    }

    /// <summary>
    /// 土壤状态枚举（与FarmPlotView视觉状态一一对应）
    /// </summary>
    public enum PlotState
    {
        Locked,          // 未解锁（灰色/带锁）
        Unlocked_Empty,  // 已解锁未种植（普通土壤）
        Unlocked_Planted // 已解锁已种植（翻耕土壤）
    }
    #endregion

    #region 系统基础属性（保留原有，优化命名）
    public override string SystemName => "种田系统";
    public int ActivePlotCount => _activePlots.Count;
    public int ReadyToHarvestCount => _activePlots.Count(plot => plot.IsGrown);
    #endregion

    #region 系统生命周期（优化初始化流程，增强容错）
    protected override void OnInitialize()
    {
        LogDebug("=== 种田系统初始化开始 ===");
        try
        {
            // 1. 校验关键配置（提前暴露错误，避免后续崩溃）
            ValidateCriticalConfig();
            // 2. 初始化作物配置（优先用自定义，无则用默认）
            InitCropConfig();
            // 3. 生成所有地块坐标（含未解锁）
            GenerateAllPlotPositions();
            // 4. 生成土壤预制体（仅视觉，无业务逻辑）
            SpawnAllSoilPrefabs();
            // 5. 初始化初始解锁地块
            InitStartingPlots();
            // 6. 注册事件（预留框架）
            RegisterEvents();

            LogDebug($"=== 种田系统初始化完成 | 总地块数：{_allPlotPositions.Count} | 初始解锁：{_initialPlotCount} ===");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FarmingSystem] 初始化失败：{e.Message}");
            enabled = false; // 初始化失败则禁用系统
        }
    }

    protected override void OnStart()
    {
        _growthTimer = 0f;
        LogDebug("种田系统启动：生长计时器重置");
    }

    protected override void OnUpdate(float deltaTime)
    {
        if (_isPaused) return;

        _growthTimer += deltaTime;
        if (_growthTimer >= _growthUpdateInterval)
        {
            UpdateAllCropGrowth();  // 更新所有作物生长
            SyncSoilVisualState();  // 同步土壤视觉（仅状态，不含作物）
            _growthTimer = 0f;
        }
    }

    /// <summary>
    /// 场景销毁时清理资源（避免内存泄漏）
    /// </summary>
    private void OnDestroy()
    {
        ClearAllCropInstances();
        ClearAllSoilPrefabs();
        UnregisterEvents();
        LogDebug("种田系统资源清理完成");
    }
    #endregion

    #region 核心业务逻辑（优化代码复用，移除无效调用）
    /// <summary>
    /// 种植作物（优化逻辑：先判空→再判断状态→最后执行种植）
    /// </summary>
    public bool PlantCrop(Vector3Int plotPos, CropType cropType)
    {
        // 1. 基础限制判断
        if (_activePlots.Count >= _maxFarmPlots)
        {
            LogWarning($"种植失败：已达最大地块数（{_maxFarmPlots}）");
            return false;
        }
        if (!_cropConfigMap.ContainsKey(cropType))
        {
            LogWarning($"种植失败：未找到作物配置（类型：{cropType}）");
            return false;
        }

        // 2. 地块状态判断（空地块→覆盖；已种植→拒绝）
        if (_plotDataMap.TryGetValue(plotPos, out FarmPlot existingPlot))
        {
            if (existingPlot.IsPlanted)
            {
                LogWarning($"种植失败：地块（{plotPos}）已种植作物");
                return false;
            }
            // 空地块：移除旧数据（避免重复）
            _activePlots.Remove(existingPlot);
            _plotDataMap.Remove(plotPos);
        }

        // 3. 获取作物配置，创建新地块数据
        CropData cropConfig = _cropConfigMap[cropType];
        FarmPlot newPlot = new FarmPlot(plotPos)
        {
            CropType = cropType,
            PlantTime = Time.time,
            IsPlanted = true,
            SoilState = PlotState.Unlocked_Planted
        };

        // 4. 实例化初始阶段作物（调用通用方法，避免重复代码）
        newPlot.CropInstance = SpawnCropInstance(cropConfig.GrowthStages[0].Prefab, plotPos, cropType, cropConfig.GrowthStages[0]);
        if (newPlot.CropInstance == null)
        {
            LogWarning($"种植失败：作物预制体实例化失败（类型：{cropType}）");
            return false;
        }

        // 5. 更新数据集合
        _activePlots.Add(newPlot);
        _plotDataMap[plotPos] = newPlot;

        // 6. 同步土壤视觉状态（仅切换土壤材质，不含作物）
        SyncSoilState(plotPos, PlotState.Unlocked_Planted);

        LogDebug($"种植成功：地块（{plotPos}）| 作物：{cropConfig.Name} | 成熟时间：{cropConfig.GrowthTime}秒");
        return true;
    }

    /// <summary>
    /// 收获作物（优化：清理资源→重置状态→同步视觉）
    /// </summary>
    public ItemData HarvestCrop(Vector3Int plotPos)
    {
        // 1. 地块数据校验
        if (!_plotDataMap.TryGetValue(plotPos, out FarmPlot plot) || !plot.IsPlanted)
        {
            LogWarning($"收获失败：地块（{plotPos}）无作物");
            return null;
        }
        if (!plot.IsGrown)
        {
            LogWarning($"收获失败：地块（{plotPos}）作物未成熟（进度：{plot.GrowthProgress:P0}）");
            return null;
        }
        if (!_cropConfigMap.TryGetValue(plot.CropType, out CropData cropConfig))
        {
            LogWarning($"收获失败：未找到作物配置（类型：{plot.CropType}）");
            return null;
        }

        // 2. 清理作物实例
        if (plot.CropInstance != null)
        {
            Destroy(plot.CropInstance); // 运行时用Destroy，避免编辑器卡顿
            plot.CropInstance = null;
        }

        // 3. 重置地块状态
        plot.IsPlanted = false;
        plot.IsGrown = false;
        plot.CropType = CropType.None;
        plot.GrowthProgress = 0f;
        plot.PlantTime = 0f;
        plot.SoilState = PlotState.Unlocked_Empty;

        // 4. 同步土壤视觉（恢复为空土地）
        SyncSoilState(plotPos, PlotState.Unlocked_Empty);

        // 5. 生成收获物品
        ItemData harvestedItem = new ItemData
        {
            itemName = cropConfig.Name,
            itemType = ItemType.Consumable,
            currentStack = cropConfig.BaseYield,
            sellPrice = cropConfig.SellPrice
        };

        LogDebug($"收获成功：地块（{plotPos}）| 作物：{cropConfig.Name} | 数量：{cropConfig.BaseYield} | 价值：{cropConfig.SellPrice * cropConfig.BaseYield}金币");
        return harvestedItem;
    }

    /// <summary>
    /// 更新所有作物生长状态（优化：仅处理未成熟作物，减少计算）
    /// </summary>
    private void UpdateAllCropGrowth()
    {
        float currentTime = Time.time;
        foreach (var plot in _activePlots.Where(p => p.IsPlanted && !p.IsGrown))
        {
            if (!_cropConfigMap.TryGetValue(plot.CropType, out CropData cropConfig)) continue;

            // 1. 计算当前生长进度
            float elapsedTime = currentTime - plot.PlantTime;
            plot.GrowthProgress = Mathf.Clamp01(elapsedTime / cropConfig.GrowthTime); // 限制在0~1
            bool isStageChanged = false;

            // 2. 判断当前应显示的生长阶段
            GrowthStage targetStage = cropConfig.GrowthStages
                .OrderByDescending(s => s.ProgressThreshold)
                .FirstOrDefault(s => s.ProgressThreshold <= plot.GrowthProgress);

            // 3. 检查阶段是否变化（需更新外观）
            if (targetStage != null)
            {
                CropIdentifier cropId = plot.CropInstance?.GetComponent<CropIdentifier>();
                isStageChanged = cropId == null || cropId.CurrentStage != targetStage;
            }

            // 4. 处理成熟逻辑
            if (plot.GrowthProgress >= 1f)
            {
                plot.IsGrown = true;
                isStageChanged = true;
                LogDebug($"作物成熟：地块（{plot.Position}）| 作物：{cropConfig.Name}");
            }

            // 5. 阶段变化时更新作物外观
            if (isStageChanged && targetStage != null && targetStage.Prefab != null)
            {
                UpdateCropStage(plot, targetStage);
            }
        }
    }
    #endregion

    #region 视觉同步逻辑（严格分离：土壤视觉→FarmPlotView，作物视觉→系统直接控制）
    /// <summary>
    /// 同步所有土壤的视觉状态（仅材质，不含作物）
    /// </summary>
    private void SyncSoilVisualState()
    {
        foreach (var (plotPos, plotData) in _plotDataMap)
        {
            SyncSoilState(plotPos, plotData.SoilState);
        }
    }

    /// <summary>
    /// 同步单个土壤的状态（公开方法，供土地开拓使用）
    /// </summary>
    public void SyncSoilState(Vector3Int plotPos, PlotState targetState)
    {
        if (_soilViewMap.TryGetValue(plotPos, out FarmPlotView soilView))
        {
            soilView.UpdatePlotState(targetState);
        }
        else
        {
            LogWarning($"同步土壤状态失败：未找到土壤视觉脚本（地块：{plotPos}）");
        }
    }
    #endregion

    #region 工具方法（抽离重复逻辑，提升可维护性）
    /// <summary>
    /// 校验关键配置（提前暴露错误）
    /// </summary>
    private void ValidateCriticalConfig()
    {
        if (_farmPlotPrefab == null)
            throw new Exception("请为'_farmPlotPrefab'赋值土壤预制体！");
        if (_initialPlotCount > _maxFarmPlots)
            throw new Exception($"初始地块数（{_initialPlotCount}）不能大于最大地块数（{_maxFarmPlots}）！");
        if (_cropPrefabs == null || _cropPrefabs.Length < 3)
            Debug.LogWarning("作物预制体数组长度不足3，可能影响默认小麦生长阶段显示！");
    }

    /// <summary>
    /// 初始化作物配置（自定义优先，无则用默认小麦）
    /// </summary>
    private void InitCropConfig()
    {
        _cropConfigMap.Clear();

        // 1. 加载自定义作物配置（Inspector中配置）
        if (_cropDefinitions != null && _cropDefinitions.Count > 0)
        {
            foreach (var cropDef in _cropDefinitions)
            {
                if (cropDef.GrowthStages == null || cropDef.GrowthStages.Count == 0)
                {
                    LogWarning($"跳过无效作物配置：{cropDef.Name}（未设置生长阶段）");
                    continue;
                }
                if (_cropConfigMap.ContainsKey(cropDef.Type))
                {
                    LogWarning($"作物类型重复：{cropDef.Type}（后配置的会覆盖前配置）");
                }
                // 排序生长阶段（确保按进度升序，避免判断错误）
                cropDef.GrowthStages = cropDef.GrowthStages.OrderBy(s => s.ProgressThreshold).ToList();
                _cropConfigMap[cropDef.Type] = cropDef;
            }
            LogDebug($"加载自定义作物配置：{_cropConfigMap.Count}种");
        }

        // 2. 无自定义配置时，加载默认小麦配置
        if (_cropConfigMap.Count == 0)
        {
            AddDefaultWheatConfig();
            LogDebug("未找到自定义作物配置，加载默认小麦配置");
        }
    }

    /// <summary>
    /// 添加默认小麦配置（兼容原有逻辑）
    /// </summary>
    private void AddDefaultWheatConfig()
    {
        var wheatStages = new List<GrowthStage>
        {
            new GrowthStage { ProgressThreshold = 0f, Prefab = GetCropPrefabByIndex(0) },
            new GrowthStage { ProgressThreshold = 0.33f, Prefab = GetCropPrefabByIndex(1) },
            new GrowthStage { ProgressThreshold = 0.66f, Prefab = GetCropPrefabByIndex(2) }
        };

        _cropConfigMap[CropType.Wheat] = new CropData
        {
            Type = CropType.Wheat,
            Name = "小麦",
            GrowthTime = 60f,
            BaseYield = 2,
            SellPrice = 10,
            GrowthStages = wheatStages
        };
    }

    /// <summary>
    /// 安全获取作物预制体（避免索引越界）
    /// </summary>
    private GameObject GetCropPrefabByIndex(int index)
    {
        return _cropPrefabs != null && index < _cropPrefabs.Length ? _cropPrefabs[index] : null;
    }

    /// <summary>
    /// 生成所有地块坐标（含未解锁）
    /// </summary>
    private void GenerateAllPlotPositions()
    {
        _allPlotPositions.Clear();
        int actualCols = Mathf.Max(_gridCols, 1); // 避免0列导致循环不执行
        int actualRows = Mathf.Max(_gridRows, 1);

        for (int x = 0; x < actualCols; x++)
        {
            for (int z = 0; z < actualRows; z++)
            {
                Vector3Int plotPos = new Vector3Int(
                    _gridStartPos.x + Mathf.RoundToInt(x * _plotSpacing),
                    _gridStartPos.y,
                    _gridStartPos.z + Mathf.RoundToInt(z * _plotSpacing)
                );
                _allPlotPositions.Add(plotPos);
            }
        }
    }

    /// <summary>
    /// 生成所有土壤预制体（仅视觉，无业务逻辑）
    /// </summary>
    private void SpawnAllSoilPrefabs()
    {
        ClearAllSoilPrefabs(); // 先清理旧的，避免重复

        foreach (var plotPos in _allPlotPositions)
        {
            Vector3 worldPos = new Vector3(plotPos.x, plotPos.y, plotPos.z);
            GameObject soilObj = Instantiate(_farmPlotPrefab, worldPos, Quaternion.Euler(90, 0, 0), transform);
            soilObj.name = $"Soil_{plotPos.x}_{plotPos.z}"; // 命名规范：便于调试

            // 获取/添加土壤视觉脚本
            FarmPlotView soilView = soilObj.GetComponent<FarmPlotView>();
            if (soilView == null)
            {
                soilView = soilObj.AddComponent<FarmPlotView>();
                LogWarning($"土壤预制体（{soilObj.name}）缺少FarmPlotView脚本，已自动添加");
            }

            // 初始化土壤视觉（默认未解锁）
            soilView.Init(plotPos, PlotState.Locked);
            _soilViewMap[plotPos] = soilView;
        }
    }

    /// <summary>
    /// 初始化初始解锁地块（仅数据，不生成视觉）
    /// </summary>
    private void InitStartingPlots()
    {
        ClearAllPlotData();
        int actualInitCount = Mathf.Min(_initialPlotCount, _allPlotPositions.Count);

        for (int i = 0; i < actualInitCount; i++)
        {
            Vector3Int plotPos = _allPlotPositions[i];
            FarmPlot newPlot = new FarmPlot(plotPos)
            {
                SoilState = PlotState.Unlocked_Empty
            };

            _activePlots.Add(newPlot);
            _plotDataMap[plotPos] = newPlot;
            SyncSoilState(plotPos, PlotState.Unlocked_Empty); // 同步为未种植状态
        }
    }

    /// <summary>
    /// 实例化作物（抽成通用方法，避免PlantCrop和UpdateCropStage重复代码）
    /// </summary>
    private GameObject SpawnCropInstance(GameObject cropPrefab, Vector3Int plotPos, CropType cropType, GrowthStage stage)
    {
        if (cropPrefab == null) return null;

        // 计算作物位置（土地坐标+Y轴偏移，避免穿地）
        Vector3 cropPos = new Vector3(plotPos.x, plotPos.y + _cropYOffset, plotPos.z);
        GameObject cropInstance = Instantiate(cropPrefab, cropPos, Quaternion.identity, transform);
        cropInstance.name = $"Crop_{cropType}_{plotPos.x}_{plotPos.z}";

        // 添加作物标识符（用于阶段判断）
        CropIdentifier cropId = cropInstance.AddComponent<CropIdentifier>();
        cropId.CropType = cropType;
        cropId.CurrentStage = stage;
        cropId.PlotPosition = plotPos;

        return cropInstance;
    }

    /// <summary>
    /// 更新作物生长阶段（销毁旧实例，创建新实例）
    /// </summary>
    private void UpdateCropStage(FarmPlot plot, GrowthStage targetStage)
    {
        // 销毁旧实例
        if (plot.CropInstance != null)
        {
            Destroy(plot.CropInstance);
        }
        // 创建新实例
        plot.CropInstance = SpawnCropInstance(
            targetStage.Prefab,
            plot.Position,
            plot.CropType,
            targetStage
        );
    }
    #endregion

    #region 资源清理方法（优化清理逻辑，避免内存泄漏）
    /// <summary>
    /// 清理所有土壤预制体
    /// </summary>
    private void ClearAllSoilPrefabs()
    {
        foreach (var (_, soilView) in _soilViewMap)
        {
            if (soilView != null && soilView.gameObject != null)
            {
                DestroyImmediate(soilView.gameObject);
            }
        }
        _soilViewMap.Clear();
    }

    /// <summary>
    /// 清理所有地块数据
    /// </summary>
    private void ClearAllPlotData()
    {
        ClearAllCropInstances();
        _activePlots.Clear();
        _plotDataMap.Clear();
    }

    /// <summary>
    /// 清理所有作物实例
    /// </summary>
    private void ClearAllCropInstances()
    {
        foreach (var plot in _activePlots)
        {
            if (plot.CropInstance != null)
            {
                Destroy(plot.CropInstance);
                plot.CropInstance = null;
            }
        }
    }
    #endregion

    #region 事件与日志（预留框架，规范日志格式）
    private void RegisterEvents()
    {
        // TODO：后续可添加事件（如作物成熟事件、收获事件）
        // 示例：EventManager.AddListener<OnCropGrownEvent>(OnCropGrown);
    }

    private void UnregisterEvents()
    {
        // TODO：与RegisterEvents对应，注销事件
    }

    private void LogDebug(string msg) => Debug.Log($"[FarmingSystem] {msg}");
    private void LogWarning(string msg) => Debug.LogWarning($"[FarmingSystem] {msg}");
    #endregion

    #region 公共查询方法（保留原有，优化命名）
    public FarmPlot GetPlotData(Vector3Int plotPos) => _plotDataMap.TryGetValue(plotPos, out var plot) ? plot : null;
    public bool CanPlantAt(Vector3Int plotPos) => !_plotDataMap.ContainsKey(plotPos) && _activePlots.Count < _maxFarmPlots;
    public List<FarmPlot> GetAllActivePlots() => new List<FarmPlot>(_activePlots);
    public List<Vector3Int> GetAllPlotPositions() => new List<Vector3Int>(_allPlotPositions);
    public CropData GetCropConfig(CropType cropType) => _cropConfigMap.TryGetValue(cropType, out var config) ? config : null;

    // 新增：添加新地块（用于土地开拓）
    public void AddNewPlot(FarmPlot newPlot)
    {
        if (!_plotDataMap.ContainsKey(newPlot.Position))
        {
            _plotDataMap[newPlot.Position] = newPlot;
            _activePlots.Add(newPlot);
        }
    }
    public List<CropType> GetAllValidCropTypes()
    {
        // 直接返回 _cropConfigMap 的所有键（即已配置的作物类型），排除 None 类型
        return _cropConfigMap.Keys.Where(type => type != CropType.None).ToList();
    }

    #endregion
}

/// <summary>
/// 作物标识符组件（仅存储作物标识信息，无业务逻辑）
/// </summary>
public class CropIdentifier : MonoBehaviour
{
    public CropType CropType;
    public FarmingSystem.GrowthStage CurrentStage;
    public Vector3Int PlotPosition;
}