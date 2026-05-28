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

    private void Awake()
    {
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
    }
}
