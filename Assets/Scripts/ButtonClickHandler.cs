using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonSceneLoader : MonoBehaviour
{
    private Button button;

    void Awake()
    {
        // ��ư ������Ʈ ��������
        button = GetComponent<Button>();

        // ��ư Ŭ�� �̺�Ʈ ������ �߰�
        if (button != null)
        {
            button.onClick.AddListener(LoadGameScene);
        }
        else
        {
            Debug.LogError("Button ������Ʈ�� ã�� �� �����ϴ�.");
        }
    }

    // �� �ε� �Լ�
    void LoadGameScene()
    {
        Debug.Log("Game Scene���� �̵��մϴ�.");
        SceneManager.LoadScene("Game Scene");
    }
}
