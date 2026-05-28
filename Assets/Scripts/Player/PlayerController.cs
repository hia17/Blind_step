using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private float moveSpeed = 5f;
    private float xAxis;
    private float yAxis;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        if(InputManager.usePressed)
        {
            Debug.Log("Use Pressed");
        }
    }
    void FixedUpdate()
    {
        Vector3 moveVector = new Vector3(xAxis * moveSpeed, yAxis * moveSpeed, 0f);
        rb.linearVelocity = moveVector;
    }

    private void GetInput()
    {
        Vector2 moveDir = InputManager.moveDir;
        xAxis = moveDir.x;
        yAxis = moveDir.y;
    }
}
