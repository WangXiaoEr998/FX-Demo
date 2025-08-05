using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NPC基类
/// 作者：Liang
/// 创建时间：2025-07-21
/// </summary>
public abstract class NPC : MonoBehaviour, INPC, INPCInteraction
{
    #region 字段

    private NPCData _npcData;
    private Queue<BaseAction> _actionList;
    private BaseAction _curAction;
    private bool _isInit = false;

    #endregion

    #region 属性

    NPCData INPC.NPCData
    {
        get => _npcData;
        set => _npcData = value;
    }

    /// <summary>
    /// 当前正在执行的行为
    /// </summary>
    public BaseAction CurAction => _curAction;

    /// <summary>
    /// 是否已经初始化
    /// </summary>
    public bool IsInit => _isInit;

    #endregion

    #region NPC生命周期

    public virtual void OnInitNPC(NPCData npcData)
    {
        _npcData = npcData;
        _isInit = true;
        Global.Event.TriggerEvent(Global.Events.NPC.INITIALIZE, _npcData);
    }

    public virtual void OnUpdate(float deltaTime)
    {
    }

    public virtual void OnDispose()
    {
        _npcData = null;
        _isInit = false;
    }

    public virtual void ExecuteInteraction()
    {
        if (_npcData == null)
        {
            return;
        }

        if (!IsCanInteraction())
        {
            return;
        }

        OnInteraction();
        Global.Event.TriggerEvent(Global.Events.NPC.INTERACTION_STARTED, _npcData.npcId);
    }


    public abstract bool IsCanInteraction();

    /// <summary>
    /// 交互回调
    /// </summary>
    public abstract void OnInteraction();

    #endregion
}