using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScreen : MonoBehaviour{

    [SerializeField] GameObject loadScreen;

    private void Start(){
        loadScreen.SetActive(false);
    }

    public void LoadingScene(int sceneID){
        StartCoroutine(AsyncLoadingScreen(sceneID));
    }

    IEnumerator AsyncLoadingScreen(int sceneID){

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneID);

        loadScreen.SetActive(true);

        while (!operation.isDone){

            yield return null;
        }
    }
}