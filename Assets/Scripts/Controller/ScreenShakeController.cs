using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShakeController : MonoBehaviour
{
    public static ScreenShakeController Instance; // 单例实例

    [Header("震动参数")]
    [SerializeField] float defaultShakeDuration = 0.5f;
    [SerializeField] float defaultShakeMagnitude = 0.2f;

    private Vector3 originalCamPos;
    private Camera mainCamera;
    private bool isShaking = false;


    void Awake()
    {
        // 单例初始化
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

    // 外部调用接口（使用默认参数）
    public void ShakeScreen()
    {
        if (!isShaking) StartCoroutine(Shake(defaultShakeDuration, defaultShakeMagnitude));
    }

    // 带参数的重载版本
    public void ShakeScreen(float duration, float magnitude)
    {
        if (!isShaking) StartCoroutine(Shake(duration, magnitude));
    }

    // 实际震动协程
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
