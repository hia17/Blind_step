using UnityEngine;

public class TriggerInteractionBase : MonoBehaviour, IInteractable
{
    public GameObject Player { get; set; }
    public bool CanInteract { get; set; }

    protected virtual void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player"); 
    }

    protected virtual void Update()
    {
        if (CanInteract)
        {
            if (InputManager.usePressed)
                Interact();
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == Player)
        {
            CanInteract = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == Player)
        {
            CanInteract = false;
        }
    }
    public virtual void Interact() {}
}
