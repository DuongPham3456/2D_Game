using UnityEngine;

public class InputPlayerManager : MonoBehaviour
{
    [SerializeField] InputSystem inputSystem;
    [SerializeField] PlayerManager _playerManager;

    [Header("Input Properties")]
    public Vector2 movingInput;
    public bool dodgeInput = false;

    private void Awake()
    {
        inputSystem = new InputSystem();
        BindingInput();
    }

    private void BindingInput()
    {
        inputSystem.Player.Move.performed += ctx => movingInput = ctx.ReadValue<Vector2>();
        inputSystem.Player.Move.canceled += ctx => movingInput = ctx.ReadValue<Vector2>();
        inputSystem.Player.Dodge.performed += ctx => dodgeInput = true;
    }

    private void OnEnable()
    {
        inputSystem.Enable();
    }

    private void OnDisable()
    {
        inputSystem.Disable();
    }

    public void Update()
    {
        HandleMoveInput();
        HandleDodgeInput();
    }

    private void HandleMoveInput()
    {
        // clamping vì input chỉ có thể trả về 1 hoặc -1 nên nếu là 0.7 thì phải floor lên 1
        if (movingInput.x > 0) movingInput.x = 1;
        else if (movingInput.x < 0) movingInput.x = -1;
        else movingInput.x = 0;

        if (movingInput.y > 0) movingInput.y = 1;
        else if (movingInput.y < 0) movingInput.y = -1;
        else movingInput.y = 0;

        _playerManager.HandleMoveInput((int)movingInput.x, (int)movingInput.y);
    }

    private void HandleDodgeInput()
    {
        if (!dodgeInput)
        {
            return;
        }
        dodgeInput = false;
        _playerManager.HandleDodgeInput();
    }
}
