using FanXing.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

/// <summary>
/// 3D���ص��������������������3D���ش�����ֲ/�ջ��߼�
/// </summary>
public class PlotInteractionManager : MonoBehaviour
{
    [Header("�������ã�3D��")]
    [SerializeField] private Camera _farmCamera; // ũ���������Ĭ��ȡMainCamera��
    [SerializeField] private CropType _defaultPlantCrop = CropType.Wheat; // Ĭ����ֲ����
    [SerializeField] private float _raycastDistance = 100f; // ���߼����루����3D������

    private FarmingSystem _farmingSystem;
    private bool _isInteracting = false; // ��ֹ�ظ����
    private CropType _selectedCropType;  // ��ǰѡ�е���������

    // �������ԣ���UI���ʵ�ǰѡ�е���������
    public CropType SelectedCropType => _selectedCropType;
    public CropType DefaultPlantCrop => _defaultPlantCrop;

    private void Start()
    {
        InitDependencies();
        // ��ʼ��ѡ�е�����ΪĬ������
        _selectedCropType = _defaultPlantCrop;
    }

    private void Update()
    {
        // �����������δ���UIʱ��������
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI() && !_isInteracting)
        {
            CheckPlotClick();
        }
    }

    /// <summary>
    /// ��ʼ������ϵͳ������3D�����
    /// </summary>
    private void InitDependencies()
    {
        // �Զ���ȡ�����
        if (_farmCamera == null)
        {
            _farmCamera = Camera.main;
            if (_farmCamera == null)
            {
                Debug.LogError("[PlotInteractionManager] ������δ�ҵ��������");
                enabled = false;
                return;
            }
        }

        // ��ȡ����ϵͳ
        _farmingSystem = Global.Farming;//FindObjectOfType<FarmingSystem>();
        if (_farmingSystem == null)
        {
            Debug.LogError("[PlotInteractionManager] ������δ�ҵ�FarmingSystem��");
            enabled = false;
            return;
        }
        Debug.Log("[PlotInteractionManager] 3D������������ʼ�����");
    }

    /// <summary>
    /// ������Ƿ�����3D���أ�ʹ��3D���߼�⣩
    /// </summary>
    private void CheckPlotClick()
    {
        // ����3D���ߣ�����������λ�ã�
        Ray ray = _farmCamera.ScreenPointToRay(Input.mousePosition);

        // 3D���߼��
        if (Physics.Raycast(ray, out RaycastHit hit, _raycastDistance))
        {
            // ���Ի�ȡ��������ϵ�ũ���Ӿ����
            if (hit.collider.TryGetComponent<FarmPlotView>(out FarmPlotView plotView))
            {
                Vector3Int plotGridPos = plotView.GetGridPosition();
                HandlePlotInteraction(plotGridPos);
            }
        }
    }

    /// <summary>
    /// �������ؽ����߼�
    /// </summary>
    private void HandlePlotInteraction(Vector3Int plotPos)
    {
        _isInteracting = true;

        // ��ȡ�ؿ�����
        FarmingSystem.FarmPlot plotData = _farmingSystem.GetPlotData(plotPos);
        if (plotData == null)
        {
            Debug.LogWarning($"[PlotInteraction] ���� {plotPos} δ��ʼ��������δ������");
            _isInteracting = false;
            return;
        }

        // ��֧1������ֲ�ҳ��� �� �ջ�
        if (plotData.IsPlanted && plotData.IsGrown)
        {
            HarvestTargetPlot(plotData, plotPos);
        }
        // ��֧2���ѽ�����δ��ֲ �� ��ֲ��ʹ�õ�ǰѡ�е����
        else if (plotData.SoilState == PlotState.Unlocked_Empty)
        {
            PlantOnTargetPlot(plotPos, _selectedCropType);
        }
        // ��֧3������ֲ��δ���� �� ��ʾ
        else if (plotData.IsPlanted && !plotData.IsGrown)
        {
            float growthPercent = Mathf.Round(plotData.GrowthProgress * 100);
            Debug.Log($"[PlotInteraction] ���� {plotPos} ��������������{growthPercent}%��");
        }
        // ��֧4��δ���� �� ��ʾ
        else if (plotData.SoilState == PlotState.Locked)
        {
            Debug.Log($"[PlotInteraction] ���� {plotPos} δ��������Ҫ������ʹ�ã�");
        }

        _isInteracting = false;
    }

    // ��ֲ�߼����޸�Ϊʹ�ô�����������ͣ�
    private void PlantOnTargetPlot(Vector3Int plotPos, CropType cropType)
    {
        // ǿ��ˢ�µؿ�����
        FarmingSystem.FarmPlot latestPlotData = _farmingSystem.GetPlotData(plotPos);
        if (latestPlotData != null && latestPlotData.IsPlanted)
        {
            Debug.LogWarning($"[PlotInteraction] ���� {plotPos} ����ֲ���޷��ظ���ֲ");
            return;
        }

        bool plantSuccess = _farmingSystem.PlantCrop(plotPos, cropType);

        //if (plantSuccess)
        //{
        //    FarmingSystem.CropData cropData = _farmingSystem.GetCropConfig(cropType);
        //    if (cropData != null)
        //    {
        //        Debug.Log($"[PlotInteraction] ��ֲ�ɹ������� {plotPos} ��ֲ�� {cropData.Name}��{cropData.GrowthTime}����죩");
        //    }
        //    else
        //    {
        //        Debug.Log($"[PlotInteraction] ��ֲ�ɹ������� {plotPos}��δ�ҵ��������ݣ�");
        //    }
        //}
        //else
        //{
        //    Debug.LogWarning($"[PlotInteraction] ��ֲʧ�ܣ����� {plotPos} �޷���ֲ");
        //}
    }

    private void HarvestTargetPlot(FarmingSystem.FarmPlot plotData, Vector3Int plotPos)
    {
        ItemData harvestedItem = _farmingSystem.HarvestCrop(plotPos);
        //if (harvestedItem != null)
        //{
        //    Debug.Log($"[PlotInteraction] �ջ�ɹ������� {plotPos} ��� {harvestedItem.itemName} x{harvestedItem.currentStack}");
        //}
        //else
        //{
        //    Debug.LogWarning($"[PlotInteraction] �ջ�ʧ�ܣ����� {plotPos} �޳�������");
        //}
    }

    /// <summary>
    /// ����Ƿ�����UI��3D/2Dͨ�ã�
    /// </summary>
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;
        return EventSystem.current.IsPointerOverGameObject();
    }

    /// <summary>
    /// ����ѡ�е��������ͣ���UI���ã�
    /// </summary>
    public void SetSelectedCrop(CropType cropType)
    {
        _selectedCropType = cropType;
        Debug.Log($"[PlotInteractionManager] ��ѡ������: {cropType}");
    }
}
