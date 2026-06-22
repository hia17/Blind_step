using UnityEngine;
using UnityEngine.EventSystems;
public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager instance;
    [SerializeField] private GameObject mainMenu;




    private bool isPaused;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;

        }

        mainMenu.SetActive(false);

       
    }

    private void Update()
    {
        if (InputManager.settingopenPressed)
        {
            if (!isPaused)
            {
                Pause();
            }
            else
            {
                Unpause();
            }
        }
        
    }

    private void Pause()
    {
        
        InputManager.DeactivatePlayerControls();
        OpenMainMenu();
    }

    public void Unpause()
    {
        InputManager.ActivatePlayerControls();
        CloseAllMenu();
    }

    private void OpenMainMenu()
    {
        mainMenu.SetActive(true);
    }

    private void CloseAllMenu()
    {
        mainMenu.SetActive(false);
    
    }



    public void OnResumePress()
    {
        Unpause();
    }


}
