using UnityEngine;
using UnityEngine.UI;

public class GameExit : MonoBehaviour
{
    public Button outButton; // OutButton 연결

    void Start()
    {
        // 버튼 클릭 이벤트 연결
        if (outButton != null)
        {
            outButton.onClick.AddListener(ExitGame);
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
}
