using System;
using UnityEngine;

public class PlayerManager : CharacterManager
{
    // Chia nhỏ các thành phần
    [Header("Player")]
    [SerializeField] PlayerLocomotionManager _playerLocomotionManager;
    //[SerializeField] PlayerEquipmentManager _playerEquipmentManager;
    //[SerializeField] PlayerHealthManager _playerHealthManager;
    [SerializeField] PlayerAnimationManager _playerAnimationManager;


    public override void Awake()
    {
        base.Awake();
        if (_playerLocomotionManager == null) _playerLocomotionManager = GetComponent<PlayerLocomotionManager>();
        //if (_playerEquipmentManager == null) _playerEquipmentManager = GetComponent<PlayerEquipmentManager>();
        //if (_playerHealthManager == null) _playerHealthManager = GetComponent<PlayerHealthManager>();
        if (_playerAnimationManager == null) _playerAnimationManager = GetComponent<PlayerAnimationManager>();
    }

    public void HandleMoveInput(int x, int y)
    {
        // update moveInput
        _playerLocomotionManager.horizontalInput = x;
        _playerLocomotionManager.verticalInput = y;

        //
        _playerAnimationManager.UpdateMovingParameter(x, y);

    }

    internal void HandleDodgeInput()
    {
        StartCoroutine(_playerLocomotionManager.HandleDodge());
    }

    //internal void HandleShootInput(Vector2 lookDir)
    //{
    //    _playerEquipmentManager.HandleShoot(lookDir);
    //}

    //public void HandleMousePos(Quaternion lookAngle)
    //{
    //    _playerEquipmentManager.RotateGun(lookAngle);
    //}

    // bây giờ sẽ làm kế thừa để cho cả enemies cx dùng đc hàm Handle Damage


}
