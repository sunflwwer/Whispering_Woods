using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class GameExit : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public Button outButton; // OutButton 연결

    // 버튼 색상 관리
    private TextMeshProUGUI buttonText;
    private Color normalColor = new Color(0f, 0f, 0f); // 검정색
    private Color highlightedColor = new Color(0.3f, 0.3f, 0.3f); // 회색
    private Color pressedColor = new Color(0f, 0f, 0f); // 밝은 회색

    void Start()
    {
        // 버튼 클릭 이벤트 연결
        if (outButton != null)
        {
            outButton.onClick.AddListener(ExitGame);
        }

        // 버튼 텍스트 가져오기
        buttonText = outButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.color = normalColor; // 초기 색상 설정
        }
    }

    public void ExitGame()
    {
        Debug.Log("게임 종료"); // 종료 로그 (에디터에서 확인 가능)

#if UNITY_EDITOR
        // 에디터 모드에서는 실행을 멈추기만 함
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 빌드된 게임에서는 실제로 종료
        Application.Quit();
#endif
    }

    // 버튼 색상 이벤트 처리 (IPointer 인터페이스)
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
