using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 무기 슬롯 UI. 무기 추가·장착·제거 이벤트를 구독하여
/// 아이콘 표시 및 선택 하이라이트를 자동 갱신합니다.
/// </summary>
public class UIWeaponSlot : MonoBehaviour
{
    /// <summary>개별 슬롯 UI 요소 그룹.</summary>
    [System.Serializable]
    public class SlotUI
    {
        /// <summary>무기 아이콘 이미지.</summary>
        public Image iconImage;
        /// <summary>슬롯 번호 이미지 (하이라이트 적용 대상).</summary>
        public Image slotNumImage;
    }

    [Header("References")]
    [SerializeField] private SlotUI[] weaponSlots; 
    
    [Header("Highlight Settings")]
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color unselectedColor = Color.gray;

    private void OnEnable()
    {
        EventManager.OnWeaponAdded += UpdateSlotIcon;
        EventManager.OnWeaponEquipped += HighlightSlot;
        EventManager.OnWeaponRemoved += ClearSlotIcon;
    }

    private void OnDisable()
    {
        EventManager.OnWeaponAdded -= UpdateSlotIcon;
        EventManager.OnWeaponEquipped -= HighlightSlot;
        EventManager.OnWeaponRemoved -= ClearSlotIcon;
    }
    
    private void UpdateSlotIcon(int slotIndex, WeaponData weaponData)
    {
        if (slotIndex < 0 || slotIndex >= weaponSlots.Length) return;
        
        weaponSlots[slotIndex].iconImage.sprite = weaponData.WeaponIcon;
        weaponSlots[slotIndex].iconImage.enabled = true;
    }

    private void HighlightSlot(int slotIndex)
    {
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            bool isSelected = (i == slotIndex);
            if (weaponSlots[i].slotNumImage != null)
                weaponSlots[i].slotNumImage.color = isSelected ? selectedColor : unselectedColor;
        }
    }

    private void ClearSlotIcon(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= weaponSlots.Length) return;
        
        weaponSlots[slotIndex].iconImage.sprite = null;
        weaponSlots[slotIndex].iconImage.enabled = false;
    }
}
