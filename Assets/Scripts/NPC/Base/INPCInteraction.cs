/// <summary>
/// NPC交互接口
/// </summary>
public interface INPCInteraction
{
    /// <summary>
    /// 执行交互
    /// </summary>
    public void ExecuteInteraction();

    /// <summary>
    /// 是否可交互
    /// </summary>
    /// <returns></returns>
    public bool IsCanInteraction();
}