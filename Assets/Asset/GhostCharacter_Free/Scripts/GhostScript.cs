using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // 추가
using UnityEngine.SceneManagement; // 씬 전환을 위한 네임스페이스


namespace Sample
{
    public class GhostScript : MonoBehaviour
    {
        private Animator Anim;
        private CharacterController Ctrl;
        private Vector3 MoveDirection = Vector3.zero;
        private Vector3 initialPosition;
        private Quaternion initialRotation;

        private static readonly int IdleState = Animator.StringToHash("Base Layer.idle");
        private static readonly int MoveState = Animator.StringToHash("Base Layer.move");
        private static readonly int SurprisedState = Animator.StringToHash("Base Layer.surprised");
        private static readonly int AttackState = Animator.StringToHash("Base Layer.attack_shift");
        private static readonly int DissolveState = Animator.StringToHash("Base Layer.dissolve");
        private static readonly int AttackTag = Animator.StringToHash("Attack");

        [SerializeField] private SkinnedMeshRenderer[] MeshR;
        private float Dissolve_value = 1;
        private bool DissolveFlg = false;

        [SerializeField] private Slider hpSlider; // HP Slider 참조 추가

        private const int maxHP = 100;
        private int HP = 100;

        [SerializeField] private float Speed = 3;
        [SerializeField] private float sprintSpeed = 5.5f;

        /*        [SerializeField] private Transform cameraTransform;
                [SerializeField] private Transform playerBody;
                [SerializeField] private float mouseSensitivity = 100f;

                private float xRotation = 0f;*/

        [SerializeField] private float mouseSensitivity = 100f;


        private bool isJumping = false;
        [SerializeField] private float jumpForce = 4.5f;
        [SerializeField] private float fallMultiplier = 2.0f;
        private float verticalVelocity = 0f;

        private bool isMoving = false;

        [SerializeField] private float waterSpeedMultiplier = 0.5f;
        private bool isInWater = false;

        // [SerializeField] private float waterHeight = 0f;

/*        // 추가된 변수
        private GameObject currentRock = null; // 현재 밀 수 있는 돌
        private bool isPushing = false; // 돌 밀기 상태
        [SerializeField] private float pushSpeed = 3f; // 돌 밀기 속도*/

        private bool isDead = false; // 플레이어가 죽었는지 여부

        [SerializeField] private GameObject CinemachineCameraTarget; // 카메라가 따라갈 목표
        [SerializeField] private float TopClamp = 70.0f; // 위로 이동 제한
        [SerializeField] private float BottomClamp = -30.0f; // 아래로 이동 제한
        [SerializeField] private float CameraAngleOverride = 0.0f; // 카메라 회전 각도 오버라이드
        [SerializeField] private bool LockCameraPosition = false; // 카메라 위치 잠금

        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        [SerializeField] private GameObject gameOverPanel; // 게임 오버 패널 참조
        [SerializeField] private GameObject uiCanvas; // 전체 UI Canvas 참조

        [SerializeField] private DayNightCycle dayNightCycle; // DayNightCycle 스크립트 참조

        [SerializeField] private GameObject successFadeImage; // 성공 페이드 이미지 참조
        [SerializeField] private TextMeshProUGUI successDayText; // 성공 텍스트 (예: Day 3)


        [SerializeField] private AudioClip attackSound; // 공격 사운드 클립
        private AudioSource audioSource; // 오디오 소스 컴포넌트
        [SerializeField][Range(0f, 1f)] private float attackVolume = 1.0f; // 공격 사운드 볼륨 조절

        [SerializeField] private AudioClip enterWaterSound; // 물에 들어갈 때 소리
        [SerializeField][Range(0f, 1f)] private float enterWaterVolume = 1.0f; // 입수 사운드 볼륨

        [SerializeField] private AudioClip exitWaterSound; // 물에서 나올 때 소리
        [SerializeField][Range(0f, 1f)] private float exitWaterVolume = 1.0f; // 물에서 나올 때 사운드 볼륨

