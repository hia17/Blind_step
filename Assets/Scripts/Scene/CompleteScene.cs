using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CompleteScene : MonoBehaviour
{
    [SerializeField] private string completeScene;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            

            StartCoroutine(Clear(completeScene));
        }
    }
    IEnumerator Clear(string scene)
    {
        BGMManager.Instance.FadeOutVolume();
        yield return new WaitForSeconds(1.5f);
        SceneFadeManager.instance.StartFadeOut();
        SoundFXManager.instance.PlaySoundFXClip(SoundFXManager.SFX.endingdoor, transform, 0.5f);
        yield return new WaitForSeconds(4f);
        
        SceneManager.LoadScene(scene);

    }
    
}
