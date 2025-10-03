using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class Flower : MonoBehaviour
{
    [SerializeField] private GameObject interactionUI; // 상호작용 UI
    [SerializeField] private string interactKey = "v"; // 상호작용 키 (기본값: V)
    private bool isPlayerNearby = false; // 플레이어가 가까이 있는지 여부
    private TMP_Text flowerText; // Canvas의 Flower 텍스트
    private TMP_Text gravestoneMessageText; // 비석 캔버스 텍스트 참조

    private List<GameObject> lightFlowers = new List<GameObject>(); // LightFlower 오브젝트 리스트

    private int interactionCount = 0; // V키 입력 횟수
    private float lastInteractionTime = 0f; // 마지막 입력 시간 기록
    private float interactionTimeout = 2f; // 2초 시간 제한
    private int requiredInteractions = 5; // 연속 입력 필요 횟수

    // 오디오 관련 변수 추가
    [SerializeField] private AudioClip flowerConsumeSound; // 꽃이 없어질 때 소리
    [SerializeField][Range(0f, 1f)] private float flowerConsumeVolume = 1.0f; // 꽃 사운드 볼륨

    [SerializeField] private AudioClip interactionSound; // 상호작용 키로 공격당할 때 소리
    [SerializeField][Range(0f, 1f)] private float interactionSoundVolume = 1.0f; // 상호작용 사운드 볼륨

    private AudioSource audioSource; // 오디오 소스

    private bool isConsuming = false; // 꽃 제거 중인지 여부 확인


    private void Start()
    {
        // Flower 텍스트 찾기
        GameObject flowerTextObject = GameObject.Find("Canvas/Flower");
        if (flowerTextObject != null)
        {
            flowerText = flowerTextObject.GetComponent<TextMeshProUGUI>();
            if (flowerText != null)
            {
                flowerText.text = $"악의꽃 {GlobalCounter.FlowerCounter}개 남음"; // 전역 변수 값으로 설정
            }
        }

        // 비석 메시지 텍스트 찾기
        GameObject gravestoneMessageObject = GameObject.Find("ETC/Gravestone group/PT_Menhir_Rock_044/Canvas/Text");
        if (gravestoneMessageObject != null)
        {
            gravestoneMessageText = gravestoneMessageObject.GetComponent<TextMeshProUGUI>();
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
        }

        // 상호작용 UI 비활성화
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }

        // 오디오 소스 컴포넌트 추가 및 초기화
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false; // 자동 재생 비활성화

    }
    private void Update()
    {
        if (isConsuming) return; // 꽃 제거 중이면 입력 무시

        // 2초가 지나면 입력 횟수 초기화
        if (interactionCount > 0 && Time.time - lastInteractionTime > interactionTimeout)
        {
            Debug.Log($"[Flower] 입력 시간 초과로 초기화됨. 마지막 입력 시간: {lastInteractionTime}, 현재 시간: {Time.time}");
            interactionCount = 0;
        }

        // 플레이어가 가까이 있을 때 상호작용 키 입력 감지
        if (isPlayerNearby && Input.GetKeyDown(interactKey))
        {
            PlaySound(interactionSound, interactionSoundVolume); // 상호작용 시 소리 재생
            interactionCount++;
            lastInteractionTime = Time.time; // 마지막 입력 시간 갱신
            Debug.Log($"[Flower] 상호작용 키 입력 감지. 현재 입력 횟수: {interactionCount}");

            if (interactionCount >= requiredInteractions)
            {
                Debug.Log("[Flower] 입력 횟수 충족, 꽃 제거 실행");
                ConsumeFlower(); // 꽃 제거
                interactionCount = 0; // 입력 횟수 초기화
            }
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
        StartCoroutine(ConsumeFlowerCoroutine());
    }

    private IEnumerator ConsumeFlowerCoroutine()
    {
        isConsuming = true; // 꽃 제거 시작

        PlaySound(flowerConsumeSound, flowerConsumeVolume); // 꽃 제거 시 소리 재생

        // 0.5초 대기 후 삭제
        yield return new WaitForSeconds(0.5f);

        // 전역 카운터 감소
        GlobalCounter.FlowerCounter = Mathf.Max(0, GlobalCounter.FlowerCounter - 1);
        UpdateFlowerText();
        ActivateNextLightFlower(); // 랜덤으로 활성화

        if (GlobalCounter.FlowerCounter == 3)
        {
            UpdateGravestoneMessage(); // 비석 텍스트 업데이트
        }

        interactionUI?.SetActive(false); // 상호작용 UI 비활성화
        Destroy(gameObject); // 꽃 오브젝트 삭제
    }



    private void UpdateFlowerText()
    {
        if (flowerText != null)
        {
            flowerText.text = $"악의꽃 {GlobalCounter.FlowerCounter}개 남음";
        }
    }

    private void ActivateNextLightFlower()
    {
        List<GameObject> inactiveFlowers = lightFlowers.FindAll(flower => !flower.activeSelf);

        if (inactiveFlowers.Count > 0)
        {
            int randomIndex = Random.Range(0, inactiveFlowers.Count);
            GameObject lightFlower = inactiveFlowers[randomIndex];

            if (lightFlower != null)
            {
                lightFlower.SetActive(true); // 랜덤으로 활성화
            }
        }
    }

    private void UpdateGravestoneMessage()
    {
        if (gravestoneMessageText != null)
        {
            gravestoneMessageText.text = "악의꽃을 모두 제거했어요! 비석 활성화를 위해 F키를 눌러주세요";
        }
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volume); // 입력된 볼륨으로 재생
        }
    }


}
