using UnityEngine;
using UnityEngine.UI;

public class ShowSettingPanel : MonoBehaviour
{
    public GameObject settingPanel;  // SettingPanel 연결
    public Button settingButton;     // SettingButton 연결
    public Button closeButton;       // CloseButton 연결

    private bool isPanelActive = false; // 패널이 활성화되어 있는지 여부

    void Start()
    {
        // 시작 시 SettingPanel과 CloseButton 비활성화
        settingPanel.SetActive(false);
        closeButton.gameObject.SetActive(false);

        // 버튼 클릭 이벤트 연결
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
        // SettingPanel과 CloseButton 활성화
        settingPanel.SetActive(true);
        closeButton.gameObject.SetActive(true);
        isPanelActive = true;

        // CloseButton 항상 맨 위에 배치
        closeButton.transform.SetAsLastSibling();
    }

    public void HidePanel()
    {
        // SettingPanel과 CloseButton 비활성화
        settingPanel.SetActive(false);
        closeButton.gameObject.SetActive(false);
        isPanelActive = false;
    }
}
