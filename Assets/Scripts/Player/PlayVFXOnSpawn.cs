using UnityEngine;

public class PlayVFXOnSpawn : MonoBehaviour
{
    void Start()
    {
        ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();

        foreach (ParticleSystem particle in particles)
        {
            particle.Play();
        }
    }
}
