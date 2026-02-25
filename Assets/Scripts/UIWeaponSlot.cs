using UnityEngine;
using UnityEngine.UI;

public class UIWeaponSlot : MonoBehaviour
{
    [System.Serializable]
    public class SlotUI
    {
        public Image iconImage;
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
    }

    private void OnDisable()
    {
        EventManager.OnWeaponAdded -= UpdateSlotIcon;
        EventManager.OnWeaponEquipped -= HighlightSlot;
    }
    
    // 무기를 먹었을 때 아이콘 갱신
    private void UpdateSlotIcon(int slotIndex, WeaponData weaponData)
    {
        if (slotIndex < 0 || slotIndex >= weaponSlots.Length) return;
        
        weaponSlots[slotIndex].iconImage.sprite = weaponData.weaponIcon;
        weaponSlots[slotIndex].iconImage.enabled = true;
    }

    // 무기를 들었을 때 하이라이트 갱신
    private void HighlightSlot(int slotIndex)
    {
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            bool isSelected = (i == slotIndex);
            if (weaponSlots[i].slotNumImage != null)
            {
                weaponSlots[i].slotNumImage.color = isSelected ? selectedColor : unselectedColor;
            }
        }
    }
}
