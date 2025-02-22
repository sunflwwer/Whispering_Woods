using UnityEngine;
using UnityEngine.UI; // Image�� ����Ϸ��� �ʿ�

[RequireComponent(typeof(Camera))]
public class AspectRatioEnforcer : MonoBehaviour
{
    public Color blackBarColor = Color.black; // �� �� ���� ����

    private void Start()
    {
        EnforceAspectRatio(16, 9); // 16:9 ȭ�� ���� ����
    }

    private void EnforceAspectRatio(int targetWidth, int targetHeight)
    {
        float targetAspect = (float)targetWidth / targetHeight;
        float windowAspect = (float)Screen.width / Screen.height;

        if (Mathf.Approximately(windowAspect, targetAspect)) return; // �̹� 16:9�� �״�� ����

        Camera camera = GetComponent<Camera>();
        Rect rect;

        if (windowAspect > targetAspect) // �¿� �� �� �߰�
        {
            float scaleHeight = targetAspect / windowAspect;
            rect = new Rect(0, (1f - scaleHeight) / 2f, 1f, scaleHeight);
        }
        else // ���� �� �� �߰�
        {
            float scaleWidth = windowAspect / targetAspect;
            rect = new Rect((1f - scaleWidth) / 2f, 0, scaleWidth, 1f);
        }

        camera.rect = rect; // ī�޶��� ȭ�� ���� ����
    }
}
