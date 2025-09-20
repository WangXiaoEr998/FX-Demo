using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public string[] dialogueLines;

    private bool playerInRange = false;
    private bool isDialogueActive = false;
    private int currentLine = 0;

    // 触发范围检测
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            dialogueManager.ShowPrompt(); // 显示"按Z键"提示
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            // 离开时强制结束对话
            if (isDialogueActive)
            {
                EndDialogue();
            }

            // 隐藏所有UI
            dialogueManager.HidePrompt();
            dialogueManager.HideDialogue();
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.Z))
        {
            if (!isDialogueActive)
            {
                StartDialogue();
            }
            else
            {
                NextDialogue();
            }
        }
    }

    void StartDialogue()
    {
        isDialogueActive = true;
        dialogueManager.HidePrompt();  // 隐藏按Z键提示
        dialogueManager.ShowDialogue(dialogueLines[currentLine]);
        currentLine++;
    }

    void NextDialogue()
    {
        if (currentLine < dialogueLines.Length)
        {
            dialogueManager.ShowDialogue(dialogueLines[currentLine]);
            currentLine++;
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        isDialogueActive = false;
        currentLine = 0;
        dialogueManager.HideDialogue();

        // 如果玩家仍在范围内，重新显示提示
        if (playerInRange)
        {
            dialogueManager.ShowPrompt();
        }
    }
}
