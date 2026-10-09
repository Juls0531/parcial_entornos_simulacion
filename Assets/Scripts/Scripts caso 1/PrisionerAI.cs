using UnityEngine;

public class PrisonerAI : MonoBehaviour
{
    public enum PrisonerState
    {
        Confined,
        Running,
        Evading,
        Hiding,
        Distracting,
        Escaped
    }

    [Header("Referencias")]
    public PrisonCell cell;
    public Transform exitDoor;
    public Transform guard;

    [Header("Movimiento")]
    public float runSpeed = 3f;
    public float evadeSpeed = 4f;

    [Header("Detección del guardia")]
    public float guardDetectionRadius = 2.5f;
    public float safeDistance = 3.5f;

    [Header("Salida")]
    public float escapeDistance = 0.5f;

    public PrisonerState state = PrisonerState.Confined;

    private bool hasLeftCell = false;

    public void SimulateEntity()
    {
        if (state == PrisonerState.Escaped)
            return;

        if (cell == null || exitDoor == null)
            return;

        // Buscar al guardia si no se ha asignado manualmente.
        if (guard == null)
        {
            GuardAI guardAI = FindFirstObjectByType<GuardAI>();

            if (guardAI != null)
                guard = guardAI.transform;
        }

        // El prisionero espera dentro de la celda.
        if (!hasLeftCell)
        {
            if (!cell.CanEscape())
            {
                state = PrisonerState.Confined;
                return;
            }

            hasLeftCell = true;
            state = PrisonerState.Running;
        }

        // Comprobar la distancia al guardia.
        if (guard != null)
        {
            float distanceToGuard =
                Vector2.Distance(transform.position, guard.position);

            if (distanceToGuard < guardDetectionRadius)
            {
                state = PrisonerState.Evading;

                Vector2 directionAway =
                    (Vector2)transform.position - (Vector2)guard.position;

                // Evitar un vector de dirección nulo.
                if (directionAway.sqrMagnitude > 0.001f)
                {
                    directionAway.Normalize();

                    transform.position +=
                        (Vector3)(directionAway * evadeSpeed * Time.deltaTime);
                }

                return;
            }
        }

        // Si ya estaba evadiendo, comprobar que haya suficiente distancia.
        if (state == PrisonerState.Evading)
        {
            if (guard != null &&
                Vector2.Distance(transform.position, guard.position) < safeDistance)
            {
                return;
            }

            state = PrisonerState.Running;
        }

        // Cuando no hay peligro, avanzar hacia la salida.
        if (state == PrisonerState.Running)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                exitDoor.position,
                runSpeed * Time.deltaTime
            );

            if (Vector2.Distance(transform.position, exitDoor.position)
                <= escapeDistance)
            {
                state = PrisonerState.Escaped;
                Debug.Log("¡El prisionero ha escapado!");
            }
        }
    }

    public void ReturnToCell()
    {
        hasLeftCell = false;
        state = PrisonerState.Confined;
    }

    public void Hide()
    {
        if (state == PrisonerState.Running)
            state = PrisonerState.Hiding;
    }

    public void DistractGuard()
    {
        if (state == PrisonerState.Running)
            state = PrisonerState.Distracting;
    }
}