using Sample;
using UnityEngine;
using TMPro; // TextMeshPro ����� ���� �߰�

public class Mushroom : MonoBehaviour
{
    [SerializeField] private int healAmount = 1;
    private GameObject interactionUI;
    private bool isPlayerNearby = false;
    private GhostScript player;
    private TerrainObjectManager terrainObjectManager;

    private static int totalMushroomsConsumed = 0; // �� �Һ�� ���� ����
    private TMP_Text mushroomText; // Canvas���� Mushroom �ؽ�Ʈ ����

    // Gravestone ������Ʈ ����
    private GameObject gravestone02;
    private GameObject gravestone022;

    private void Start()
    {
        interactionUI = transform.Find("Canvas")?.gameObject;
        interactionUI?.SetActive(false);

        // ������ ���� TerrainObjectManager ã��
        TerrainObjectManager[] managers = FindObjectsOfType<TerrainObjectManager>();
        foreach (var manager in managers)
        {
            if (manager.targetTerrain.terrainData.bounds.Contains(transform.position - manager.targetTerrain.transform.position))
            {
                terrainObjectManager = manager;
                break;
            }
        }

        if (terrainObjectManager == null)
        {
            Debug.LogError("TerrainObjectManager not found for mushroom position.");
        }

        // Canvas���� Mushroom �ؽ�Ʈ ã�� �� �ʱ�ȭ
        GameObject mushroomTextObject = GameObject.Find("Canvas/Mushroom");
        if (mushroomTextObject != null)
        {
            mushroomText = mushroomTextObject.GetComponent<TextMeshProUGUI>();
            if (mushroomText != null)
            {
                mushroomText.text = $"Mushroom = {totalMushroomsConsumed}"; // �ʱⰪ ����
            }
            else
            {
                Debug.LogError("Mushroom �ؽ�Ʈ ������Ʈ�� ã�� �� �����ϴ�.");
            }
        }
        else
        {
            Debug.LogError("Canvas �ȿ� 'Mushroom' �ؽ�Ʈ ������Ʈ�� ã�� �� �����ϴ�.");
        }


        // Gravestone �׷� ���� ������Ʈ ������ �������� (�ʱ� Ȱ��ȭ/��Ȱ��ȭ ���� ����)
        GameObject etcGroup = GameObject.Find("ETC");
        if (etcGroup != null)
        {
            Transform gravestoneGroup = etcGroup.transform.Find("Gravestone group");
            if (gravestoneGroup != null)
            {
                gravestone02 = gravestoneGroup.Find("PT_Menhir_Rock_02")?.gameObject;
                gravestone022 = gravestoneGroup.Find("PT_Menhir_Rock_022")?.gameObject;

                if (gravestone02 == null || gravestone022 == null)
                {
                    //Debug.LogError("Gravestone ������Ʈ���� ã�� �� �����ϴ�.");
                }
            }
        }
    }

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.F))
        {
            ConsumeMushroom();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out GhostScript ghost))
        {
            isPlayerNearby = true;
            player = ghost;
            interactionUI?.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out GhostScript ghost))
        {
            isPlayerNearby = false;
            player = null;
            interactionUI?.SetActive(false);
        }
    }

    private void ConsumeMushroom()
    {
        // �÷��̾� ü�� ȸ��
        player?.Heal(healAmount);

        // ���� �Һ� �� ī��Ʈ ���� �� �ؽ�Ʈ ������Ʈ
        totalMushroomsConsumed++;
        UpdateMushroomText();

        // 5�� �̻� �Һ� �� Gravestone ���� ����
        if (totalMushroomsConsumed == 5)
        {
            ToggleGravestoneObjects();
        }

        // ��ȣ�ۿ� UI ��Ȱ��ȭ �� ���� ����
        interactionUI?.SetActive(false);
        terrainObjectManager?.OnMushroomConsumed(transform.position);
        Destroy(gameObject);
    }

    // Gravestone ������Ʈ ���� ��ȯ (�ʱ�ȭ ���� ���� 5�� �̻� �Ծ��� ���� ����)
    private void ToggleGravestoneObjects()
    {
        if (gravestone02 != null && gravestone022 != null)
        {
            gravestone02.SetActive(true);  // PT_Menhir_Rock_02 Ȱ��ȭ
            gravestone022.SetActive(false); // PT_Menhir_Rock_022 ��Ȱ��ȭ
            Debug.Log("PT_Menhir_Rock_02 Ȱ��ȭ, PT_Menhir_Rock_022 ��Ȱ��ȭ �Ϸ�");
        }
    }

    // Mushroom �ؽ�Ʈ ������Ʈ
    private void UpdateMushroomText()
    {
        if (mushroomText != null)
        {
            mushroomText.text = $"Mushroom = {totalMushroomsConsumed}";
        }
    }
}
