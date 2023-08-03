using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadingManager : MonoBehaviour
{

    
    private static LoadingManager _instance { set; get; }
    public static LoadingManager instance
    {
        get
        {
            
            if (_instance == null)
            {
                _instance = FindObjectOfType<LoadingManager>();

                DontDestroyOnLoad(_instance);
            }
            return _instance;
        }
    }

    public int selectedTexIndex = 0;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        //_instance = this;
    }

    public void LoadSceneAsync(int sceneIndex)
    {
        StartCoroutine(LoadSceneAsyncCoroutine(sceneIndex));
        
    }

    private IEnumerator LoadSceneAsyncCoroutine(int sceneIndex)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);

        while (!asyncLoad.isDone)
        {
            // You can do something here while the scene is loading (e.g., show a loading screen).
            yield return null;
        }
    }
}
