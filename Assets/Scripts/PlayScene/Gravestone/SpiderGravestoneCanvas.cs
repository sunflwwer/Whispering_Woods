using UnityEngine;
using System.Collections; // 코루틴 사용을 위한 네임스페이스 추가

public class SpiderGravestoneCanvas : MonoBehaviour
{
    private Camera mainCamera;
    private Canvas canvas; // 캔버스를 제어하기 위한 참조
    private bool isPlayerNearby = false; // 플레이어 근처 여부

    [SerializeField] private AudioClip gravestoneActivateSound; // 비석 활성화 시 소리
    [SerializeField][Range(0f, 1f)] private float gravestoneActivateVolume = 1.0f; // 소리 볼륨 조절

    private AudioSource audioSource; // 오디오 소스 컴포넌트

    private bool isGravestoneActivated = false; // 비석 활성화 후 입력 제한


    private void Start()
    {
        mainCamera = Camera.main;
        canvas = GetComponent<Canvas>();

        if (canvas != null)
        {
            canvas.enabled = false;
        }

        // 오디오 소스 초기화
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false; // 자동 재생 비활성화
        audioSource.spatialBlend = 1.0f; // 3D 사운드 적용

        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider>();
        }
        collider.isTrigger = true;
    }

    private void LateUpdate()
    {
        if (mainCamera != null && canvas != null && canvas.enabled)
        {
            transform.LookAt(transform.position + mainCamera.transform.forward);
        }

        // 거미를 1마리 이상 죽였을 때만 F 키 입력 허용
        if (isPlayerNearby && !isGravestoneActivated && GlobalCounter.SpiderCounter >= 15 && Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(ActivateGravestoneCoroutine()); // 코루틴 실행
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canvas.enabled = true;
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canvas.enabled = false;
            isPlayerNearby = false;
        }
    }

    // 비석 활성화 코루틴 (0.5초 딜레이 후 활성화)
    private IEnumerator ActivateGravestoneCoroutine()
    {
        isGravestoneActivated = true; // 입력 제한 시작

        PlaySound(gravestoneActivateSound, gravestoneActivateVolume); // 소리 재생

        yield return new WaitForSeconds(0.5f); // 0.5초 대기 후 비석 활성화

        ActivateGravestone();
    }


    // 비석 활성화 메서드
    private void ActivateGravestone()
    {
        GameObject etcGroup = GameObject.Find("ETC");
        if (etcGroup != null)
        {
            Transform gravestoneGroup = etcGroup.transform.Find("Gravestone group");
            if (gravestoneGroup != null)
            {
                GameObject rock03 = gravestoneGroup.Find("PT_Menhir_Rock_03")?.gameObject;
                GameObject rock033 = gravestoneGroup.Find("PT_Menhir_Rock_033")?.gameObject;

                if (rock03 != null && rock033 != null)
                {
                    rock03.SetActive(false);
                    rock033.SetActive(true);
                    Debug.Log("거미 비석이 성공적으로 활성화되었습니다.");
                    canvas.enabled = false;

                    // 전역 변수 업데이트
                    GlobalCounter.IsSpiderGravestoneActivated = true;

                    // 모든 비석이 활성화되었는지 확인
                    CheckAllGravestonesActivated();
                }
            }
        }
    }

    private void CheckAllGravestonesActivated()
    {
        if (GlobalCounter.AreAllGravestonesActivated())
        {
            Debug.Log("모든 비석이 활성화되었습니다. 게임 성공 처리!");
            // 여기서 게임 성공 처리를 실행 (성공 패널 띄우기 등)
        }
    }

    // 소리 재생 함수
    private void PlaySound(AudioClip clip, float volume)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volume); // 지정된 볼륨으로 소리 재생
        }
    }
}
