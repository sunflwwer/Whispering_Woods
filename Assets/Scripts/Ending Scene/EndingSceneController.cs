using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // 씬 전환을 위해 추가

public class EndingSceneController : MonoBehaviour
{
    [SerializeField] private Image fadeImage; // 페이드 인/아웃용 이미지
    [SerializeField] private float fadeDuration = 1f; // 페이드 시간
    [SerializeField] private TMP_Text[] texts; // 텍스트 배열 (Text 1 ~ Text 4)

    [SerializeField] private AudioClip backgroundMusic; // 배경음악 클립
    [SerializeField][Range(0f, 1f)] private float bgmVolume = 1.0f; // 배경음악 볼륨
    private AudioSource audioSource; // 오디오 소스 컴포넌트

    void Start()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true); // 페이드 이미지 활성화
            StartCoroutine(FadeIn()); // 씬 시작 시 페이드 인 실행
        }

        // BGM 설정
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = backgroundMusic;
        audioSource.volume = 0f; // 시작 시 볼륨 0
        audioSource.loop = true; // 루프 재생 설정
        audioSource.playOnAwake = false;

        // 2초 후 BGM 재생 및 페이드 인 시작
        StartCoroutine(DelayedPlayBGM(1f)); // 2초 지연
    }

    // 2초 대기 후 배경음악 재생 및 페이드 인
    IEnumerator DelayedPlayBGM(float delay)
    {
        yield return new WaitForSeconds(delay); // 설정된 시간 동안 대기
        audioSource.Play(); // 음악 재생
        StartCoroutine(FadeInBGM(fadeDuration)); // 페이드 인 효과 적용
    }


    // 페이드 인 효과
    IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;
        color.a = 1f; // 시작 시 불투명 (검은 화면)

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = 1f - (elapsedTime / fadeDuration); // 점차 투명해짐
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color; // 페이드 인 완료 후 완전 투명

        // 텍스트 페이드인/아웃 시작
        if (texts.Length > 0)
        {
            StartCoroutine(FadeTextsSequentially());
        }
    }

    // 텍스트 순차적으로 페이드 인/유지/아웃
    IEnumerator FadeTextsSequentially()
    {
        yield return new WaitForSeconds(1f); // 첫 텍스트 등장 전에 짧은 대기 추가

        foreach (TMP_Text text in texts)
        {
            yield return StartCoroutine(FadeTextIn(text, 1f)); // 텍스트 페이드 인 (1초)
            yield return new WaitForSeconds(5f); // 유지 (3초)
            yield return StartCoroutine(FadeTextOut(text, 1f)); // 텍스트 페이드 아웃 (1초)
        }

        // 모든 텍스트가 끝난 후 1초 대기
        yield return new WaitForSeconds(1f);

        // 화면 전체 페이드 아웃
        yield return StartCoroutine(FadeOutScene());

        // Start Scene으로 씬 전환
        SceneManager.LoadScene("Start Scene");
    }

    // 텍스트 페이드 인
    IEnumerator FadeTextIn(TMP_Text text, float duration)
    {
        float elapsedTime = 0f;
        Color color = text.color;
        color.a = 0f; // 시작 시 완전 투명
        text.color = color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsedTime / duration); // 서서히 나타남
            text.color = color;
            yield return null;
        }

        color.a = 1f; // 완전 불투명
        text.color = color;
    }

    // 텍스트 페이드 아웃
    IEnumerator FadeTextOut(TMP_Text text, float duration)
    {
        float elapsedTime = 0f;
        Color color = text.color;
        color.a = 1f; // 시작 시 완전 불투명
        text.color = color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsedTime / duration); // 서서히 사라짐
            text.color = color;
            yield return null;
        }

        color.a = 0f; // 완전 투명
        text.color = color;
    }

    // 전체 씬 페이드 아웃
    IEnumerator FadeOutScene()
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;
        color.a = 0f; // 시작 시 투명

        StartCoroutine(FadeOutBGM(fadeDuration)); // BGM 페이드 아웃 시작

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration); // 점차 불투명해짐
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f; // 완전 불투명
        fadeImage.color = color;
    }


    // BGM 페이드 인
    IEnumerator FadeInBGM(float duration)
    {
        float elapsedTime = 0f;
        audioSource.volume = 0f; // 시작 시 볼륨 0

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, bgmVolume, elapsedTime / duration); // 볼륨 증가
            yield return null;
        }

        audioSource.volume = bgmVolume; // 최대 볼륨 유지
    }

    // BGM 페이드 아웃
    IEnumerator FadeOutBGM(float duration)
    {
        float elapsedTime = 0f;
        float startVolume = audioSource.volume;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / duration); // 볼륨 감소
            yield return null;
        }

        audioSource.volume = 0f; // 볼륨 0으로 설정
    }

}
