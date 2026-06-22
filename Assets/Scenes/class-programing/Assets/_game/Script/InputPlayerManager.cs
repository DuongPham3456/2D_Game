using UnityEngine;

public class InputPlayerManager : MonoBehaviour
{
    [SerializeField] InputSystem inputSystem;
    [SerializeField] PlayerManager _playerManager;

    [Header("Input Properties")]
    public Vector2 movingInput;
    public Vector2 mousePos;
    public bool dodgeInput = false;
    public bool shootInput = false;


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

      //  inputSystem.Player.Look.performed += ctx => mousePos = ctx.ReadValue<Vector2>();
      //  inputSystem.Player.Shoot.performed += ctx => shootInput = true;
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
    //    HandleShootInput();
    //    HandleMouse();
    }

    private void HandleMoveInput()
    {
        // clamping vì input chỉ có thể trả về 1 hoặc -1 nên nếu là 0.7 thì phải floor lên 1
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

    // private void HandleShootInput()
    // {
    //     if (!shootInput)
    //     {
    //         return;
    //     }
    //     shootInput = false;

    //     Vector2 lookDir = mousePos - centerScreen;
    //     lookDir = lookDir.normalized;
    //     _playerManager.HandleShootInput(lookDir);
    // }

    //  private void HandleMouse()
    //  {
    //      // 2 cách 
    //      // cách 1: Có vị trí của mouse trên screen thì cast qua world rồi tính vecto ra góc xoay
    //      // cách 2: tính luôn góc xoay dựa trên vị trí của mouse trên screen, sau đó chuyển góc xoay sang world
    //      Vector2 lookDir = mousePos - centerScreen;

    //      Quaternion lookAngle = Quaternion.FromToRotation(Vector3.right, lookDir);

    //      _playerManager.HandleMousePos(lookAngle);
    //  }
 }
