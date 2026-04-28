using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class SmartFollower : MonoBehaviour
{
    [Header("Optimization")]
    [SerializeField] private float minUpdateInterval = 3f;
    [SerializeField] private float maxUpdateInterval = 5f;
    [SerializeField] private float emergencyDistance = 10f; 

    private float pathUpdateTimer;

    [Header("Refs")]
    public Transform player;
    [SerializeField] private Transform orderTarget; 
    [SerializeField] private GameObject mark;

    [Header("Follow Settings")]
    [SerializeField] private float followRadius = 3f;
    [SerializeField] private float moveSpeed = 4f;

    [Header("Orders")]
    [SerializeField] private List<string> orderList = new List<string> { "Пицца", "Кофе", "Посылка" };
    public string currentOrderName = "";

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
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        mark.SetActive(false);// Изначально спрятать маркеры заказов .Тест пот убрать когда будут сохранения
        if (agent != null)
            agent.speed = moveSpeed;

        lastPlayerPos = player.position;
        // Сразу задаем первый интервал
        pathUpdateTimer = Random.Range(minUpdateInterval, maxUpdateInterval);
    }

    void Update()
    {
        // Кнопка U для заказа
        if (Input.GetKeyDown(KeyCode.U) && (currentState == State.Follow || currentState == State.Idle))
        {
            AcceptOrder();
        }

        switch (currentState)
        {
            case State.Idle: HandleIdle(); break;
            case State.Follow: HandleFollow(); break;
            case State.GoToOrder: HandleOrderLogic(); break;
            case State.ReturnWithOrder: HandleReturnLogic(); break;
            case State.HasOrder: HandleHasOrder(); break;
        }

        UpdateAnimations();
    }

    // ---------------- СИСТЕМА ЗАКАЗОВ ----------------

    void AcceptOrder()
    {
        if (orderList.Count > 0 && orderTarget != null)
        {
            currentOrderName = orderList[0];
            orderList.RemoveAt(0);

            Debug.Log($"Принял заказ: {currentOrderName}. Выдвигаюсь!");
            currentState = State.GoToOrder;
            agent.isStopped = false;
            agent.SetDestination(orderTarget.position);
        }
    }

    void HandleOrderLogic()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Debug.Log($"Забрал {currentOrderName}! Возвращаюсь.");
            currentState = State.ReturnWithOrder;
        }
    }

    void HandleReturnLogic()
    {
        mark.SetActive(true); // Показываем маркеры заказов, когда возвращаемся к игроку
        agent.isStopped = false;
        agent.SetDestination(player.position);

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

    // ---------------- УМНОЕ ПРЕСЛЕДОВАНИЕ ----------------

    void HandleFollow()
    {
        // Считаем вектор движения игрока
        Vector3 playerMovement = player.position - lastPlayerPos;
        float playerSpeed = playerMovement.magnitude / Time.deltaTime;
        Vector3 playerDir = playerMovement.normalized;
        lastPlayerPos = player.position;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Обновляем таймер
        pathUpdateTimer -= Time.deltaTime;

        // 1. Проверка на "столкновение" (делаем каждый кадр для плавности)
        // Если бот слишком близко, а игрок почти стоит - стоп
        if (distanceToPlayer < (followRadius * 0.5f) && playerSpeed < 0.5f)
        {
            HandleIdle();
            return;
        }

        // 2. Логика обновления пути по таймеру
        bool timeToUpdate = pathUpdateTimer <= 0f;
        bool playerTooFar = distanceToPlayer > emergencyDistance;

        if (timeToUpdate || playerTooFar)
        {
            pathUpdateTimer = Random.Range(minUpdateInterval, maxUpdateInterval);

            Vector3 targetDestination = player.position;

            // Если игрок идет, целимся ему "на ход"
            if (playerSpeed > 0.8f)
            {
                Vector3 futurePoint = player.position + (playerDir * followRadius);
                if (NavMesh.SamplePosition(futurePoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                {
                    targetDestination = hit.position;
                }
            }

            if (agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(targetDestination);
            }
        }
        
        // Если мы в движении, но уже вошли в радиус и игрок не убегает - притормаживаем
        if (distanceToPlayer <= followRadius && playerSpeed < 0.2f)
        {
            HandleIdle();
        }
    }

    // ---------------- ФИЗИКА И ВИЗУАЛ ----------------

    void HandleIdle()
    {
        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath(); // Сбрасываем путь, чтобы он не пытался "додавить" до точки
        }
        LookAtPlayer();
    }

    void LookAtPlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;
        
        if (dir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    void UpdateAnimations()
    {
        if (animator != null)
        {
            // Используем agent.velocity.magnitude для плавного перехода в Blend Tree
            float speed = agent.velocity.magnitude;
            animator.SetFloat(animSpeedHash, speed, 0.1f, Time.deltaTime);
        }
    }
}