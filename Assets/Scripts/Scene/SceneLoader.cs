using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("시작 버튼 설정")]
    public string targetSceneName = "1FTo2F_New"; // 시작 버튼 누를 때 넘어갈 씬 이름

    private bool isFading = false;

    // ★ 시작 버튼(StartBtn)을 클릭했을 때 실행될 함수
    public void StartGame()
    {
        if (!isFading)
        {
            StartCoroutine(FadeAndLoadRoutine(targetSceneName));
        }
    }

    // ★ 추가됨: 유니티 버튼에서 씬 이름을 직접 입력해서 이동할 수 있게 해주는 만능 함수
    public void GoToSceneWithFade(string sceneName)
    {
        if (!isFading)
        {
            StartCoroutine(FadeAndLoadRoutine(sceneName));
        }
    }

    private IEnumerator FadeAndLoadRoutine(string sceneName)
    {
        isFading = true;

        // 1. 화면 검게 페이드 아웃
        if (SceneFadeManager.instance != null)
        {
            SceneFadeManager.instance.StartFadeOut();
            while (SceneFadeManager.instance.IsFadingOut)
                yield return null;
        }

        // 2. 비동기 로딩
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
            yield return null;

        // 3. 로딩 완료 후 부드럽게 페이드 인
        yield return new WaitForSeconds(0.1f);
        if (SceneFadeManager.instance != null)
        {
            SceneFadeManager.instance.StartFadeIn();
        }
    }

    // --- 아래는 기존 기능들 ---


    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentIndex + 1);
    }

    public void LoadPreviousScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        if (currentIndex > 0)
        {
            SceneManager.LoadScene(currentIndex - 1);
        }
    }

    // ★ 종료 버튼(ExitBtn)을 클릭했을 때 실행될 함수
    public void ExitGame()
    {
        Debug.Log("게임 종료");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}