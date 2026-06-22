using System.Collections;
using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public static BGMManager Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip normalBGM;
    [SerializeField] private AudioClip watcherBGM;
    [SerializeField] private float fadeDuration = 1f;   // 페이드 시간
    private AudioClip currentClip;

    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PlayNormalBGM();
    }

    public void PlayNormalBGM()
    {
        FadeTo(normalBGM);
    }

    public void PlayWatcherBGM()
    {
        FadeTo(watcherBGM);
    }
    public void FadeOutVolume()
    {
        StartCoroutine(FadeOut());
    }
    private IEnumerator FadeOut()
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < 2f)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0f;
    }
    private void FadeTo(AudioClip nextClip)
    {
        if (nextClip == null) return;

        if (currentClip == nextClip)
            return;

        currentClip = nextClip;

        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(FadeRoutine(nextClip));
    }

    private IEnumerator FadeRoutine(AudioClip nextClip)
    {
        // 현재 볼륨에서 0으로 페이드 아웃
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.clip = nextClip;
        currentClip = nextClip;
        audioSource.loop = true;
        audioSource.Play();

        // 0에서 원래 볼륨으로 페이드 인
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, startVolume, elapsed / fadeDuration);
            yield return null;
        }

        audioSource.volume = startVolume;
        _fadeCoroutine = null;
    }
}