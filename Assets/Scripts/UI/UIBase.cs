using UnityEngine;

public abstract class UIBase : MonoBehaviour
{
    public UILayerConst layer = UILayerConst.Panel; // Ĭ���ڹ��ܲ㣬�ɱ�������д
    public string uiName { get;  set; }
    public bool IsOpened { get; private set; }

    /// <summary>
    /// �򿪽���ʱ����
    /// </summary>
    public virtual void OnOpen(object param = null)
    {
        IsOpened = true;
       // gameObject.SetActive(true);
    }

    /// <summary>
    /// �رս���ʱ����
    /// </summary>
    public virtual void OnClose()
    {
        IsOpened = false;
      //  gameObject.SetActive(false);
    }

    public virtual void OnFocus()
    {




    }

    public virtual void LoseFocus()
    {




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

    public void Hide()
    {

        UIManager.Instance.HideUI(uiName,false);

    }
}
