using UnityEngine;

// Componente reutilizable para representar cualquiera de las entidades del parcial.
// Añade este script a un GameObject y configura Role, Behavior y Target Role en el Inspector.
public class AgentEntity : MonoBehaviour
{
    public enum Role
    {
        A, B, C, Other
    }

    public enum Behavior
    {
        Static,          // Permanece en el sitio
        Wander,          // Se mueve aleatoriamente
        SeekTarget,      // Persigue una entidad de Target Role
        FleeTarget,      // Huye de una entidad de Target Role
        Patrol,          // Alterna entre desplazamientos aleatorios
        CollectResource, // Se acerca a un recurso y lo consume
        AttackTarget,    // Reduce la salud de un objetivo cercano
        WorkAtStation,   // Se acerca a una estación y suma tareas
        EscapeBoundary   // Intenta llegar al borde del área
    }

    [Header("Identidad")]
    public string displayName = "Entidad";
    public Role role = Role.A;
    public Behavior behavior = Behavior.Wander;
    public Role targetRole = Role.B;

    [Header("Movimiento")]
    public float speed = 2f;
    public float detectionRadius = 6f;
    public float interactionRadius = 1.2f;
    public Vector2 areaMin = new Vector2(-9, -5);
    public Vector2 areaMax = new Vector2(9, 5);

    [Header("Estado interno")]
    public float health = 10f;
    public float maxHealth = 10f;
    public float energy = 10f;
    public float maxEnergy = 10f;
    public int resources = 0;
    public int tasksCompleted = 0;
    public bool active = true;
    public bool isProtected = false;

    [Header("Interacción")]
    public float damage = 1f;
    public float actionCooldown = 1f;
    public float resourceAmount = 1f;

    private float cooldownTimer;
    private Vector3 wanderTarget;

    public void Simulate()
    {
        if (!active) return;

        float dt = Time.deltaTime;
        cooldownTimer -= dt;
        energy = Mathf.Max(0f, energy - 0.03f * dt);

        switch (behavior)
        {
            case Behavior.Static:
                break;
            case Behavior.Wander:
            case Behavior.Patrol:
                MoveWander(dt);
                break;
            case Behavior.SeekTarget:
                MoveTowardNearestTarget(dt, false);
                break;
            case Behavior.FleeTarget:
                MoveTowardNearestTarget(dt, true);
                break;
            case Behavior.CollectResource:
                Collect(dt);
                break;
            case Behavior.AttackTarget:
                Attack(dt);
                break;
            case Behavior.WorkAtStation:
                Work(dt);
                break;
            case Behavior.EscapeBoundary:
                Escape(dt);
                break;
        }

        ClampToArea();
        if (health <= 0f || energy <= 0f)
            active = false;
    }

    private AgentEntity FindNearest(Role wantedRole, float radius)
    {
        AgentEntity[] all = FindObjectsOfType<AgentEntity>();
        AgentEntity nearest = null;
        float best = radius;

        foreach (AgentEntity candidate in all)
        {
            if (candidate == this || !candidate.active || candidate.role != wantedRole)
                continue;

            float d = Vector3.Distance(transform.position, candidate.transform.position);
            if (d < best)
            {
                best = d;
                nearest = candidate;
            }
        }
        return nearest;
    }

    private void MoveWander(float dt)
    {
        if (wanderTarget == Vector3.zero ||
            Vector3.Distance(transform.position, wanderTarget) < 0.25f)
        {
            wanderTarget = new Vector3(
                Random.Range(areaMin.x, areaMax.x),
                Random.Range(areaMin.y, areaMax.y),
                transform.position.z);
        }
        transform.position = Vector3.MoveTowards(
            transform.position, wanderTarget, speed * dt);
    }

    private void MoveTowardNearestTarget(float dt, bool flee)
    {
        AgentEntity target = FindNearest(targetRole, detectionRadius);
        if (target == null)
        {
            MoveWander(dt);
            return;
        }

        Vector3 destination = target.transform.position;
        if (flee)
        {
            Vector3 away = (transform.position - destination).normalized;
            destination = transform.position + away * detectionRadius;
        }

        transform.position = Vector3.MoveTowards(
            transform.position, destination, speed * dt);
    }

    private void Collect(float dt)
    {
        AgentEntity resource = FindNearest(targetRole, detectionRadius);
        if (resource == null)
        {
            MoveWander(dt);
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position, resource.transform.position, speed * dt);

        if (Vector3.Distance(transform.position, resource.transform.position) <= interactionRadius
            && cooldownTimer <= 0f)
        {
            resources++;
            resource.resourceAmount -= 1f;
            cooldownTimer = actionCooldown;
            if (resource.resourceAmount <= 0f)
                resource.active = false;
        }
    }

    private void Attack(float dt)
    {
        AgentEntity target = FindNearest(targetRole, detectionRadius);
        if (target == null)
        {
            MoveWander(dt);
            return;
        }

        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance > interactionRadius)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, target.transform.position, speed * dt);
        }
        else if (cooldownTimer <= 0f)
        {
            target.health -= damage;
            cooldownTimer = actionCooldown;
            if (target.health <= 0f)
                target.active = false;
        }
    }

    private void Work(float dt)
    {
        AgentEntity station = FindNearest(targetRole, detectionRadius);
        if (station == null)
        {
            MoveWander(dt);
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position, station.transform.position, speed * dt);

        if (Vector3.Distance(transform.position, station.transform.position) <= interactionRadius
            && cooldownTimer <= 0f)
        {
            tasksCompleted++;
            cooldownTimer = actionCooldown;
        }
    }

    private void Escape(float dt)
    {
        Vector3 target = new Vector3(
            Mathf.Abs(transform.position.x - areaMin.x) <
            Mathf.Abs(transform.position.x - areaMax.x) ? areaMin.x : areaMax.x,
            transform.position.y,
            transform.position.z);

        transform.position = Vector3.MoveTowards(transform.position, target, speed * dt);

        if (transform.position.x <= areaMin.x + 0.1f ||
            transform.position.x >= areaMax.x - 0.1f)
            active = false;
    }

    private void ClampToArea()
    {
        Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x, areaMin.x, areaMax.x);
        p.y = Mathf.Clamp(p.y, areaMin.y, areaMax.y);
        transform.position = p;
    }

    public void ResetState()
    {
        active = true;
        health = maxHealth;
        energy = maxEnergy;
        resources = 0;
        tasksCompleted = 0;
        cooldownTimer = 0f;
        wanderTarget = Vector3.zero;
    }
}
