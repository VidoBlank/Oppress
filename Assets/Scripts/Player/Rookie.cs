using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rookie : PlayerController
{
    private RookieLevelSystem rookieLevelSystem;

    void Awake()
    {
        base.Awake();
        // ��ȡ RookieLevelSystem ���
        rookieLevelSystem = GetComponent<RookieLevelSystem>();
        if (rookieLevelSystem == null)
        {
            Debug.LogError("Rookie δ�ҵ� RookieLevelSystem �����");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        level = Mathf.Clamp(level, 0, 0); // Rookie �ȼ��̶�Ϊ 0
        rookieLevelSystem?.SetRookieAttributes(this); // ���� 0 ������
    }

    public override void SetLevel(int newLevel)
    {
        if (newLevel > 0)
        {
            // �������� 1 ��ʱ��ת��Ϊ����ְҵ��λ
            TransformToClass(newLevel);
        }
        else
        {
            Debug.LogWarning("Rookie ֻ������Ϊ 1 �����ϵ�ְҵ��λ��");
        }
    }

    private void TransformToClass(int targetLevel)
    {
        GameObject newClassPrefab = null;

        // ����Ŀ��ְҵ��λ�����ض�Ӧ��Ԥ����
        switch (targetLevel)
        {
            case 1:
                newClassPrefab = Resources.Load<GameObject>("Prefabs/Rifleman");
                break;
            case 2:
                newClassPrefab = Resources.Load<GameObject>("Prefabs/Engineer");
                break;
            case 3:
                newClassPrefab = Resources.Load<GameObject>("Prefabs/Sniper");
                break;
            case 4:
                newClassPrefab = Resources.Load<GameObject>("Prefabs/Support");
                break;
            default:
                Debug.LogError("δ֪��ְҵ��λĿ��ȼ���");
                return;
        }

        if (newClassPrefab != null)
        {
            // �滻��ǰ����ΪĿ��ְҵ��λ
            GameObject newClassInstance = Instantiate(newClassPrefab, transform.position, transform.rotation);
            Destroy(gameObject); // ���ٵ�ǰ Rookie ����
            Debug.Log($"Rookie ����Ϊ {newClassPrefab.name} ְҵ��λ��");
        }
        else
        {
            Debug.LogError("δ�ҵ�Ŀ��ְҵ��λ��Ԥ���壡");
        }
    }
}
