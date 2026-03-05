using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIShop : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject panel;
    [SerializeField] private Button exitButton;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Item Buttons")]
    [SerializeField] private Button[] itemButtons;
    [SerializeField] private TMP_Text[] moneyTexts;

    [Header("Item Settings")]
    [SerializeField] private GameObject[] itemPrefabs;

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

    private void Update()
    {
        if (panel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

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

        // 상점 열린 동안 플레이어 입력 비활성화
        if (_player.TryGetComponent(out PlayerController controller))
            controller.SetInputEnabled(false);
    }

    private void Close()
    {
        panel.SetActive(false);

        // 플레이어 입력 다시 활성화
        if (_player && _player.TryGetComponent(out PlayerController controller))
            controller.SetInputEnabled(true);
    }

    private void UpdatePriceTexts()
    {
        if (_shopType == ShopType.WeaponUpgrade && itemPrefabs != null)
        {
            for (int i = 0; i < itemButtons.Length; i++)
            {
                if (i < itemPrefabs.Length && itemPrefabs[i] != null && i < moneyTexts.Length)
                {
                    if (itemPrefabs[i].TryGetComponent(out WeaponItem weaponItem))
                    {
                        moneyTexts[i].text = $"${weaponItem.Data.GetCurrentUpgradePrice()}";
                    }
                }
            }
        }
    }

    private void OnPurchase(int index)
    {
        if (_player == null || index >= moneyTexts.Length) return;

        // MoneyTxt에서 가격 파싱
        if (!int.TryParse(moneyTexts[index].text.Replace("$", "").Replace(",", "").Trim(), out int price)) return;

        // 구매 시도
        if (_player.TryGetComponent(out PlayerStats stats))
        {
            if (stats.SpendMoney(price))
            {
                dialogueText.text = _purchaseDialogue;

                if (_shopType == ShopType.Item)
                {
                    SpawnAndDeliver(index);
                }
                else if (_shopType == ShopType.WeaponUpgrade)
                {
                    if (index < itemPrefabs.Length && itemPrefabs[index] != null && itemPrefabs[index].TryGetComponent(out WeaponItem weaponItem))
                    {
                        weaponItem.Data.currentUpgradeLevel++;
                        UpdatePriceTexts(); // 즉시 가격 업데이트
                    }
                }
            }
            else
            {
                dialogueText.text = _failDialogue;
            }
        }
    }

    private void SpawnAndDeliver(int index)
    {
        if (itemPrefabs[index] == null || _spawnPoint == null) return;

        GameObject item = Instantiate(itemPrefabs[index], _spawnPoint.position, Quaternion.identity);
        StartCoroutine(DeliverToPlayer(item));
    }

    private IEnumerator DeliverToPlayer(GameObject item)
    {
        while (item && _player)
        {
            Vector3 target = _player.transform.position;
            item.transform.position = Vector3.MoveTowards(
                item.transform.position, target, deliverySpeed * Time.deltaTime);

            if (Vector3.Distance(item.transform.position, target) < 0.3f)
                yield break;

            yield return null;
        }
    }
}