        [SerializeField] private AudioClip surprisedSound; // 놀라는 애니메이션 소리
        [SerializeField][Range(0f, 1f)] private float surprisedVolume = 1.0f; // 놀라는 소리 볼륨

        [SerializeField] private AudioClip deathSound; // 사망 시 재생할 소리
        [SerializeField][Range(0f, 1f)] private float deathVolume = 1.0f; // 사망 소리 볼륨

        [SerializeField] private AudioClip gameClearSound; // 게임 클리어 사운드
        [SerializeField][Range(0f, 1f)] private float gameClearVolume = 1.0f; // 게임 클리어 사운드 볼륨


        void Start()
        {

            // 씬 시작 시 모든 전역 변수 초기화
            GlobalCounter.ResetCounters();

            Anim = this.GetComponent<Animator>();
            Ctrl = this.GetComponent<CharacterController>();

            // 오디오 소스 초기화
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false; // 자동 재생 비활성화
            audioSource.spatialBlend = 1.0f; // 3D 사운드 적용
            audioSource.volume = 1.0f; // 기본 볼륨 설정

            initialPosition = this.transform.position;
            initialRotation = this.transform.rotation;

            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            if (hpSlider != null)
            {
                hpSlider.maxValue = maxHP;
                hpSlider.value = HP;

                // 초기 체력 색상 설정 (더 진한 그린으로 시작)
                Image fillImage = hpSlider.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    fillImage.color = new Color(0.13f, 0.55f, 0.26f); // 더 진한 초록색 (#228B45)
                }
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }

            // 모든 비석이 활성화되었을 때 GameClear 호출
            GlobalCounter.OnAllGravestonesActivated += GameClear;

            Cursor.lockState = CursorLockMode.Locked;
        }



        // 이벤트 구독 해제 (메모리 누수 방지)
        void OnDestroy()
        {
            GlobalCounter.OnAllGravestonesActivated -= GameClear;
        }




        void Update()
        {

            STATUS();
            MOVE();
            HandleKeyActions();
            //HandlePush(); // 추가된 돌 밀기 로직
            //Respawn();
            CameraRotation(); // 새로운 카메라 회전 로직

            if (HP <= 0 && !DissolveFlg)
            {
                Anim.CrossFade(DissolveState, 0.1f, 0, 0);
                DissolveFlg = true;
            }
            else if (HP == maxHP && DissolveFlg)
            {
                DissolveFlg = false;
            }
        }



        void FixedUpdate()
        {
            CheckGrounded(); // 물리 연산 최적화
        }


        private const int Dissolve = 1;
        private const int Attack = 2;
        private const int Surprised = 3;
        private Dictionary<int, bool> PlayerStatus = new Dictionary<int, bool>
        {
            { Dissolve, false },
            { Attack, false },
            { Surprised, false },
        };

        private void STATUS()
        {
            if (DissolveFlg && HP <= 0)
            {
                PlayerStatus[Dissolve] = true;
            }
            else if (!DissolveFlg)
            {
                PlayerStatus[Dissolve] = false;
            }

            if (Anim.GetCurrentAnimatorStateInfo(0).tagHash == AttackTag)
            {
                PlayerStatus[Attack] = true;
            }
            else if (Anim.GetCurrentAnimatorStateInfo(0).tagHash != AttackTag)
            {
                PlayerStatus[Attack] = false;
            }

            if (Anim.GetCurrentAnimatorStateInfo(0).fullPathHash == SurprisedState)
            {
                PlayerStatus[Surprised] = true;
            }
            else if (Anim.GetCurrentAnimatorStateInfo(0).fullPathHash != SurprisedState)
            {
                PlayerStatus[Surprised] = false;
            }

        }

