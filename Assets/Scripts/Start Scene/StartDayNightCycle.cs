using UnityEngine;

public class StartDayNightCycle : MonoBehaviour
{
    public Light directionalLight; // 태양 라이트 (Directional Light)
    public Light moonLight; // 달 라이트 (MoonLight)
    public Material skyboxMaterial; // Skybox Material (셰이더 적용된 머티리얼)
    public float dayDuration = 20f; // 하루 시간 (초 단위)
    public Color dayFogColor = Color.cyan; // 낮의 Fog 색상
    public Color nightFogColor = new Color(0.05f, 0.05f, 0.2f); // 밤의 Fog 색상 (짙은 남색)

    private float rotationSpeed;
    private float fogLerpValue = 0f; // Fog 전환 비율
    private int currentDay = 1; // 현재 Day (Day 1부터 시작)
    private float elapsedTime = 0f; // 하루 경과 시간 추적
    private bool isEvening = false; // 저녁 상태

    public bool IsEvening => isEvening; // 외부에서 저녁 상태를 확인하는 프로퍼티

    [Header("BGM Clips")]
    public AudioClip morningBGM; // 아침 배경음
    public AudioClip eveningBGM; // 저녁 배경음
    private AudioSource bgmSource; // 배경음 재생용 AudioSource

    [Header("Audio Volume Settings")]
    [Range(0f, 1f)] public float morningVolume = 1f; // 아침 BGM 볼륨
    [Range(0f, 1f)] public float eveningVolume = 1f; // 저녁 BGM 볼륨


    void Start()
    {
        if (directionalLight == null || moonLight == null)
        {
            Debug.LogError("Directional Light or MoonLight is not assigned!");
            return;
        }

        if (skyboxMaterial == null)
        {
            Debug.LogError("Skybox Material is not assigned!");
            return;
        }

        rotationSpeed = 360f / dayDuration;

        directionalLight.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        moonLight.transform.rotation = Quaternion.Euler(-180f, 0f, 0f);

        RenderSettings.fogColor = dayFogColor;

        // 배경음 설정
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        PlayMorningBGM(); // 시작 시 아침 BGM 재생
    }

    void Update()
    {
        float deltaRotation = rotationSpeed * Time.deltaTime;

        // 태양과 달 회전
        directionalLight.transform.Rotate(Vector3.right, deltaRotation);
        moonLight.transform.rotation = Quaternion.Euler(
            directionalLight.transform.eulerAngles.x - 180f,
            directionalLight.transform.eulerAngles.y,
            directionalLight.transform.eulerAngles.z
        );

        UpdateLightIntensities();
        skyboxMaterial.SetVector("_LightDirection", -directionalLight.transform.forward);
        UpdateFogColor();
        UpdateEveningStatus();

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= dayDuration)
        {
            elapsedTime = 0f;
            currentDay++;
        }
    }

    private void UpdateLightIntensities()
    {
        float sunHeight = Vector3.Dot(directionalLight.transform.forward, Vector3.down);
        float moonHeight = Vector3.Dot(moonLight.transform.forward, Vector3.down);

        directionalLight.intensity = Mathf.Clamp01(sunHeight);
        moonLight.intensity = Mathf.Clamp01(moonHeight * 0.3f);
        fogLerpValue = Mathf.Clamp01(sunHeight);
    }

    private void UpdateFogColor()
    {
        RenderSettings.fogColor = Color.Lerp(nightFogColor, dayFogColor, fogLerpValue);
    }

    private void UpdateEveningStatus()
    {
        float sunRotationX = directionalLight.transform.eulerAngles.x;

        bool wasEvening = isEvening;
        isEvening = sunRotationX >= 180f && sunRotationX < 360f;

        // 저녁 상태가 바뀔 때만 BGM 전환
        if (isEvening && !wasEvening)
        {
            PlayEveningBGM();
        }
        else if (!isEvening && wasEvening)
        {
            PlayMorningBGM();
        }
    }


    private void PlayMorningBGM()
    {
        if (bgmSource.clip != morningBGM && AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeBGM(bgmSource, morningBGM, 3f); // 3초 페이드
            bgmSource.volume = morningVolume; // 인스펙터에서 설정한 아침 볼륨 적용
        }
    }

    private void PlayEveningBGM()
    {
        if (bgmSource.clip != eveningBGM && AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeBGM(bgmSource, eveningBGM, 3f); // 3초 페이드
            bgmSource.volume = eveningVolume; // 인스펙터에서 설정한 저녁 볼륨 적용
        }
    }



}
