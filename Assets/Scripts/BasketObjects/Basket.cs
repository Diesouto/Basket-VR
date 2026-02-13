using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Basket : MonoBehaviour
{
    enum BasketEventType
    {
        SizeIncrease,
        Movement,
        Teleportation,
        Shake,
        None
    }

    [Header("Basket Settings")]
    [SerializeField] float maxSizeIncreaseAmount = 0.5f;        // Mayor escalado posible de la canasta
    [SerializeField] float maxMoveSpeedIncreaseAmount = 2f;   // Mayor aumento de velocidad de movimiento posible de la canasta
    [SerializeField] float minEventsDuration = 5f;              // Duración mínima de los eventos posibles de la canasta
    [SerializeField] float maxEventsDuration = 10f;             // Duración máxima de los eventos posibles de la canasta
    [SerializeField] float minEventCooldownTime = 5f;          // Duración mínima entre eventos posibles de la canasta
    [SerializeField] float maxEventCooldownTime = 10f;         // Duración máxima entre eventos posibles de la canasta
    [SerializeField] float targetReachThreshold = 0.1f;         // Distancia a la que la canasta se considera que ha llegado a su objetivo de movimiento
    [SerializeField] float shakeIntensity = 0.2f;               // Intensidad del temblor de la canasta

    [Header("Movement Area")]
    [SerializeField] Vector3 movementBoundsSize = new Vector3(5f, 3f, 5f);

    bool isPlaying;
    Vector3 initialPosition;
    Vector3 initialScale;
    Vector3 movementTarget;
    BasketEventType currentEvent = BasketEventType.None;
    float eventTimer;
    float cooldownTimer;
    float moveSpeed;

    void Start()
    {
        isPlaying = false;
        initialPosition = transform.position;
        initialScale = transform.localScale;

        SetCooldown();

        GameManager.Instance.OnStateChanged += Basket_OnStateChanged;
    }

    private void Basket_OnStateChanged(object sender, System.EventArgs e)
    {
        isPlaying = GameManager.Instance.IsGamePlaying();
    }

    void Update()
    {
        if (!isPlaying) return;

        if (currentEvent == BasketEventType.None)
        {
            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer <= 0f)
                TriggerRandomEvent();
        }
        else
        {
            eventTimer -= Time.deltaTime;

            if (currentEvent == BasketEventType.Movement)
                HandleMovement();

            if (eventTimer <= 0f)
                EndCurrentEvent();

            if (currentEvent == BasketEventType.Movement)
                HandleMovement();

            if (currentEvent == BasketEventType.Shake)
                HandleShake();

        }
    }

    #region Event Logic

    void TriggerRandomEvent()
    {
        currentEvent = (BasketEventType)Random.Range(0, Enum.GetNames(typeof(BasketEventType)).Length - 1); // Exclude None

        eventTimer = Random.Range(minEventsDuration, maxEventsDuration);

        switch (currentEvent)
        {
            case BasketEventType.SizeIncrease:
                IncreaseSize();
                break;

            case BasketEventType.Movement:
                StartMovement();
                break;

            case BasketEventType.Teleportation:
                Teleport();
                break;

            case BasketEventType.Shake:
                StartShake();
                break;
        }
    }

    void EndCurrentEvent()
    {
        ResetBasket();
        currentEvent = BasketEventType.None;
        SetCooldown();
    }

    void SetCooldown()
    {
        cooldownTimer = Random.Range(minEventCooldownTime, maxEventCooldownTime);
    }

    #endregion

    #region Eventos de la canasta

    // Aumenta el tamaño de la canasta de forma aleatoria, hasta el máximo permitido
    void IncreaseSize()
    {
        float increaseAmount = Random.Range(0f, maxSizeIncreaseAmount);
        transform.localScale = initialScale + Vector3.one * increaseAmount;
    }

    void StartMovement()
    {
        moveSpeed = Random.Range(0.5f, maxMoveSpeedIncreaseAmount);
        movementTarget = GetRandomPointInBounds();
    }

    void HandleMovement()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            movementTarget,
            moveSpeed * Time.deltaTime
        );

        float distance = Vector3.Distance(transform.position, movementTarget);

        if (distance <= targetReachThreshold)
            movementTarget = GetRandomPointInBounds();
    }

    void StartShake()
    {
        moveSpeed = Random.Range(5f, 10f); // rápido
    }

    void HandleShake()
    {
        Vector3 randomOffset = Random.insideUnitSphere * shakeIntensity;
        transform.position = initialPosition + randomOffset;
    }

    // Teletransporta la canasta a una posición aleatoria dentro de un área determinada
    void Teleport()
    {
        transform.position = GetRandomPointInBounds();
    }
    #endregion

    #region Utilities

    Vector3 GetRandomPointInBounds()
    {
        Vector3 halfSize = movementBoundsSize * 0.5f;

        return new Vector3(
            Random.Range(initialPosition.x - halfSize.x, initialPosition.x + halfSize.x),
            Random.Range(initialPosition.y - halfSize.y, initialPosition.y + halfSize.y),
            Random.Range(initialPosition.z - halfSize.z, initialPosition.z + halfSize.z)
        );
    }

    void ResetBasket()
    {
        transform.position = initialPosition;
        transform.localScale = initialScale;
    }

    #endregion

    #region Gizmos

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            Application.isPlaying ? initialPosition : transform.position,
            movementBoundsSize
        );
    }

    #endregion
}
