using UnityEngine;
using Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraController : MonoBehaviour
{
    [Header("基础设置")]
    public CinemachineVirtualCamera cinemachineCam;
    public PolygonCollider2D cameraBounds;
    public static bool enableAutoFocusOnClick = true;

    [SerializeField] public bool isCameraLocked = false;


    [Header("相机移动与缩放")]
    public float moveSpeedX = 20f;
    public float moveSpeedY = 15f;
    public float zoomSpeed = 5f;
    public float minFOV = 15f;
    public float maxFOV = 60f;
    public float edgeBoundary = 10f;

    [Header("平滑参数")]
    public float moveSmoothTime = 0.3f;
    public float zoomSmoothTime = 0.2f;

    [Header("虚焦效果设置")]
    public bool enableZoomBlur = true;
    public float zoomBlurDuration = 0.5f;
    public Volume globalVolume;

    [Header("音效")]
    public AudioSource zoomSoundEffect;
    public AudioSource lockSoundEffect;
    public AudioSource unlockSoundEffect;

    // 状态缓存
    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero;
    private float targetFOV;
    private float fovVelocity = 0f;
    private Vector3 mousePos;

    // 虚焦控制
    private DepthOfField depthOfField;
    private bool isZooming = false;
    private float zoomBlurRemainingTime = 0f;

    private PolygonCollider2D originalBounds;

    private const float BaseFOV = 60f;
    public static CameraController instance;




    private void Awake()
    {
        instance = this;
    }





    private void Start()
    {
        targetPosition = transform.position;
        targetFOV = cinemachineCam.m_Lens.FieldOfView;
        originalBounds = cameraBounds;

        if (globalVolume != null && globalVolume.profile.TryGet(out depthOfField))
        {
            ResetDepthOfField();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            FocusOnSelectedPlayer();
        }

        HandleCameraLockToggle();

        if (!isCameraLocked)
        {
            MoveCamera();
            ZoomCamera();
        }

        mousePos = Input.mousePosition;

        SmoothMoveAndZoom();

        if (enableZoomBlur)
            HandleZoomBlurEffect();
    }



    public void FocusOnPlayer(PlayerController target)
    {
        if (target == null) return;

        Vector3 targetPos = target.transform.position;
        targetPosition = new Vector3(targetPos.x, targetPos.y, transform.position.z); // 瞬移的目标
        transform.position = targetPosition; // 直接设置位置（瞬移）
    }





    private void FocusOnSelectedPlayer()
    {
        if (PlayerManager.instance == null) return;

        var selectedPlayers = PlayerManager.instance.selectedPlayers;
        if (selectedPlayers.Count == 0) return;

        PlayerController targetPlayer;

        if (PlayerManager.instance.highlightedPlayerIndex >= 0 &&
            PlayerManager.instance.highlightedPlayerIndex < selectedPlayers.Count)
        {
            targetPlayer = selectedPlayers[PlayerManager.instance.highlightedPlayerIndex];
        }
        else
        {
            targetPlayer = selectedPlayers[0];
        }

        if (targetPlayer != null)
        {
            Vector3 playerPos = targetPlayer.transform.position;
            Vector3 instantPos = new Vector3(playerPos.x, playerPos.y, transform.position.z);

            // 限制在相机边界内
            Vector3 limitedPos = LimitPositionToBounds(instantPos);

            // 直接设置相机位置
            transform.position = limitedPos;

            // 同时设置 targetPosition，避免下一帧又被 SmoothDamp 拉回来
            targetPosition = limitedPos;
        }
    }

    private void HandleCameraLockToggle()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            isCameraLocked = !isCameraLocked;
            PlaySoundEffect(isCameraLocked ? lockSoundEffect : unlockSoundEffect);
        }
    }

    private void MoveCamera()
    {
        Vector3 direction = Vector3.zero;

        if (mousePos.x >= Screen.width - edgeBoundary) direction.x += 1;
        else if (mousePos.x <= edgeBoundary) direction.x -= 1;

        if (mousePos.y >= Screen.height - edgeBoundary) direction.y += 1;
        else if (mousePos.y <= edgeBoundary) direction.y -= 1;

        float speedX = moveSpeedX * (cinemachineCam.m_Lens.FieldOfView / BaseFOV);
        float speedY = moveSpeedY * (cinemachineCam.m_Lens.FieldOfView / BaseFOV);

        Vector3 move = new Vector3(direction.x * speedX * Time.unscaledDeltaTime,
                                   direction.y * speedY * Time.unscaledDeltaTime, 0f);
        targetPosition += move;

        targetPosition = LimitPositionToBounds(targetPosition);
    }

    private void ZoomCamera()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            targetFOV -= scrollInput * zoomSpeed;
            targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);

            PlaySoundEffect(zoomSoundEffect);

            if (enableZoomBlur)
            {
                isZooming = true;
                zoomBlurRemainingTime = zoomBlurDuration;
            }
        }
    }

    private void SmoothMoveAndZoom()
    {
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, moveSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
        cinemachineCam.m_Lens.FieldOfView = Mathf.SmoothDamp(cinemachineCam.m_Lens.FieldOfView, targetFOV, ref fovVelocity, zoomSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
    }

    private void HandleZoomBlurEffect()
    {
        if (!isZooming) return;

        zoomBlurRemainingTime -= Time.unscaledDeltaTime;

        if (zoomBlurRemainingTime > 0f)
        {
            ApplyZoomBlurEffect(zoomBlurRemainingTime / zoomBlurDuration);
        }
        else
        {
            isZooming = false;
            ResetDepthOfField();
        }
    }

    private void ApplyZoomBlurEffect(float intensity)
    {
        if (depthOfField == null) return;

        depthOfField.gaussianStart.value = Mathf.Lerp(0f, 2f, intensity);
        depthOfField.gaussianEnd.value = Mathf.Lerp(0f, 10f, intensity);
        depthOfField.focusDistance.value = Mathf.Lerp(100f, 5f, intensity);
    }

    private void ResetDepthOfField()
    {
        if (depthOfField == null) return;

        depthOfField.gaussianStart.value = 0f;
        depthOfField.gaussianEnd.value = 0f;
        depthOfField.focusDistance.value = 100f;
    }

    private void PlaySoundEffect(AudioSource audioSource)
    {
        if (audioSource != null && audioSource.clip != null)
            audioSource.PlayOneShot(audioSource.clip);
    }

    private Vector3 LimitPositionToBounds(Vector3 position)
    {
        Vector2 pos2D = new Vector2(position.x, position.y);
        if (!cameraBounds.OverlapPoint(pos2D))
        {
            pos2D = cameraBounds.ClosestPoint(pos2D);
            position.x = pos2D.x;
            position.y = pos2D.y;
        }
        return position;
    }

    // 外部控制接口
    public void SetCameraBounds(PolygonCollider2D newBounds)
    {
        if (newBounds == null)
        {
            Debug.LogWarning("尝试设置相机边界，但新边界为空！");
            return;
        }

        cameraBounds = newBounds;
        Debug.Log("相机边界已更新为新边界。");
    }

    public void ResetCameraBounds()
    {
        if (originalBounds != null)
        {
            cameraBounds = originalBounds;
            Debug.Log("相机边界已重置为初始边界。");
        }
        else
        {
            Debug.LogWarning("没有保存初始边界，无法重置！");
        }
    }

    public float GetCurrentFov()
    {
        return cinemachineCam.m_Lens.FieldOfView;
    }
}
