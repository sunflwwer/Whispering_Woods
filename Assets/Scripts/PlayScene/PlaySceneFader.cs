using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlaySceneFader : MonoBehaviour
{
    [SerializeField] private Image fadeImage; // 페이드 인/아웃용 이미지
    [SerializeField] private float fadeDuration = 1f; // 페이드 시간

    void Start()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true); // 페이드 이미지 활성화
            StartCoroutine(FadeIn()); // 씬 시작 시 페이드 인 실행
        }
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
        fadeImage.gameObject.SetActive(false); // 완료 후 비활성화
    }
}
