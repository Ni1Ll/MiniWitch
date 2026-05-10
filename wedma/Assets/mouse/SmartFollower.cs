using UnityEngine;
using UnityEngine.AI;

public class SmartFollower : MonoBehaviour
{
    [Header("Optimization")]
    [SerializeField] private float minUpdateInterval = 3f;
    [SerializeField] private float maxUpdateInterval = 5f;
    [SerializeField] private float emergencyDistance = 10f;

    private float pathUpdateTimer;
    private Marker3d mark;

    [Header("Refs")]
    public Transform player;
    public Transform orderTarget;

    [Header("Follow Settings")]
    [SerializeField] private float followRadius = 3f;
    [SerializeField] private float moveSpeed = 4f;

    [Header("Rotation Settings")]
    [SerializeField] private float lookRotationSpeed = 18f;
    [SerializeField] private float agentAngularSpeed = 720f;
    [SerializeField] private float agentAcceleration = 20f;

    [Header("Orders")]
    public string currentOrderName = "";

    [Header("Marker")]
    [SerializeField] private int orderMarkerIndex = 0;

    private NavMeshAgent agent;
    private Animator animator;
    private readonly int animSpeedHash = Animator.StringToHash("Speed");

    private Vector3 lastPlayerPos;

    public enum State
    {
        Idle,
        Follow,
        GoToOrder,
        ReturnWithOrder,
        HasOrder
    }

    [SerializeField] private State currentState = State.Follow;

    void Start()
    {
        mark = GetComponent<Marker3d>();

        if (mark != null)
            mark.DisableMarker(orderMarkerIndex);

        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.angularSpeed = agentAngularSpeed;
            agent.acceleration = agentAcceleration;
        }

        if (player != null)
            lastPlayerPos = player.position;

        pathUpdateTimer = Random.Range(minUpdateInterval, maxUpdateInterval);
    }

    void Update()
    {
        if (player == null || agent == null)
            return;

        switch (currentState)
        {
            case State.Idle:
                HandleIdle();
                break;

            case State.Follow:
                HandleFollow();
                break;

            case State.GoToOrder:
                HandleOrderLogic();
                break;

            case State.ReturnWithOrder:
                HandleReturnLogic();
                break;

            case State.HasOrder:
                HandleHasOrder();
                break;
        }

        UpdateAnimations();
    }

    // ---------------- СИСТЕМА ЗАКАЗОВ ДЛЯ NPC ----------------

    public void AcceptOrder()
    {
        if (orderTarget == null)
        {
            Debug.LogWarning("[SmartFollower] Order Target не назначен.");
            return;
        }

        currentOrderName = "Новый заказ";

        Debug.Log($"Принял заказ: {currentOrderName}. Выдвигаюсь!");

        currentState = State.GoToOrder;

        if (mark != null)
            mark.DisableMarker(orderMarkerIndex);

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.ResetPath();
            agent.SetDestination(orderTarget.position);
        }
    }

    void HandleOrderLogic()
    {
        if (agent == null || !agent.isOnNavMesh)
            return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Debug.Log($"Забрал {currentOrderName}! Возвращаюсь.");
            currentState = State.ReturnWithOrder;
        }
    }

    void HandleReturnLogic()
    {
        // Маркер включается именно когда курьер уже возвращается с заказом.
        if (mark != null)
            mark.EnableMarker(orderMarkerIndex);

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }

        if (Vector3.Distance(transform.position, player.position) <= followRadius)
        {
            Debug.Log($"Заказ '{currentOrderName}' у меня!");
            currentState = State.HasOrder;
        }
    }

    void HandleHasOrder()
    {
        HandleFollow();
    }

    public void HideOrderMarker()
    {
        if (mark != null)
            mark.DisableMarker(orderMarkerIndex);
    }

    // ---------------- УМНОЕ ПРЕСЛЕДОВАНИЕ ----------------

    void HandleFollow()
    {
        Vector3 playerMovement = player.position - lastPlayerPos;

        float playerSpeed = 0f;
        if (Time.deltaTime > 0f)
            playerSpeed = playerMovement.magnitude / Time.deltaTime;

        Vector3 playerDir = playerMovement.sqrMagnitude > 0.001f
            ? playerMovement.normalized
            : transform.forward;

        lastPlayerPos = player.position;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        pathUpdateTimer -= Time.deltaTime;

        if (distanceToPlayer < followRadius * 0.5f && playerSpeed < 0.5f)
        {
            HandleIdle();
            return;
        }

        bool timeToUpdate = pathUpdateTimer <= 0f;
        bool playerTooFar = distanceToPlayer > emergencyDistance;

        if (timeToUpdate || playerTooFar)
        {
            pathUpdateTimer = Random.Range(minUpdateInterval, maxUpdateInterval);

            Vector3 targetDestination = player.position;

            if (playerSpeed > 0.8f)
            {
                Vector3 futurePoint = player.position + playerDir * followRadius;

                if (NavMesh.SamplePosition(futurePoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                    targetDestination = hit.position;
            }

            if (agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(targetDestination);
            }
        }

        if (distanceToPlayer <= followRadius && playerSpeed < 0.2f)
            HandleIdle();
    }

    // ---------------- ФИЗИКА И ВИЗУАЛ ----------------

    void HandleIdle()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        LookAtPlayer();
    }

    void LookAtPlayer()
    {
        if (player == null)
            return;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir.normalized);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * lookRotationSpeed
            );
        }
    }

    void UpdateAnimations()
    {
        if (animator == null || agent == null)
            return;

        float speed = agent.velocity.magnitude;
        animator.SetFloat(animSpeedHash, speed, 0.1f, Time.deltaTime);
    }
}