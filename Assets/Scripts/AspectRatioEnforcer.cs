using UnityEngine;
using UnityEngine.UI; // Image를 사용하려면 필요

[RequireComponent(typeof(Camera))]
public class AspectRatioEnforcer : MonoBehaviour
{
    public Color blackBarColor = Color.black; // 블랙 바 색상 설정

    private void Start()
    {
        EnforceAspectRatio(16, 9); // 16:9 화면 비율 강제
    }

    private void EnforceAspectRatio(int targetWidth, int targetHeight)
    {
        float targetAspect = (float)targetWidth / targetHeight;
        float windowAspect = (float)Screen.width / Screen.height;

        if (Mathf.Approximately(windowAspect, targetAspect)) return; // 이미 16:9면 그대로 유지

        Camera camera = GetComponent<Camera>();
        Rect rect;

        if (windowAspect > targetAspect) // 좌우 블랙 바 추가
        {
            float scaleHeight = targetAspect / windowAspect;
            rect = new Rect(0, (1f - scaleHeight) / 2f, 1f, scaleHeight);
        }
        else // 상하 블랙 바 추가
        {
            float scaleWidth = windowAspect / targetAspect;
            rect = new Rect((1f - scaleWidth) / 2f, 0, scaleWidth, 1f);
        }

        camera.rect = rect; // 카메라의 화면 영역 설정
    }
}
