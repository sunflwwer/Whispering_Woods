using UnityEngine;
using TMPro;
using System.Collections; // 코루틴 사용을 위한 네임스페이스 추가

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
    private TextMeshProUGUI gravestoneMessageText; // 비석 메시지 텍스트

    private float keyHoldTime = 0f; // V키 누른 시간 기록
    private float[] requiredHoldTimes = { 1f, 1.5f, 2f }; // 각 단계별로 눌러야 하는 시간
    private bool isHoldingKey = false; // V키를 누르고 있는지 여부

    // 중간 프리팹 교체 사운드 설정 추가 (인스펙터에서 조정 가능)
    [SerializeField] private AudioClip intermediateTreeSound; // 중간 프리팹 전환 시 사운드
    [SerializeField][Range(0f, 1f)] private float intermediateTreeSoundVolume = 1.0f; // 중간 사운드 볼륨


    // 1. 필요한 변수 추가 (클래스 상단에 선언)
    [SerializeField] private AudioClip finalTreeSound; // 최종 프리팹 전환 시 소리
    [SerializeField][Range(0f, 1f)] private float finalTreeSoundVolume = 1.0f; // 소리 볼륨 조절
    private AudioSource audioSource; // 오디오 소스 컴포넌트

    private bool isFinalStage = false; // 최종 프리팹 상태 여부

    private bool isTransforming = false; // 프리팹 변환 중 여부 확인
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
                specialTreeCounter.text = $"나무 {GlobalCounter.TreeCounter}개 자람";

            }
        }

        // Gravestone 메시지 텍스트 찾기
        GameObject gravestoneMessageObject = GameObject.Find("ETC/Gravestone group/PT_Menhir_Rock_01/Canvas/Text");
        if (gravestoneMessageObject != null)
        {
            gravestoneMessageText = gravestoneMessageObject.GetComponent<TextMeshProUGUI>();
        }

        // 2. Start() 메서드에 오디오 소스 초기화 추가
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false; // 자동 재생 비활성화
        audioSource.spatialBlend = 1.0f; // 3D 사운드 적용

    }
    private void Update()
    {
        if (isPlayerNearby && !isFinalStage && !isTransforming) // 프리팹 전환 중일 때 입력 금지
        {
            if (Input.GetKey(KeyCode.F))
            {
                isHoldingKey = true;
                keyHoldTime += Time.deltaTime;

                float requiredTime = (currentStage >= 0 && currentStage < requiredHoldTimes.Length) ? requiredHoldTimes[currentStage] : 1f;

                if (keyHoldTime >= requiredTime)
                {
                    TransformTree();
                    keyHoldTime = 0f;
                    isHoldingKey = false;
                }
            }
            else if (isHoldingKey && Input.GetKeyUp(KeyCode.F))
            {
                keyHoldTime = 0f;
                isHoldingKey = false;
            }
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
        if (isFinalStage || isTransforming) return; // 최종 프리팹 또는 이미 변환 중이면 중단

        isTransforming = true; // 변환 중 상태 설정

        if (currentStage >= treePrefabs.Length - 1)
        {
            Debug.Log("최종 프리팹으로 변환됩니다.");
            StartCoroutine(DelayedReplaceFinalTree());
            return;
        }

        currentStage++;
        StartCoroutine(DelayedReplaceIntermediateTree(treePrefabs[currentStage])); // 중간 프리팹 교체
    }

    private IEnumerator DelayedReplaceIntermediateTree(GameObject newTreePrefab)
    {
        // 중간 프리팹 사운드 재생
        if (intermediateTreeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(intermediateTreeSound, intermediateTreeSoundVolume);
            Debug.Log("중간 프리팹 사운드가 재생되었습니다.");
        }

        yield return new WaitForSeconds(0.5f); // 0.5초 대기 후 프리팹 교체

        ReplaceTree(newTreePrefab);
    }


    private IEnumerator DelayedReplaceFinalTree()
    {
        yield return new WaitForSeconds(0.5f); // 0.5초 대기
        ReplaceWithFinalTree(); // 최종 프리팹 전환
    }


    private void ReplaceTree(GameObject newTreePrefab)
    {
        GameObject newTree = Instantiate(newTreePrefab, transform.position, transform.rotation);

        SpecialTreeInteraction script = newTree.AddComponent<SpecialTreeInteraction>();
        script.treePrefabs = treePrefabs;
        script.finalTreePrefab = finalTreePrefab;
        script.detectionRange = detectionRange;
        script.detectionAngle = detectionAngle;
        script.currentStage = currentStage;

        // 오디오 설정 값 복사
        script.finalTreeSound = finalTreeSound;
        script.finalTreeSoundVolume = finalTreeSoundVolume;

        // 중간 프리팹 사운드 값 복사
        script.intermediateTreeSound = intermediateTreeSound;
        script.intermediateTreeSoundVolume = intermediateTreeSoundVolume;

        // 최종 단계 여부 복사
        script.isFinalStage = isFinalStage;

        if (interactionUI != null)
        {
            interactionUI.transform.SetParent(newTree.transform, true);
        }

        if (lightObject != null)
        {
            lightObject.transform.SetParent(newTree.transform);
            lightObject.transform.localPosition = Vector3.zero;
        }

        Destroy(gameObject); // 기존 프리팹 제거
    }



    // 3. ReplaceWithFinalTree() 메서드 수정
    private void ReplaceWithFinalTree()
    {
        if (finalTreePrefab == null)
        {
            Debug.LogError("최종 프리팹이 지정되지 않았습니다!");
            return;
        }

        // 소리 재생
        if (finalTreeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(finalTreeSound, finalTreeSoundVolume);
            Debug.Log("최종 프리팹 사운드가 재생되었습니다."); // 소리 재생 확인 로그
        }
        else
        {
            Debug.LogWarning("사운드 클립이나 오디오 소스가 설정되지 않았습니다."); // 오디오 설정 확인용 경고 로그
        }
        isFinalStage = true; // 최종 프리팹 활성화 이후 상호작용 비활성화

        // 0.5초 대기 후 최종 프리팹 전환
        StartCoroutine(DelayedFinalTreeReplacement());
    }


    private IEnumerator DelayedFinalTreeReplacement()
    {
        yield return new WaitForSeconds(0.5f); // 0.5초 대기

        GameObject newTree = Instantiate(finalTreePrefab, transform.position, transform.rotation);

        SpecialTreeInteraction script = newTree.AddComponent<SpecialTreeInteraction>();
        script.treePrefabs = treePrefabs;
        script.finalTreePrefab = finalTreePrefab;
        script.detectionRange = detectionRange;
        script.detectionAngle = detectionAngle;
        script.currentStage = currentStage;

        // 오디오 설정 값 복사
        script.finalTreeSound = finalTreeSound;
        script.finalTreeSoundVolume = finalTreeSoundVolume;


        // 중간 프리팹 사운드 값 복사
        script.intermediateTreeSound = intermediateTreeSound;
        script.intermediateTreeSoundVolume = intermediateTreeSoundVolume;

        // 최종 프리팹 여부 복사 (중요!)
        script.isFinalStage = true;

        if (interactionUI != null)
        {
            interactionUI.transform.SetParent(newTree.transform, true);

            TextMeshProUGUI treeCanvasText = interactionUI.GetComponentInChildren<TextMeshProUGUI>();
            if (treeCanvasText != null)
            {
                treeCanvasText.text = "나무가 다 자랐어요";
            }
        }

        if (lightObject != null)
        {
            lightObject.transform.SetParent(newTree.transform);
            lightObject.transform.localPosition = Vector3.zero;
        }

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
                        child.gameObject.SetActive(true);
                        Debug.Log($"{child.gameObject.name} 오브젝트가 활성화되었습니다.");
                    }
                }
            }
            else
            {
                Debug.LogError("FinalTree group을 ETC 그룹 내에서 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogError("ETC 그룹을 찾을 수 없습니다.");
        }

        // 최종 프리팹 생성 시 카운터 증가
        GlobalCounter.TreeCounter++;
        UpdateSpecialTreeCounter();

        Destroy(gameObject);
    }





    private void UpdateSpecialTreeCounter()
    {
        if (specialTreeCounter != null)
        {
            specialTreeCounter.text = $"나무 {GlobalCounter.TreeCounter}개 자람";
        }

        // GlobalCounter.TreeCounter가 3이 되면 메시지 업데이트
        if (GlobalCounter.TreeCounter == 3)
        {
            UpdateGravestoneMessage();
        }
    }


    // 텍스트 메시지를 업데이트하는 메서드
    private void UpdateGravestoneMessage()
    {
        if (gravestoneMessageText != null)
        {
            gravestoneMessageText.text = "나무를 모두 자라게했어요!\nF키를 눌러 비석을 활성화하세요!";

        }
    }


}