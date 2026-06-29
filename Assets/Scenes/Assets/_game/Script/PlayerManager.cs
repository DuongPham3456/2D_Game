using UnityEngine;

public class PlayerManager : CharacterManager
{
    [Header("Player")]
    [SerializeField] PlayerLocomotionManager _playerLocomotionManager;
    [SerializeField] PlayerAnimationManager _playerAnimationManager;

    public override void Awake()
    {
        base.Awake();
        if (_playerLocomotionManager == null) _playerLocomotionManager = GetComponent<PlayerLocomotionManager>();
        if (_playerAnimationManager == null) _playerAnimationManager = GetComponent<PlayerAnimationManager>();
    }

    public void HandleMoveInput(int x, int y)
    {
        _playerLocomotionManager.horizontalInput = x;
        _playerLocomotionManager.verticalInput = y;
        _playerAnimationManager.UpdateMovingParameter(x, y);
    }

    internal void HandleDodgeInput()
    {
        StartCoroutine(_playerLocomotionManager.HandleDodge());
    }
}
