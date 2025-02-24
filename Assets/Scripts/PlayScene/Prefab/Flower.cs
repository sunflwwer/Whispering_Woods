using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class Flower : MonoBehaviour
{
    [SerializeField] private GameObject interactionUI; // 상호작용 UI
    [SerializeField] private string interactKey = "v"; // 상호작용 키 (기본값: V)
    private bool isPlayerNearby = false; // 플레이어가 가까이 있는지 여부
    private static int flowerCount = 4; // 초기 Flower 개수
    private TMP_Text flowerText; // Canvas의 Flower 텍스트

    private List<GameObject> lightFlowers = new List<GameObject>(); // LightFlower 오브젝트 리스트

    private void Start()
    {
        // Flower 텍스트 찾기
        GameObject flowerTextObject = GameObject.Find("Canvas/Flower");
        if (flowerTextObject != null)
        {
            flowerText = flowerTextObject.GetComponent<TextMeshProUGUI>();
            if (flowerText != null)
            {
                flowerText.text = $"Flower = {flowerCount}"; // 초기 값 설정
            }
            else
            {
                Debug.LogError("Flower 텍스트 컴포넌트를 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogError("Canvas 안에 'Flower' 텍스트 오브젝트를 찾을 수 없습니다.");
        }


        // ETC 그룹 내에서 Flower group 찾기
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
                        child.gameObject.SetActive(false); // 초기에는 비활성화
                        lightFlowers.Add(child.gameObject);
                    }
                }
            }
            else
            {
                //Debug.LogError("Flower group 오브젝트를 ETC 그룹에서 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogError("ETC 그룹 오브젝트를 찾을 수 없습니다.");
        }

        // 상호작용 UI 비활성화
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }

    private void Update()
    {
        // 플레이어가 가까이 있을 때 상호작용 키 입력 감지
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
        flowerCount = Mathf.Max(0, flowerCount - 1); // 개수가 0 이하로 내려가지 않게
        UpdateFlowerText();
        ActivateNextLightFlower(); // 랜덤으로 활성화

        if (flowerCount == 0)
        {
            ToggleGravestoneObjects(); // Gravestone 오브젝트 상태 전환
        }

        interactionUI?.SetActive(false); // 상호작용 UI 비활성화
        Destroy(gameObject); // 꽃 오브젝트 삭제
    }

    // Gravestone group 안의 오브젝트 상태 전환
    private void ToggleGravestoneObjects()
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
                    rock04.SetActive(true); // PT_Menhir_Rock_04 활성화
                    rock044.SetActive(false); // PT_Menhir_Rock_044 비활성화
                    Debug.Log("PT_Menhir_Rock_04 활성화, PT_Menhir_Rock_044 비활성화 완료");
                }
                else
                {
                    Debug.LogError("Gravestone group 내에서 필요한 오브젝트를 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogError("Gravestone group 오브젝트를 ETC 그룹에서 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogError("ETC 그룹 오브젝트를 찾을 수 없습니다.");
        }
    }


    private void UpdateFlowerText()
    {
        if (flowerText != null)
        {
            flowerText.text = $"Flower = {flowerCount}";
        }
    }

    // 랜덤으로 LightFlower 활성화
    private void ActivateNextLightFlower()
    {
        List<GameObject> inactiveFlowers = lightFlowers.FindAll(flower => !flower.activeSelf); // 비활성화된 꽃만 찾기

        if (inactiveFlowers.Count > 0)
        {
            int randomIndex = Random.Range(0, inactiveFlowers.Count); // 비활성화된 꽃 중 랜덤 선택
            GameObject lightFlower = inactiveFlowers[randomIndex];

            if (lightFlower != null)
            {
                lightFlower.SetActive(true); // 랜덤으로 활성화
                Debug.Log($"LightFlower 랜덤 활성화됨: {lightFlower.name}"); // 활성화된 오브젝트 이름 출력
            }
            else
            {
                Debug.LogError("선택된 LightFlower 오브젝트가 null입니다.");
            }
        }
        else
        {
            Debug.Log("모든 LightFlower 오브젝트가 이미 활성화되었습니다.");
        }
    }

}