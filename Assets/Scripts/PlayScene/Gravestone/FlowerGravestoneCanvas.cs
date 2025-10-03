using UnityEngine;
using System.Collections; // 코루틴 사용을 위한 네임스페이스 추가


public class FlowerGravestoneCanvas : MonoBehaviour
{
    private Camera mainCamera;
    private Canvas canvas; // 캔버스를 제어하기 위한 참조
    private bool isPlayerNearby = false; // 플레이어 근처 여부

    [SerializeField] private AudioClip gravestoneActivateSound; // 비석 활성화 시 소리
    [SerializeField][Range(0f, 1f)] private float gravestoneActivateVolume = 1.0f; // 비석 사운드 볼륨 조절

    private AudioSource audioSource; // 오디오 소스

    private bool isGravestoneActivated = false; // 비석 활성화 후 입력 제한



    private void Start()
    {
        // 게임에서 활성화된 카메라를 자동으로 찾음
        mainCamera = Camera.main;

        // 오디오 소스 초기화
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false; // 자동 재생 비활성화
        audioSource.spatialBlend = 1.0f; // 3D 사운드 적용


        // 캔버스 컴포넌트 참조 가져오기
        canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.enabled = false; // 시작 시 캔버스 비활성화
        }
        else
        {
            Debug.LogError("Canvas component not found on the GameObject.");
        }

        // Collider 확인 및 설정
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider>(); // Collider가 없으면 자동 추가
        }
        collider.isTrigger = true; // 트리거 활성화
    }

    private void LateUpdate()
    {
        if (mainCamera != null && canvas != null && canvas.enabled)
        {
            // 캔버스가 카메라를 향하도록 회전
            transform.LookAt(transform.position + mainCamera.transform.forward);
        }

        // 전역 꽃 카운터가 0일 때만 F 키 입력으로 비석 활성화
        if (isPlayerNearby && !isGravestoneActivated && GlobalCounter.FlowerCounter == 0 && Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(ActivateGravestoneCoroutine());
        }

    }

    // 비석 오브젝트 활성화 코루틴
    private IEnumerator ActivateGravestoneCoroutine()
    {
        isGravestoneActivated = true; // 입력 제한 시작

        // 비석 활성화 시 소리 재생
        PlaySound(gravestoneActivateSound, gravestoneActivateVolume);

        // 0.5초 대기
        yield return new WaitForSeconds(0.5f);

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
                    rock044.SetActive(false);
                    rock04.SetActive(true);
                    Debug.Log("꽃 비석이 성공적으로 활성화되었습니다.");
                    canvas.enabled = false;

                    // 전역 변수 업데이트
                    GlobalCounter.IsFlowerGravestoneActivated = true;

                    // 모든 비석이 활성화되었는지 확인
                    CheckAllGravestonesActivated();
                }
            }
        }
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (canvas != null)
            {
                canvas.enabled = true; // 언제나 플레이어가 접근하면 캔버스 활성화
            }
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (canvas != null)
            {
                canvas.enabled = false; // 플레이어가 나가면 캔버스 비활성화
            }
            isPlayerNearby = false;
        }
    }

    // 비석 오브젝트 활성화
    private void ActivateGravestone()
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
                    rock044.SetActive(false);
                    rock04.SetActive(true);
                    Debug.Log("꽃 비석이 성공적으로 활성화되었습니다.");
                    canvas.enabled = false;

                    // 비석 활성화 시 소리 재생
                    PlaySound(gravestoneActivateSound, gravestoneActivateVolume);

                    // 전역 변수 업데이트
                    GlobalCounter.IsFlowerGravestoneActivated = true;

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
            // 게임 성공 로직 실행
        }
    }
    private void PlaySound(AudioClip clip, float volume)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volume); // 입력된 볼륨으로 소리 재생
        }
    }

}
