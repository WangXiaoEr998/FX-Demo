using UnityEngine;

public abstract class UIBase : MonoBehaviour
{
    public virtual string Layer => UIConst.PauseMenu; // Ĭ���ڹ��ܲ㣬�ɱ�������д

    public bool IsOpened { get; private set; }

    /// <summary>
    /// �򿪽���ʱ����
    /// </summary>
    public virtual void OnOpen(object param = null)
    {
        IsOpened = true;
        gameObject.SetActive(true);
    }

    /// <summary>
    /// �رս���ʱ����
    /// </summary>
    public virtual void OnClose()
    {
        IsOpened = false;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// ����ǰ����
    /// </summary>
    public virtual void OnDestroyWindow()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// ��ѡ�����Ŵ򿪶���
    /// </summary>
    public virtual void PlayOpenAnim()
    {
        // ������ DOTween��Animator ���򿪶���
    }

    /// <summary>
    /// ��ѡ�����Źرն���
    /// </summary>
    public virtual void PlayCloseAnim(System.Action onComplete)
    {
        // ������������� onComplete
        onComplete?.Invoke();
    }
}
