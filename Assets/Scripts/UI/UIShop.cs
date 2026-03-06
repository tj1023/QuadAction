using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>상점 유형. 아이템 판매와 무기 업그레이드를 구분합니다.</summary>
public enum ShopType
{
    Item,
    WeaponUpgrade
}

/// <summary>
/// 상점 UI 패널. 아이템 구매 및 무기 업그레이드 기능을 제공합니다.
/// 
/// <para><b>구매 흐름</b>: 버튼 클릭 → itemPrices 배열에서 가격 조회 →
/// PlayerStats.SpendMoney → 성공 시 아이템 스폰 또는 업그레이드 적용.</para>
/// 
/// <para><b>가격 관리</b>: UI 텍스트 파싱 대신 itemPrices 배열에 가격을 별도 관리하여
/// 데이터와 표시를 분리합니다. 업그레이드 시 Weapon의 런타임 레벨을 변경합니다.</para>
/// </summary>
public class UIShop : MonoBehaviour, IPopupUI
{
    private const float DeliveryThreshold = 0.3f;

    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button exitButton;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Item Buttons")]
    [SerializeField] private Button[] itemButtons;
    [SerializeField] private TMP_Text[] moneyTexts;

    [Header("Item Settings")]
    [SerializeField] private GameObject[] itemPrefabs;
    [SerializeField] private int[] itemPrices;

    [Header("Audio")]
    [SerializeField] private AudioClip purchaseSuccessSound;
    [SerializeField] private AudioClip purchaseFailSound;

    [Header("Delivery")]
    [SerializeField] private float deliverySpeed = 10f;

    private GameObject _player;
    private Transform _spawnPoint;
    private ShopType _shopType;
    private string _defaultDialogue;
    private string _purchaseDialogue;
    private string _failDialogue;

    private void Awake()
    {
        panel.SetActive(false);

        exitButton.onClick.AddListener(Close);

        for (int i = 0; i < itemButtons.Length; i++)
        {
            int index = i;
            itemButtons[i].onClick.AddListener(() => OnPurchase(index));
        }
    }

    /// <summary>상점 UI를 열고 플레이어 입력을 비활성화합니다.</summary>
    public void Open(GameObject player, Transform spawnPoint, ShopType shopType,
        string defaultDialogue, string purchaseDialogue, string failDialogue)
    {
        _player = player;
        _spawnPoint = spawnPoint;
        _shopType = shopType;
        _defaultDialogue = defaultDialogue;
        _purchaseDialogue = purchaseDialogue;
        _failDialogue = failDialogue;
        panel.SetActive(true);
        dialogueText.text = _defaultDialogue;
        
        UpdatePriceTexts();

        if (_player.TryGetComponent(out PlayerController controller))
            controller.SetInputEnabled(false);
            
        if (UIManager.Instance != null)
            UIManager.Instance.PushUI(this);
    }

    /// <inheritdoc/>
    public void Close()
    {
        panel.SetActive(false);

        if (_player != null && _player.TryGetComponent(out PlayerController controller))
            controller.SetInputEnabled(true);
            
        if (UIManager.Instance != null)
            UIManager.Instance.PopUI(this);
    }

    /// <summary>가격 텍스트를 갱신합니다. 무기 업그레이드 상점에서는 현재 업그레이드 가격을 표시합니다.</summary>
    private void UpdatePriceTexts()
    {
        for (int i = 0; i < itemButtons.Length && i < moneyTexts.Length; i++)
        {
            int price = GetItemPrice(i);
            if (price >= 0)
            {
                moneyTexts[i].text = $"${price}";
                if (itemButtons[i] != null) itemButtons[i].interactable = true;
            }
            else
            {
                moneyTexts[i].text = "N/A"; // 장착하지 않은 무기 등 구매 불가 상태 표시
                if (itemButtons[i] != null) itemButtons[i].interactable = false;
            }
        }
    }

    /// <summary>
    /// 인덱스에 해당하는 아이템의 현재 가격을 반환합니다.
    /// 무기 업그레이드 상점이면 Weapon의 업그레이드 가격을, 아이템 상점이면 고정 가격을 사용합니다.
    /// </summary>
    private int GetItemPrice(int index)
    {
        if (_shopType == ShopType.WeaponUpgrade
            && itemPrefabs != null && index < itemPrefabs.Length
            && itemPrefabs[index] != null
            && itemPrefabs[index].TryGetComponent(out WeaponItem weaponItem))
        {
            // 무기 업그레이드: 플레이어가 장착하고 있는 무기인지 확인 후 런타임 가격 반환
            // 장착하고 있지 않다면 구매 불가 처리 (-1)
            if (_player != null && _player.TryGetComponent(out PlayerWeaponManager wm))
            {
                Weapon weapon = wm.GetWeapon(weaponItem.Data);
                if (weapon != null)
                {
                    return weapon.GetCurrentUpgradePrice();
                }
            }
            return -1;
        }

        // 아이템 상점: Inspector에서 설정된 고정 가격
        if (itemPrices != null && index < itemPrices.Length)
            return itemPrices[index];

        // 가격 배열 미설정 시 텍스트에서 파싱 (레거시 호환)
        if (index < moneyTexts.Length
            && int.TryParse(moneyTexts[index].text.Replace("$", "").Replace(",", "").Trim(), out int parsed))
            return parsed;

        return -1;
    }

    private void OnPurchase(int index)
    {
        if (_player == null || index >= moneyTexts.Length) return;

        int price = GetItemPrice(index);
        if (price < 0) return;

        if (_player.TryGetComponent(out PlayerStats stats))
        {
            if (stats.SpendMoney(price))
            {
                dialogueText.text = _purchaseDialogue;

                if (purchaseSuccessSound != null)
                    SoundManager.Instance.PlayUiSfx(purchaseSuccessSound);

                if (_shopType == ShopType.Item)
                {
                    SpawnAndDeliver(index);
                }
                else if (_shopType == ShopType.WeaponUpgrade)
                {
                    // 실제 무기 업그레이드 적용
                    if (itemPrefabs != null && index < itemPrefabs.Length
                        && itemPrefabs[index] != null
                        && itemPrefabs[index].TryGetComponent(out WeaponItem weaponItem))
                    {
                        if (_player.TryGetComponent(out PlayerWeaponManager wm))
                        {
                            Weapon weapon = wm.GetWeapon(weaponItem.Data);
                            if (weapon != null)
                            {
                                weapon.Upgrade();
                            }
                        }
                    }
                    UpdatePriceTexts();
                }
            }
            else
            {
                dialogueText.text = _failDialogue;
                
                if (purchaseFailSound != null)
                    SoundManager.Instance.PlayUiSfx(purchaseFailSound);
            }
        }
    }

    private void SpawnAndDeliver(int index)
    {
        if (index >= itemPrefabs.Length || itemPrefabs[index] == null || _spawnPoint == null) return;

        GameObject item = Instantiate(itemPrefabs[index], _spawnPoint.position, Quaternion.identity);
        StartCoroutine(DeliverToPlayer(item));
    }

    /// <summary>생성된 아이템을 플레이어 위치까지 자동 이동시킵니다.</summary>
    private IEnumerator DeliverToPlayer(GameObject item)
    {
        while (item != null && _player != null)
        {
            Vector3 target = _player.transform.position;
            item.transform.position = Vector3.MoveTowards(
                item.transform.position, target, deliverySpeed * Time.deltaTime);

            if (Vector3.Distance(item.transform.position, target) < DeliveryThreshold)
                yield break;

            yield return null;
        }
    }
}
