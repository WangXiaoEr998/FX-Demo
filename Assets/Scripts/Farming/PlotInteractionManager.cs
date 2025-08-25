using FanXing.Data;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 3D土地点击交互管理器：处理点击3D土地触发种植/收获逻辑
/// </summary>
public class PlotInteractionManager : MonoBehaviour
{
    [Header("交互配置（3D）")]
    [SerializeField] private Camera _farmCamera; // 农场主相机（默认取MainCamera）
    [SerializeField] private CropType _defaultPlantCrop = CropType.Wheat; // 默认种植作物
    [SerializeField] private float _raycastDistance = 100f; // 射线检测距离（适配3D场景）

    private FarmingSystem _farmingSystem;
    private bool _isInteracting = false; // 防止重复点击

    private void Start()
    {
        InitDependencies();
    }

    private void Update()
    {
        // 鼠标左键点击且未点击UI时触发交互
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI() && !_isInteracting)
        {
            CheckPlotClick();
        }
    }

    /// <summary>
    /// 初始化依赖系统（适配3D相机）
    /// </summary>
    private void InitDependencies()
    {
        // 自动获取主相机
        if (_farmCamera == null)
        {
            _farmCamera = Camera.main;
            if (_farmCamera == null)
            {
                Debug.LogError("[PlotInteractionManager] 场景中未找到主相机！");
                enabled = false;
                return;
            }
        }

        // 获取种田系统
        _farmingSystem = FindObjectOfType<FarmingSystem>();
        if (_farmingSystem == null)
        {
            Debug.LogError("[PlotInteractionManager] 场景中未找到FarmingSystem！");
            enabled = false;
            return;
        }

        // 3D场景推荐使用透视投影
        if (_farmCamera.orthographic)
        {
            Debug.LogWarning("[PlotInteractionManager] 3D场景建议使用透视投影相机，当前为正交投影可能影响交互体验");
        }

        Debug.Log("[PlotInteractionManager] 3D交互管理器初始化完成");
    }

    /// <summary>
    /// 检测点击是否命中3D土地（使用3D射线检测）
    /// </summary>
    private void CheckPlotClick()
    {
        // 生成3D射线（从相机到鼠标位置）
        Ray ray = _farmCamera.ScreenPointToRay(Input.mousePosition);

        // 3D射线检测
        if (Physics.Raycast(ray, out RaycastHit hit, _raycastDistance))
        {
            // 尝试获取点击对象上的农田视觉组件
            if (hit.collider.TryGetComponent<FarmPlotView>(out FarmPlotView plotView))
            {
                Vector3Int plotGridPos = plotView.GetGridPosition();
                HandlePlotInteraction(plotGridPos);
            }
        }
    }

    /// <summary>
    /// 处理土地交互逻辑（修复：移除对FarmPlotView的直接依赖）
    /// </summary>
    private void HandlePlotInteraction(Vector3Int plotPos)
    {
        _isInteracting = true;

        // 修复1：使用新的方法名GetPlotData获取地块数据
        FarmingSystem.FarmPlot plotData = _farmingSystem.GetPlotData(plotPos);
        if (plotData == null)
        {
            Debug.LogWarning($"[PlotInteraction] 土地 {plotPos} 未初始化（可能未解锁）");
            _isInteracting = false;
            return;
        }

        // 分支1：已种植且成熟 → 收获
        if (plotData.IsPlanted && plotData.IsGrown)
        {
            HarvestTargetPlot(plotData, plotPos);
        }
        // 分支2：已解锁且未种植 → 种植
        else if (plotData.SoilState == FarmingSystem.PlotState.Unlocked_Empty)
        {
            PlantOnTargetPlot(plotPos);
        }
        // 分支3：已种植但未成熟 → 提示
        else if (plotData.IsPlanted && !plotData.IsGrown)
        {
            float growthPercent = Mathf.Round(plotData.GrowthProgress * 100);
            Debug.Log($"[PlotInteraction] 土地 {plotPos} 作物正在生长（{growthPercent}%）");
        }
        // 分支4：未解锁 → 提示
        else if (plotData.SoilState == FarmingSystem.PlotState.Locked)
        {
            Debug.Log($"[PlotInteraction] 土地 {plotPos} 未解锁（需要解锁后使用）");
        }

        _isInteracting = false;
    }

    // 种植逻辑（修复方法调用和属性名）
    private void PlantOnTargetPlot(Vector3Int plotPos)
    {
        // 强制刷新地块数据
        FarmingSystem.FarmPlot latestPlotData = _farmingSystem.GetPlotData(plotPos);
        if (latestPlotData != null && latestPlotData.IsPlanted)
        {
            Debug.LogWarning($"[PlotInteraction] 土地 {plotPos} 已种植，无法重复种植");
            return;
        }

        bool plantSuccess = _farmingSystem.PlantCrop(plotPos, _defaultPlantCrop);

        if (plantSuccess)
        {
            // 修复2：使用新的方法名GetCropConfig获取作物数据
            FarmingSystem.CropData cropData = _farmingSystem.GetCropConfig(_defaultPlantCrop);
            if (cropData != null)
            {
                Debug.Log($"[PlotInteraction] 种植成功！土地 {plotPos} 种植了 {cropData.Name}（{cropData.GrowthTime}秒成熟）");
            }
            else
            {
                Debug.Log($"[PlotInteraction] 种植成功！土地 {plotPos}（未找到作物数据）");
            }
        }
        else
        {
            Debug.LogWarning($"[PlotInteraction] 种植失败！土地 {plotPos} 无法种植");
        }
    }

    private void HarvestTargetPlot(FarmingSystem.FarmPlot plotData, Vector3Int plotPos)
    {
        ItemData harvestedItem = _farmingSystem.HarvestCrop(plotPos);
        if (harvestedItem != null)
        {
            Debug.Log($"[PlotInteraction] 收获成功！土地 {plotPos} 获得 {harvestedItem.itemName} x{harvestedItem.currentStack}");
        }
        else
        {
            Debug.LogWarning($"[PlotInteraction] 收获失败！土地 {plotPos} 无成熟作物");
        }
    }

    /// <summary>
    /// 检测是否点击到UI（3D/2D通用）
    /// </summary>
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;
        return EventSystem.current.IsPointerOverGameObject();
    }
}
