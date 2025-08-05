using UnityEngine;

/// <summary>
/// NPC行为基类
/// </summary>
public abstract class BaseAction : IAction
{
    #region 字段

    private GameObject _targetObject;

    #endregion

    #region 属性

    public GameObject TargetObject
    {
        get { return _targetObject; }
    }

    #endregion

    public virtual void InitAction()
    {
    }

    public virtual void ExecuteAction()
    {
    }

    public virtual void UpdateAction()
    {
    }

    public virtual void ExitAction()
    {
    }
}