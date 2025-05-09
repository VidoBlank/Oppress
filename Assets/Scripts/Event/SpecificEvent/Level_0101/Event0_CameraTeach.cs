using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using UnityEngine.UI;

public class Event0_CameraTeach : BaseEvent
{
    private CameraController cameraController;
    public TypewriterColorJitterEffect typewriterEffect;
    public string[] narrativeTexts; // 剧情文本数组
    public string[] tutorialTexts; // 教学文本数组
    public float textDisplayDelay = 2f; // 每段文本显示之间的间隔

    private int currentTextIndex = 0; // 当前显示的提示索引
    public bool cameraUnlocked = false; // 相机是否已经解锁

    public CanvasGroup blackScreen; // 黑场的 CanvasGroup
    public float fadeDuration = 1f; // 黑场淡入淡出的时间
    public GlitchEffect glitchEffect; 
    public PolygonCollider2D tutorialCameraBounds; // 教学过程中的相机边界

    private bool glitchTriggered = false;
    private Vector3 lastCameraPosition;
    private float lastCameraFOV;


    public AudioSource audioSource;
    public GameObject audioFadeInObject;
    public GameObject Effect;


    private void Start()
    {
        Transform[] allTransforms = FindObjectsOfType<Transform>(true);
        foreach (Transform t in allTransforms)
        {
            if (t.name == "PlayerFrame_2")
            {
                t.gameObject.SetActive(false);
                break;
            }
        }

        // 初始化黑屏状态
        if (blackScreen != null)
        {
            blackScreen.alpha = 1f; 
            blackScreen.blocksRaycasts = true; 
        }

        typewriterEffect = FindObjectOfType<TypewriterColorJitterEffect>();
        glitchEffect = FindObjectOfType<GlitchEffect>(); 

        cameraController = FindObjectOfType<CameraController>();
        if (cameraController == null)
        {
            Debug.LogError("CameraController 未找到，请确保场景中有相机控制器。");
        }
    }

    public override void EnableEvent()
    {
        Debug.Log("开始相机教学");

        // 锁定相机
        if (cameraController != null)
        {
            cameraController.isCameraLocked = true;
            Debug.Log("相机已锁定");
        }
        // 启用 AudioFadeIn 功能
        if (audioFadeInObject != null)
        {
            AudioFadeIn audioFadeIn = audioFadeInObject.GetComponent<AudioFadeIn>();
            if (audioFadeIn != null)
            {
                audioFadeIn.enableEffect = true; // 启用 AudioFadeIn 的效果
                Debug.Log("AudioFadeIn 已启用。");
            }
            else
            {
                Debug.LogWarning("AudioFadeIn 脚本未找到！");
            }
        }
        else
        {
            Debug.LogWarning("audioFadeInObject 未指定！");
        }
        // 开始教学流程
        StartCoroutine(StartCameraTeach());
    }

    private IEnumerator StartCameraTeach()
    {
        // 阶段 1：黑场淡入过渡
        
        yield return StartCoroutine(FadeBlackScreen(1, 0)); // 从全黑淡出到透明

        // 阶段 2：黑场中显示剧情文本
        yield return StartCoroutine(ShowNarrativeTexts());
       


        // 阶段 3：设置相机边界
        SetTutorialCameraBounds();
        

        // 阶段 4：教学开始
        yield return StartCoroutine(CameraTutorial());
    }


    private IEnumerator FadeBlackScreen(float from, float to)
    {
        float elapsedTime = 0f;

        // 确保获取到 Color Adjustments
        ColorAdjustments colorAdjustments = null;
        if (glitchEffect.globalVolume.profile.TryGet(out colorAdjustments))
        {
            colorAdjustments.active = true;
        }
        else
        {
            Debug.LogWarning("Color Adjustments not found in Volume Profile.");
        }

        // 第一段 Lerp，调整曝光值
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            // 如果 Color Adjustments 存在，调整 postExposure
            if (colorAdjustments != null)
            {
                colorAdjustments.postExposure.value = Mathf.Lerp(-10f, -7.5f, elapsedTime / fadeDuration);
            }

            yield return null;
        }

