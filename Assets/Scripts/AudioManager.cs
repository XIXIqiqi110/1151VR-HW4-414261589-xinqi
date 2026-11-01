using UnityEngine;

// 1151VR-HW4: 作業指定三種音效管理器
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("作業指定 3 種音效")]
    public AudioClip engineClip;     // 1. 引擎聲音 (發動、飛行循環、推力調變)
    public AudioClip crashClip;      // 2. 撞擊障礙物聲音 (迫降爆炸)
    public AudioClip passRingClip;   // 3. 順利通過障礙物聲音 (光環穿梭成功)

    [Header("播放器")]
    public AudioSource engineAudioSource;
    public AudioSource sfxAudioSource;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartEngine()
    {
        if (engineAudioSource != null && engineClip != null)
        {
            engineAudioSource.clip = engineClip;
            engineAudioSource.loop = true;
            if (!engineAudioSource.isPlaying) engineAudioSource.Play();
        }
    }

    public void UpdateEnginePitch(float ratio)
    {
        if (engineAudioSource != null)
        {
            engineAudioSource.pitch = Mathf.Clamp(0.85f + (ratio - 1f) * 0.45f, 0.7f, 1.45f);
        }
    }

    public void PlayCrashSound()
    {
        if (sfxAudioSource != null && crashClip != null)
        {
            sfxAudioSource.PlayOneShot(crashClip);
        }
    }

    public void PlayPassRingSound()
    {
        if (sfxAudioSource != null && passRingClip != null)
        {
            sfxAudioSource.PlayOneShot(passRingClip);
        }
    }

    public void StopEngine()
    {
        if (engineAudioSource != null) engineAudioSource.Stop();
    }
}