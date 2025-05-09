using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoreController : SingleTon<CoreController>
{
    [Header("��������")]
    public Transform scene;

    //��Ϸ��ʽ��ʼʱ��ί��
    public delegate void InitGameHandler();
    //��Ϸ��ʽ��ʼʱ���õ��¼���ʵ����������ü���
    public event InitGameHandler InitGameEntity;
    public event InitGameHandler InitGameConfig;

    private void OnInitGame()
    {
        //����Ϸ��ʵ�����ʵ����
        if (InitGameEntity != null)
            InitGameEntity();
        else
            Debug.Log("�¼�������");
        //��ȡ��������Ϸʵ���������к�����UI������
        if (InitGameConfig != null)
            InitGameConfig();
        else
            Debug.Log("�¼�������");
    }


    void Start()
    {
        scene = transform.Find("����");
        OnInitGame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
