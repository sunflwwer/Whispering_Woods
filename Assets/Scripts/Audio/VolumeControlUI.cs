using UnityEngine;
using UnityEngine.UI;

public class VolumeControlUI : MonoBehaviour
{
    public Slider masterSlider;  // 마스터 볼륨 슬라이더
    public Slider bgmSlider;     // BGM 볼륨 슬라이더
    public Slider sfxSlider;     // SFX 볼륨 슬라이더

    private void Start()
    {
        // 슬라이더 값이 변경될 때 해당 볼륨 조절
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        // 슬라이더 초기값 설정 (필요시)
        masterSlider.value = 1f;
        bgmSlider.value = 1f;
        sfxSlider.value = 1f;
    }

    private void SetMasterVolume(float value)
    {
        AudioManager.Instance.SetMasterVolume(value); // 마스터 볼륨 설정
    }

    private void SetBGMVolume(float value)
    {
        AudioManager.Instance.SetBGMVolume(value); // BGM 볼륨 설정
    }

    private void SetSFXVolume(float value)
    {
        AudioManager.Instance.SetSFXVolume(value); // SFX 볼륨 설정
    }
}
