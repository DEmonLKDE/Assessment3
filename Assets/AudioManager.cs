using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Clips")]
    public AudioClip countdownBGM;  
    public AudioClip normalBGM;
    public AudioClip scaredBGM;
    public AudioClip deadGhostBGM;

    void Start()
    {
        // 保留空闲状态，等待 GameManager 控制
    }

    // --- 新增 ---
    public void PlayCountdownBGM(bool loop = false)
    {
        if (bgmSource == null || countdownBGM == null) return;
        bgmSource.clip = countdownBGM;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void PlayNormalBGM(bool loop = true)
    {
        if (bgmSource == null || normalBGM == null) return;
        bgmSource.clip = normalBGM;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void PlayScaredBGM(bool loop = true)
    {
        if (bgmSource == null || scaredBGM == null) return;
        bgmSource.clip = scaredBGM;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void PlayDeadGhostBGM()
    {
        if (bgmSource == null || deadGhostBGM == null) return;
        bgmSource.clip = deadGhostBGM;
        bgmSource.loop = false;
        bgmSource.Play();
    }

    public void StopAllBGM()
    {
        if (bgmSource != null) bgmSource.Stop();
    }

    public void StopAllSFX()
    {
        if (sfxSource != null) sfxSource.Stop();
    }
}

