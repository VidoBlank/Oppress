using UnityEngine;

public class AudioListenerController : MonoBehaviour
{
    public Transform cameraTransform; // �����Transform
    public float positionLerpSpeed = 0.1f; // ��������ƶ��Ĳ�ֵ�ٶ�
    public float moveDamping = 0.5f; // �������ƶ���˥��ϵ��
    public float minZValue = 0f; // ��СZֵ��FOV��Сʱ�ĸ߶ȣ�
    public float maxZValue = 10f; // ���Zֵ��FOV�Ŵ�ʱ�ĸ߶ȣ�

    private Vector3 targetPosition; // Ŀ��λ��
    private Cinemachine.CinemachineVirtualCamera virtualCamera; // Cinemachine�������
    private float originalFOV; // �����ԭʼFOV

    private void Start()
    {
        if (cameraTransform == null)
        {
            Debug.LogError("��ָ�������Transform��");
            return;
        }

        // ��ȡCinemachine�������
        virtualCamera = cameraTransform.GetComponent<Cinemachine.CinemachineVirtualCamera>();
        if (virtualCamera != null)
        {
            originalFOV = virtualCamera.m_Lens.FieldOfView;
        }
        else
        {
            Debug.LogError("�����δ�ҵ�CinemachineVirtualCamera�����");
        }

        targetPosition = transform.position;
    }

    private void Update()
    {
        if (cameraTransform == null || virtualCamera == null)
        {
            return;
        }

        // ��ȡ�����λ�ñ仯
        Vector3 cameraDelta = cameraTransform.position - targetPosition;

        // �������������Ŀ��λ�ã������ƶ���������ʣ�
        Vector3 moveOffset = cameraDelta * moveDamping;
        targetPosition = Vector3.Lerp(targetPosition, cameraTransform.position - moveOffset, positionLerpSpeed * Time.deltaTime);

        // ���������FOV����Zֵ
        float currentFOV = virtualCamera.m_Lens.FieldOfView;
        float zValue = Mathf.Lerp(maxZValue, minZValue, (currentFOV - minFOV) / (maxFOV - minFOV));

        // ���¼�����λ��
        transform.position = new Vector3(targetPosition.x, targetPosition.y, zValue);
    }

    // FOV��Χ
    public float minFOV = 15f; // ��СFOV
    public float maxFOV = 60f; // ���FOV
}
