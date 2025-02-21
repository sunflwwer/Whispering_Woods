using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonSceneLoader : MonoBehaviour
{
    private Button button;

    void Awake()
    {
        // 버튼 컴포넌트 가져오기
        button = GetComponent<Button>();

        // 버튼 클릭 이벤트 리스너 추가
        if (button != null)
        {
            button.onClick.AddListener(LoadGameScene);
        }
        else
        {
            Debug.LogError("Button 컴포넌트를 찾을 수 없습니다.");
        }
    }

    // 씬 로드 함수
    void LoadGameScene()
    {
        Debug.Log("Game Scene으로 이동합니다.");
        SceneManager.LoadScene("Game Scene");
    }
}
