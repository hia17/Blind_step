using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwapManager : MonoBehaviour
{
    public static SceneSwapManager Instance;
    private static bool _loadFromDoor;

    private GameObject _player;
    private Collider2D _playerColl;
    private Collider2D _doorColl;
    private Vector3 _playerSpawnPosition;

    private DoorTriggerInteraction.DoorToSpawnAt _doorToSpawnTo;




    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this; 
        }
        _player = GameObject.FindGameObjectWithTag("Player");
        if(_player !=null)
            _playerColl = _player.GetComponent<Collider2D>();

    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    public static void SwapSceneFromDoorUse(SceneField myScene, DoorTriggerInteraction.DoorToSpawnAt doorToSpawnAt)
    {
        
        
        _loadFromDoor = true;
        Instance.StartCoroutine(Instance.FadeOutThenChangeScene(myScene, doorToSpawnAt));
      
      
    }


    private IEnumerator FadeOutThenChangeScene(SceneField myScene, DoorTriggerInteraction.DoorToSpawnAt doorToSpawnAt = DoorTriggerInteraction.DoorToSpawnAt.None)
    {
        //start fading to black
        InputManager.DeactivatePlayerControls();
        SceneFadeManager.instance.StartFadeOut();
        //keep fading out
        while (SceneFadeManager.instance.IsFadingOut)
        {
            yield return null;
        }
        yield return new WaitForSeconds(1f);

        _doorToSpawnTo = doorToSpawnAt;
  
        //SceneManager.LoadScene(myScene);
        #region this code load scene first and then fade in
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(myScene);

        //// 씬 로드가 완료될 때까지 대기
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 씬 로드 완료 후 페이드인
        yield return new WaitForSeconds(0.1f);
        if (SceneFadeManager.instance != null)
        {
            SceneFadeManager.instance.StartFadeIn();
        }
        #endregion

    }
    //private IEnumerator BrightOutThenChangeScene(SceneField myScene, DoorTriggerInteraction.DoorToSpawnAt doorToSpawnAt = DoorTriggerInteraction.DoorToSpawnAt.None)
    //{
    //    _isBox = true;
    //    //start fading to black
    //    InputManager.DeactivatePlayerControls();
    //    SceneBrightManager.instance.StartBrightOut();
    //    SoundFXManager.instance.PlaySoundFXClip(SoundFXManager.SFX.brightout, transform, 1f);
    //    //keep fading out
    //    while (SceneBrightManager.instance.IsBrightOut)
    //    {
    //        yield return null;
    //    }
    //    yield return new WaitForSeconds(1f);

    //    _doorToSpawnTo = doorToSpawnAt;
    //    //카메라 초기화
    //    CameraUtility.InvalidateCache();
        
        
    //    SceneManager.LoadScene(myScene);
    //}

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //플레이어 다시찾아서 위치시키기
        if (_loadFromDoor)
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            _playerColl = _player.GetComponent<Collider2D>();
            FindDoor(_doorToSpawnTo);
            _player.transform.position = _playerSpawnPosition;
            _loadFromDoor = false;
        }
   
        

        //카메라 초기화
       



     
     }



    private IEnumerator ActivatePlayerControl()
    {
        while(SceneFadeManager.instance.IsFadingIn)
        {
            yield return null;
        }
        InputManager.ActivatePlayerControls();
    }
    private void FindDoor(DoorTriggerInteraction.DoorToSpawnAt doorSpawnNumber)
    {
        DoorTriggerInteraction[] doors = FindObjectsByType<DoorTriggerInteraction>(FindObjectsSortMode.None);

        for (int i = 0; i<doors.Length; i++)
        {
            if (doors[i].CurrentDoorPosition == doorSpawnNumber)
            {
                _doorColl = doors[i].GetComponent<Collider2D>();

                //calculate spwan position 
                CalculateSpawnPosition();

                return;
            }
        }
    }

    private void CalculateSpawnPosition()
    {
        float colliderHeight = _playerColl.bounds.extents.y;
        _playerSpawnPosition = _doorColl.transform.position - new Vector3(0f, colliderHeight, 0f);

    }


}
