using UnityEngine;

public class AgentEntity : MonoBehaviour
{
    public enum EntityType { Prisoner, Guard, Cell }
    public enum EntityState { Idle, Confined, Escaping, Patrolling, Chasing, Captured, Escaped, Open, Closed }

    [Header("Entidad")]
    public EntityType entityType;
    public EntityState state = EntityState.Idle;

    [Header("Movimiento")]
    public float speed = 2f;
    public float detectionRadius = 2f;
    public float captureDistance = 0.6f;

    [Header("Celda")]
    public bool isOpen;
    [Header("Simulación")]
    public bool activeEntity = true;

    private Vector3 initialPosition;
    private Vector3 patrolTarget;
    private float leftLimit = -7f;
    private float rightLimit = 8f;

    private void Start()
    {
        initialPosition = transform.position;
        ChoosePatrolTarget();
        SetInitialState();
    }

    private void SetInitialState()
    {
        if (entityType == EntityType.Prisoner) state = EntityState.Confined;
        else if (entityType == EntityType.Guard) state = EntityState.Patrolling;
        else state = isOpen ? EntityState.Open : EntityState.Closed;
    }

    public void Simulate()
    {
        if (!activeEntity) return;
        switch (entityType)
        {
            case EntityType.Prisoner: SimulatePrisoner(); break;
            case EntityType.Guard: SimulateGuard(); break;
            case EntityType.Cell: SimulateCell(); break;
        }
    }

    private void Update()
    {
        // Simulate.cs is the central manager; Update is intentionally not used for entity logic.
    }

    private void SimulatePrisoner()
    {
        if (state == EntityState.Captured || state == EntityState.Escaped) return;
        AgentEntity cell = FindNearest(EntityType.Cell);
        if (state == EntityState.Confined)
        {
            if (cell == null || !cell.isOpen) return;
            state = EntityState.Escaping;
        }

        if (state == EntityState.Escaping)
        {
            transform.position += Vector3.right * speed * Time.deltaTime;
            if (transform.position.x > rightLimit)
            {
                state = EntityState.Escaped;
                activeEntity = false;
            }
        }
    }

    private void SimulateGuard()
    {
        AgentEntity prisoner = FindEscapingPrisoner();
        if (prisoner != null)
        {
            state = EntityState.Chasing;
            transform.position = Vector3.MoveTowards(transform.position, prisoner.transform.position, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, prisoner.transform.position) <= captureDistance)
            {
                prisoner.ReturnToCell();
                state = EntityState.Patrolling;
                ChoosePatrolTarget();
            }
        }
        else
        {
            state = EntityState.Patrolling;
            transform.position = Vector3.MoveTowards(transform.position, patrolTarget, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, patrolTarget) < 0.2f) ChoosePatrolTarget();
        }
    }

    private void SimulateCell()
    {
        state = isOpen ? EntityState.Open : EntityState.Closed;
    }

    public void ToggleCell()
    {
        if (entityType != EntityType.Cell) return;
        isOpen = !isOpen;
        state = isOpen ? EntityState.Open : EntityState.Closed;
    }

    private AgentEntity FindEscapingPrisoner()
    {
        AgentEntity[] entities = FindObjectsByType<AgentEntity>(FindObjectsSortMode.None);
        AgentEntity nearest = null;
        float minDistance = detectionRadius;
        foreach (AgentEntity entity in entities)
        {
            if (entity.entityType != EntityType.Prisoner || entity.state != EntityState.Escaping) continue;
            float distance = Vector3.Distance(transform.position, entity.transform.position);
            if (distance < minDistance) { minDistance = distance; nearest = entity; }
        }
        return nearest;
    }

    private AgentEntity FindNearest(EntityType type)
    {
        AgentEntity[] entities = FindObjectsByType<AgentEntity>(FindObjectsSortMode.None);
        AgentEntity nearest = null;
        float minDistance = float.MaxValue;
        foreach (AgentEntity entity in entities)
        {
            if (entity.entityType != type) continue;
            float distance = Vector3.Distance(transform.position, entity.transform.position);
            if (distance < minDistance) { minDistance = distance; nearest = entity; }
        }
        return nearest;
    }

    private void ReturnToCell()
    {
        AgentEntity cell = FindNearest(EntityType.Cell);
        if (cell != null)
        {
            transform.position = cell.transform.position + Vector3.right * 0.8f;
            state = EntityState.Confined;
            activeEntity = true;
        }
    }

    private void ChoosePatrolTarget()
    {
        patrolTarget = new Vector3(Random.Range(-6.5f, 6.5f), Random.Range(-2.5f, 2.5f), 0f);
    }

    public void ResetEntity()
    {
        transform.position = initialPosition;
        activeEntity = true;
        SetInitialState();
        ChoosePatrolTarget();
    }
}
