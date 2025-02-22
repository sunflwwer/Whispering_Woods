using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShowRulePanel : MonoBehaviour
{
    public List<GameObject> rulePanels; // RulePanel 1, 2, 3 ���� (����Ʈ�� ����)
    public Button ruleButton;           // Rule ��ư ����
    public Button closeButton;          // �ݱ� ��ư ����
    public Button prevButton;           // ���� ��ư ����
    public Button nextButton;           // ���� ��ư ����

    private int currentPanelIndex = 0;  // ���� Ȱ��ȭ�� �г� �ε���
    private bool isPanelActive = false; // �г��� Ȱ��ȭ�Ǿ� �ִ��� Ȯ��

    void Start()
    {
        // ���� ���� �� ��� �гΰ� ��ư ��Ȱ��ȭ
        foreach (var panel in rulePanels)
        {
            panel.SetActive(false);
        }

        closeButton.gameObject.SetActive(false);
        prevButton.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);

        // ��ư Ŭ�� �̺�Ʈ ����
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
    }

    void Update()
    {
        // �г��� Ȱ��ȭ�� ���¿��� ����Ű �Է� ó��
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
        // RulePanel 1 Ȱ��ȭ �� ��ư Ȱ��ȭ
        currentPanelIndex = 0;
        ActivatePanel(currentPanelIndex);

        closeButton.gameObject.SetActive(true);
        prevButton.gameObject.SetActive(false); // ù ��° �г��̹Ƿ� ���� ��ư ��Ȱ��ȭ
        nextButton.gameObject.SetActive(true);
        isPanelActive = true; // �г� Ȱ��ȭ ���� ǥ��
    }

    public void HideAllPanels()
    {
        // ��� �гΰ� ��ư ��Ȱ��ȭ
        foreach (var panel in rulePanels)
        {
            panel.SetActive(false);
        }

        closeButton.gameObject.SetActive(false);
        prevButton.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        isPanelActive = false; // �г� ��Ȱ��ȭ ���� ǥ��
    }

    public void ShowNextPanel()
    {
        if (currentPanelIndex < rulePanels.Count - 1)
        {
            currentPanelIndex++;
            ActivatePanel(currentPanelIndex);

            prevButton.gameObject.SetActive(true); // ���� ��ư Ȱ��ȭ

            // ������ �гο� ���� �� Next ��ư ��Ȱ��ȭ
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

            nextButton.gameObject.SetActive(true); // ���� ��ư Ȱ��ȭ

            // ù ��° �гη� ���ƿ��� Prev ��ư ��Ȱ��ȭ
            if (currentPanelIndex == 0)
            {
                prevButton.gameObject.SetActive(false);
            }
        }
    }

    private void ActivatePanel(int index)
    {
        // ��� �г� ��Ȱ��ȭ �� ���� �ε��� �гθ� Ȱ��ȭ
        for (int i = 0; i < rulePanels.Count; i++)
        {
            rulePanels[i].SetActive(i == index);
        }

        // CloseButton �׻� ���� ����
        closeButton.transform.SetAsLastSibling();
    }
}
