using UnityEngine;

public class AudioListenerController : MonoBehaviour
{
    public Transform cameraTransform; // 相机的Transform
    public float positionLerpSpeed = 0.1f; // 跟随相机移动的插值速度
    public float moveDamping = 0.5f; // 监听器移动的衰减系数
    public float minZValue = 0f; // 最小Z值（FOV缩小时的高度）
    public float maxZValue = 10f; // 最大Z值（FOV放大时的高度）

    private Vector3 targetPosition; // 目标位置
    private Cinemachine.CinemachineVirtualCamera virtualCamera; // Cinemachine虚拟相机
    private float originalFOV; // 相机的原始FOV

    private void Start()
    {
        if (cameraTransform == null)
        {
            Debug.LogError("请指定相机的Transform！");
            return;
        }

        // 获取Cinemachine虚拟相机
        virtualCamera = cameraTransform.GetComponent<Cinemachine.CinemachineVirtualCamera>();
        if (virtualCamera != null)
        {
            originalFOV = virtualCamera.m_Lens.FieldOfView;
        }
        else
        {
            Debug.LogError("相机上未找到CinemachineVirtualCamera组件！");
        }

        targetPosition = transform.position;
    }

    private void Update()
    {
        if (cameraTransform == null || virtualCamera == null)
        {
            return;
        }

        // 获取相机的位置变化
        Vector3 cameraDelta = cameraTransform.position - targetPosition;

        // 计算监听器的新目标位置（减少移动距离和速率）
        Vector3 moveOffset = cameraDelta * moveDamping;
        targetPosition = Vector3.Lerp(targetPosition, cameraTransform.position - moveOffset, positionLerpSpeed * Time.deltaTime);

        // 根据相机的FOV调整Z值
        float currentFOV = virtualCamera.m_Lens.FieldOfView;
        float zValue = Mathf.Lerp(maxZValue, minZValue, (currentFOV - minFOV) / (maxFOV - minFOV));

        // 更新监听器位置
        transform.position = new Vector3(targetPosition.x, targetPosition.y, zValue);
    }

    // FOV范围
    public float minFOV = 15f; // 最小FOV
    public float maxFOV = 60f; // 最大FOV
}