        private void CameraRotation()
        {
            if (!LockCameraPosition)
            {
                float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
                float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

                _cinemachineTargetYaw += mouseX;
                _cinemachineTargetPitch -= mouseY;

                _cinemachineTargetYaw = Mathf.Clamp(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
                _cinemachineTargetPitch = Mathf.Clamp(_cinemachineTargetPitch, BottomClamp, TopClamp);

                CinemachineCameraTarget.transform.rotation = Quaternion.Euler(
                    _cinemachineTargetPitch + CameraAngleOverride,
                    _cinemachineTargetYaw,
                    0.0f
                );
            }
        }
        private void MOVE()
        {
            if (isDead) return;

            ApplyGravity();

            float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : Speed;

            Vector3 inputDirection = new Vector3(
                Input.GetAxis("Horizontal"),
                0,
                Input.GetAxis("Vertical")
            );

            if (inputDirection.magnitude > 0.1f)
            {
                inputDirection.Normalize();

                // 카메라 기준 방향으로 이동
                Vector3 moveDirection = CinemachineCameraTarget.transform.forward * inputDirection.z +
                                        CinemachineCameraTarget.transform.right * inputDirection.x;
                moveDirection.y = 0f; // 수평 이동 유지

                // 캐릭터가 이동 방향을 바라보도록 회전 추가
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

                // 실제 이동 처리
                Vector3 horizontalMove = moveDirection * currentSpeed;
                MoveDirection = new Vector3(horizontalMove.x, MoveDirection.y, horizontalMove.z);

                Ctrl.Move(MoveDirection * Time.deltaTime);

                if (!isMoving)
                {
                    Anim.CrossFade(MoveState, 0.1f, 0, 0);
                    isMoving = true;
                }
            }
            else
            {
                MoveDirection = Vector3.Lerp(MoveDirection, Vector3.zero, 10 * Time.deltaTime);
                Ctrl.Move(MoveDirection * Time.deltaTime);

                if (isMoving)
                {
                    Anim.CrossFade(IdleState, 0.1f, 0, 0);
                    isMoving = false;
                }
            }
        }



        private void HandleKeyActions()
        {
            if (isDead) return; // 플레이어가 죽었으면 공격 불가능

            if (Input.GetMouseButtonDown(0)) // 마우스 왼쪽 버튼으로 공격
            {
                if (InteractWithFence()) // 문과 상호작용이 성공하면 공격하지 않음
                {
                    Debug.Log("Fence interaction detected, skipping attack.");
                    return; // 공격 애니메이션 실행 X
                }

                Anim.CrossFade(AttackState, 0.1f, 0, 0); // 공격 애니메이션 실행
                PlayAttackSound(); // 공격 사운드 재생

                // 기존 공격 로직 유지
                float interactionDistance = 5f; // 상호작용 거리
                Vector3 rayStartPos = transform.position + Vector3.up * 0.8f; // 시작 위치를 살짝 아래로 (1.0 -> 0.8)
                Vector3 rayDirection = (transform.forward + Vector3.down * 0.1f).normalized; // 살짝 아래로 기울이기

                Debug.DrawRay(rayStartPos, rayDirection * interactionDistance, Color.red, 2.0f); // Ray 시각화

                // 모든 충돌체에 대해 Raycast 실행 (트리거 포함)
                RaycastHit[] hits = Physics.RaycastAll(rayStartPos, rayDirection, interactionDistance, ~0, QueryTriggerInteraction.Collide);

                RaycastHit? closestHit = null;
                float closestDistance = float.MaxValue;

                // SpiderEnemy 태그가 붙은 오브젝트 중 가장 가까운 것을 찾기
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.CompareTag("SpiderEnemy"))
                    {
                        float distance = Vector3.Distance(rayStartPos, hit.point);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestHit = hit;
                        }
                    }
                }


                if (closestHit.HasValue)
                {
                    SpiderScript spider = closestHit.Value.collider.GetComponentInParent<SpiderScript>();
                    if (spider != null)
                    {
                        Debug.Log("Spider hit detected! Calling TakeDamage()");
                        spider.TakeDamage();
                    }
                    else
                    {
                        Debug.LogError("SpiderScript not found on hit object!");
                    }
                }
                else
                {
                    Debug.Log("No valid hit detected with Raycast.");
                }
            }


