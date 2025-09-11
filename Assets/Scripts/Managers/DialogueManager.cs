using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    // �Ի���壨�������������֣�
    public GameObject dialoguePanel;
    public Text dialogueText;

    // ��Z����ʾ�������֣�
    public GameObject dialoguePrompt;

    // ��ʾ/���ضԻ����
    public void ShowDialogue(string text)
    {
        dialoguePanel.SetActive(true);
        dialogueText.text = text;
    }

    public void HideDialogue()
    {
        dialoguePanel.SetActive(false);
    }

    // ��ʾ/���ذ�Z����ʾ
    public void ShowPrompt()
    {
        dialoguePrompt.SetActive(true);
    }

    public void HidePrompt()
    {
        dialoguePrompt.SetActive(false);
    }
}
