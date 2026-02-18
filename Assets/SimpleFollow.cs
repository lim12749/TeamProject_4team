using UnityEngine;
using UnityEngine.AI;

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

    // ✅ NavMesh
    private NavMeshAgent agent;
    private bool hasAgent;

    void Start()
    {
        animator = GetComponent<Animator>();

        // NavMeshAgent 가져오기(이미 붙어있다고 했으니 Add 안 함)
        agent = GetComponent<NavMeshAgent>();
        hasAgent = (agent != null);

        if (hasAgent)
        {
            agent.updateRotation = true;
            agent.updateUpAxis = true;
        }

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

    // ✅ agent가 실제로 NavMesh 위에 있을 때만 제어
    bool AgentReady()
    {
        return hasAgent && agent != null && agent.isOnNavMesh && agent.enabled;
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
                if (AgentReady()) agent.isStopped = true;
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
                if (AgentReady()) agent.isStopped = true;
                break;
        }
    }

    public void TakeDamage(int amount)
    {
        if (currentState == State.Damage && damType == 0) return;

        currentHP -= amount;

        if (currentHP <= 0)
        {
            currentHP = 0;
            damType = 0;
            ChangeState(State.Damage);
            return;
        }

        damType = 1;
        ChangeState(State.Damage);
    }

    void CheckPlayerDistance()
    {
        if (currentState == State.Damage && damType == 0) return;
        if (player == null) return;

        // ✅ 거리 판단은 수평 기준(사다리/박스 꼼수 완화)
        float dist = FlatDistToPlayer();

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
        // ✅ NavMesh 이동
        if (AgentReady())
        {
            agent.isStopped = false;
            agent.speed = walkSpeed;
            agent.stoppingDistance = 0.1f;
            agent.SetDestination(targetPatrolPoint);

            bool arrived = !agent.pathPending && agent.remainingDistance <= 0.5f;
            if (arrived)
                ChangeState(State.Idle);

            return;
        }

        // (백업) NavMesh 못 쓰는 상황이면 기존 이동 유지
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

        // ✅ NavMesh로 추적 이동
        if (AgentReady())
        {
            agent.isStopped = false;
            agent.speed = jumpMoveSpeed;
            agent.stoppingDistance = attackDistance * 0.9f;
            agent.SetDestination(player.position);
            return;
        }

        // (백업) 기존 이동
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

        // ✅ 공격 중 접근/멈춤 판단도 수평거리로(높이 차이 때문에 헛도는 것 완화)
        float dist = FlatDistToPlayer();

        if (dist > attackDistance * 0.5f)
        {
            if (AgentReady())
            {
                agent.isStopped = false;
                agent.speed = 3f;
                agent.stoppingDistance = attackDistance * 0.5f;
                agent.SetDestination(player.position);
            }
            else
            {
                Vector3 dir = (player.position - transform.position).normalized;
                transform.position += dir * 3f * Time.deltaTime;
            }
        }
        else
        {
            if (AgentReady()) agent.isStopped = true;
        }

        // 바라보기(기존 유지)
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

    // ✅ 애니메이션 이벤트
    public void AlertObservers(string msg)
    {
        if (msg == "AnimationAttackEnded" || msg == "AnimationJumpEnded")
        {
            ChangeState(State.Idle);
        }

        if (msg == "AnimationDamageEnded")
        {
            if (damType == 0)
            {
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
                hp.TakeDamage(10);
        }
    }

    public void AnimationAttackHit()
    {
        if (player == null) return;

        // ✅ 공격 판정도 수평거리로(사다리/박스 위여도 “수평으로 가까우면” 맞게)
        float dist = FlatDistToPlayer();
        if (dist <= attackDistance + 0.5f)
        {
            PlayerHealth hp = player.GetComponent<PlayerHealth>();
            if (hp != null)
                hp.TakeDamage(10);
        }
    }

    // ✅ 수평거리 계산 함수
    float FlatDistToPlayer()
    {
        if (player == null) return Mathf.Infinity;
        Vector3 a = transform.position;
        Vector3 b = player.position;
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }
}
