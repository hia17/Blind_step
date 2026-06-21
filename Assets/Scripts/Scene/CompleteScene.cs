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
            SceneFadeManager.instance.StartFadeOut();

            StartCoroutine(Clear(completeScene));
        }
    }
    IEnumerator Clear(string scene)
    {
        yield return new WaitForSeconds(4f);
        
        SceneManager.LoadScene(scene);

    }
    
}
