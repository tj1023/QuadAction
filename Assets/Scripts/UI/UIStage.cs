using UnityEngine;
using TMPro;

/// <summary>
/// 스테이지 표시 UI. EventManager.OnStageChanged를 구독하여
/// 현재 스테이지 번호와 유형(Combat/Shop)을 표시합니다.
/// </summary>
public class UIStage : MonoBehaviour
{
    [SerializeField] private TMP_Text stageText;

    private void OnEnable()
    {
        EventManager.OnStageChanged += OnStageChanged;
    }

    private void OnDisable()
    {
        EventManager.OnStageChanged -= OnStageChanged;
    }

    private void OnStageChanged(int stageNumber, StageManager.StageType stageType)
    {
        if (stageText == null) return;

        string typeName = stageType switch
        {
            StageManager.StageType.Start => "Start",
            StageManager.StageType.Combat => "Combat",
            StageManager.StageType.Shop => "Shop",
            _ => ""
        };

        stageText.text = $"Stage {stageNumber} - {typeName}";
    }
}
