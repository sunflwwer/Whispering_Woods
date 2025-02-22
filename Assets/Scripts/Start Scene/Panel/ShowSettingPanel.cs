using UnityEngine;
using UnityEngine.UI;

public class ShowSettingPanel : MonoBehaviour
{
    public GameObject settingPanel;  // SettingPanel ����
    public Button settingButton;     // SettingButton ����
    public Button closeButton;       // CloseButton ����

    private bool isPanelActive = false; // �г��� Ȱ��ȭ�Ǿ� �ִ��� ����

    void Start()
    {
        // ���� �� SettingPanel�� CloseButton ��Ȱ��ȭ
        settingPanel.SetActive(false);
        closeButton.gameObject.SetActive(false);

        // ��ư Ŭ�� �̺�Ʈ ����
        if (settingButton != null)
        {
            settingButton.onClick.AddListener(ShowPanel);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HidePanel);
        }
    }

    public void ShowPanel()
    {
        // SettingPanel�� CloseButton Ȱ��ȭ
        settingPanel.SetActive(true);
        closeButton.gameObject.SetActive(true);
        isPanelActive = true;

        // CloseButton �׻� �� ���� ��ġ
        closeButton.transform.SetAsLastSibling();
    }

    public void HidePanel()
    {
        // SettingPanel�� CloseButton ��Ȱ��ȭ
        settingPanel.SetActive(false);
        closeButton.gameObject.SetActive(false);
        isPanelActive = false;
    }
}
