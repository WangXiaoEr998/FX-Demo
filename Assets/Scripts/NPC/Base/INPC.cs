/// <summary>
/// NPC接口
/// 作者：Liang
/// 创建时间：2025-07-21
/// </summary>
public interface INPC
{
    /// <summary>
    /// NPC基础数据
    /// </summary>
    public NPCData NPCData { get; protected set; }

    /// <summary>
    /// 初始化NPC
    /// </summary>
    /// <param name="npcData"></param>
    /// <param name="npcInteraction"></param>
    public void OnInitNPC(NPCData npcData);

    /// <summary>
    /// 更新NPC
    /// </summary>
    /// <param name="deltaTime"></param>
    public void OnUpdate(float deltaTime);

    /// <summary>
    /// 释放NPC
    /// </summary>
    public void OnDispose();
}