/*            if (Input.GetKeyDown(KeyCode.Mouse1)) // 마우스 오른쪽 버튼으로 문 회전
            {
                InteractWithFence();
                Debug.Log("InteractWithFence.");
            }
*/
        }

        private void PlayAttackSound()
        {
            if (attackSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(attackSound, attackVolume); // 볼륨 조절 추가
            }
        }



        // InteractWithFence() 메서드를 bool로 변경
        private bool InteractWithFence()
        {
            float interactionDistance = 6.0f; // 문과의 상호작용 거리
            Vector3 rayStartPos = transform.position + Vector3.up * 1.0f; // 플레이어 위치에서 조금 위에서 시작
            Vector3 rayDirection = transform.forward; // 플레이어 기준 정면 방향

            Debug.DrawRay(rayStartPos, rayDirection * interactionDistance, Color.blue, 2.0f); // Ray 시각화

            RaycastHit[] hits = Physics.RaycastAll(rayStartPos, rayDirection, interactionDistance, ~0, QueryTriggerInteraction.Collide);

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Fence")) // Fence 태그 감지
                {
                    Debug.Log("Fence detected, toggling doors...");
                    DoorScript.ToggleAllDoors(); // 모든 문 열기/닫기
                    return true; // 상호작용 성공 시 true 반환
                }
            }

            return false; // 상호작용 실패 시 false 반환
        }






        public void TakeDamage(int damage)
        {
            if (isDead) return; // 이미 죽었으면 추가 피해 받지 않음

            HP = Mathf.Max(HP - damage, 0);
            UpdateHPUI(); // 체력 UI 업데이트

            if (HP > 0)
            {
                // 놀라는 애니메이션 실행 (죽지 않은 경우)
                Anim.CrossFade(SurprisedState, 0.1f, 0, 0);
                PlaySound(surprisedSound, surprisedVolume); // 놀라는 소리 재생
            }

            else
            {
                // HP가 0이면 사망 처리
                isDead = true;
                Die();
            }
        }


        private void Die()
        {
            isDead = true;

            Debug.Log("Player has died!");
            Anim.CrossFade(DissolveState, 0.1f, 0, 0); // 사망 애니메이션 실행
            PlaySound(deathSound, deathVolume); // 사망 소리 재생

            // 이동 및 피격 불가 처리
            Ctrl.enabled = false;
            GetComponent<Collider>().enabled = false;

            // DayNightCycle의 텍스트 동기화 중지
            if (dayNightCycle != null)
            {
                dayNightCycle.StopDaySync();
            }

            // UI 비활성화 및 게임 오버 패널 활성화
            if (uiCanvas != null)
            {
                foreach (Transform child in uiCanvas.transform)
                {
                    child.gameObject.SetActive(false); // 모든 UI 요소 비활성화
                }
            }

            if (gameOverPanel != null)
            {
                StartCoroutine(FadeInGameOverPanel()); // 페이드인 코루틴 시작
            }

            // 커서 락 해제 및 보이게 설정
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            StartCoroutine(WaitBeforeDissolve());
        }


        // 게임 오버 패널 페이드인 코루틴
        private IEnumerator FadeInGameOverPanel()
        {
            gameOverPanel.SetActive(true); // 게임 오버 패널 활성화

            CanvasGroup canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.alpha = 0f; // 시작 시 투명

                float duration = 1f; // 페이드인 지속 시간 (1초)
                float elapsedTime = 0f;

                while (elapsedTime < duration)
                {
                    elapsedTime += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration); // 점진적 투명도 증가
                    yield return null;
                }

                canvasGroup.alpha = 1f; // 완전히 보이게 설정
            }
        }






        // 1초 대기 후 Dissolve 효과 실행
        private IEnumerator WaitBeforeDissolve()
        {
            yield return new WaitForSeconds(1.0f); // 1초 대기
            StartCoroutine(DissolveEffect()); // Dissolve 효과 실행
        }

        // 서서히 사라지는 효과 코루틴
        private IEnumerator DissolveEffect()
        {
            while (Dissolve_value > 0)
            {
                Dissolve_value -= Time.deltaTime * 0.5f; // 천천히 사라지도록 조정
                foreach (var mesh in MeshR)
                {
                    mesh.material.SetFloat("_Dissolve", Dissolve_value);
                }
                yield return null;
            }
        }


