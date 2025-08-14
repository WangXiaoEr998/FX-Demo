using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // --- 玩家移动设置 ---
    public float moveSpeed = 5.0f;
    public float rotationSpeed = 180f; // 每秒旋转角度
    private Rigidbody rb;

    // --- 鼠标设置 ---
    public float mouseSensitivity = 100.0f;
    private float xRotation = 0f;

    private Animator animator;
    
    // --- 动画状态管理 ---
    private bool isRunning = false;
    private bool isIdle = true;
    private float lastInputTime = 0f;
    private const float INPUT_THRESHOLD = 0.1f; // 输入阈值，避免微小输入触发动画
    
    // --- 输入平滑设置 ---
    private Vector3 smoothedInput = Vector3.zero;
    private Vector3 inputVelocity = Vector3.zero;
    public float inputSmoothTime = 0.1f; // 输入平滑时间
    public float idleSwitchDelay = 0.2f; // 切换到待机的延迟时间
    
    // --- 动画状态检查 ---
    private bool isAnimationPlaying = false;
    private float animationStartTime = 0f;
    public float minAnimationDuration = 3.0f; // 最小动画播放时间

    void Start()
    {
        // 获取 Rigidbody 组件
        rb = GetComponent<Rigidbody>();

        // 锁定鼠标光标，使其不显示并固定在屏幕中央
        Cursor.lockState = CursorLockMode.Locked;

        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 调用处理移动和转向的方法
        HandleMovementInput();
        // HandleMouseLook();
    }

    private void HandleMovementInput()
    {
        // 获取 WASD 的输入
        float xInput = Input.GetAxis("Horizontal");
        float zInput = Input.GetAxis("Vertical");
        Vector3 rawInput = new Vector3(xInput, 0, zInput);

        // 平滑输入
        smoothedInput = Vector3.SmoothDamp(smoothedInput, rawInput, ref inputVelocity, inputSmoothTime);
        
        // 计算移动方向，使用 Transform.right 和 Transform.forward 来确保移动方向正确
        Vector3 moveDirection = (transform.right * smoothedInput.x + transform.forward * smoothedInput.z).normalized;
        
        // 检查是否有有效输入
        bool hasInput = smoothedInput.magnitude > INPUT_THRESHOLD;
        
        // 处理移动和动画
        if (hasInput)
        {
            lastInputTime = Time.time; // 记录最后输入时间
            
            // 更新朝向（平滑旋转）
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, 
                targetRotation, 
                rotationSpeed * Time.deltaTime
            );

            // 执行移动
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
            
            // 处理跑步动画状态
            if (!isRunning && !isAnimationPlaying)
            {
                isRunning = true;
                isIdle = false;
                isAnimationPlaying = true;
                animationStartTime = Time.time;
                animator.SetTrigger("RunTrigger");
                Debug.Log("开始跑步");
            }
        }
        else
        {
            // 只有在超过延迟时间且动画播放足够时间后才切换到待机
            bool canSwitchToIdle = Time.time - lastInputTime > idleSwitchDelay && 
                                  Time.time - animationStartTime > minAnimationDuration;
            
            if (canSwitchToIdle && !isIdle)
            {
                isIdle = true;
                isRunning = false;
                isAnimationPlaying = true;
                animationStartTime = Time.time;
                animator.SetTrigger("TailWhipTrigger");
                Debug.Log("切换到待机状态");
            }
        }
        
        // 检查动画是否播放完成
        if (isAnimationPlaying && Time.time - animationStartTime > minAnimationDuration)
        {
            isAnimationPlaying = false;
        }
    }

//     private void HandleMouseLook()
//     {
//         return;
//         // 获取鼠标输入
//         float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
//         float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

//         // 绕 Y 轴旋转整个玩家对象，实现左右转向
//         transform.Rotate(Vector3.up * mouseX);

//         // 绕 X 轴旋转摄像机，实现上下视角（需要将此脚本挂在摄像机上才能生效，或在玩家中单独处理）
//         xRotation -= mouseY;
//         xRotation = Mathf.Clamp(xRotation, -90f, 90f); // 限制上下视角，防止翻转

//         // 可以在这里获取子物体（如摄像机）并旋转，例如：
//         // Camera.main.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
//     }
}
