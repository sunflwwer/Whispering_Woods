using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Flower : MonoBehaviour
{
    [SerializeField] private GameObject interactionUI; // ��ȣ�ۿ� UI
    [SerializeField] private string interactKey = "v"; // ��ȣ�ۿ� Ű (�⺻��: V)
    private bool isPlayerNearby = false; // �÷��̾ ������ �ִ��� ����
    private static int flowerCount = 4; // �ʱ� Flower ����
    private TMP_Text flowerText; // Canvas�� Flower �ؽ�Ʈ

    private List<GameObject> lightFlowers = new List<GameObject>(); // LightFlower ������Ʈ ����Ʈ
    private int nextLightFlowerIndex = 0; // ���� Ȱ��ȭ�� ���� �ε���

    private void Start()
    {
        // Flower �ؽ�Ʈ ã��
        GameObject canvasObject = GameObject.Find("Canvas");
        if (canvasObject != null)
        {
            GameObject flowerTextObject = canvasObject.transform.Find("Flower")?.gameObject;
            if (flowerTextObject != null)
            {
                flowerText = flowerTextObject.GetComponent<TMP_Text>();
                flowerText.text = $"Flower = {flowerCount}";
            }
            else
            {
                Debug.LogError("Canvas �ȿ� 'Flower' �ؽ�Ʈ ������Ʈ�� ã�� �� �����ϴ�.");
            }
        }

        // ETC �׷� ������ Flower group ã��
        GameObject etcGroup = GameObject.Find("ETC");
        if (etcGroup != null)
        {
            Transform flowerGroup = etcGroup.transform.Find("Flower group");
            if (flowerGroup != null)
            {
                foreach (Transform child in flowerGroup)
                {
                    if (child.gameObject.name.StartsWith("LightFlower"))
                    {
                        child.gameObject.SetActive(false); // �ʱ⿡�� ��Ȱ��ȭ
                        lightFlowers.Add(child.gameObject);
                    }
                }
            }
            else
            {
                Debug.LogError("Flower group ������Ʈ�� ETC �׷쿡�� ã�� �� �����ϴ�.");
            }
        }
        else
        {
            Debug.LogError("ETC �׷� ������Ʈ�� ã�� �� �����ϴ�.");
        }

        // ��ȣ�ۿ� UI ��Ȱ��ȭ
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }

    private void Update()
    {
        // �÷��̾ ������ ���� �� ��ȣ�ۿ� Ű �Է� ����
        if (isPlayerNearby && Input.GetKeyDown(interactKey))
        {
            ConsumeFlower();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            interactionUI?.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            interactionUI?.SetActive(false);
        }
    }

    private void ConsumeFlower()
    {
        flowerCount = Mathf.Max(0, flowerCount - 1); // ������ 0 ���Ϸ� �������� �ʰ�
        UpdateFlowerText();
        ActivateNextLightFlower(); // ���������� Ȱ��ȭ

        interactionUI?.SetActive(false); // ��ȣ�ۿ� UI ��Ȱ��ȭ
        Destroy(gameObject); // �� ������Ʈ ����
    }

    private void UpdateFlowerText()
    {
        if (flowerText != null)
        {
            flowerText.text = $"Flower = {flowerCount}";
        }
    }

    // �������� LightFlower Ȱ��ȭ
    private void ActivateNextLightFlower()
    {
        List<GameObject> inactiveFlowers = lightFlowers.FindAll(flower => !flower.activeSelf); // ��Ȱ��ȭ�� �ɸ� ã��

        if (inactiveFlowers.Count > 0)
        {
            int randomIndex = Random.Range(0, inactiveFlowers.Count); // ��Ȱ��ȭ�� �� �� ���� ����
            GameObject lightFlower = inactiveFlowers[randomIndex];

            if (lightFlower != null)
            {
                lightFlower.SetActive(true); // �������� Ȱ��ȭ
                Debug.Log($"LightFlower ���� Ȱ��ȭ��: {lightFlower.name}"); // Ȱ��ȭ�� ������Ʈ �̸� ���
            }
            else
            {
                Debug.LogError("���õ� LightFlower ������Ʈ�� null�Դϴ�.");
            }
        }
        else
        {
            Debug.Log("��� LightFlower ������Ʈ�� �̹� Ȱ��ȭ�Ǿ����ϴ�.");
        }
    }

}
