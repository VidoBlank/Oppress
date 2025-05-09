using UnityEngine;

public class ZSync : MonoBehaviour
{
    [Header("Ŀ�����壨�ṩ Z ��ο���")]
    public Transform target; // Ŀ�����壬�ṩ Z ���׼

    private void Update()
    {
        if (target == null)
        {
            Debug.LogError("���� Inspector ��ָ��Ŀ�����壨target����");
            return;
        }

        // ͬ�� Z ��ֵ
        transform.position = new Vector3(transform.position.x, transform.position.y, target.position.z);
    }
}
