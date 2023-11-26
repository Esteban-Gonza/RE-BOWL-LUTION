using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(ParticleSystem))]
public class Particle_System : MonoBehaviour{

    private ParticleSystem particle;
    private ParticleSystemRenderer particleRender;
    private Material particleMaterial;

    private void Start(){
        particle = GetComponent<ParticleSystem>();
        particleRender = GetComponent<ParticleSystemRenderer>();
        particleMaterial = particleRender.material;
    }

    public void PlayParticleAtPosition(Vector3 position){

        var pmain = particle.main;

        transform.position = position;
        particle.Play();
        particleMaterial.DOFade(1, 0);
        particleMaterial.DOFade(0, pmain.startLifetime.constant).SetEase(Ease.InExpo);
    }
}