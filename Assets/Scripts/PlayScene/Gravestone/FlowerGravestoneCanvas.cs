using UnityEngine;
using TMPro; // 텍스트 메시지를 위한 추가

public class FlowerGravestoneCanvas : MonoBehaviour
{
    private Camera mainCamera;
    private Canvas canvas; // 캔버스 참조
    private TMP_Text activationMessageText; // 메시지 텍스트 참조
    private bool canActivate = false; // 활성화 가능 여부

    private void Start()
    {
        // 메인 카메라 찾기
        mainCamera = Camera.main;

        // 캔버스 컴포넌트 가져오기
        canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.enabled = false; // 시작 시 비활성화
        }
        else
        {
            Debug.LogError("Canvas component not found on the GameObject.");
        }

        // 텍스트 컴포넌트 찾기
        activationMessageText = GetComponentInChildren<TMP_Text>();
        if (activationMessageText != null)
        {
            activationMessageText.text = ""; // 초기화 시 텍스트 비우기
        }

        // Collider 설정
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider>();
        }
        collider.isTrigger = true; // 트리거 설정
    }

    private void LateUpdate()
    {
        if (mainCamera != null && canvas != null && canvas.enabled)
        {
            // 플레이어를 향하도록 캔버스 회전
            transform.LookAt(transform.position + mainCamera.transform.forward);
        }

        // 활성화 가능 시 F키 입력 감지
        if (canActivate && Input.GetKeyDown(KeyCode.F))
        {
            ActivateGravestone();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && canvas != null)
        {
            canvas.enabled = true; // 캔버스 활성화
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && canvas != null)
        {
            canvas.enabled = false; // 캔버스 비활성화
        }
    }

    // 꽃이 모두 제거된 후 메시지 활성화
    public void EnableActivationMessage(string message)
    {
        if (activationMessageText != null)
        {
            activationMessageText.text = message; // 텍스트 설정
        }
        canActivate = true; // 활성화 가능 상태로 설정
    }

    // 비석 활성화 로직
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
                    rock04.SetActive(true); // 비석 활성화
                    rock044.SetActive(false); // 이전 비석 비활성화
                    Debug.Log("비석이 성공적으로 활성화되었습니다.");
                }
            }
        }

        if (activationMessageText != null)
        {
            activationMessageText.text = ""; // 메시지 제거
        }
        canActivate = false; // 다시 비활성화 상태로 전환
    }
}
