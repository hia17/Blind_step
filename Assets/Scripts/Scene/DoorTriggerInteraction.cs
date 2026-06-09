using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class DoorTriggerInteraction : TriggerInteractionBase
{
    public enum DoorToSpawnAt
    {
        None,
        One,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten
    }
 


    [Header("Spwan To")]
    [SerializeField] private DoorToSpawnAt _doorToSpawnTo;
    [SerializeField] private SceneField _sceneToLoad;
    [SerializeField] private bool isForRespawn;
    private bool isTrigger = false;
    [Space(10f)]
    [Header("This Door")]
    public DoorToSpawnAt CurrentDoorPosition;



    protected override void Start()
    {
        base.Start();
    
    }
    protected override void Update()
    {
        base.Update();
       
    }
    public override void Interact()
    {

        Debug.Log("interact");

        if (!isTrigger)
        {
            InputManager.DeactivatePlayerControls();
            SceneSwapManager.SwapSceneFromDoorUse(_sceneToLoad, _doorToSpawnTo);
        }
      
            
    }
   

}
