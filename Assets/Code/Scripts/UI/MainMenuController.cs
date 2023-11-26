using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour{

    [SerializeField] GameObject mainMenuUI;
    [SerializeField] GameObject settingsUI;
    [SerializeField] GameObject creditsUI;
    [SerializeField] GameObject fadeUI;

    private Animator mainMenuAnimator;
    public Animator fadeAnimator;

    bool isInFirstScreen;

    private void Start(){

        mainMenuUI.SetActive(true);
        settingsUI.SetActive(false);
        creditsUI.SetActive(false);

        isInFirstScreen = true;
        mainMenuAnimator = mainMenuUI.GetComponent<Animator>();
        fadeAnimator = fadeUI.GetComponent<Animator>();
    }

    private void Update(){

        if (isInFirstScreen && Input.GetKeyDown(KeyCode.X)){
            isInFirstScreen = false;
            mainMenuAnimator.SetTrigger("KeyPressed");
        }
    }

    // Manage canvas screens
    public void GoToSaveFiels(){
        mainMenuAnimator.SetTrigger("ToSaveFiels");
    }

    public void GoToSettings(){
        StartCoroutine(FadeDuration(mainMenuUI, settingsUI));
    }

    public void GoToCredits(){
        StartCoroutine(FadeDuration(mainMenuUI, creditsUI));
    }

    public void BackFromSettings(){
        StartCoroutine(FadeDuration(settingsUI, mainMenuUI));
        isInFirstScreen = true;
    }

    public void BackFromCredits(){
        StartCoroutine(FadeDuration(creditsUI, mainMenuUI));
        isInFirstScreen = true;
    }

    public void ExitGame(){
        Debug.Log("Has exit");
        Application.Quit();
    }

    IEnumerator FadeDuration(GameObject hideUI, GameObject showUI){
        FadeAnimation();
        yield return new WaitForSeconds(1f);
        hideUI.SetActive(false);
        showUI.SetActive(true);
    }

    // Manage UI animations
    public void JustFadeOutAnimation(){
        fadeAnimator.SetTrigger("FadeIn");
        fadeAnimator.SetBool("willFadeOut", false);
    }
    
    void FadeAnimation(){
        fadeAnimator.SetTrigger("FadeIn");
        fadeAnimator.SetBool("willFadeOut", true);
    }
}