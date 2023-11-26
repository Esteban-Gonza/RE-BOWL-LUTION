using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoController : MonoBehaviour {

    [SerializeField] GameObject skipText;
    [SerializeField] int sceneIndex;
    private LoadScreen loadScreen;

    private void Start() {
        skipText.SetActive(false);
        loadScreen = FindObjectOfType<LoadScreen>();
        StartCoroutine(EndAnimation());
    }

    private void Update() {

        if (Input.anyKey && skipText.activeSelf == false){
            skipText.SetActive(true);
        }

        if (Input.GetKey(KeyCode.X) && skipText.activeSelf == true) {
            loadScreen.LoadingScene(sceneIndex);
        }
    }

    IEnumerator EndAnimation(){
        yield return new WaitForSeconds(63f);
        loadScreen.LoadingScene(sceneIndex);
    }
}