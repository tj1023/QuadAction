using System;

public static class EventManager
{
    public static Action<int, WeaponData> OnWeaponAdded;
    public static Action<int> OnWeaponEquipped;
    public static Action<int> OnWeaponRemoved;
    public static Action<int, int> OnAmmoChanged;
    public static Action<int, int> OnHpChanged;
    public static Action<int> OnMoneyChanged;
    public static Action OnPlayerDeath;
    public static Action OnPlayerHit;
}