/*
        private void HandlePush()
        {
            // V키를 눌렀을 때 밀기 시작
            if (Input.GetKeyDown(KeyCode.V) && currentRock != null)
            {
                isPushing = true;
                Debug.Log("Started pushing the rock");
            }

            // V키를 뗐을 때 밀기 중지
            if (Input.GetKeyUp(KeyCode.V))
            {
                isPushing = false;
                Debug.Log("Stopped pushing the rock");

                // 돌의 속도와 회전을 완전히 멈춤
                if (currentRock != null)
                {
                    Rigidbody rockRb = currentRock.GetComponent<Rigidbody>();
                    if (rockRb != null)
                    {
                        rockRb.velocity = Vector3.zero;
                        rockRb.angularVelocity = Vector3.zero;
                    }
                }
            }

            // 밀고 있는 상태일 때만 돌 이동
            if (isPushing && currentRock != null)
            {
                Rigidbody rockRb = currentRock.GetComponent<Rigidbody>();
                Vector3 pushDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;

                if (pushDirection.magnitude > 0.1f)
                {
                    // 돌을 입력 방향으로 이동
                    rockRb.velocity = pushDirection * pushSpeed;

                    // 돌을 항상 땅에 붙어 있도록 설정 (중력 보정)
                    Vector3 correctedPosition = currentRock.transform.position;
                    correctedPosition.y = Mathf.Max(correctedPosition.y, transform.position.y - 0.1f);
                    currentRock.transform.position = correctedPosition;
                }
                else
                {
                    // 입력이 없으면 속도를 멈춤
                    rockRb.velocity = Vector3.zero;
                }
            }
            else if (currentRock != null)
            {
                // V키가 눌리지 않았을 때 속도를 강제로 0으로 유지
                Rigidbody rockRb = currentRock.GetComponent<Rigidbody>();
                if (rockRb != null)
                {
                    rockRb.velocity = Vector3.zero;

                    // 중력 적용으로 돌이 땅에 붙어 있도록 강제
                    Vector3 correctedPosition = currentRock.transform.position;
                    correctedPosition.y = Mathf.Max(correctedPosition.y, transform.position.y - 0.1f);
                    currentRock.transform.position = correctedPosition;
                }
            }
        }*/


        private void ApplyGravity()
        {
            if (CheckGrounded())
            {
                if (!isJumping)
                {
                    verticalVelocity = -0.1f;
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    verticalVelocity = jumpForce;
                    isJumping = true;
                }
            }
            else
            {
                verticalVelocity += Physics.gravity.y * (verticalVelocity < 0 ? fallMultiplier : 1) * Time.deltaTime;
            }

            if (isJumping && verticalVelocity <= 0)
            {
                isJumping = false;
            }

            MoveDirection.y = verticalVelocity;
        }


        private bool CheckGrounded()
        {
            if (Ctrl.isGrounded)
            {
                return true;
            }

            // 바닥 감지를 위한 레이 시작 위치 및 길이 수정
            Vector3 rayOrigin = transform.position + Vector3.up * 0.1f; // 플레이어 위치에서 약간 위쪽으로 발사
            Vector3 rayDirection = Vector3.down; // 아래 방향으로 쏘기
            float range = isInWater ? 0.7f : 0.4f; // 물 속에서는 좀 더 넓은 범위 감지

            Debug.DrawRay(rayOrigin, rayDirection * range, Color.green); // 디버깅용 시각화

            // 트리거 충돌 무시하고 바닥 감지
            return Physics.Raycast(rayOrigin, rayDirection, range, ~0, QueryTriggerInteraction.Ignore);
        }


