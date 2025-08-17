using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // --- 玩家移动设置 ---
    private Animator animator;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float smoothTime = 0.1f;
    public float acceleration = 20f;
    public float deceleration = 20f;
    public float rotationSpeed = 180f; // 旋转速度（度/秒）
    

    [Header("Mouse Settings")]
    public float mouseSensitivity = 100f;
    public float xRotation = 0f;

    private CharacterController characterController;
    private Vector3 moveDirection;
    private Vector3 currentVelocity;
    private Vector3 velocitySmoothDamp; 
    
    // 动画速度平滑
    private float smoothedAnimationSpeed = 0f;
    private float animationSpeedVelocity = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        // 锁定鼠标光标，使其不显示并固定在屏幕中央
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // 调用处理移动和转向的方法
        HandleMovementInput();
        
        // 处理鼠标控制
        HandleMouseLook();
    }

    private void HandleMovementInput()
    {
        // 1. 获取输入
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 inputDirection = new Vector3(horizontal, 0, vertical);
        Vector3 worldDirection = transform.TransformDirection(inputDirection);
        // moveDirection = Vector3.Lerp(moveDirection, Vector3.zero, deceleration * Time.deltaTime);
        // if (inputDirection.magnitude > 0.1f)
        // {
        //     moveDirection = worldDirection.normalized;
        // }

        // Vector3 targetVelocity = moveDirection * moveSpeed;
        // float lerpFactor = (inputDirection.magnitude > 0.1f ? acceleration : deceleration) * Time.deltaTime;
        // currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, lerpFactor);
        // if (currentVelocity.magnitude < 0.1f)
        // {
        //     currentVelocity = Vector3.zero;
        // }



           // 如果有输入，目标速度是移动方向乘以速度
        Vector3 targetVelocity = Vector3.zero;
        if (inputDirection.magnitude > 0.1f)
        {
            targetVelocity = worldDirection.normalized * moveSpeed;
        }

        // 2. 使用 SmoothDamp 平滑地将当前速度过渡到目标速度
        currentVelocity = Vector3.SmoothDamp(
            currentVelocity, 
            targetVelocity, 
            ref velocitySmoothDamp, 
            smoothTime,
            moveSpeed
        );



        animator.SetFloat("CharacterSpeed", currentVelocity.magnitude);
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        
        // simple move 自动应用重力加速度
        characterController.SimpleMove(currentVelocity);
    }
    
    private void HandleMouseLook() {
        // 获取鼠标输入
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 绕 Y 轴旋转整个玩家对象，实现左右转向
        transform.Rotate(Vector3.up * mouseX);

        // 绕 X 轴旋转摄像机，实现上下视角（需要将此脚本挂在摄像机上才能生效，或在玩家中单独处理）
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // 限制上下视角，防止翻转
    }
}