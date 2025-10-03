using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance; // 싱글톤 인스턴스

    [Header("Audio Mixer")]
    public AudioMixer audioMixer; // Unity AudioMixer 연결

    private void Awake()
    {
        // 싱글톤 패턴 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 이동 시 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 볼륨 설정 (로그 단위로 변환)
    public void SetMasterVolume(float volume)
    {
        volume = Mathf.Clamp(volume, 0.0001f, 1f); // 로그 에러 방지
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
    }

    public void SetBGMVolume(float volume)
    {
        volume = Mathf.Clamp(volume, 0.0001f, 1f); // 로그 에러 방지
        audioMixer.SetFloat("BGMVolume", Mathf.Log10(volume) * 20);
    }

    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp(volume, 0.0001f, 1f); // 로그 에러 방지
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
    }

    // 페이드 인/아웃 기능 추가
    public void FadeBGM(AudioSource source, AudioClip newClip, float duration)
    {
        StartCoroutine(FadeBGMCoroutine(source, newClip, duration));
    }

    private IEnumerator FadeBGMCoroutine(AudioSource source, AudioClip newClip, float duration)
    {
        // 현재 볼륨 저장
        float currentTime = 0;
        float startVolume = source.volume;

        // 페이드 아웃
        while (currentTime < duration / 2)
        {
            currentTime += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0, currentTime / (duration / 2));
            yield return null;
        }

        source.clip = newClip;
        source.Play();

        // 페이드 인
        currentTime = 0;
        while (currentTime < duration / 2)
        {
            currentTime += Time.deltaTime;
            source.volume = Mathf.Lerp(0, startVolume, currentTime / (duration / 2));
            yield return null;
        }
    }
}