        // 确保最终曝光值
        if (colorAdjustments != null)
        {
            colorAdjustments.postExposure.value = -7.5f; // 最终曝光值
        }

        // 重置 elapsedTime 以便开始第二段 Lerp
        elapsedTime = 0f;

        // 第二段 Lerp，调整黑屏透明度
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            // Lerp the black screen alpha
            blackScreen.alpha = Mathf.Lerp(from, to, elapsedTime / fadeDuration);

            yield return null;
        }

        // 确保最终透明度值正确
        blackScreen.alpha = to;
        blackScreen.blocksRaycasts = to > 0; // 如果黑场完全透明，取消对输入的阻挡
    }



    private IEnumerator ShowNarrativeTexts()
    {
        for (int i = 0; i < narrativeTexts.Length; i++)
        {
            yield return DisplayText(narrativeTexts[i]);

            
            if (i == 3 && !glitchTriggered && glitchEffect != null)
            {
                glitchTriggered = true; 
                yield return StartCoroutine(glitchEffect.StartGlitchEffect());
            }

        }
    }

    private void SetTutorialCameraBounds()
    {
        if (cameraController != null && tutorialCameraBounds != null)
        {
            cameraController.SetCameraBounds(tutorialCameraBounds);
            Effect.SetActive(true);
            Debug.Log("相机边界已更新。");

        }
       
    }

    private IEnumerator CameraTutorial()
    {
        if (typewriterEffect == null)
        {
            typewriterEffect = FindObjectOfType<TypewriterColorJitterEffect>();
        }

        // 提示玩家解锁相机
        yield return DisplayText("按下[Z]键以解锁/锁定监视器。");
        yield return new WaitUntil(() => !cameraController.isCameraLocked);

        Debug.Log("相机已成功解锁。");
        audioSource.Play();
        cameraUnlocked = true;

        // 初始化相机位置和缩放状态
        if (Camera.main != null)
        {
            lastCameraPosition = Camera.main.transform.position;
            lastCameraFOV = Camera.main.fieldOfView;
        }

        // 提示玩家移动相机
        yield return DisplayText("移动鼠标到屏幕边缘以移动监视器。");
        yield return new WaitUntil(() => CameraMoved());
        audioSource.Play();
        Debug.Log("检测到相机移动。");

        // 提示玩家缩放相机
        yield return DisplayText("使用鼠标滚轮调整监视器缩放。");
        yield return new WaitUntil(() => CameraZoomed());
        audioSource.Play();
        Debug.Log("检测到相机缩放。");

        // 教学结束
        yield return DisplayText("显示模块校准完成。正在部署一名职能为<color=#FFFF00>步枪手</color>的Synaptic干员。");
        Debug.Log("相机教学结束。");
        EndEvent(); // 教学结束
    }

    private bool CameraMoved()
    {
        // 检测相机位置变化
        if (Camera.main != null)
        {
            Vector3 currentCameraPosition = Camera.main.transform.position;
            if (currentCameraPosition != lastCameraPosition)
            {
                lastCameraPosition = currentCameraPosition; // 更新上一次记录的位置
                return true;
            }
        }
        return false;
    }

    private bool CameraZoomed()
    {
        // 检测相机 FOV (Field of View) 缩放变化
        if (Camera.main != null)
        {
            float currentFOV = Camera.main.fieldOfView;
            if (Mathf.Abs(currentFOV - lastCameraFOV) > 0.1f) // 检测缩放变化幅度
            {
                lastCameraFOV = currentFOV; // 更新上一次记录的缩放值
                return true;
            }
        }
        return false;
    }

















    private IEnumerator DisplayText(string text)
    {
        // 设置新的提示文本
        typewriterEffect.SetText(text);

        // 等待文本打字效果完成
        while (typewriterEffect.isTyping)
        {
            yield return null;
        }

        // 等待一段时间再显示下一段提示
        yield return new WaitForSeconds(textDisplayDelay);
    }
}
