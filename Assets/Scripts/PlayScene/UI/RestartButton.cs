using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 씬 전환을 위한 네임스페이스 추가
using TMPro; // 텍스트 색상 변경을 위해 필요
using UnityEngine.EventSystems; // 버튼 색상 인터랙션을 위한 네임스페이스 추가

public class RestartButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Image fadeImage; // 페이드 인/아웃용 이미지
    [SerializeField] private float fadeDuration = 1f; // 페이드 시간

    // 버튼 색상 관리
    private Button button;
    private TextMeshProUGUI buttonText;
    private Color normalColor = new Color(0f, 0f, 0f); // 기본 검정색
    private Color highlightedColor = new Color(0.3f, 0.3f, 0.3f); // 마우스 오버 시 회색
    private Color pressedColor = new Color(0f, 0f, 0f); // 클릭 시 검정색

    void Start()
    {
        // 페이드 인 설정
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true); // 페이드 이미지 활성화
            StartCoroutine(FadeIn()); // 씬 시작 시 페이드 인 실행
        }

        // 버튼 및 텍스트 설정
        button = GetComponent<Button>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();

        if (button != null && buttonText != null)
        {
            button.onClick.AddListener(OnButtonClicked);
            buttonText.color = normalColor; // 초기 색상 설정
        }
        else
        {
            Debug.LogError("Button 또는 TextMeshProUGUI 컴포넌트를 찾을 수 없습니다.");
        }
    }

    // 씬 시작 시 페이드 인 효과
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

    // 버튼 클릭 시 호출될 메서드 (씬 전환 전에 페이드 아웃)
    public void OnButtonClicked()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true); // 페이드 이미지 활성화
            StartCoroutine(FadeOutAndReloadScene()); // 페이드 아웃 후 현재 씬 재로드
        }
    }

    // 버튼 클릭 시 실행될 페이드 아웃 및 현재 씬 재로드 코루틴
    IEnumerator FadeOutAndReloadScene()
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;
        color.a = 0f; // 시작 시 완전 투명

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = elapsedTime / fadeDuration; // 점차 불투명해짐
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color; // 페이드 완료 후 완전 불투명

        // 현재 씬을 다시 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 버튼 색상 관련 인터페이스 메서드 구현
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonText != null)
        {
            buttonText.color = highlightedColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonText != null)
        {
            buttonText.color = normalColor;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (buttonText != null)
        {
            buttonText.color = pressedColor;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (buttonText != null)
        {
            buttonText.color = highlightedColor;
        }
    }
}
