using UnityEngine;

public class GravestoneCanvas : MonoBehaviour
{
    private Camera mainCamera;
    private Canvas canvas; // 캔버스를 제어하기 위한 참조

    private void Start()
    {
        // 게임에서 활성화된 카메라를 자동으로 찾음
        mainCamera = Camera.main;

        // 캔버스 컴포넌트 참조 가져오기
        canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.enabled = false; // 시작 시 캔버스 비활성화
        }
        else
        {
            Debug.LogError("Canvas component not found on the GameObject.");
        }

        // Collider 확인 및 설정
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider>(); // Collider가 없으면 자동 추가
        }
        collider.isTrigger = true; // 트리거 활성화
    }

    private void LateUpdate()
    {
        if (mainCamera != null && canvas != null && canvas.enabled)
        {
            // 캔버스가 카메라를 향하도록 회전
            transform.LookAt(transform.position + mainCamera.transform.forward);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어 태그가 있는 오브젝트가 트리거에 들어오면 캔버스 활성화
        if (other.CompareTag("Player"))
        {
            if (canvas != null)
            {
                canvas.enabled = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 플레이어가 트리거에서 나가면 캔버스 비활성화
        if (other.CompareTag("Player"))
        {
            if (canvas != null)
            {
                canvas.enabled = false;
            }
        }
    }
}
