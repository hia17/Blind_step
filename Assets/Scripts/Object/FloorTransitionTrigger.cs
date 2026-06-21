
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class FloorTransitionTrigger : TriggerInteractionBase
{
    [Header("층 전환 설정")]
    [SerializeField] private Transform destination;
    [SerializeField] private CinemachineCamera cameraToActivate;

    [Header("전환 연출")]
    [SerializeField] private float fadeOutSpeed = 5f;
    [SerializeField] private float fadeInSpeed = 5f;
    [SerializeField] private float holdDuration = 0.3f;
  
    private bool _isTransitioning = false;

    protected override void Start() => base.Start();
    protected override void Update() => base.Update();

    public override void Interact()
    {
        if (_isTransitioning) return;
        if (destination == null)
        {
            Debug.LogWarning("[FloorTransitionTrigger] destination이 설정되지 않았습니다.");
            return;
        }
        if (cameraToActivate == null)
        {
            Debug.LogWarning("[FloorTransitionTrigger] cameraToActivate가 설정되지 않았습니다.");
            return;
        }

        StartCoroutine(DoTransition());
    }

    private IEnumerator DoTransition()
    {
      
        _isTransitioning = true;

        InputManager.DeactivatePlayerControls();

        SceneFadeManager.instance.ChangeSpeedSettings(fadeOutSpeed, fadeInSpeed);
        SceneFadeManager.instance.StartFadeOut();
        while (SceneFadeManager.instance.IsFadingOut)
            yield return null;
        // 플레이어 이동
        Player.transform.position = destination.position;
        foreach (var cam in FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None))
            cam.gameObject.SetActive(false);
        cameraToActivate.gameObject.SetActive(true);

        yield return new WaitForSeconds(holdDuration);



        // 씬의 모든 시네머신 카메라를 끄고 지정한 것만 켜기
        

        SceneFadeManager.instance.StartFadeIn();
        while (SceneFadeManager.instance.IsFadingIn)
            yield return null;

        InputManager.ActivatePlayerControls();

        _isTransitioning = false;
    }
}