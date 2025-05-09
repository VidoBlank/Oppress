using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SkillCooldownManager : MonoBehaviour
{
    [Header("�� PlayerController�����ڻ�ȡ��ǰװ�����ܣ�")]
    public PlayerController playerController;

    [Header("��ȴ����ͼ�����ʱ��˸Ч����")]
    public Image cooldownBGImage;

    [Header("������ȴ Slider����һ����")]
    public Slider cooldownSlider;

    [Header("����ͼ�� Image����һ����")]
    public Image skillImage;

    [Header("��ɫ����")]
    [Tooltip("��ȴ״̬�µ���ɫ����ң�")]
    public Color cooldownColor = new Color(0.29f, 0.29f, 0.29f, 1f);
    [Tooltip("��ȴ���ʱ����ɫ����ɫ��")]
    public Color readyColor = Color.white;
    [Tooltip("���ܼ����У�����������ܣ�����ɫ")]
    public Color activeColor = Color.cyan;
    [Tooltip("����ͼ��ȴ�е���ɫ")]
    public Color bgCooldownColor = new Color(0.15f, 0.15f, 0.15f, 1f);
    [Tooltip("����״̬����ɫ")]
    public Color bgActiveColor = new Color(0.1f, 0.3f, 0.3f, 1f);

    // ״̬
    private bool isCoolingDown = false;
    private bool isActive = false; // ��¼�����Ƿ��ڼ���״̬
    private float currentCooldown = 0f;
    private float currentTimer = 0f;
    private Color originalBGColor;
    private Vector3 originalScale;
    private Sequence activeSequence; // ���ڳ������ܵĶ�������

    private void Awake()
    {
        // ��Awake�м�¼ԭʼ���ţ�������Start֮ǰ��Ҫʹ��
        if (skillImage != null)
        {
            originalScale = skillImage.transform.localScale;
        }
    }

    private void Start()
    {
        if (cooldownSlider != null)
        {
            cooldownSlider.value = 0f;
            cooldownSlider.gameObject.SetActive(false);
        }

        if (skillImage != null)
        {
            skillImage.color = readyColor;
            originalScale = skillImage.transform.localScale;
        }

        if (cooldownBGImage != null)
        {
            // ǿ���� originalBGColor = �ű������õ� bgCooldownColor
            originalBGColor = bgCooldownColor;
            // ͬ������ǰ��ʾ
            cooldownBGImage.color = bgCooldownColor;
        }
    }

    private void Update()
    {
        if (!isCoolingDown || currentCooldown <= 0f || playerController == null)
            return;

        currentTimer += Time.deltaTime;
        float ratio = 1f - (currentTimer / currentCooldown);
        ratio = Mathf.Clamp01(ratio);

        if (cooldownSlider != null)
            cooldownSlider.value = ratio;

        if (ratio <= 0f)
        {
            isCoolingDown = false;

            if (skillImage != null && !isActive) // ֻ���ڷǼ���״̬�Żָ���ɫ
                skillImage.color = readyColor;

            // ��ȴ��ɣ�����ͼ�ָ���ɫ����˸
            if (cooldownBGImage != null && !isActive)
            {
                cooldownBGImage.color = originalBGColor;
                PlayCooldownFinishedEffect();
            }
        }
    }

    /// <summary>
    /// ������ȴ������˲�����ܣ�
    /// </summary>
    public void StartCooldownNow(IPlayerSkill skill)
    {
        if (skill == null)
        {
            Debug.LogWarning("StartCooldownNow �����õ�����Ϊ��");
            return;
        }

        // ֹͣ�κ����ڲ��ŵļ����
        if (activeSequence != null)
        {
            activeSequence.Kill();
            activeSequence = null;
        }

        // ȷ�����ܲ�����ʾΪ����״̬
        isActive = false;

        currentCooldown = skill.Cooldown;
        currentTimer = 0f;
        isCoolingDown = true;

        if (cooldownSlider != null)
        {
            cooldownSlider.gameObject.SetActive(true);
            cooldownSlider.value = 1f;
        }

        if (skillImage != null)
        {
            skillImage.color = cooldownColor;
            // ȷ��ͼ��ָ�ԭʼ��С
            skillImage.transform.localScale = originalScale;
        }

        if (cooldownBGImage != null)
            cooldownBGImage.color = bgCooldownColor;
    }

    /// <summary>
    /// ����ͼ��Ϊ"������"״̬�Ӿ�Ч�����������ܣ�
    /// </summary>
    public void SetSkillActiveVisual(bool active)
    {
        if (skillImage == null) return;

        // ����״̬����
        isActive = active;

        // ֹͣ�κ����ڲ��ŵļ����
        if (activeSequence != null)
        {
            activeSequence.Kill();
            activeSequence = null;
        }

        if (active)
        {
            // ����״̬ - ������ɫ��������嶯��Ч��
            skillImage.color = activeColor;
            if (cooldownBGImage != null)
                cooldownBGImage.color = bgActiveColor;

            // �����µĶ������� - ѭ������Ч��
            activeSequence = DOTween.Sequence();

            // ���ź���ɫ����仯�����嶯��
            activeSequence.Append(skillImage.transform.DOScale(originalScale * 1.2f, 0.4f).SetEase(Ease.OutQuad));
            activeSequence.Join(skillImage.DOColor(activeColor, 0.4f).SetEase(Ease.OutQuad));

            activeSequence.Append(skillImage.transform.DOScale(originalScale * 1.1f, 0.4f).SetEase(Ease.InQuad));
            activeSequence.Join(skillImage.DOColor(new Color(activeColor.r, activeColor.g, activeColor.b, 0.8f), 0.4f).SetEase(Ease.InQuad));

            // ����Ϊѭ������
            activeSequence.SetLoops(-1, LoopType.Restart);

            // ��ӱ���Ч��
            if (cooldownBGImage != null)
            {
                DOTween.Sequence()
                    .Append(cooldownBGImage.DOColor(bgActiveColor, 0.2f))
                    .SetEase(Ease.OutQuad);
            }
        }
        else
        {
            // �Ǽ���״̬ - �ָ�������ɫ�ʹ�С
            skillImage.color = isCoolingDown ? cooldownColor : readyColor;
            skillImage.transform.DOScale(originalScale, 0.2f).SetEase(Ease.OutQuad);

            // �ָ�����ɫ
            if (cooldownBGImage != null)
            {
                cooldownBGImage.DOColor(isCoolingDown ? bgCooldownColor : originalBGColor, 0.2f)
                    .SetEase(Ease.OutQuad);
            }
        }
    }

    /// <summary>
    /// ȡ����ȴ UI�����缼��ȡ��ʹ�ã�
    /// </summary>
    public void CancelCooldown()
    {
        isCoolingDown = false;
        currentCooldown = 0f;
        currentTimer = 0f;

        if (cooldownSlider != null)
            cooldownSlider.gameObject.SetActive(false);

        if (skillImage != null && !isActive) // ֻ���ڷǼ���״̬�Żָ���ɫ
            skillImage.color = readyColor;

        if (cooldownBGImage != null && !isActive) // ֻ���ڷǼ���״̬�Żָ�����ɫ
            cooldownBGImage.color = originalBGColor;
    }

    /// <summary>
    /// ��ȴ�����˸����ͼ����ɫ �� ԭɫ��
    /// </summary>
    private void PlayCooldownFinishedEffect()
    {
        if (cooldownBGImage == null || isActive) return; // �ڼ��ܼ���״̬�²�������ȴ���Ч��

        float duration = 0.25f;
        Color flashColor = Color.white;

        cooldownBGImage.DOColor(flashColor, duration).SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                if (!isActive) // �ٴμ�飬ȷ����Ч�����ǰ����û�б�����
                    cooldownBGImage.DOColor(originalBGColor, duration).SetEase(Ease.InQuad);
            });
    }

    /// <summary>
    /// ��ʾUIʱͬ����ȴ��
    /// </summary>
    public void SyncCooldown()
    {
        if (playerController == null || playerController.equippedSkill == null)
            return;

        // ����Ƿ��ǳ����ͼ��ܲ��������ڼ���״̬
        if (playerController.equippedSkill.IsSustained && playerController.equippedSkill.IsActive)
        {
            // �����ǰ�Ǽ���״̬��UIδ��ʾ����Ч���������UI
            if (!isActive)
            {
                SetSkillActiveVisual(true);
            }
            return;
        }

        // �ȼ����ȴ״̬
        float cooldown = playerController.equippedSkill.Cooldown;
        float timer = playerController.equippedSkillCooldownTimer;

        if (cooldown <= 0f || timer >= cooldown)
        {
            // �����Ѿ���ȴ���
            cooldownSlider.gameObject.SetActive(false);

            // ֻ���ڷǼ���״̬�Żָ�Ĭ����ɫ
            if (!isActive)
            {
                skillImage.color = readyColor;
                if (cooldownBGImage != null)
                    cooldownBGImage.color = originalBGColor;
            }

            isCoolingDown = false;
            return;
        }

        // ���ܻ�����ȴ��
        cooldownSlider.gameObject.SetActive(true);
        float ratio = 1f - (timer / cooldown);
        ratio = Mathf.Clamp01(ratio);
        cooldownSlider.value = ratio;

        // ֻ���ڷǼ���״̬����ʾ��ȴ��ɫ
        if (!isActive)
        {
            skillImage.color = cooldownColor;
            if (cooldownBGImage != null)
                cooldownBGImage.color = bgCooldownColor;
        }

        isCoolingDown = true;
        currentCooldown = cooldown;
        currentTimer = timer;
    }

    /// <summary>
    /// ��鼼���Ƿ�����ȴ��
    /// </summary>
    public bool IsInCooldown()
    {
        return isCoolingDown;
    }

    /// <summary>
    /// ��鼼���Ƿ��ڼ���״̬
    /// </summary>
    public bool IsSkillActive()
    {
        return isActive;
    }

    /// <summary>
    /// ��ȡ��ǰ��ȴ���� (0-1)��0��ʾ�������ȴ��1��ʾ�տ�ʼ��ȴ
    /// </summary>
    public float GetCooldownRatio()
    {
        if (!isCoolingDown || currentCooldown <= 0f)
            return 0f;

        return 1f - (currentTimer / currentCooldown);
    }

    /// <summary>
    /// ��ȡʣ����ȴʱ�䣨�룩
    /// </summary>
    public float GetRemainingCooldownTime()
    {
        if (!isCoolingDown || currentCooldown <= 0f)
            return 0f;

        return currentCooldown - currentTimer;
    }

    /// <summary>
    /// ��ʾ������������ȴUI
    /// </summary>
    public void SetUIVisibility(bool visible)
    {
        if (skillImage != null)
            skillImage.gameObject.SetActive(visible);

        if (cooldownBGImage != null)
            cooldownBGImage.gameObject.SetActive(visible);

        if (cooldownSlider != null && isCoolingDown)
            cooldownSlider.gameObject.SetActive(visible);
    }

    private void OnDisable()
    {
        // ȷ��ֹͣ���ж���
        if (activeSequence != null)
        {
            activeSequence.Kill();
            activeSequence = null;
        }

        DOTween.Kill(skillImage);
        DOTween.Kill(cooldownBGImage);
    }
}