
/// <summary>
/// NPC行为接口
/// </summary>
public interface IAction
{
    /// <summary>
    /// 初始化行为
    /// </summary>
    public void InitAction();

    /// <summary>
    /// 执行行为
    /// </summary>
    public void ExecuteAction();

    /// <summary>
    /// 更新行为
    /// </summary>
    public void UpdateAction();

    /// <summary>
    /// 结束行为
    /// </summary>
    public void ExitAction();
}