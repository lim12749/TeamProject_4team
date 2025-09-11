using UnityEngine;


    [RequireComponent(typeof(CharacterController))]
    public class SimpleMover : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] Transform cam;       // ī�޶� ���� �̵���(������ ���� ����)
        [SerializeField] InputReader input;   // ����: ������ WASD+Shift ���

        [Header("Move")]
        [SerializeField] bool cameraRelative = true;
        [SerializeField] float walkSpeed = 3.5f;
        [SerializeField] float runSpeed = 6f;
        [SerializeField] float turnSpeed = 720f;
        [SerializeField] bool faceMoveDir = true;
    public Animator animator;    
        // �ִϸ��̼� ��(����)
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
            // 1) �Է�
            Vector2 mv; bool sprint;
            if (input) { mv = input.Move; sprint = input.SprintHeld; }
            else
            {
                mv = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            }
            IsMoving = mv.sqrMagnitude > 0.0001f;

            // 2) ���� ��(ī�޶� or ����)
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

            // 3) ����/�ӵ�
            Vector3 moveDir = (f * mv.y + r * mv.x);
            if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();
            float targetSpeed = IsMoving ? (sprint ? runSpeed : walkSpeed) : 0f;

            // 4) �̵� (�߷� �ڵ� ����)
            cc.SimpleMove(moveDir * targetSpeed);
            animator.SetFloat("Speed", targetSpeed);

            // 5) ȸ��(�̵� ���� �ٶ󺸱�)
            if (faceMoveDir && IsMoving)
            {
                Quaternion look = Quaternion.LookRotation(moveDir, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, look, turnSpeed * Time.deltaTime);
            }

            // 6) �ִ� �ӵ�(0~1)
            Speed01 = Mathf.MoveTowards(Speed01, Mathf.InverseLerp(0f, runSpeed, targetSpeed), 8f * Time.deltaTime);
        }
    }

