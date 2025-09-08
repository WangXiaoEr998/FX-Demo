using FanXing.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 3D农田视觉控制脚本（仅负责土壤外观，不处理作物显示）
/// </summary>
[RequireComponent(typeof(MeshRenderer), typeof(BoxCollider))]
public class FarmPlotView : MonoBehaviour
{
    [Header("农田土壤配置（3D）")]
    [SerializeField] private Material _lockedMaterial;       // 未解锁状态材质
    [SerializeField] private Material _emptyMaterial;        // 已解锁未种植材质
    [SerializeField] private Material _plantedMaterial;      // 已解锁已种植材质

    private MeshRenderer _plotRenderer;  // 土壤渲染器
    private Vector3Int _gridPosition;    // 关联的网格位置
    private BoxCollider _plotCollider;   // 交互碰撞体

    /// <summary>
    /// 初始化农田土壤（仅处理土壤相关设置）
    /// </summary>
    public void Init(Vector3Int gridPos, PlotState initialState)
    {
        _gridPosition = gridPos;
        _plotRenderer = GetComponent<MeshRenderer>();
        _plotCollider = GetComponent<BoxCollider>();

        // 配置碰撞体（仅用于交互检测）
        _plotCollider.isTrigger = true;
        _plotCollider.center = new Vector3(0, 0, 0);
        _plotCollider.size = new Vector3(1, 1, 1);

        // 初始化土壤状态
        UpdatePlotState(initialState);
    }

    /// <summary>
    /// 更新土壤状态（核心功能：仅切换土壤材质）
    /// </summary>
    public void UpdatePlotState(PlotState newState)
    {
        if (_plotRenderer == null) return;

        switch (newState)
        {
            case PlotState.Locked:
                _plotRenderer.material = _lockedMaterial;
                break;
            case PlotState.Unlocked_Empty:
                _plotRenderer.material = _emptyMaterial;
                break;
            case PlotState.Unlocked_Planted:
                _plotRenderer.material = _plantedMaterial;
                break;
        }
    }

    /// <summary>
    /// 获取关联的网格位置（供交互系统使用）
    /// </summary>
    public Vector3Int GetGridPosition() => _gridPosition;
}
