using UnityEngine;
using TMPro; // �ؽ�Ʈ �޽����� ���� �߰�

public class FlowerGravestoneCanvas : MonoBehaviour
{
    private Camera mainCamera;
    private Canvas canvas; // ĵ���� ����
    private TMP_Text activationMessageText; // �޽��� �ؽ�Ʈ ����
    private bool canActivate = false; // Ȱ��ȭ ���� ����

    private void Start()
    {
        // ���� ī�޶� ã��
        mainCamera = Camera.main;

        // ĵ���� ������Ʈ ��������
        canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.enabled = false; // ���� �� ��Ȱ��ȭ
        }
        else
        {
            Debug.LogError("Canvas component not found on the GameObject.");
        }

        // �ؽ�Ʈ ������Ʈ ã��
        activationMessageText = GetComponentInChildren<TMP_Text>();
        if (activationMessageText != null)
        {
            activationMessageText.text = ""; // �ʱ�ȭ �� �ؽ�Ʈ ����
        }

        // Collider ����
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider>();
        }
        collider.isTrigger = true; // Ʈ���� ����
    }

    private void LateUpdate()
    {
        if (mainCamera != null && canvas != null && canvas.enabled)
        {
            // �÷��̾ ���ϵ��� ĵ���� ȸ��
            transform.LookAt(transform.position + mainCamera.transform.forward);
        }

        // Ȱ��ȭ ���� �� FŰ �Է� ����
        if (canActivate && Input.GetKeyDown(KeyCode.F))
        {
            ActivateGravestone();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && canvas != null)
        {
            canvas.enabled = true; // ĵ���� Ȱ��ȭ
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && canvas != null)
        {
            canvas.enabled = false; // ĵ���� ��Ȱ��ȭ
        }
    }

    // ���� ��� ���ŵ� �� �޽��� Ȱ��ȭ
    public void EnableActivationMessage(string message)
    {
        if (activationMessageText != null)
        {
            activationMessageText.text = message; // �ؽ�Ʈ ����
        }
        canActivate = true; // Ȱ��ȭ ���� ���·� ����
    }

    // �� Ȱ��ȭ ����
    private void ActivateGravestone()
    {
        GameObject etcGroup = GameObject.Find("ETC");
        if (etcGroup != null)
        {
            Transform gravestoneGroup = etcGroup.transform.Find("Gravestone group");
            if (gravestoneGroup != null)
            {
                GameObject rock04 = gravestoneGroup.Find("PT_Menhir_Rock_04")?.gameObject;
                GameObject rock044 = gravestoneGroup.Find("PT_Menhir_Rock_044")?.gameObject;

                if (rock04 != null && rock044 != null)
                {
                    rock04.SetActive(true); // �� Ȱ��ȭ
                    rock044.SetActive(false); // ���� �� ��Ȱ��ȭ
                    Debug.Log("���� ���������� Ȱ��ȭ�Ǿ����ϴ�.");
                }
            }
        }

        if (activationMessageText != null)
        {
            activationMessageText.text = ""; // �޽��� ����
        }
        canActivate = false; // �ٽ� ��Ȱ��ȭ ���·� ��ȯ
    }
}
