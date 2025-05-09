using UnityEngine;

public abstract class SingleTon<T> : MonoBehaviour where T : MonoBehaviour
{
    //��̬ʵ��
    private static T _instance = null;

    /// <summary>
    ///��������
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                if (FindObjectOfType<T>())
                    _instance = FindObjectOfType<T>();
                else if (GameObject.Find("CoreController"))
                    _instance = GameObject.Find("CoreController").AddComponent<T>();
                else
                    _instance = new GameObject("CoreController").AddComponent<T>();
            }
            return _instance;
        }
    }

    public virtual void Init()
    {
        Debug.Log("��ʼ���ɹ�");
    }


}
