using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUIController : MonoBehaviour
{
    [Header("Teleport Skill UI")]
    [SerializeField] private Image teleportCooldownImage;
    [SerializeField] private TMP_Text teleportCooldownText;

    [Header("Defense Skill UI")]
    [SerializeField] private Image defenseCooldownImage;
    [SerializeField] private TMP_Text defenseCooldownText;

    [Header("Summon Skill UI")]
    [SerializeField] private Image summonCooldownImage;
    [SerializeField] private TMP_Text summonCooldownText;

    [SerializeField] private Color cooldownColor = new Color(0.3f, 0.3f, 0.3f, 0.7f);
    private void Start()
    {
        //jn skillSystem = FindObjectOfType<jn>();

        //// 订阅冷却事件
        //skillSystem.OnTeleportCooldownUpdate += UpdateTeleportUI;
        //skillSystem.OnDefenseCooldownUpdate += UpdateDefenseUI;
        //skillSystem.OnSummonCooldownUpdate += UpdateSummonUI;

        // 初始化UI
        UpdateTeleportUI(0, 1);
        UpdateDefenseUI(0, 1);
        UpdateSummonUI(0, 1);
    }

    private void UpdateTeleportUI(float remaining, float total)
    {
        UpdateSkillUI(teleportCooldownImage, teleportCooldownText, remaining, total);
    }

    private void UpdateDefenseUI(float remaining, float total)
    {
        UpdateSkillUI(defenseCooldownImage, defenseCooldownText, remaining, total);
    }

    private void UpdateSummonUI(float remaining, float total)
    {
        UpdateSkillUI(summonCooldownImage, summonCooldownText, remaining, total);
    }
  

    private void UpdateSkillUI(Image cooldownImage, TMP_Text cooldownText, float remaining, float total)
    {
        Image skillIcon = cooldownImage.transform.parent.GetComponent<Image>();

        if (remaining > 0)
        {
            skillIcon.color = cooldownColor;
            cooldownImage.gameObject.SetActive(true);
        }
        else
        {
            skillIcon.color = Color.white;
            cooldownImage.gameObject.SetActive(false);
        }

        cooldownImage.fillAmount = Mathf.Clamp01(remaining / total);
        cooldownText.text = remaining > 0 ? Mathf.Ceil(remaining).ToString() : "";
    }
}