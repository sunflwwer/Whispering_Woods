using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace Sample
{
    public class StartGhostScript : MonoBehaviour
    {
        private Animator Anim;
        private CharacterController Ctrl;
        private Vector3 MoveDirection = Vector3.zero;
        private Vector3 initialPosition;
        private Quaternion initialRotation;

        private static readonly int IdleState = Animator.StringToHash("Base Layer.idle");
        private static readonly int MoveState = Animator.StringToHash("Base Layer.move");
        private static readonly int AttackState = Animator.StringToHash("Base Layer.attack_shift");

        [SerializeField] private float Speed = 3;
        [SerializeField] private float sprintSpeed = 5.5f;

        [SerializeField] private float mouseSensitivity = 100f;

        private bool isJumping = false;
        [SerializeField] private float jumpForce = 4.5f;
        [SerializeField] private float fallMultiplier = 2.0f;
        private float verticalVelocity = 0f;

        private bool isMoving = false;

        [SerializeField] private float waterSpeedMultiplier = 0.5f;
        private bool isInWater = false;

        [SerializeField] private Camera playerCamera; // 사용자가 지정한 카메라를 연결

        void Start()
        {
            Anim = this.GetComponent<Animator>();
            Ctrl = this.GetComponent<CharacterController>();

            initialPosition = this.transform.position;
            initialRotation = this.transform.rotation;

            //Cursor.lockState = CursorLockMode.Locked;
        }

        void Update()
        {
            MOVE();
            //HandleKeyActions();
        }

        void FixedUpdate()
        {
            CheckGrounded();
        }

        private void MOVE()
        {
            // 마우스 클릭 중이고 UI 요소 위에 있을 때만 이동 중지
            if (Input.GetMouseButton(0) && EventSystem.current.IsPointerOverGameObject()) return;

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

                Vector3 moveDirection = playerCamera.transform.forward * inputDirection.z +
                                        playerCamera.transform.right * inputDirection.x;
                moveDirection.y = 0f;

                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

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



        /*        private void HandleKeyActions()
                {
                    if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                    {
                        Anim.CrossFade(AttackState, 0.1f, 0, 0);
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

            Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
            Vector3 rayDirection = Vector3.down;
            float range = isInWater ? 0.7f : 0.4f;

            Debug.DrawRay(rayOrigin, rayDirection * range, Color.green);

            return Physics.Raycast(rayOrigin, rayDirection, range, ~0, QueryTriggerInteraction.Ignore);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Water"))
            {
                if (!isInWater)
                {
                    isInWater = true;
                    Speed *= waterSpeedMultiplier;
                    sprintSpeed *= waterSpeedMultiplier;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Water"))
            {
                if (isInWater)
                {
                    isInWater = false;
                    Speed /= waterSpeedMultiplier;
                    sprintSpeed /= waterSpeedMultiplier;
                }
            }
        }
    }
}
