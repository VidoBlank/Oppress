using UnityEngine;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instance;

    [Header("Audio Components")]
    public AudioSource audioSource;

    [Header("General Clips")]
    public AudioClip successClip;
    public AudioClip failClip;

    [Header("Unit Clips")]
    public AudioClip addUnitClip;
    public AudioClip removeUnitClip;

    private void Awake()
    {
        // ����ģʽ
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>���ųɹ���Ч</summary>
    public void PlaySuccess() => PlayClip(successClip);

    /// <summary>����ʧ����Ч</summary>
    public void PlayFail() => PlayClip(failClip);

    /// <summary>������Ӹ�Ա��Ч</summary>
    public void PlayAddUnit() => PlayClip(addUnitClip);

    /// <summary>����ɾ����Ա��Ч</summary>
    public void PlayRemoveUnit() => PlayClip(removeUnitClip);

    /// <summary>ͳһ���� AudioClip �ķ���</summary>
    private void PlayClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
