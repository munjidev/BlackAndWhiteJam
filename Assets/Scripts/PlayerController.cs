using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : PortalableObject
{
    private PlayerMovement _playerMovement;
    protected override void Awake()
    {
        base.Awake();
        _playerMovement = GetComponent<PlayerMovement>();
    }

    public override void Warp()
    {
        base.Warp();
        _playerMovement.ResetTargetRotation();
    }
}