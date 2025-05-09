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
        // 单例模式
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

    /// <summary>播放成功音效</summary>
    public void PlaySuccess() => PlayClip(successClip);

    /// <summary>播放失败音效</summary>
    public void PlayFail() => PlayClip(failClip);

    /// <summary>播放添加干员音效</summary>
    public void PlayAddUnit() => PlayClip(addUnitClip);

    /// <summary>播放删除干员音效</summary>
    public void PlayRemoveUnit() => PlayClip(removeUnitClip);

    /// <summary>统一播放 AudioClip 的方法</summary>
    private void PlayClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
