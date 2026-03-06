using System;

/// <summary>
/// 전역 이벤트 버스(Event Bus)로, 시스템 간 결합도를 낮추기 위해 Observer 패턴을 적용합니다.
/// MonoBehaviour 간 직접 참조 대신 이벤트를 통해 통신하므로,
/// UI·사운드·게임 로직 등 각 레이어가 독립적으로 확장·변경 가능합니다.
/// </summary>
public static class EventManager
{
    #region Player Events

    /// <summary>플레이어 HP 변경 시 발행. (currentHp, maxHp)</summary>
    public static Action<int, int> OnHpChanged;

    /// <summary>플레이어가 피격당했을 때 발행. 카메라 흔들림 등 시각적 피드백에 활용.</summary>
    public static Action OnPlayerHit;

    /// <summary>플레이어 사망 시 발행. Game Over UI 및 입력 비활성화에 활용.</summary>
    public static Action OnPlayerDeath;

    /// <summary>플레이어 소지금 변경 시 발행. (currentMoney)</summary>
    public static Action<int> OnMoneyChanged;

    #endregion

    #region Weapon Events

    /// <summary>새 무기가 슬롯에 추가되었을 때 발행. (slotIndex, weaponData)</summary>
    public static Action<int, WeaponData> OnWeaponAdded;

    /// <summary>무기가 장착(활성화)되었을 때 발행. (slotIndex)</summary>
    public static Action<int> OnWeaponEquipped;

    /// <summary>무기가 슬롯에서 제거되었을 때 발행. (slotIndex)</summary>
    public static Action<int> OnWeaponRemoved;

    /// <summary>탄약 수 변경 시 발행. (currentAmmo, reserveAmmo) — 비 원거리 무기는 (-1, -1).</summary>
    public static Action<int, int> OnAmmoChanged;

    #endregion

    #region Boss Events

    /// <summary>보스 등장 시 발행. (maxHp) — 보스 HP 바 초기화에 활용.</summary>
    public static Action<int> OnBossAppeared;

    /// <summary>보스 HP 변경 시 발행. (currentHp, maxHp)</summary>
    public static Action<int, int> OnBossHpChanged;

    /// <summary>보스 사망 시 발행. 보스 HP 바 페이드아웃에 활용.</summary>
    public static Action OnBossDied;

    #endregion

    #region Stage Events

    /// <summary>적 사망 시 발행. StageManager의 남은 적 카운트 감소에 활용.</summary>
    public static Action OnEnemyDied;

    /// <summary>스테이지 전환 시 발행. (stageNumber, stageType)</summary>
    public static Action<int, StageManager.StageType> OnStageChanged;

    #endregion
}
