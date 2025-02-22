using UnityEngine;

public class CameraCollisionHandler : MonoBehaviour
{
    [SerializeField] private LayerMask collisionLayers; // �浹 ���� ���̾� (Terrain ��)
    [SerializeField] private float collisionBuffer = 0.2f; // �浹 �� ���� �Ÿ�

    private Vector3 lastValidPosition;

    void Start()
    {
        lastValidPosition = transform.position; // ���� �� ī�޶� ��ġ ����
    }

    void LateUpdate()
    {
        HandleCameraCollision();
    }

    private void HandleCameraCollision()
    {
        // ���� ��ġ���� ���� ��ġ������ ���� ���
        Vector3 movementDirection = (transform.position - lastValidPosition).normalized;
        float movementDistance = Vector3.Distance(transform.position, lastValidPosition);

        // Ray�� ��Ƽ� �浹 ���� Ȯ��
        if (Physics.Raycast(lastValidPosition, movementDirection, out RaycastHit hit, movementDistance, collisionLayers))
        {
            // �浹 �� �浹 ���� ��ġ�� ī�޶� �̵�
            transform.position = hit.point - movementDirection * collisionBuffer;
            Debug.Log("Collision detected! Camera stopped at: " + transform.position);
        }
        else
        {
            // �浹�� ������ �̵�
            lastValidPosition = transform.position;
        }
    }
}
