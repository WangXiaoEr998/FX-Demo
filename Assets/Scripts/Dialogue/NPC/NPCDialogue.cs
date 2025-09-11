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

    // ������Χ���
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            dialogueManager.ShowPrompt(); // ��ʾ"��Z��"��ʾ
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            // �뿪ʱǿ�ƽ����Ի�
            if (isDialogueActive)
            {
                EndDialogue();
            }

            // ��������UI
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
        dialogueManager.HidePrompt();  // ���ذ�Z����ʾ
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

        // ���������ڷ�Χ�ڣ�������ʾ��ʾ
        if (playerInRange)
        {
            dialogueManager.ShowPrompt();
        }
    }
}
