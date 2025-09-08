using FanXing.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 3Dũ���Ӿ����ƽű���������������ۣ�������������ʾ��
/// </summary>
[RequireComponent(typeof(MeshRenderer), typeof(BoxCollider))]
public class FarmPlotView : MonoBehaviour
{
    [Header("ũ���������ã�3D��")]
    [SerializeField] private Material _lockedMaterial;       // δ����״̬����
    [SerializeField] private Material _emptyMaterial;        // �ѽ���δ��ֲ����
    [SerializeField] private Material _plantedMaterial;      // �ѽ�������ֲ����

    private MeshRenderer _plotRenderer;  // ������Ⱦ��
    private Vector3Int _gridPosition;    // ����������λ��
    private BoxCollider _plotCollider;   // ������ײ��

    /// <summary>
    /// ��ʼ��ũ������������������������ã�
    /// </summary>
    public void Init(Vector3Int gridPos, PlotState initialState)
    {
        _gridPosition = gridPos;
        _plotRenderer = GetComponent<MeshRenderer>();
        _plotCollider = GetComponent<BoxCollider>();

        // ������ײ�壨�����ڽ�����⣩
        _plotCollider.isTrigger = true;
        _plotCollider.center = new Vector3(0, 0, 0);
        _plotCollider.size = new Vector3(1, 1, 1);

        // ��ʼ������״̬
        UpdatePlotState(initialState);
    }

    /// <summary>
    /// ��������״̬�����Ĺ��ܣ����л��������ʣ�
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
    /// ��ȡ����������λ�ã�������ϵͳʹ�ã�
    /// </summary>
    public Vector3Int GetGridPosition() => _gridPosition;
}
