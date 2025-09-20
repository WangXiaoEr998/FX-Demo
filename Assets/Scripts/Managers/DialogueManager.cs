using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    // 对话面板（包含背景和文字）
    public GameObject dialoguePanel;
    public Text dialogueText;

    // 按Z键提示（纯文字）
    public GameObject dialoguePrompt;

    // 显示/隐藏对话面板
    public void ShowDialogue(string text)
    {
        dialoguePanel.SetActive(true);
        dialogueText.text = text;
    }

    public void HideDialogue()
    {
        dialoguePanel.SetActive(false);
    }

    // 显示/隐藏按Z键提示
    public void ShowPrompt()
    {
        dialoguePrompt.SetActive(true);
    }

    public void HidePrompt()
    {
        dialoguePrompt.SetActive(false);
    }
}
