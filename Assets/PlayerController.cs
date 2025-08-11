using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
        // --- 玩家移动设置 ---
    public float moveSpeed = 5.0f;
    private Rigidbody rb;

    // --- 鼠标设置 ---
    public float mouseSensitivity = 100.0f;
    private float xRotation = 0f;

  void Start()
    {
        // 获取 Rigidbody 组件
        rb = GetComponent<Rigidbody>();

        // 锁定鼠标光标，使其不显示并固定在屏幕中央
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // 调用处理移动和转向的方法
        HandleMovementInput();
        HandleMouseLook();
    }

    private void HandleMovementInput()
    {
        // 获取 WASD 的输入
        float xInput = Input.GetAxis("Horizontal");
        float zInput = Input.GetAxis("Vertical");

        // 计算移动方向，使用 Transform.right 和 Transform.forward 来确保移动方向正确
        Vector3 moveDirection = (transform.right * xInput + transform.forward * zInput).normalized;

        // 设置 Rigidbody 的速度
        rb.velocity = new Vector3(moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z * moveSpeed);
    }

    private void HandleMouseLook()
    {
        // 获取鼠标输入
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 绕 Y 轴旋转整个玩家对象，实现左右转向
        transform.Rotate(Vector3.up * mouseX);

        // 绕 X 轴旋转摄像机，实现上下视角（需要将此脚本挂在摄像机上才能生效，或在玩家中单独处理）
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // 限制上下视角，防止翻转

        // 可以在这里获取子物体（如摄像机）并旋转，例如：
        // Camera.main.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
