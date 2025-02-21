using UnityEngine;

public class GravestoneCanvas : MonoBehaviour
{
    private Camera mainCamera;
    private Canvas canvas; // ĵ������ �����ϱ� ���� ����

    private void Start()
    {
        // ���ӿ��� Ȱ��ȭ�� ī�޶� �ڵ����� ã��
        mainCamera = Camera.main;

        // ĵ���� ������Ʈ ���� ��������
        canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.enabled = false; // ���� �� ĵ���� ��Ȱ��ȭ
        }
        else
        {
            Debug.LogError("Canvas component not found on the GameObject.");
        }

        // Collider Ȯ�� �� ����
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider>(); // Collider�� ������ �ڵ� �߰�
        }
        collider.isTrigger = true; // Ʈ���� Ȱ��ȭ
    }

    private void LateUpdate()
    {
        if (mainCamera != null && canvas != null && canvas.enabled)
        {
            // ĵ������ ī�޶� ���ϵ��� ȸ��
            transform.LookAt(transform.position + mainCamera.transform.forward);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // �÷��̾� �±װ� �ִ� ������Ʈ�� Ʈ���ſ� ������ ĵ���� Ȱ��ȭ
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
        // �÷��̾ Ʈ���ſ��� ������ ĵ���� ��Ȱ��ȭ
        if (other.CompareTag("Player"))
        {
            if (canvas != null)
            {
                canvas.enabled = false;
            }
        }
    }
}
