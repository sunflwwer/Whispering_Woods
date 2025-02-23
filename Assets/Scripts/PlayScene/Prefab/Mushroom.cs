using Sample;
using UnityEngine;
using TMPro; // TextMeshPro 사용을 위해 추가

public class Mushroom : MonoBehaviour
{
    [SerializeField] private int healAmount = 1;
    private GameObject interactionUI;
    private bool isPlayerNearby = false;
    private GhostScript player;
    private TerrainObjectManager terrainObjectManager;

    private static int totalMushroomsConsumed = 0; // 총 소비된 버섯 개수
    private TMP_Text mushroomText; // Canvas에서 Mushroom 텍스트 추적

    // Gravestone 오브젝트 참조
    private GameObject gravestone02;
    private GameObject gravestone022;

    private void Start()
    {
        interactionUI = transform.Find("Canvas")?.gameObject;
        interactionUI?.SetActive(false);

        // 버섯이 속한 TerrainObjectManager 찾기
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

        // Canvas에서 Mushroom 텍스트 찾기 및 초기화
        GameObject mushroomTextObject = GameObject.Find("Canvas/Mushroom");
        if (mushroomTextObject != null)
        {
            mushroomText = mushroomTextObject.GetComponent<TextMeshProUGUI>();
            if (mushroomText != null)
            {
                mushroomText.text = $"Mushroom = {totalMushroomsConsumed}"; // 초기값 설정
            }
            else
            {
                Debug.LogError("Mushroom 텍스트 컴포넌트를 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogError("Canvas 안에 'Mushroom' 텍스트 오브젝트를 찾을 수 없습니다.");
        }


        // Gravestone 그룹 내의 오브젝트 참조만 가져오기 (초기 활성화/비활성화 설정 제거)
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
                    //Debug.LogError("Gravestone 오브젝트들을 찾을 수 없습니다.");
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
        // 플레이어 체력 회복
        player?.Heal(healAmount);

        // 버섯 소비 시 카운트 증가 및 텍스트 업데이트
        totalMushroomsConsumed++;
        UpdateMushroomText();

        // 5개 이상 소비 시 Gravestone 상태 변경
        if (totalMushroomsConsumed == 5)
        {
            ToggleGravestoneObjects();
        }

        // 상호작용 UI 비활성화 및 버섯 제거
        interactionUI?.SetActive(false);
        terrainObjectManager?.OnMushroomConsumed(transform.position);
        Destroy(gameObject);
    }

    // Gravestone 오브젝트 상태 전환 (초기화 설정 없이 5개 이상 먹었을 때만 실행)
    private void ToggleGravestoneObjects()
    {
        if (gravestone02 != null && gravestone022 != null)
        {
            gravestone02.SetActive(true);  // PT_Menhir_Rock_02 활성화
            gravestone022.SetActive(false); // PT_Menhir_Rock_022 비활성화
            Debug.Log("PT_Menhir_Rock_02 활성화, PT_Menhir_Rock_022 비활성화 완료");
        }
    }

    // Mushroom 텍스트 업데이트
    private void UpdateMushroomText()
    {
        if (mushroomText != null)
        {
            mushroomText.text = $"Mushroom = {totalMushroomsConsumed}";
        }
    }
}
