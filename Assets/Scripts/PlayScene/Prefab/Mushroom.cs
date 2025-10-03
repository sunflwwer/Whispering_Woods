using Sample;
using UnityEngine;
using TMPro; // TextMeshPro 사용을 위해 추가
using System.Collections; // IEnumerator 사용을 위한 네임스페이스 추가


public class Mushroom : MonoBehaviour
{
    [SerializeField] private int healAmount = 1;
    private GameObject interactionUI;
    private bool isPlayerNearby = false;
    private GhostScript player;
    private TerrainObjectManager terrainObjectManager;

    private TMP_Text mushroomText; // Canvas에서 Mushroom 텍스트 추적
    private TMP_Text gravestoneMessageText; // 비석 캔버스 텍스트 참조

    private float holdTime = 0f; // 키를 누르고 있는 시간
    private bool isHoldingKey = false; // 키가 눌리고 있는지 여부

    // 오디오 관련 변수 추가
    [SerializeField] private AudioClip mushroomConsumeSound; // 버섯이 없어질 때 소리
    [SerializeField][Range(0f, 1f)] private float mushroomConsumeVolume = 1.0f; // 사운드 볼륨 조절

    private AudioSource audioSource; // 오디오 소스 컴포넌트



    private void Start()
    {
        interactionUI = transform.Find("Canvas")?.gameObject;
        interactionUI?.SetActive(false);

        // 오디오 소스 초기화
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false; // 자동 재생 비활성화


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
                mushroomText.text = $"버섯 {GlobalCounter.MushroomCounter}개 먹음"; // 전역 카운터 사용
            }
        }

        // 비석 메시지 텍스트 찾기
        GameObject gravestoneMessageObject = GameObject.Find("ETC/Gravestone group/PT_Menhir_Rock_022/Canvas/Text");
        if (gravestoneMessageObject != null)
        {
            gravestoneMessageText = gravestoneMessageObject.GetComponent<TextMeshProUGUI>();
        }
    }

    private void Update()
    {
        if (isPlayerNearby)
        {
            if (Input.GetKey(KeyCode.E)) // 키를 누르고 있는 동안
            {
                isHoldingKey = true;
                holdTime += Time.deltaTime;

                if (holdTime >= 0.5f) // 0.5초 이상 유지 시 버섯 소비
                {
                    ConsumeMushroom();
                    holdTime = 0f; // 초기화
                    isHoldingKey = false;
                }
            }
            else if (isHoldingKey) // 키에서 손을 뗐을 때
            {
                holdTime = 0f;
                isHoldingKey = false;
            }
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
        StartCoroutine(ConsumeMushroomCoroutine());
    }

    private IEnumerator ConsumeMushroomCoroutine()
    {
        // 소리 재생
        PlaySound(mushroomConsumeSound, mushroomConsumeVolume);

        // 0.5초 대기
        yield return new WaitForSeconds(0.5f);

        // 플레이어 체력 회복
        player?.Heal(healAmount);

        // 전역 버섯 카운터 증가 및 텍스트 업데이트
        GlobalCounter.MushroomCounter++;
        UpdateMushroomText();

        // 30개 소비 시 비석 텍스트 변경
        if (GlobalCounter.MushroomCounter == 30)
        {
            UpdateGravestoneMessage();
        }

        // 상호작용 UI 비활성화 및 버섯 제거
        interactionUI?.SetActive(false);
        terrainObjectManager?.OnMushroomConsumed(transform.position);
        Destroy(gameObject);
    }



    // 버섯 텍스트 업데이트
    private void UpdateMushroomText()
    {
        if (mushroomText != null)
        {
            mushroomText.text = $"버섯 {GlobalCounter.MushroomCounter}개 먹음";
        }
    }

    // 비석 텍스트 업데이트
    private void UpdateGravestoneMessage()
    {
        if (gravestoneMessageText != null)
        {
            gravestoneMessageText.text = "버섯  30개를 먹었어요!\nF키를 눌러 비석을 활성화하세요!";
        }
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

}
