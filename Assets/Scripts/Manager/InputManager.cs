using UnityEngine;
using UnityEngine.InputSystem;
public class InputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    public static Vector2 moveDir;
    private InputAction moveAction;

    private InputAction useAction;
    public static bool usePressed;
    public static bool useReleased;
    public static bool useHeld;

    public static Vector2 mouseScreenPos;  // 스크린 좌표 (픽셀)
    public static Vector2 mouseWorldPos;   // 월드 좌표 (씬 내 위치)


    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];

        useAction = playerInput.actions["Use"];

    }

    private void Update()
    {
        moveDir = moveAction.ReadValue<Vector2>();

        usePressed = useAction.WasPressedThisFrame();
        useReleased = useAction.WasReleasedThisFrame();
        useHeld = useAction.IsPressed();

        mouseScreenPos = Mouse.current.position.ReadValue();
        mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
    }
}