/*        private void Respawn()
        {
            if (Input.GetKeyDown(KeyCode.R)) // R키를 누르면 리스폰 가능
            {
                Debug.Log("Respawning player...");

                // 체력 50으로 회복
                HP = 50;
                UpdateHPUI(); // 체력 UI 업데이트

                // 초기 위치 및 방향 복원
                this.transform.position = initialPosition;
                this.transform.rotation = initialRotation;

                // 이동 및 피격 가능하도록 변경
                Ctrl.enabled = true;
                GetComponent<Collider>().enabled = true;

                // Dissolve 효과 초기화
                Dissolve_value = 1;
                foreach (var mesh in MeshR)
                {
                    mesh.material.SetFloat("_Dissolve", Dissolve_value);
                }

                // 애니메이션 초기화
                Anim.CrossFade(IdleState, 0.1f, 0, 0);

                isDead = false; // 다시 살아남
                Debug.Log("Player respawned at initial position with HP: " + HP);
            }
        }*/


        public bool IsDead()
        {
            return isDead;
        }



        public void Heal(int amount)
        {
            HP = Mathf.Min(HP + amount, maxHP);
            UpdateHPUI(); // 체력 슬라이더 업데이트
        }


        public int GetCurrentHP()
        {
            return HP;
        }

        private void UpdateHPUI()
        {
            if (hpSlider != null)
            {
                hpSlider.value = HP; // 슬라이더로 체력 업데이트

                // 체력 비율 계산 (0 ~ 1)
                float hpPercent = (float)HP / maxHP;

                // Fill 색상 업데이트
                Image fillImage = hpSlider.fillRect.GetComponent<Image>();
                if (fillImage != null)
                {
                    Color fillColor;

                    // 체력 구간에 따른 더 진한 파스텔톤 색상 그라데이션 적용
                    if (hpPercent > 0.7f)
                    {
                        // 더 진한 초록색 (#228B45) → 진한 옐로우 (#FFCC00)
                        fillColor = Color.Lerp(
                            new Color(1f, 0.8f, 0f), // 진한 옐로우
                            new Color(0.13f, 0.55f, 0.26f), // 더 진한 초록색
                            (hpPercent - 0.7f) / 0.3f
                        );
                    }

                    else if (hpPercent > 0.4f)
                    {
                        // 진한 옐로우 (#FFCC00) → 진한 오렌지 (#FF8C00)
                        fillColor = Color.Lerp(
                            new Color(1f, 0.55f, 0f), // 진한 오렌지
                            new Color(1f, 0.8f, 0f), // 진한 옐로우
                            (hpPercent - 0.4f) / 0.3f
                        );
                    }
                    else if (hpPercent > 0.1f)
                    {
                        // 진한 오렌지 (#FF8C00) → 진한 레드 (#CC3333)
                        fillColor = Color.Lerp(
                            new Color(0.8f, 0.2f, 0.2f), // 진한 레드
                            new Color(1f, 0.55f, 0f), // 진한 오렌지
                            (hpPercent - 0.1f) / 0.3f
                        );
                    }
                    else
                    {
                        // 진한 레드 (#CC3333) → 어두운 레드 (#800000)
                        fillColor = Color.Lerp(
                            new Color(0.5f, 0f, 0f), // 어두운 레드
                            new Color(0.8f, 0.2f, 0.2f), // 진한 레드
                            hpPercent / 0.1f
                        );
                    }

                    fillImage.color = fillColor;
                }
            }
        }








        private void OnTriggerEnter(Collider other)
        {
            // 물 상호작용
            if (other.CompareTag("Water"))
            {
                Debug.Log("Entered Water Trigger");
                if (!isInWater)
                {
                    isInWater = true;
                    Speed *= waterSpeedMultiplier;
                    sprintSpeed *= waterSpeedMultiplier;

                    // 물에 들어갈 때 소리 재생
                    PlaySound(enterWaterSound, enterWaterVolume);
                }
            }

            /*            // 돌 상호작용
                        if (other.CompareTag("PushableRock"))
                        {
                            currentRock = other.gameObject;
                            Debug.Log($"Entered range of rock: {currentRock.name}");
                        }*/
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Water"))
            {
                Debug.Log("Exited Water Trigger");
                if (isInWater)
                {
                    isInWater = false;
                    Speed /= waterSpeedMultiplier;
                    sprintSpeed /= waterSpeedMultiplier;

                    // 물에서 나올 때 소리 재생
                    PlaySound(exitWaterSound, exitWaterVolume);
                }
            }

            /*            // 돌 상호작용 해제
                        if (other.CompareTag("PushableRock") && currentRock == other.gameObject)
                        {
                            Debug.Log($"Exited range of rock: {currentRock.name}");
                            currentRock = null;
                            isPushing = false;
                        }*/
        }

        private void PlaySound(AudioClip clip, float volume)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip, volume); // 지정된 볼륨으로 소리 재생
            }
        }


        private void GameClear()
        {
            isDead = true;

            Debug.Log("Game Clear!"); // 게임 클리어 로그

            // GameClear 사운드 재생
            PlaySound(gameClearSound, gameClearVolume);

            // 이동 및 피격 불가 처리
            Ctrl.enabled = false;
            GetComponent<Collider>().enabled = false;

            // DayNightCycle의 텍스트 동기화 중지
            if (dayNightCycle != null)
            {
                dayNightCycle.StopDaySync();
            }

            // UI 비활성화
            if (uiCanvas != null)
            {
                foreach (Transform child in uiCanvas.transform)
                {
                    child.gameObject.SetActive(false); // 모든 UI 요소 비활성화
                }
            }

            // Dissolve 효과 완료 후 Success Fade Image 활성화 시작
            StartCoroutine(WaitBeforeDissolveAndShowSuccessImage());

            // 커서 락 해제 및 보이게 설정
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }


        // 1초 대기 후 Dissolve 효과 실행 및 Success Fade Image 활성화
        private IEnumerator WaitBeforeDissolveAndShowSuccessImage()
        {
            yield return new WaitForSeconds(1.0f); // 1초 대기
            yield return StartCoroutine(DissolveEffect()); // Dissolve 효과 실행
            yield return StartCoroutine(FadeInSuccessImage()); // Success Fade Image 페이드인

            yield return new WaitForSeconds(2.0f); // 페이드인 후 2초 유지
            yield return StartCoroutine(FadeOutSuccessDayText()); // Day 텍스트 서서히 사라지기

            yield return new WaitForSeconds(1.0f); // 텍스트 사라진 후 1초 대기
            LoadEndingScene(); // Ending Scene으로 전환
        }


        // 성공 페이드 이미지 서서히 나타나기
        private IEnumerator FadeInSuccessImage()
        {
            if (successFadeImage != null)
            {
                successFadeImage.SetActive(true); // 이미지 활성화

                CanvasGroup canvasGroup = successFadeImage.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                    canvasGroup.alpha = 0f; // 시작 시 투명

                    float duration = 1f; // 1초 동안 페이드인
                    float elapsedTime = 0f;

                    while (elapsedTime < duration)
                    {
                        elapsedTime += Time.deltaTime;
                        canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration); // 서서히 투명도 증가
                        yield return null;
                    }

                    canvasGroup.alpha = 1f; // 완전하게 보이도록 설정
                }
            }
        }

        // Day 텍스트 서서히 사라지기
        private IEnumerator FadeOutSuccessDayText()
        {
            if (successDayText != null)
            {
                float duration = 1f; // 1초 동안 페이드 아웃
                float elapsedTime = 0f;

                Color textColor = successDayText.color;
                textColor.a = 1f; // 시작 시 완전히 보이게 설정

                while (elapsedTime < duration)
                {
                    elapsedTime += Time.deltaTime;
                    textColor.a = Mathf.Lerp(1f, 0f, elapsedTime / duration); // 알파값 점진적으로 감소
                    successDayText.color = textColor;
                    yield return null;
                }

                textColor.a = 0f; // 완전히 사라지도록 설정
                successDayText.color = textColor;
            }
        }

        private void LoadEndingScene()
        {
            SceneManager.LoadScene("Ending Scene"); // EndingScene으로 씬 전환
        }


    }
}
