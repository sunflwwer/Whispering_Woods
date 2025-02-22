using UnityEngine;

public class BillboardCanvas : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        // ���ӿ��� Ȱ��ȭ�� ī�޶� �ڵ����� ã��
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera != null)
        {
            // ĵ������ ī�޶� ���ϵ��� ȸ��
            transform.LookAt(transform.position + mainCamera.transform.forward);
        }
    }
}
