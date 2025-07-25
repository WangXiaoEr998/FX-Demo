using UnityEngine;

public abstract class UIBase : MonoBehaviour
{
    public virtual string Layer => UIConst.PauseMenu; // 默认在功能层，可被子类重写

    public bool IsOpened { get; private set; }

    /// <summary>
    /// 打开界面时调用
    /// </summary>
    public virtual void OnOpen(object param = null)
    {
        IsOpened = true;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 关闭界面时调用
    /// </summary>
    public virtual void OnClose()
    {
        IsOpened = false;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 销毁前调用
    /// </summary>
    public virtual void OnDestroyWindow()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// 可选：播放打开动画
    /// </summary>
    public virtual void PlayOpenAnim()
    {
        // 可以用 DOTween、Animator 做打开动画
    }

    /// <summary>
    /// 可选：播放关闭动画
    /// </summary>
    public virtual void PlayCloseAnim(System.Action onComplete)
    {
        // 动画播放完调用 onComplete
        onComplete?.Invoke();
    }
}
