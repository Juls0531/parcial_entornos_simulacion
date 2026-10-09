using UnityEngine;

public class Simulate : MonoBehaviour
{
    public bool simulationRunning = true;

    private PrisonerAI[] prisoners;
    private GuardAI[] guards;
    private PrisonCell[] cells;

    private void Start()
    {
        RefreshEntities();
    }

    private void Update()
    {
        if (!simulationRunning) return;

        if (prisoners == null || guards == null || cells == null)
            RefreshEntities();

        foreach (PrisonCell cell in cells)
            if (cell != null) cell.SimulateEntity();

        foreach (PrisonerAI prisoner in prisoners)
            if (prisoner != null) prisoner.SimulateEntity();

        foreach (GuardAI guard in guards)
            if (guard != null) guard.SimulateEntity();
    }

    private void RefreshEntities()
    {
        prisoners = FindObjectsByType<PrisonerAI>(
            FindObjectsSortMode.None);

        guards = FindObjectsByType<GuardAI>(
            FindObjectsSortMode.None);

        cells = FindObjectsByType<PrisonCell>(
            FindObjectsSortMode.None);
    }
}