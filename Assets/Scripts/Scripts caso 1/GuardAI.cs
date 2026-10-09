using UnityEngine;

public class GuardAI : MonoBehaviour
{
    public enum GuardState { Patrolling, Chasing }

    public GuardState state = GuardState.Patrolling;
    public float speed = 1.5f;
    public float detectionRadius = 3f;
    public float captureDistance = 0.7f;

    public PrisonerAI prisoner;

    private Vector3 patrolTarget;

    private void Start()
    {
        ChoosePatrolTarget();
    }

    public void SimulateEntity()
    {
        if (prisoner == null)
            prisoner = FindFirstObjectByType<PrisonerAI>();

        if (prisoner == null) return;

        float distance = Vector3.Distance(
            transform.position, prisoner.transform.position);

        bool canDetect =
            distance <= detectionRadius &&
            (prisoner.state == PrisonerAI.PrisonerState.Running ||
             prisoner.state == PrisonerAI.PrisonerState.Distracting);

        if (canDetect)
            state = GuardState.Chasing;

        if (state == GuardState.Chasing)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                prisoner.transform.position,
                speed * Time.deltaTime);

            if (distance <= captureDistance)
            {
                if (prisoner.cell != null)
                    prisoner.cell.ReturnPrisoner();
                else
                    prisoner.ReturnToCell();

                state = GuardState.Patrolling;
                ChoosePatrolTarget();
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                patrolTarget,
                speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, patrolTarget) < 0.2f)
                ChoosePatrolTarget();
        }
    }

    private void ChoosePatrolTarget()
    {
        patrolTarget = new Vector3(
            Random.Range(-6f, 6f),
            Random.Range(-2.5f, 2.5f),
            0f);
    }
}