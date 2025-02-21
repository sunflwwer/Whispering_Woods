using UnityEngine;
using TMPro;


public class SpecialTreeInteraction : MonoBehaviour
{
    public GameObject[] treePrefabs; // ��ü�� ������ �迭
    public GameObject finalTreePrefab; // Ư���� ���� ������ (�ϳ��� ���� ����)
    public float detectionRange = 5f;
    public float detectionAngle = 30f;
    private int currentStage = -1;

    private GameObject interactionUI;
    private GameObject lightObject;
    private bool isPlayerNearby = false;
    private GameObject player;

    private static int globalTreeCounter = 0; // ��� ������ �����ϴ� �۷ι� ī����
    private TextMeshProUGUI specialTreeCounter; // SpecialTree �ؽ�Ʈ ��ü


    private void Start()
    {
        interactionUI = transform.Find("Canvas")?.gameObject;
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }

        lightObject = transform.Find("Light")?.gameObject;

        GameObject counterTextObject = GameObject.Find("Canvas/SpecialTree");
        if (counterTextObject != null)
        {
            specialTreeCounter = counterTextObject.GetComponent<TextMeshProUGUI>();
            if (specialTreeCounter != null)
            {
                specialTreeCounter.text = globalTreeCounter.ToString(); // �ʱ� �� ����
            }
        }
        else
        {
            Debug.LogError("SpecialTree �ؽ�Ʈ ������Ʈ�� ã�� �� �����ϴ�.");
        }

        // �ʱ� Gravestone ���� ����
        GameObject etcGroup = GameObject.Find("ETC");
        if (etcGroup != null)
        {
            Transform gravestoneGroup = etcGroup.transform.Find("Gravestone group");
            if (gravestoneGroup != null)
            {
                GameObject rock01 = gravestoneGroup.Find("PT_Menhir_Rock_01")?.gameObject;
                GameObject rock011 = gravestoneGroup.Find("PT_Menhir_Rock_011")?.gameObject;

                if (rock01 != null && rock011 != null)
                {
                    rock01.SetActive(true);  // ó������ Ȱ��ȭ
                    rock011.SetActive(false); // ó������ ��Ȱ��ȭ
                }
            }
        }


    }

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.V))
        {
            TransformTree();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            player = other.gameObject;
            interactionUI?.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            player = null;
            interactionUI?.SetActive(false);
        }
    }

    private void TransformTree()
    {
        if (currentStage >= treePrefabs.Length - 1)
        {
            Debug.Log("���� ���������� ��ȯ�˴ϴ�.");
            ReplaceWithFinalTree(); // ������ �ƴ϶� ������ �ϳ��� ���� ���������� ��ȯ
            return;
        }

        currentStage++;
        ReplaceTree(treePrefabs[currentStage]);
    }

    private void ReplaceTree(GameObject newTreePrefab)
    {
        GameObject newTree = Instantiate(newTreePrefab, transform.position, transform.rotation);

        SpecialTreeInteraction script = newTree.AddComponent<SpecialTreeInteraction>();
        script.treePrefabs = treePrefabs;
        script.finalTreePrefab = finalTreePrefab; // ���� ������ ����
        script.detectionRange = detectionRange;
        script.detectionAngle = detectionAngle;
        script.currentStage = currentStage;

        if (interactionUI != null)
        {
            interactionUI.transform.SetParent(newTree.transform);
            interactionUI.transform.localPosition = Vector3.zero;
        }

        if (lightObject != null)
        {
            lightObject.transform.SetParent(newTree.transform);
            lightObject.transform.localPosition = Vector3.zero;
        }

        Destroy(gameObject);
    }

    private void ReplaceWithFinalTree()
    {
        if (finalTreePrefab == null)
        {
            Debug.LogError("���� �������� �������� �ʾҽ��ϴ�!");
            return;
        }

        // ���� ������ ����
        GameObject newTree = Instantiate(finalTreePrefab, transform.position, transform.rotation);

        // �̸� ��� (������ �̸� Ȯ��)
        Debug.Log($"���� �������� �����Ǿ����ϴ�: {finalTreePrefab.name}");

        // ETC �׷� ������ FinalTree group ã��
        GameObject etcGroup = GameObject.Find("ETC");
        if (etcGroup != null)
        {
            Transform finalTreeGroup = etcGroup.transform.Find("FinalTree group");
            if (finalTreeGroup != null)
            {

                foreach (Transform child in finalTreeGroup)
                {
                    if (child.gameObject.name == finalTreePrefab.name)
                    {
                        child.gameObject.SetActive(true); // �ش� ������Ʈ Ȱ��ȭ
                        Debug.Log($"{child.gameObject.name} ������Ʈ�� Ȱ��ȭ�Ǿ����ϴ�.");

                        globalTreeCounter++; // ���� ī���� ����
                        if (specialTreeCounter != null)
                        {
                            specialTreeCounter.text = globalTreeCounter.ToString();
                        }

                        // ī���Ͱ� 3�� �Ǿ��� �� Gravestone ������Ʈ ����
                        if (globalTreeCounter == 3)
                        {
                            ActivateGravestoneObject();
                        }

                    }
                }
            }
            else
            {
                Debug.LogError("FinalTree group ������Ʈ�� ETC �׷쿡�� ã�� �� �����ϴ�.");
            }
        }
        else
        {
            Debug.LogError("ETC �׷� ������Ʈ�� ã�� �� �����ϴ�.");
        }

        // �Һ� ����
        if (lightObject != null)
        {
            lightObject.transform.SetParent(newTree.transform);
            lightObject.transform.localPosition = Vector3.zero;
        }

        // ���ͷ��� UI ����
        if (interactionUI != null)
        {
            Destroy(interactionUI);
        }

        Destroy(gameObject);
    }

    // Gravestone �׷� �� ������Ʈ ���� ���� �޼���
    private void ActivateGravestoneObject()
    {
        GameObject etcGroup = GameObject.Find("ETC");
        if (etcGroup != null)
        {
            Transform gravestoneGroup = etcGroup.transform.Find("Gravestone group");
            if (gravestoneGroup != null)
            {
                GameObject rock01 = gravestoneGroup.Find("PT_Menhir_Rock_01")?.gameObject;
                GameObject rock011 = gravestoneGroup.Find("PT_Menhir_Rock_011")?.gameObject;

                if (rock01 != null && rock011 != null)
                {
                    rock01.SetActive(false);  // ��Ȱ��ȭ
                    rock011.SetActive(true);  // Ȱ��ȭ

                    Debug.Log("Gravestone ������Ʈ ���� ���� �Ϸ�: PT_Menhir_Rock_01 ��Ȱ��ȭ, PT_Menhir_Rock_011 Ȱ��ȭ");
                }
                else
                {
                    Debug.LogError("Gravestone group ���� �ʿ��� ������Ʈ�� ã�� �� �����ϴ�.");
                }
            }
            else
            {
                Debug.LogError("ETC �׷� ���� Gravestone group�� ã�� �� �����ϴ�.");
            }
        }
        else
        {
            Debug.LogError("ETC �׷��� ã�� �� �����ϴ�.");
        }
    }



}
