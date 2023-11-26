using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class SoundManager : MonoBehaviour{
    
    public static SoundManager instance;

    public AudioClip[] hits;
    public AudioSource source;

    private void Awake(){
        if(instance == null){
            instance = this;
        }else{
            Destroy(gameObject);
        }
    }

    private void Start(){
        source = GetComponent<AudioSource>();
    }

    public void PlayHit(){
        int randomHit = Random.Range(0, hits.Length);
        source.PlayOneShot(hits[randomHit]);
    }
}