using System;

public static class EventManager
{
    public static Action<int, WeaponData> OnWeaponAdded;
    public static Action<int> OnWeaponEquipped;
    public static Action<int, int> OnAmmoChanged;
}
