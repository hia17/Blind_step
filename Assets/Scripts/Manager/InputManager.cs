using UnityEngine;
using UnityEngine.InputSystem;
public class InputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    public static Vector2 moveDir;
    private InputAction moveAction;

    private InputAction useAction;
    public static bool usePressed;

    private InputAction detectAction;
    public static bool detectPressed;

    public static Vector2 mouseScreenPos;  // 스크린 좌표 (픽셀)
    public static Vector2 mouseWorldPos;   // 월드 좌표 (씬 내 위치)

    public static bool getPressed;
    private InputAction getAction;

    public static bool dropPressed;
    private InputAction dropAction;



    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];

        useAction = playerInput.actions["Use"];
        getAction = playerInput.actions["Get"];
        dropAction = playerInput.actions["Drop"];
        detectAction = playerInput.actions["Detect"];

    }

    private void Update()
    {
        moveDir = moveAction.ReadValue<Vector2>();

        usePressed = useAction.WasPressedThisFrame();
       

        mouseScreenPos = Mouse.current.position.ReadValue();
        mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);

        getPressed = getAction.WasPressedThisFrame();
        dropPressed = dropAction.WasPressedThisFrame();
        detectPressed = detectAction.IsPressed();
     
    }
}
