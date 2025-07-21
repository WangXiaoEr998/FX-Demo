using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NPC组件
/// 作者：梁
/// 创建时间：2025-07-21
/// </summary>
public class NPC : MonoBehaviour
{
    #region 字段

    /// <summary>
    /// NPC数据
    /// </summary>
    private NPCData _npcData;

    #endregion

    #region 属性
    public NPCData NpcData { get; }

    #endregion

    #region 公共方法

    public void InitNPC(NPCData npcData)
    {
        if (npcData == null)
        {
            Debug.LogError("NPC数据为空");
            return;
        }

        _npcData = npcData;
    }

    #endregion

    #region 私有方法

    #endregion
}