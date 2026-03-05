using UnityEngine;
using TMPro;

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
        if (!stageText) return;

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
