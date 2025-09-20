using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    public AudioClip introClip;
    public AudioClip ghostsNormalClip;

    public AudioClip moveClip;
    public AudioClip pelletClip;
    public AudioClip collisionClip;
    public AudioClip deathClip;

    void Start()
    {
        bgmSource.clip = introClip;
        bgmSource.loop = false;
        bgmSource.Play();

        Invoke("PlayNormalBGM", 3f);
    }

    void PlayNormalBGM()
    {
        bgmSource.clip = ghostsNormalClip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlayMoveSFX()
    {
        sfxSource.PlayOneShot(moveClip);
    }

    public void PlayPelletSFX()
    {
        sfxSource.PlayOneShot(pelletClip);
    }

    public void PlayCollisionSFX()
    {
        sfxSource.PlayOneShot(collisionClip);
    }

    public void PlayDeathSFX()
    {
        sfxSource.PlayOneShot(deathClip);
    }
}
