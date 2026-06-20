using UnityEngine;

public class TriggerUI : MonoBehaviour
{
    [SerializeField] private GameObject targetUI;

    private void Start()
    {
        if (targetUI != null)
            targetUI.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (targetUI != null)
            targetUI.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (targetUI != null)
            targetUI.SetActive(false);
    }
}