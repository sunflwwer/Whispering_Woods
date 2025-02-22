using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

public class ButtonSceneLoader : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Button button;
    private TextMeshProUGUI buttonText;

    // 색상 정의
    private Color normalColor = new Color(0f, 0f, 0f); // 검정색
    private Color highlightedColor = new Color(0.3f, 0.3f, 0.3f); // 회색
    private Color pressedColor = new Color(0f, 0f, 0f); // 밝은 회색

    // 페이드 효과 관련
    [SerializeField] private Image fadeImage; // 페이드 인/아웃용 이미지 (캔버스 상단에 위치해야 함)
    [SerializeField] private float fadeDuration = 1f; // 페이드 시간

    void Awake()
    {
        button = GetComponent<Button>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();

        if (button != null && buttonText != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
        else
        {
            Debug.LogError("Button 또는 TextMeshProUGUI 컴포넌트를 찾을 수 없습니다.");
        }

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true); // 페이드 이미지 활성화
            Color color = fadeImage.color;
            color.a = 0f; // 시작 시 투명하게 설정
            fadeImage.color = color;
        }
    }


    void OnEnable()
    {
        if (button != null)
        {
            buttonText.color = normalColor;
        }
    }

    // 마우스가 버튼 위로 올라왔을 때
    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonText.color = highlightedColor;
    }

    // 마우스가 버튼에서 빠져나갔을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        buttonText.color = normalColor;
    }

    // 버튼을 눌렀을 때
    public void OnPointerDown(PointerEventData eventData)
    {
        buttonText.color = pressedColor;
    }

    // 버튼을 눌렀다 뗐을 때
    public void OnPointerUp(PointerEventData eventData)
    {
        buttonText.color = highlightedColor;
    }

    // 버튼 클릭 시 실행할 함수
    void OnButtonClick()
    {
        Debug.Log("버튼 클릭 후 페이드 아웃 시작");
        StartCoroutine(FadeOutAndLoadScene());
    }

    // 씬 로드 전 페이드 아웃 후 씬 전환
    IEnumerator FadeOutAndLoadScene()
    {
        yield return StartCoroutine(FadeOut());
        //yield return new WaitForSeconds(2f); // 페이드 아웃 후 1초 대기
        SceneManager.LoadScene("Play Scene");
    }

/*    // 페이드 인 효과 (씬 로드 후)
    IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;
        color.a = 1f; // 완전히 투명하지 않게 시작
        fadeImage.color = color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = 1f - (elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;
    }*/

    // 페이드 아웃 효과 (씬 전환 전)
    IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color color = fadeImage.color;
        color.a = 0f; // 투명한 상태로 시작
        fadeImage.color = color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = elapsedTime / fadeDuration;
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;
    }
}
