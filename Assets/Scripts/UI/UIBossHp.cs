using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 보스 HP 바 UI. 보스 등장 시 페이드인, 사망 시 페이드아웃됩니다.
/// 
/// <para><b>딜레이 바</b>: 즉시 반응하는 HP 바(hpBar)와 일정 딜레이 후 따라오는
/// 지연 바(delayedBar)를 함께 사용하여 데미지 량의 직관적으로 보여줍니다.</para>
/// </summary>
public class UIBossHp : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image hpBar;
    [SerializeField] private Image delayedBar;
    [SerializeField] private TextMeshProUGUI hpText;
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float delayedBarSpeed = 2f;
    [SerializeField] private float barDelay = 0.5f;

    private float _targetFillAmount;
    private bool _isVisible;

    private void Awake()
    {
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        EventManager.OnBossAppeared += OnBossAppeared;
        EventManager.OnBossHpChanged += OnBossHpChanged;
        EventManager.OnBossDied += OnBossDied;
    }

    private void OnDisable()
    {
        EventManager.OnBossAppeared -= OnBossAppeared;
        EventManager.OnBossHpChanged -= OnBossHpChanged;
        EventManager.OnBossDied -= OnBossDied;
    }

    private void Update()
    {
        if (!_isVisible || delayedBar == null) return;

        // 딜레이 바가 실제 HP 바를 부드럽게 따라감
        if (delayedBar.fillAmount > _targetFillAmount)
        {
            delayedBar.fillAmount -= delayedBarSpeed * Time.deltaTime;
            delayedBar.fillAmount = Mathf.Max(delayedBar.fillAmount, _targetFillAmount);
        }
    }

    private void OnBossAppeared(int maxHp)
    {
        _targetFillAmount = 1f;
        if (hpBar != null) hpBar.fillAmount = 1f;
        if (delayedBar != null) delayedBar.fillAmount = 1f;
        
        StopAllCoroutines();
        StartCoroutine(FadeIn());
    }

    private void OnBossHpChanged(int currentHp, int maxHp)
    {
        if (maxHp <= 0) return;
        
        float ratio = (float)currentHp / maxHp;
        _targetFillAmount = ratio;

        if (hpBar != null) hpBar.fillAmount = ratio;
        if (hpText != null) hpText.text = $"{currentHp}/{maxHp}";
    }

    private void OnBossDied()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        _isVisible = true;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        
        if (canvasGroup != null) canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(barDelay);

        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha = 0f;
        _isVisible = false;
    }
}
