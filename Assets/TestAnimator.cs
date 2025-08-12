using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAnimator : MonoBehaviour
{
    private Animator animator;
    
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            Debug.Log("找到了Animator组件！");
        }
        else
        {
            Debug.Log("未找到Animator组件。");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown) {
            if (Input.GetKeyDown(KeyCode.J)) {
                // run
                Debug.Log("run now");
                animator.SetTrigger("RunTrigger");
                return;
            }

            if (Input.GetKeyDown(KeyCode.K)) {
                // run
                Debug.Log("tailwhip now");
                animator.SetTrigger("TailWhipTrigger");
                return;
            }
        }
    }
}
