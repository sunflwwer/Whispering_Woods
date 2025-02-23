using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // 추가

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



        void Start()
        {
            Anim = this.GetComponent<Animator>();
            Ctrl = this.GetComponent<CharacterController>();

            initialPosition = this.transform.position;
            initialRotation = this.transform.rotation;

            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            if (hpSlider != null)
            {
                hpSlider.maxValue = maxHP;
                hpSlider.value = HP; // 시작 시 체력 설정
            }

            Cursor.lockState = CursorLockMode.Locked;
        }



        void Update()
        {

            STATUS();
            MOVE();
            HandleKeyActions();
            //HandlePush(); // 추가된 돌 밀기 로직
            Respawn();
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
                Anim.CrossFade(AttackState, 0.1f, 0, 0);

                // 플레이어 위치에서 시작
                Vector3 rayStartPos = transform.position + Vector3.up * 1.0f; // 플레이어 위치에서 조금 위에서 시작 (1.0f 정도)

                // 플레이어 기준 정면 방향으로 발사
                Vector3 rayDirection = transform.forward; // 플레이어 기준 정면 방향

                rayDirection.Normalize(); // 정규화

                // Ray 거리
                float rayDistance = 6.0f;

                // Debug용 Ray 시각화
                Debug.DrawRay(rayStartPos, rayDirection * rayDistance, Color.red, 1.0f);

                // RaycastAll 사용 → 트리거까지 감지 가능
                RaycastHit[] hits = Physics.RaycastAll(rayStartPos, rayDirection, rayDistance);

                // 가장 가까운 거미 한 마리만 공격하기 위해 변수 추가
                RaycastHit? closestHit = null;
                float closestDistance = float.MaxValue;

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

                // 가장 가까운 거미 한 마리만 공격
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

            if (Input.GetKeyDown(KeyCode.Mouse1)) // 마우스 오른쪽 버튼으로 문 회전
            {
                InteractWithFence();
                Debug.Log("InteractWithFence.");
            }

        }

        private void InteractWithFence()
        {
            float interactionDistance = 7.0f; // 문과의 상호작용 거리
                                              // 플레이어 위치에서 시작
            Vector3 rayStartPos = transform.position + Vector3.up * 1.0f; // 플레이어 위치에서 조금 위에서 시작 (1.0f 정도)

            // 플레이어 기준 정면 방향으로 발사
            Vector3 rayDirection = transform.forward; // 플레이어 기준 정면 방향


            Debug.DrawRay(rayStartPos, rayDirection * interactionDistance, Color.blue, 2.0f); // Ray를 시각적으로 확인

            RaycastHit[] hits = Physics.RaycastAll(rayStartPos, rayDirection, interactionDistance, ~0, QueryTriggerInteraction.Collide);

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Fence")) // Fence 태그 감지
                {
                    Debug.Log("Fence detected, toggling doors...");
                    DoorScript.ToggleAllDoors(); // 모든 문 동시에 열거나 닫기
                    return;
                }
            }
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

            // 이동 및 피격 불가 처리
            Ctrl.enabled = false;
            GetComponent<Collider>().enabled = false;

            // 1초 후 Dissolve 효과 실행
            StartCoroutine(WaitBeforeDissolve());
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


        private void Respawn()
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
        }


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
            // 물 상호작용 해제
            if (other.CompareTag("Water"))
            {
                Debug.Log("Exited Water Trigger");
                if (isInWater)
                {
                    isInWater = false;
                    Speed /= waterSpeedMultiplier;
                    sprintSpeed /= waterSpeedMultiplier;
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

    }
}