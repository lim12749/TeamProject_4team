using UnityEngine;


    [RequireComponent(typeof(CharacterController))]
    public class SimpleMover : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] Transform cam;       // 카메라 기준 이동용(없으면 월드 기준)
        [SerializeField] InputReader input;   // 선택: 없으면 WASD+Shift 사용

        [Header("Move")]
        [SerializeField] bool cameraRelative = true;
        [SerializeField] float walkSpeed = 3.5f;
        [SerializeField] float runSpeed = 6f;
        [SerializeField] float turnSpeed = 720f;
        [SerializeField] bool faceMoveDir = true;
    public Animator animator;    
        // 애니메이션 훅(선택)
        public float Speed01 { get; private set; }
        public bool IsMoving { get; private set; }

        CharacterController cc;

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            if (!cam && Camera.main) cam = Camera.main.transform;
            if (!input) input = GetComponentInParent<InputReader>();
        }

        void Update()
        {
            // 1) 입력
            Vector2 mv; bool sprint;
            if (input) { mv = input.Move; sprint = input.SprintHeld; }
            else
            {
                mv = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            }
            IsMoving = mv.sqrMagnitude > 0.0001f;

            // 2) 기준 축(카메라 or 월드)
            Vector3 f, r;
            if (cameraRelative && cam)
            {
                f = cam.forward; f.y = 0f; f.Normalize();
                r = cam.right; r.y = 0f; r.Normalize();
            }
            else
            {
                f = Vector3.forward; r = Vector3.right;
            }

            // 3) 방향/속도
            Vector3 moveDir = (f * mv.y + r * mv.x);
            if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();
            float targetSpeed = IsMoving ? (sprint ? runSpeed : walkSpeed) : 0f;

            // 4) 이동 (중력 자동 적용)
            cc.SimpleMove(moveDir * targetSpeed);
            animator.SetFloat("Speed", targetSpeed);

            // 5) 회전(이동 방향 바라보기)
            if (faceMoveDir && IsMoving)
            {
                Quaternion look = Quaternion.LookRotation(moveDir, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, look, turnSpeed * Time.deltaTime);
            }

            // 6) 애니 속도(0~1)
            Speed01 = Mathf.MoveTowards(Speed01, Mathf.InverseLerp(0f, runSpeed, targetSpeed), 8f * Time.deltaTime);
        }
    }

