using UnityEngine;
using TMPro;


public class SpecialTreeInteraction : MonoBehaviour
{
    public GameObject[] treePrefabs; // 교체될 프리팹 배열
    public GameObject finalTreePrefab; // 특정한 최종 프리팹 (하나만 지정 가능)
    public float detectionRange = 5f;
    public float detectionAngle = 30f;
    private int currentStage = -1;

    private GameObject interactionUI;
    private GameObject lightObject;
    private bool isPlayerNearby = false;
    private GameObject player;

    private TextMeshProUGUI specialTreeCounter; // SpecialTree 텍스트 객체


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
                specialTreeCounter.text = "0"; // 초기 값 설정
            }
        }
        else
        {
            Debug.LogError("SpecialTree 텍스트 오브젝트를 찾을 수 없습니다.");
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
            Debug.Log("최종 프리팹으로 변환됩니다.");
            ReplaceWithFinalTree(); // 랜덤이 아니라 지정된 하나의 최종 프리팹으로 변환
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
        script.finalTreePrefab = finalTreePrefab; // 최종 프리팹 지정
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
            Debug.LogError("최종 프리팹이 지정되지 않았습니다!");
            return;
        }

        // 최종 프리팹 생성
        GameObject newTree = Instantiate(finalTreePrefab, transform.position, transform.rotation);

        // 이름 출력 (프리팹 이름 확인)
        Debug.Log($"최종 프리팹이 생성되었습니다: {finalTreePrefab.name}");

        // ETC 그룹 내에서 FinalTree group 찾기
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
                        child.gameObject.SetActive(true); // 해당 오브젝트 활성화
                        Debug.Log($"{child.gameObject.name} 오브젝트가 활성화되었습니다.");

                        // SpecialTree 텍스트 카운터 증가
                        if (specialTreeCounter != null)
                        {
                            int currentCount = int.Parse(specialTreeCounter.text);
                            currentCount++;
                            specialTreeCounter.text = currentCount.ToString();
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("FinalTree group 오브젝트를 ETC 그룹에서 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogError("ETC 그룹 오브젝트를 찾을 수 없습니다.");
        }

        // 불빛 유지
        if (lightObject != null)
        {
            lightObject.transform.SetParent(newTree.transform);
            lightObject.transform.localPosition = Vector3.zero;
        }

        // 인터랙션 UI 제거
        if (interactionUI != null)
        {
            Destroy(interactionUI);
        }

        Destroy(gameObject);
    }


}
