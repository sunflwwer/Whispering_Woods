using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
        private const int maxHP = 100;
        private int HP = 50;
        private TextMeshProUGUI HP_text;

        [SerializeField] private float Speed = 3;
        [SerializeField] private float sprintSpeed = 5.5f;

        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Transform playerBody;
        [SerializeField] private float mouseSensitivity = 100f;

        private float xRotation = 0f;

        private bool isJumping = false;
        [SerializeField] private float jumpForce = 4.5f;
        [SerializeField] private float fallMultiplier = 2.0f;
        private float verticalVelocity = 0f;

        private bool isMoving = false;

        [SerializeField] private float waterSpeedMultiplier = 0.5f;
        private bool isInWater = false;

        // [SerializeField] private float waterHeight = 0f;

        // 추가된 변수
        private GameObject currentRock = null; // 현재 밀 수 있는 돌
        private bool isPushing = false; // 돌 밀기 상태
        [SerializeField] private float pushSpeed = 3f; // 돌 밀기 속도

        private bool isDead = false; // 플레이어가 죽었는지 여부


        void Start()
        {
            Anim = this.GetComponent<Animator>();
            Ctrl = this.GetComponent<CharacterController>();

            initialPosition = this.transform.position;
            initialRotation = this.transform.rotation;

            try
            {
                HP_text = GameObject.Find("Canvas/HP").GetComponent<TextMeshProUGUI>();
                if (HP_text != null)
                {
                    HP_text.text = "HP " + HP.ToString();
                }
                else
                {
                    Debug.LogError("HP_text is null. Please check the Canvas/HP setup.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error finding HP text: " + e.Message);
            }

            if (cameraTransform == null)
            {
                cameraTransform = Camera.main.transform;
            }

            Cursor.lockState = CursorLockMode.Locked;
        }

        void Update()
        {
            HandleMouseLook();
            STATUS();
            MOVE();
            HandleKeyActions();
            HandlePush(); // 추가된 돌 밀기 로직
            Respawn();

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

        private void HandleMouseLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            playerBody.Rotate(Vector3.up * mouseX);

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 60f);

            Vector3 targetCameraPosition = cameraTransform.localPosition;
            float verticalOffset = Mathf.Lerp(0.5f, 4.0f, Mathf.InverseLerp(-90f, 60f, xRotation));
            targetCameraPosition.y = verticalOffset;
            targetCameraPosition.z = -3f;

            cameraTransform.localPosition = targetCameraPosition;
            cameraTransform.LookAt(playerBody.position + Vector3.up * 1.5f);
        }

        private void MOVE()
        {
            if (isDead) return; // 플레이어가 죽었으면 이동 로직 실행 안 함

            ApplyGravity();

            float currentSpeed = Speed;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                currentSpeed = sprintSpeed;
            }

            Vector3 inputDirection = new Vector3(
                Input.GetAxis("Horizontal"),
                0,
                Input.GetAxis("Vertical")
            );

            if (inputDirection.magnitude > 0.1f)
            {
                inputDirection.Normalize();

                Vector3 moveDirection = playerBody.forward * inputDirection.z + playerBody.right * inputDirection.x;
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

                Vector3 rayStartPos = cameraTransform.position + Vector3.up * 0.2f; // 기존보다 0.2f 위에서 시작


                // Ray 방향 (살짝 아래로 조정)
                Vector3 rayDirection = cameraTransform.forward + Vector3.down * 0.3f;
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
            Vector3 rayStartPos = cameraTransform.position; // 카메라 위치에서 시작
            Vector3 rayDirection = cameraTransform.forward; // 플레이어가 바라보는 방향

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
        }





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
                if (verticalVelocity < 0)
                {
                    verticalVelocity += Physics.gravity.y * fallMultiplier * Time.deltaTime;
                }
                else
                {
                    verticalVelocity += Physics.gravity.y * Time.deltaTime;
                }
            }

            if (isJumping && verticalVelocity <= 0 && CheckGrounded())
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

            Ray ray = new Ray(this.transform.position + Vector3.up * 0.1f, Vector3.down);
            float range = isInWater ? 0.5f : 0.3f; // 물에서는 더 넓은 범위로 감지
            Debug.DrawRay(ray.origin, ray.direction * range, Color.red);

            // Trigger Collider를 무시하도록 QueryTriggerInteraction.Ignore 설정
            return Physics.Raycast(ray, range, ~0, QueryTriggerInteraction.Ignore);
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

            if (HP_text != null)
            {
                HP_text.text = "HP " + HP.ToString();
            }
            else
            {
                Debug.LogError("HP_text is null. Cannot update HP text.");
            }
        }

        public int GetCurrentHP()
        {
            return HP;
        }

        private void UpdateHPUI()
        {
            if (HP_text != null)
            {
                HP_text.text = "HP " + HP.ToString();
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

            // 돌 상호작용
            if (other.CompareTag("PushableRock"))
            {
                currentRock = other.gameObject;
                Debug.Log($"Entered range of rock: {currentRock.name}");
            }
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

            // 돌 상호작용 해제
            if (other.CompareTag("PushableRock") && currentRock == other.gameObject)
            {
                Debug.Log($"Exited range of rock: {currentRock.name}");
                currentRock = null;
                isPushing = false;
            }
        }

    }
}
