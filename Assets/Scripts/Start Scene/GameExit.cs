using UnityEngine;
using UnityEngine.UI;

public class GameExit : MonoBehaviour
{
    public Button outButton; // OutButton ����

    void Start()
    {
        // ��ư Ŭ�� �̺�Ʈ ����
        if (outButton != null)
        {
            outButton.onClick.AddListener(ExitGame);
        }
    }

    public void ExitGame()
    {
        Debug.Log("���� ����"); // ���� �α� (�����Ϳ��� Ȯ�� ����)

#if UNITY_EDITOR
        // ������ ��忡���� ������ ���߱⸸ ��
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ����� ���ӿ����� ������ ����
        Application.Quit();
#endif
    }
}
