using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour{

    public Slider slider;
    public float SliderValue;

    public Toggle toggle;

    private void Start(){
        slider.value = PlayerPrefs.GetFloat("volumeAudio", 1f);
        AudioListener.volume = slider.value;

        if (Screen.fullScreen)
            toggle.isOn = true;
        else
            toggle.isOn = false;
    }

    public void ChangeSlider(float value){
        slider.value = value;
        PlayerPrefs.SetFloat("volumeAudio", SliderValue);
        AudioListener.volume = slider.value;
    }

    public void ActivateFullScreen(bool fullscreen){
        Screen.fullScreen = fullscreen;
    }
}