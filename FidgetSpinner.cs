using UnityEngine;
using TMPro;

public class FidgetSpinner : MonoBehaviour
{
    // 陀螺旋转相关
    private bool isDragging = false; // 是否正在拖动
    private float currentRotationSpeed = 0f; // 当前旋转速度（度/帧）
    private float dragStartAngle = 0f; // 拖动开始时的角度
    private float lastMousePosX = 0f; // 上一帧触摸X坐标
    [SerializeField] private float friction = 0.985f; // 摩擦力（越小减速越快，可调手感）
    [SerializeField] private float speedMultiplier = 0.5f; // 拖动速度转旋转速度的倍率（调手感）
    [SerializeField] private float maxRotationSpeed = 50f; // 最大旋转速度（防止数值溢出）

    // UI相关
    [SerializeField] private TMP_Text speedText; // 转速显示文本
    private float rotationPerMinute = 0f; // 每分钟旋转圈数

    // ===== 新增：文本防抖动参数 =====
    private float targetSpeed = 0f; // 真实转速（计算值）
    private float displaySpeed = 0f; // 显示转速（平滑后的值）
    private float updateTimer = 0f; // 更新计时器
    [SerializeField] private float updateInterval = 0.1f; // 文本更新间隔（0.1秒=10次/秒）
    [SerializeField] private float smoothSpeed = 5f; // 数值平滑速度
    void Update()
    {
        if (gameObject == null) return;

        HandleTouchInput();
        HandleInertiaRotation();
        CalculateTargetSpeed(); // 新增：先计算真实转速
        UpdateSpeedDisplay();   // 再防抖显示文本
    }

    // 处理触摸/鼠标输入
    private void HandleTouchInput()
    {
        // 移动端触摸
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    isDragging = true;
                    lastMousePosX = touch.position.x;
                    dragStartAngle = transform.eulerAngles.y; // 改为记录Y轴角度
                    currentRotationSpeed = 0f; // 拖动时重置惯性速度
                    break;
                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        // 计算X轴拖动距离
                        float deltaX = touch.position.x - lastMousePosX;
                        // 拖动距离转旋转角度（绕Y轴旋转）
                        float rotateAngle = deltaX * speedMultiplier;
                        transform.Rotate(0, rotateAngle, 0); // 核心修改：绕Y轴旋转
                        // 记录当前触摸位置，用于下一帧计算
                        lastMousePosX = touch.position.x;
                        // 累计拖动速度（用于松手后的初始惯性），并限制最大值
                        currentRotationSpeed = Mathf.Clamp(currentRotationSpeed + deltaX * 0.1f, -maxRotationSpeed, maxRotationSpeed);
                    }
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isDragging = false;
                    break;
            }
        }
        // PC端鼠标模拟（方便调试）
        else
        {
            if (Input.GetMouseButton(0))
            {
                if (!isDragging)
                {
                    isDragging = true;
                    lastMousePosX = Input.mousePosition.x;
                    dragStartAngle = transform.eulerAngles.y; // 改为记录Y轴角度
                    currentRotationSpeed = 0f;
                }
                else
                {
                    float deltaX = Input.mousePosition.x - lastMousePosX;
                    float rotateAngle = deltaX * speedMultiplier;
                    transform.Rotate(0, rotateAngle, 0); // 核心修改：绕Y轴旋转
                    lastMousePosX = Input.mousePosition.x;
                    // 限制旋转速度上限，防止溢出
                    currentRotationSpeed = Mathf.Clamp(currentRotationSpeed + deltaX * 0.1f, -maxRotationSpeed, maxRotationSpeed);
                }
            }
            else if (Input.GetMouseButtonUp(0) && isDragging) // 增加isDragging校验，避免重复重置
            {
                isDragging = false;
            }
        }
    }

    // 处理松手后的惯性旋转（速度递减）
    private void HandleInertiaRotation()
    {
        if (!isDragging && Mathf.Abs(currentRotationSpeed) > 0.01f)
        {
            // 应用摩擦力，减速
            currentRotationSpeed *= friction;
            // 按当前速度旋转陀螺（绕Y轴，限制最小速度，避免抖动）
            float rotateSpeed = Mathf.Abs(currentRotationSpeed) < 0.01f ? 0 : currentRotationSpeed;
            transform.Rotate(0, rotateSpeed, 0); // 核心修改：绕Y轴旋转
        }
        else if (!isDragging)
        {
            currentRotationSpeed = 0f; // 速度过小则停止
        }
    }


    // 新增：仅计算真实转速（不更新文本）
    private void CalculateTargetSpeed()
    {
        float degreesPerSecond = 0f;
        if (Time.deltaTime > 0)
        {
            degreesPerSecond = Mathf.Abs(currentRotationSpeed) / Time.deltaTime;
        }
        // 计算真实转速值，仅赋值不显示
        targetSpeed = degreesPerSecond * 60 / 360;
        targetSpeed = Mathf.Clamp(targetSpeed, 0, 9999.9f);
    }

    // 替换原有方法：平滑+防抖更新文本（核心防抖动逻辑）
    private void UpdateSpeedDisplay()
    {
        if (speedText == null) return;

        // 1. 数值平滑插值：让显示值渐变到真实值，消除跳变
        displaySpeed = Mathf.Lerp(displaySpeed, targetSpeed, Time.deltaTime * smoothSpeed);

        // 2. 降低更新频率：每隔0.1秒更新一次文本，减少闪烁
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            // 保留1位小数，避免末位数字跳动
            speedText.text = $"转速：{displaySpeed.ToString("F1")} 圈/分钟"; ; // 英文版本
                                                                          // 
            updateTimer = 0f; // 重置计时器
        }
    }

    // 可选：手动调整手感参数（在Inspector面板可调）
    [ContextMenu("ResetFriction")]
    private void ResetFriction()
    {
        friction = 0.985f;
        speedMultiplier = 0.5f;
        maxRotationSpeed = 50f;
    }
}