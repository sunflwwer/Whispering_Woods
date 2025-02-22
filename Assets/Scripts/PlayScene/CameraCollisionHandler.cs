using UnityEngine;

public class CameraCollisionHandler : MonoBehaviour
{
    [SerializeField] private LayerMask collisionLayers; // 충돌 감지 레이어 (Terrain 등)
    [SerializeField] private float collisionBuffer = 0.2f; // 충돌 시 여유 거리

    private Vector3 lastValidPosition;

    void Start()
    {
        lastValidPosition = transform.position; // 시작 시 카메라 위치 저장
    }

    void LateUpdate()
    {
        HandleCameraCollision();
    }

    private void HandleCameraCollision()
    {
        // 이전 위치에서 현재 위치까지의 방향 계산
        Vector3 movementDirection = (transform.position - lastValidPosition).normalized;
        float movementDistance = Vector3.Distance(transform.position, lastValidPosition);

        // Ray를 쏘아서 충돌 여부 확인
        if (Physics.Raycast(lastValidPosition, movementDirection, out RaycastHit hit, movementDistance, collisionLayers))
        {
            // 충돌 시 충돌 직전 위치로 카메라 이동
            transform.position = hit.point - movementDirection * collisionBuffer;
            Debug.Log("Collision detected! Camera stopped at: " + transform.position);
        }
        else
        {
            // 충돌이 없으면 이동
            lastValidPosition = transform.position;
        }
    }
}
