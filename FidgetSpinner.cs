using UnityEngine;
using TMPro;

public class FidgetSpinner : MonoBehaviour
{
    // 陀螺旋转相关
    private bool isDragging = false;
    private float currentRotationSpeed = 0f;
    private float lastMousePosX = 0f;
    [SerializeField] private float friction = 0.985f;
    [SerializeField] private float speedMultiplier = 0.5f;
    [SerializeField] private float maxRotationSpeed = 50f;

    // UI相关
    [SerializeField] private TMP_Text speedText;
    private float rotationPerMinute = 0f;

    // 文本防抖动参数
    private float targetSpeed = 0f;
    private float displaySpeed = 0f;
    private float updateTimer = 0f;
    [SerializeField] private float updateInterval = 0.1f;
    [SerializeField] private float smoothSpeed = 5f;

    void Update()
    {
        if (gameObject == null) return;

        HandleTouchInput();
        HandleInertiaRotation();
        CalculateTargetSpeed();
        UpdateSpeedDisplay();
    }

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
                    currentRotationSpeed = 0f;
                    break;
                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        float deltaX = touch.position.x - lastMousePosX;
                        float rotateAngle = deltaX * speedMultiplier;
                        // 绕Y轴旋转
                        transform.Rotate(0, rotateAngle, 0);
                        lastMousePosX = touch.position.x;
                        currentRotationSpeed = Mathf.Clamp(currentRotationSpeed + deltaX * 0.1f, -maxRotationSpeed, maxRotationSpeed);
                    }
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isDragging = false;
                    break;
            }
        }
        // PC端鼠标模拟
        else
        {
            if (Input.GetMouseButton(0))
            {
                if (!isDragging)
                {
                    isDragging = true;
                    lastMousePosX = Input.mousePosition.x;
                    currentRotationSpeed = 0f;
                }
                else
                {
                    float deltaX = Input.mousePosition.x - lastMousePosX;
                    float rotateAngle = deltaX * speedMultiplier;
                    //绕Y轴旋转
                    transform.Rotate(0, rotateAngle, 0);
                    lastMousePosX = Input.mousePosition.x;
                    currentRotationSpeed = Mathf.Clamp(currentRotationSpeed + deltaX * 0.1f, -maxRotationSpeed, maxRotationSpeed);
                }
            }
            else if (Input.GetMouseButtonUp(0) && isDragging)
            {
                isDragging = false;
            }
        }
    }

    private void HandleInertiaRotation()
    {
        if (!isDragging && Mathf.Abs(currentRotationSpeed) > 0.01f)
        {
            currentRotationSpeed *= friction;
            float rotateSpeed = Mathf.Abs(currentRotationSpeed) < 0.01f ? 0 : currentRotationSpeed;
            // 核心修改：绕Y轴惯性旋转
            transform.Rotate(0, rotateSpeed, 0);
        }
        else if (!isDragging)
        {
            currentRotationSpeed = 0f;
        }
    }

    // 以下文本防抖动代码无需修改
    private void CalculateTargetSpeed()
    {
        float degreesPerSecond = 0f;
        if (Time.deltaTime > 0)
        {
            degreesPerSecond = Mathf.Abs(currentRotationSpeed) / Time.deltaTime;
        }
        targetSpeed = degreesPerSecond * 60 / 360;
        targetSpeed = Mathf.Clamp(targetSpeed, 0, 9999.9f);
    }

    private void UpdateSpeedDisplay()
    {
        if (speedText == null) return;

        displaySpeed = Mathf.Lerp(displaySpeed, targetSpeed, Time.deltaTime * smoothSpeed);
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            speedText.text = $"{displaySpeed.ToString("F1")}/min ";
            updateTimer = 0f;
        }
    }

    [ContextMenu("ResetParams")]
    private void ResetParams()
    {
        friction = 0.985f;
        speedMultiplier = 0.5f;
        maxRotationSpeed = 50f;
        smoothSpeed = 5f;
        updateInterval = 0.1f;
    }
}
