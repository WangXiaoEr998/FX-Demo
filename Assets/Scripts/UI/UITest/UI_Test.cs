using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Test : UIBase
{
    public Button btn;
    public override void OnOpen(object param = null)
    {
        base.OnOpen(param);
        btn.onClick.AddListener(() =>
        {
            Hide();

        });
      
        Debug.Log("");
    }
    public override void LoseFocus()
    {
        Debug.Log("ʧȥ����");
    }
    public override void OnFocus()
    {
        Debug.Log("��ý���");
    }
}
