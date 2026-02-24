using System;
using UnityEngine;

public class AnimationEventHandler : MonoBehaviour
{
    private PlayerController _playerController;

    private void Awake()
    {
        _playerController = GetComponentInParent<PlayerController>();
    }
    
    public void OnDodgeEnd()
    {
        _playerController?.EndDodge();
    }
}
