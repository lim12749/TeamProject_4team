using UnityEngine;

public class SimpleFollow : MonoBehaviour
{
    public enum State { Idle, Walk, Jump, Attack, Damage }
    public State currentState = State.Idle;

    [Header("Face (표정)")]
    public Face faces;

    [Header("플레이어 추적 설정")]
    public Transform player;
    public float chaseDistance = 100f;
    public float attackDistance = 2f;
    public float jumpMoveSpeed = 6f;

    [Header("순찰 설정")]
    public float walkSpeed = 1.5f;
    public float patrolRadius = 5f;
    public float idleTime = 2f;

    [Header("HP 설정")]
    public int maxHP = 50;
    private int currentHP;

    [Header("Damage 애니메이션 설정")]
    public int damType; // 0 = 죽음, 1 = 맞는 애니메이션

    private Vector3 origin;
    private Vector3 targetPatrolPoint;
    private float idleTimer;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        currentHP = maxHP;
        origin = transform.position;

        if (player == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("Player");
            if (obj != null) player = obj.transform;
        }

        SetNewPatrolPoint();
    }

    void Update()
    {
        CheckPlayerDistance();

        switch (currentState)
        {
            case State.Idle: UpdateIdle(); break;
            case State.Walk: UpdateWalk(); break;
            case State.Jump: UpdateJump(); break;
            case State.Attack: UpdateAttack(); break;
            case State.Damage: UpdateDamage(); break;
        }
    }

    void ChangeState(State newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        idleTimer = 0f;

        switch (newState)
        {
            case State.Idle:
                SetFace(faces.Idleface);
                animator.SetFloat("Speed", 0);
                break;
            case State.Walk:
                SetFace(faces.WalkFace);
                animator.SetFloat("Speed", walkSpeed);
                break;
            case State.Jump:
                SetFace(faces.jumpFace);
                animator.SetTrigger("Jump");
                break;
            case State.Attack:
                SetFace(faces.attackFace);
                animator.SetTrigger("Attack");
                break;
            case State.Damage:
                SetFace(faces.damageFace);
                animator.SetTrigger("Damage");
                animator.SetInteger("DamageType", damType);
                break;
        }
    }

    public void TakeDamage(int amount)
    {
        if (currentState == State.Damage && damType == 0) return; // 이미 죽는 중이면 무시

        currentHP -= amount;

        if (currentHP <= 0)
        {
            currentHP = 0;
            damType = 0; // 죽는 애니메이션
            ChangeState(State.Damage);
            return;
        }

        damType = 1; // 맞는 애니메이션
        ChangeState(State.Damage);
    }

    void CheckPlayerDistance()
    {
        if (currentState == State.Damage && damType == 0) return; // 죽는 중이면 무시

        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= attackDistance)
            ChangeState(State.Attack);
        else if (dist <= chaseDistance)
            ChangeState(State.Jump);
        else
            ChangeState(State.Walk);
    }

    void UpdateIdle()
    {
        idleTimer += Time.deltaTime;
        if (idleTimer >= idleTime)
        {
            idleTimer = 0f;
            SetNewPatrolPoint();
            ChangeState(State.Walk);
        }
    }

    void UpdateWalk()
    {
        Vector3 dir = targetPatrolPoint - transform.position;
        if (dir != Vector3.zero)
        {
            transform.position += dir.normalized * walkSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir.normalized),
                3f * Time.deltaTime
            );
        }

        if (dir.magnitude < 0.5f)
            ChangeState(State.Idle);
    }

    void UpdateJump()
    {
        if (player == null) return;

        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * jumpMoveSpeed * Time.deltaTime;

        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(dir),
                8f * Time.deltaTime
            );
    }

    void UpdateAttack()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        // 공격 거리보다 멀면 조금 더 다가가기
        if (dist > attackDistance * 0.5f)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * 3f * Time.deltaTime; // 기존 1.5f → 3f
        }

        // 플레이어를 바라보기
        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(lookDir),
                8f * Time.deltaTime
            );
    }


    void UpdateDamage() { }

    void SetNewPatrolPoint()
    {
        Vector2 r = Random.insideUnitCircle * patrolRadius;
        targetPatrolPoint = new Vector3(origin.x + r.x, origin.y, origin.z + r.y);
    }

    void SetFace(Texture tex)
    {
        // 표정 기능 제거 (선택사항)
    }

    // 애니메이션 이벤트
    public void AlertObservers(string msg)
    {
        if (msg == "AnimationAttackEnded" || msg == "AnimationJumpEnded")
        {
            ChangeState(State.Idle);
        }

        if (msg == "AnimationDamageEnded")
        {
            if (damType == 0) // 죽음 애니 끝
            {
                // ✅ 킬 수 증가
                if (GameManager.Instance != null)
                    GameManager.Instance.AddKill();

                Destroy(gameObject);
                return;
            }


            float dist = Vector3.Distance(transform.position, origin);
            if (dist > 1f)
            {
                SetNewPatrolPoint();
                ChangeState(State.Walk);
            }
            else
            {
                ChangeState(State.Idle);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            PlayerHealth hp = collision.collider.GetComponent<PlayerHealth>();
            if (hp != null)
            {
                hp.TakeDamage(10);  // 플레이어에게 10 데미지
            }
        }
    }

    public void AnimationAttackHit()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= attackDistance + 0.5f)
        {
            PlayerHealth hp = player.GetComponent<PlayerHealth>();
            if (hp != null)
                hp.TakeDamage(10);
        }
    }


}
