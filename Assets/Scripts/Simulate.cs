using UnityEngine;
using UnityEngine.UI;

// Control central de la simulación.
// Requisito del parcial: las funciones Simulate() de las entidades se llaman desde aquí.
public class Simulate : MonoBehaviour
{
    [Header("Estado general")]
    public bool simulationRunning = true;
    public float elapsedTime = 0f;
    public int score = 0;

    [Header("Opcional: interfaz")]
    public Text statusText; // Si usas TextMeshPro, cambia Text por TMP_Text y añade el using correspondiente.

    private AgentEntity[] entities;

    private void Start()
    {
        CacheEntities();
    }

    private void CacheEntities()
    {
        entities = FindObjectsOfType<AgentEntity>();
    }

    private void Update()
    {
        if (!simulationRunning) return;

        elapsedTime += Time.deltaTime;

        if (entities == null || entities.Length == 0)
            CacheEntities();

        // Todas las entidades actualizan su comportamiento desde el controlador central.
        foreach (AgentEntity entity in entities)
        {
            if (entity != null)
                entity.Simulate();
        }

        UpdateStatus();
    }

    public void PauseSimulation()
    {
        simulationRunning = false;
    }

    public void ResumeSimulation()
    {
        simulationRunning = true;
    }

    public void ToggleSimulation()
    {
        simulationRunning = !simulationRunning;
    }

    public void ResetSimulation()
    {
        foreach (AgentEntity entity in FindObjectsOfType<AgentEntity>())
        {
            if (entity != null)
                entity.ResetState();
        }

        elapsedTime = 0f;
        score = 0;
        CacheEntities();
        simulationRunning = true;
    }

    public void StopSimulation()
    {
        simulationRunning = false;
    }

    private void UpdateStatus()
    {
        if (statusText == null) return;

        int activeCount = 0;
        foreach (AgentEntity entity in entities)
            if (entity != null && entity.active) activeCount++;

        statusText.text =
            "Tiempo: " + elapsedTime.ToString("F1") +
            " s | Entidades activas: " + activeCount;
    }
}
