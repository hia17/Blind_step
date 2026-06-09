using UnityEngine;

public interface IInteractable{

    public GameObject Player { get; set; }

    public bool CanInteract { get; set; }

    public void Interact();

}
