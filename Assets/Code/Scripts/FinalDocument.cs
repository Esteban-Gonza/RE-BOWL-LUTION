using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FinalDocument : MonoBehaviour{

    [SerializeField] TextMeshProUGUI inGameText;

    private bool textOnScreen;

    private void Start(){
        textOnScreen = false;
    }

    private void Update(){

        if (Input.GetKey(KeyCode.E) && textOnScreen == true){

        }
    }

    private void OnTriggerEnter(Collider other){
        inGameText.gameObject.SetActive(true);
        textOnScreen = true;
        inGameText.text = "Press E to get files";
    }

    private void OnTriggerExit(Collider other){
        inGameText.gameObject.SetActive(false);
        textOnScreen = false;
    }
}