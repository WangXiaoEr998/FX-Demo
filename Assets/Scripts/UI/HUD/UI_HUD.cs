using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using UnityEngine.UI;

public class UI_HUD : UIBase
{
    [SerializeField] private Button btn;
    // Start is called before the first frame update
    public override void OnOpen(object param = null)
    {
        base.OnOpen(param);
        btn.onClick.AddListener(() => {

            UIManager.Instance.ShowUI("UI_Test");
        
        });
        Debug.Log("<<<<<<<<<<<<<<<<<<<<<<<<<<");

    }
    public override void LoseFocus()
    {
        Debug.Log("失去焦点");
    }
    public override void OnFocus()
    {
        Debug.Log("获得焦点");
    }
}
