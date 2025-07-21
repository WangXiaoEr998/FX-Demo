using UnityEngine;
using System.Collections.Generic;
using FanXing.Data;

/// <summary>
/// NPC系统，负责NPC的生命周期管理、对话系统、交互逻辑
/// 作者：黄畅修
/// 创建时间：2025-07-13
/// </summary>
public class NPCSystem : BaseGameSystem
{
    #region 字段定义

    [Header("NPC系统配置")] 
    [SerializeField] private int _maxNPCCount = 50;
    [SerializeField] private float _npcUpdateInterval = 0.1f;
#pragma warning disable CS0414 // 字段已赋值但从未使用 - Demo版本中暂未实现相关功能
    [SerializeField] private float _interactionRange = 3.0f;
#pragma warning restore CS0414

    [Header("NPC预制体")] [SerializeField] private GameObject[] _npcPrefabs;

    // NPC管理
    private List<GameObject> _activeNPCs = new List<GameObject>();
    private Dictionary<int, GameObject> _npcDictionary = new Dictionary<int, GameObject>();
    private Queue<GameObject> _npcPool = new Queue<GameObject>();

    // 对话系统
    private GameObject _currentInteractingNPC;
    private bool _isInDialogue = false;

    // 更新计时器
    private float _updateTimer = 0f;

    #endregion

    #region 属性

    /// <summary>
    /// 系统名称
    /// </summary>
    public override string SystemName => "NPC系统";

    /// <summary>
    /// 当前活跃NPC数量
    /// </summary>
    public int ActiveNPCCount => _activeNPCs.Count;

    /// <summary>
    /// 是否正在对话中
    /// </summary>
    public bool IsInDialogue => _isInDialogue;

    /// <summary>
    /// 当前交互的NPC
    /// </summary>
    public GameObject CurrentInteractingNPC => _currentInteractingNPC;

    #endregion

    #region BaseGameSystem抽象方法实现

    /// <summary>
    /// 系统初始化时调用
    /// </summary>
    protected override void OnInitialize()
    {
        LogDebug("NPC系统初始化开始");

        // 初始化NPC对象池
        InitializeNPCPool();

        // 注册事件监听器
        RegisterEventListeners();

        LogDebug("NPC系统初始化完成");
    }

    /// <summary>
    /// 系统启动时调用
    /// </summary>
    protected override void OnStart()
    {
        LogDebug("NPC系统启动");

        // 开始更新计时器
        _updateTimer = 0f;

        LogDebug("NPC系统启动完成");
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

        // 定期更新NPC
        if (_updateTimer >= _npcUpdateInterval)
        {
            UpdateNPCs();
            _updateTimer = 0f;
        }
    }

    /// <summary>
    /// 系统关闭时调用
    /// </summary>
    protected override void OnShutdown()
    {
        base.OnShutdown();
        UnregisterEventListeners();
        ClearAllNPCs();
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 初始化NPC对象池
    /// </summary>
    private void InitializeNPCPool()
    {
        if (_npcPrefabs == null || _npcPrefabs.Length == 0)
        {
            Debug.LogWarning("NPC预制体列表为空");
            return;
        }

        // 预创建一些NPC对象
        for (int i = 0; i < 10; i++)
        {
            GameObject npc = Instantiate(_npcPrefabs[0]);
            npc.SetActive(false);
            _npcPool.Enqueue(npc);
        }
    }

    /// <summary>
    /// 更新所有NPC
    /// </summary>
    private void UpdateNPCs()
    {
        for (int i = _activeNPCs.Count - 1; i >= 0; i--)
        {
            if (_activeNPCs[i] == null)
            {
                _activeNPCs.RemoveAt(i);
                continue;
            }

            // 更新NPC逻辑
            UpdateNPCBehavior(_activeNPCs[i]);
        }
    }

    /// <summary>
    /// 更新NPC行为
    /// </summary>
    /// <param name="npc">NPC对象</param>
    private void UpdateNPCBehavior(GameObject npc)
    {
        // TODO: 实现NPC行为逻辑
        // 例如：巡逻、对话检测、状态更新等
    }

    /// <summary>
    /// 更新NPC物理
    /// </summary>
    /// <param name="fixedDeltaTime">固定时间增量</param>
    private void UpdateNPCPhysics(float fixedDeltaTime)
    {
        // TODO: 实现NPC物理更新
        // 例如：移动、碰撞检测等
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
    /// 清理所有NPC
    /// </summary>
    private void ClearAllNPCs()
    {
        foreach (var npc in _activeNPCs)
        {
            if (npc != null)
            {
                DestroyImmediate(npc);
            }
        }

        _activeNPCs.Clear();
        _npcDictionary.Clear();

        while (_npcPool.Count > 0)
        {
            var npc = _npcPool.Dequeue();
            if (npc != null)
            {
                DestroyImmediate(npc);
            }
        }
    }

    #endregion

    #region 公共方法

    /// <summary>
    /// 创建NPC
    /// </summary>
    /// <param name="npcType">NPC类型</param>
    /// <param name="position">生成位置</param>
    /// <returns>创建的NPC对象</returns>
    public GameObject CreateNPC(NPCType npcType, Vector3 position)
    {
        if (_activeNPCs.Count >= _maxNPCCount)
        {
            Debug.LogWarning("已达到最大NPC数量限制");
            return null;
        }

        GameObject npc = GetNPCFromPool();
        if (npc != null)
        {
            npc.transform.position = position;
            npc.SetActive(true);
            _activeNPCs.Add(npc);

            if (_enableDebugMode)
            {
                Debug.Log($"创建NPC: {npcType} 在位置 {position}");
            }
        }

        return npc;
    }

    /// <summary>
    /// 从对象池获取NPC
    /// </summary>
    /// <returns>NPC对象</returns>
    private GameObject GetNPCFromPool()
    {
        if (_npcPool.Count > 0)
        {
            return _npcPool.Dequeue();
        }

        // 对象池为空，创建新的NPC
        if (_npcPrefabs != null && _npcPrefabs.Length > 0)
        {
            return Instantiate(_npcPrefabs[0]);
        }

        return null;
    }

    /// <summary>
    /// 开始与NPC对话
    /// </summary>
    /// <param name="npc">NPC对象</param>
    public void StartDialogue(GameObject npc)
    {
        if (_isInDialogue)
        {
            Debug.LogWarning("已经在对话中");
            return;
        }

        _currentInteractingNPC = npc;
        _isInDialogue = true;

        if (_enableDebugMode)
        {
            Debug.Log($"开始与NPC对话: {npc.name}");
        }
    }

    /// <summary>
    /// 结束对话
    /// </summary>
    public void EndDialogue()
    {
        if (!_isInDialogue)
            return;

        if (_enableDebugMode)
        {
            Debug.Log("结束对话");
        }

        _currentInteractingNPC = null;
        _isInDialogue = false;
    }

    #endregion
}