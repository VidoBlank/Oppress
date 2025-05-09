using System.Collections;
using UnityEngine;

public class Event1_MovingTeach : BaseEvent
{
    private CameraController cameraController;
    public TypewriterColorJitterEffect typewriterEffect; // 教学文本显示效果
    public string[] tutorialTexts; // 教学文本数组
    public float textDisplayDelay = 2f; // 每段文本显示之间的间隔

    public GameObject objectToEnable; // 要启用的物体
    public GameObject uiObject; //准备启用的UI
    public GameObject objectToMove; // 要移动的物体
    public AudioSource audioSource;
    public AudioSource audioSource2;
    public float moveDistance = 1.93f; // 向上移动的距离
    public float moveDuration = 1.5f; // 移动所需时间
    public PolygonCollider2D tutorialCameraBounds; // 新的相机边界

    public float newMinFOV = 10f; // 相机的新最小 FOV
    public float newMaxFOV = 30f; // 相机的新最大 FOV

    private Outline[] outlines; // 存储物体1的子物体的Outline组件
    private bool isObjectMoved = false; // 判断物体是否移动完成


    private void Start()
    {
        typewriterEffect = FindObjectOfType<TypewriterColorJitterEffect>();
        cameraController = FindObjectOfType<CameraController>();

        if (cameraController == null)
        {
            Debug.LogError("CameraController 未找到，请确保场景中有相机控制器。");
        }
    }

    public override void EnableEvent()
    {
        Debug.Log("开始移动教学");

        // 启动教学流程
        StartCoroutine(MovingTeachProcess());
    }

    private IEnumerator MovingTeachProcess()
    {
        // 显示系统提示文本
        yield return DisplayText("[系统提示] 正在进行控制模块校准。");

        // 启用指定物体并禁用其子物体的Outline
        if (objectToEnable != null)
        {
            objectToEnable.SetActive(true);
            outlines = objectToEnable.GetComponentsInChildren<Outline>();
            foreach (var outline in outlines)
            {
                outline.enabled = false;
            }
        }

        // 开始移动另一个物体
        if (objectToMove != null)
        {
            yield return StartCoroutine(MoveObject());
        }
    }

    private IEnumerator MoveObject()
    {
        Vector3 startPosition = objectToMove.transform.position;
        Vector3 targetPosition = startPosition + Vector3.up * moveDistance;
        audioSource.Play();
        float elapsedTime = 0f;
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            objectToMove.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / moveDuration);
            yield return null;
        }

        objectToMove.transform.position = targetPosition;
        isObjectMoved = true; // 标记物体移动完成

        // 启用物体1的Outline组件
        if (outlines != null)
        {
            foreach (var outline in outlines)
            {
                outline.enabled = true;
            }
        }

        

        // 显示移动教学文本
        yield return StartCoroutine(ShowMovingTutorial());

    }

    private void SetTutorialCameraBoundsAndFOV()
    {
        if (cameraController != null)
        {
            // 更新相机边界
            if (tutorialCameraBounds != null)
            {
                cameraController.SetCameraBounds(tutorialCameraBounds);
                Debug.Log("相机边界已更新。");
            }

            // 更新相机 FOV 范围
            cameraController.minFOV = newMinFOV;
            cameraController.maxFOV = newMaxFOV;
            Debug.Log($"相机FOV范围已更新: 最小 {newMinFOV}, 最大 {newMaxFOV}");
        }
    }

    private IEnumerator ShowMovingTutorial()
    {
        // 启用 GameManager 下的 Player Input 组件
        GameObject gameManager = GameObject.Find("Gamemanager");
        if (gameManager != null)
        {
            PlayerInput playerInput = gameManager.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.enabled = true;
                Debug.Log("Player Input 组件已启用");
            }
        }
        // 显示提示文本：“请点选或框选角色”
        yield return DisplayText(tutorialTexts[0]);
        // 修改相机边界和FOV
        SetTutorialCameraBoundsAndFOV();
        // 等待玩家选择角色
        yield return new WaitUntil(() => IsPlayerSelected());
        audioSource2.Play();

        Debug.Log("检测到角色已被选中");
        if (uiObject != null)
        {
            uiObject.SetActive(true);
            Debug.Log("新物体已启用: " + uiObject.name);
        }
        else
        {
            Debug.LogError("新物体未分配，请检查引用是否正确");
        }

        // 显示提示文本：“右键单击地面控制角色移动”
        yield return DisplayText(tutorialTexts[1]);

        // 等待玩家移动角色
        yield return new WaitUntil(() => IsPlayerMoved());

        Debug.Log("检测到角色已移动");
        audioSource2.Play();
        // 提示控制模块校准完成

        yield return DisplayText("控制干员穿过楼梯移动到下方区域。");

        // 教学结束
        EndEvent();
    }

    private bool IsPlayerSelected()
    {
        //return PlayerInput.instance != null && PlayerInput.instance.selectedPlayers.Count > 0;
        return PlayerManager.instance != null && PlayerManager.instance.selectedPlayers.Count > 0;
    }

    private bool IsPlayerMoved()
    {
        if (PlayerManager.instance != null)
        {
            foreach (var player in PlayerManager.instance.selectedPlayers)
            {
                if (player.isMoving)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private IEnumerator DisplayText(string text)
    {
        if (typewriterEffect != null)
        {
            typewriterEffect.SetText(text);

            // 等待文本打字效果完成
            while (typewriterEffect.isTyping)
            {
                yield return null;
            }

            // 等待一段时间再显示下一段提示
            yield return new WaitForSeconds(textDisplayDelay);
        }
        else
        {
            Debug.LogWarning("TypewriterEffect 未找到！");
        }
    }
}
