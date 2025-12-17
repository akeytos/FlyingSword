using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    [Header("UI Referanslarý")]
    public Slider progressSlider;

    [Header("Ayar")]
    public float minLoadTime = 2.0f; 

   
    public static string nextSceneName = "SwordSlide-Batu";

    void Start()
    {
        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        
        AsyncOperation operation = SceneManager.LoadSceneAsync(nextSceneName);
        operation.allowSceneActivation = false;

        float elapsedTime = 0f;

        while (!operation.isDone)
        {
            elapsedTime += Time.deltaTime;

            
            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);

            
            float timeProgress = Mathf.Clamp01(elapsedTime / minLoadTime);

            
            progressSlider.value = Mathf.Min(realProgress, timeProgress);


            if (operation.progress >= 0.9f && elapsedTime >= minLoadTime)
            {
               
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}