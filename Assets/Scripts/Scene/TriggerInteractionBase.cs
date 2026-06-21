using UnityEngine;

public class TriggerInteractionBase : MonoBehaviour, IInteractable
{
    public GameObject Player { get; set; }
    public bool CanInteract { get; set; }

    protected virtual void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player"); 
        if(Player != null)
        {
            Debug.Log(Player.transform.position);
            Debug.Log("플레이어찾음");
        }
    }

    protected virtual void Update()
    {
       
        if (CanInteract)
        {
            
            if (InputManager.usePressed)
            {
                Interact();
                Debug.Log("인터렉트");
            }
                
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) 
        {
            Debug.Log("플레이어찾음");
            CanInteract = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CanInteract = false;
        }
    }
    public virtual void Interact() {}
}
