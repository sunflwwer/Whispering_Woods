using UnityEngine;

public class BillboardCanvas : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        // 게임에서 활성화된 카메라를 자동으로 찾음
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera != null)
        {
            // 캔버스가 카메라를 향하도록 회전
            transform.LookAt(transform.position + mainCamera.transform.forward);
        }
    }
}
