using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIBossHp : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image fillImage;
    [SerializeField] private Image delayedFillImage;
    [SerializeField] private TextMeshProUGUI hpText;
    
    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float delayedBarSpeed = 0.4f;
    [SerializeField] private float delayedBarWait = 0.5f;

    private float _targetFill;
    private float _delayedFill;
    private Coroutine _fadeCoroutine;
    private Coroutine _delayedBarCoroutine;

    private void Awake()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
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

    // ─────────────────────────── 이벤트 핸들러 ───────────────────────────

    private void OnBossAppeared(int maxHp)
    {
        // 체력바 초기화
        _targetFill = 1f;
        _delayedFill = 1f;

        if (fillImage != null)
            fillImage.fillAmount = 1f;
        if (delayedFillImage != null)
            delayedFillImage.fillAmount = 1f;

        // 페이드 인
        FadeTo(1f);
    }

    private void OnBossHpChanged(int currentHp, int maxHp)
    {
        _targetFill = maxHp > 0 ? (float)currentHp / maxHp : 0f;

        // 메인 바 즉시 반영
        if (fillImage != null)
            fillImage.fillAmount = _targetFill;

        // 지연 바 코루틴 시작
        if (_delayedBarCoroutine != null)
            StopCoroutine(_delayedBarCoroutine);
        _delayedBarCoroutine = StartCoroutine(AnimateDelayedBar());
        
        if (hpText) hpText.text = $"{currentHp}/{maxHp}";
    }

    private void OnBossDied()
    {
        // 페이드 아웃
        FadeTo(0f);
    }

    // ─────────────────────────── 지연 감소 바 ───────────────────────────

    private IEnumerator AnimateDelayedBar()
    {
        // 잠시 대기 후 줄어들기 시작
        yield return new WaitForSeconds(delayedBarWait);

        while (_delayedFill > _targetFill + 0.001f)
        {
            _delayedFill = Mathf.MoveTowards(_delayedFill, _targetFill, delayedBarSpeed * Time.deltaTime);

            if (delayedFillImage != null)
                delayedFillImage.fillAmount = _delayedFill;

            yield return null;
        }

        // 최종 값 보정
        _delayedFill = _targetFill;
        if (delayedFillImage != null)
            delayedFillImage.fillAmount = _delayedFill;
    }

    // ─────────────────────────── 페이드 ───────────────────────────

    private void FadeTo(float targetAlpha)
    {
        if (canvasGroup == null) return;

        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha));
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        canvasGroup.blocksRaycasts = targetAlpha > 0.5f;
    }
}
