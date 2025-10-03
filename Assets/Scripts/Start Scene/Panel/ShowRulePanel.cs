using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using TMPro;

public class ShowRulePanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public List<GameObject> rulePanels; // RulePanel 1, 2, 3 연결 (리스트로 관리)
    public Button ruleButton;           // Rule 버튼 연결
    public Button closeButton;          // 닫기 버튼 연결
    public Button prevButton;           // 이전 버튼 연결
    public Button nextButton;           // 다음 버튼 연결

    private int currentPanelIndex = 0;  // 현재 활성화된 패널 인덱스
    private bool isPanelActive = false; // 패널이 활성화되어 있는지 확인

    // 버튼 색상 관리
    private TextMeshProUGUI buttonText;
    private Color normalColor = new Color(0f, 0f, 0f); // 검정색
    private Color highlightedColor = new Color(0.3f, 0.3f, 0.3f); // 회색
    private Color pressedColor = new Color(0f, 0f, 0f); // 밝은 회색

    private void Start()
    {
        // 게임 시작 시 모든 패널과 버튼 비활성화
        foreach (var panel in rulePanels)
        {
            panel.SetActive(false);
        }

        closeButton.gameObject.SetActive(false);
        prevButton.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);

        // 버튼 클릭 이벤트 연결
        if (ruleButton != null)
        {
            ruleButton.onClick.AddListener(ShowPanel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideAllPanels);
        }

        if (prevButton != null)
        {
            prevButton.onClick.AddListener(ShowPreviousPanel);
        }

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(ShowNextPanel);
        }

        // 버튼 텍스트 가져오기
        buttonText = ruleButton.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.color = normalColor;
        }
    }

    void Update()
    {
        // 패널이 활성화된 상태에서 방향키 입력 처리
        if (isPanelActive)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ShowNextPanel();
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                ShowPreviousPanel();
            }
        }
    }

    public void ShowPanel()
    {
        // RulePanel 1 활성화 및 버튼 활성화
        currentPanelIndex = 0;
        ActivatePanel(currentPanelIndex);

        closeButton.gameObject.SetActive(true);
        prevButton.gameObject.SetActive(false); // 첫 번째 패널이므로 이전 버튼 비활성화
        nextButton.gameObject.SetActive(true);
        isPanelActive = true; // 패널 활성화 상태 표시
    }

    public void HideAllPanels()
    {
        // 모든 패널과 버튼 비활성화
        foreach (var panel in rulePanels)
        {
            panel.SetActive(false);
        }

        closeButton.gameObject.SetActive(false);
        prevButton.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        isPanelActive = false; // 패널 비활성화 상태 표시
    }

    public void ShowNextPanel()
    {
        if (currentPanelIndex < rulePanels.Count - 1)
        {
            currentPanelIndex++;
            ActivatePanel(currentPanelIndex);

            prevButton.gameObject.SetActive(true); // 이전 버튼 활성화

            // 마지막 패널에 도달 시 Next 버튼 비활성화
            if (currentPanelIndex == rulePanels.Count - 1)
            {
                nextButton.gameObject.SetActive(false);
            }
        }
    }

    public void ShowPreviousPanel()
    {
        if (currentPanelIndex > 0)
        {
            currentPanelIndex--;
            ActivatePanel(currentPanelIndex);

            nextButton.gameObject.SetActive(true); // 다음 버튼 활성화

            // 첫 번째 패널로 돌아오면 Prev 버튼 비활성화
            if (currentPanelIndex == 0)
            {
                prevButton.gameObject.SetActive(false);
            }
        }
    }

    private void ActivatePanel(int index)
    {
        // 모든 패널 비활성화 후 현재 인덱스 패널만 활성화
        for (int i = 0; i < rulePanels.Count; i++)
        {
            rulePanels[i].SetActive(i == index);
        }

        // CloseButton 항상 위로 유지
        closeButton.transform.SetAsLastSibling();
    }

    // 색상 변경 이벤트 처리 (IPointer 인터페이스 구현)
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
