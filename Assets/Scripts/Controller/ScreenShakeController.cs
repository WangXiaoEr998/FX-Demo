using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShakeController : MonoBehaviour
{
    public static ScreenShakeController Instance; // ����ʵ��

    [Header("�𶯲���")]
    [SerializeField] float defaultShakeDuration = 0.5f;
    [SerializeField] float defaultShakeMagnitude = 0.2f;

    private Vector3 originalCamPos;
    private Camera mainCamera;
    private bool isShaking = false;


    void Awake()
    {
        // ������ʼ��
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        mainCamera = Camera.main;
        originalCamPos = mainCamera.transform.localPosition;

    }

    // �ⲿ���ýӿڣ�ʹ��Ĭ�ϲ�����
    public void ShakeScreen()
    {
        if (!isShaking) StartCoroutine(Shake(defaultShakeDuration, defaultShakeMagnitude));
    }

    // �����������ذ汾
    public void ShakeScreen(float duration, float magnitude)
    {
        if (!isShaking) StartCoroutine(Shake(duration, magnitude));
    }

    // ʵ����Э��
    private IEnumerator Shake(float duration, float magnitude)
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Vector3 shakeOffset = Random.insideUnitCircle * magnitude;
            mainCamera.transform.localPosition = originalCamPos + shakeOffset;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        mainCamera.transform.localPosition = originalCamPos;
        isShaking = false;
    }
}
