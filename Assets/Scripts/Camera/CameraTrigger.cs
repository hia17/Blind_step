using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class CameraTrigger : MonoBehaviour
{
    [Header("입력")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("카메라")]
    [SerializeField] private CinemachineCamera playerCamera;
    [SerializeField] private CinemachineCamera eventCamera;

    [Header("이동")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform targetPoint;
    [SerializeField] private float moveDuration = 3f;

    [Header("우선순위")]
    [SerializeField] private int activePriority = 100;
    [SerializeField] private int inactivePriority = 0;

    private bool playerInside;
    private bool isPlaying;

    private void Update()
    {
        if (!playerInside)
            return;

        if (isPlaying)
            return;

        if (InputManager.getPressed)
        {
            StartCoroutine(CameraSequence());
        }
    }

    private IEnumerator CameraSequence()
    {
        isPlaying = true;
        if (!eventCamera.gameObject.activeSelf)
        {
            eventCamera.gameObject.SetActive(true);
        }
        // 항상 시작 위치로 초기화
        eventCamera.transform.position = startPoint.position;

        // 이벤트 카메라 활성화
        eventCamera.Priority = activePriority;
        playerCamera.Priority = inactivePriority;

        float elapsed = 0f;

        Vector3 startPos = startPoint.position;
        Vector3 endPos = targetPoint.position;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / moveDuration);

            eventCamera.transform.position =
                Vector3.Lerp(startPos, endPos, t);

            yield return null;
        }

        eventCamera.transform.position = endPos;
        yield return new WaitForSeconds(0.5f);

        // 원래 카메라 복귀
        playerCamera.Priority = activePriority;
        eventCamera.Priority = inactivePriority;
        eventCamera.gameObject.SetActive(false);
        isPlaying = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = false;
        }
    }
}