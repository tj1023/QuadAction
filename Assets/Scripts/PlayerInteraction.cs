using UnityEngine;
using System.Collections.Generic;

public class PlayerInteraction : MonoBehaviour
{
    private readonly List<Item> _nearbyItems = new List<Item>();
    private PlayerWeaponManager _weaponManager;

    private void Awake()
    {
        _weaponManager = GetComponent<PlayerWeaponManager>();
    }

    public void AddNearbyItem(Item item)
    {
        if(!_nearbyItems.Contains(item))
            _nearbyItems.Add(item);
    }

    public void RemoveNearbyItem(Item item)
    {
        _nearbyItems.Remove(item);
    }

    public void PickupClosestItem()
    {
        if (_nearbyItems.Count == 0) return;
        
        Item closestItem = null;
        float minDist = float.MaxValue;

        foreach (Item item in _nearbyItems)
        {
            if(item == null) continue;
            
            float dist = Vector3.Distance(transform.position, item.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closestItem = item;
            }
        }

        if (closestItem != null)
        {
            AcquireItem(closestItem);
            _nearbyItems.Remove(closestItem);
        }
    }

    public void AcquireItem(Item item)
    {
        switch (item.Type)
        {
            case Item.ItemType.Ammo:
                break;
            case Item.ItemType.Coin:
                break;
            case Item.ItemType.Grenade:
                break;
            case Item.ItemType.Heart:
                break;
            case Item.ItemType.Weapon:
                if (_weaponManager != null && item.WeaponData != null)
                {
                    _weaponManager.AddWeapon(item.WeaponData);
                }
                break;
        }
        
        item.Collect();
    }
}
