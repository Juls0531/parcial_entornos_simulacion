using UnityEngine;

public class PrisonCell : MonoBehaviour
{
    [Header("Puerta de la celda")]
    public GameObject cellDoor;
    public float interval = 10f;
    public float openDuration = 3f;

    [Header("Prisionero")]
    public PrisonerAI prisoner;

    public bool isOpen { get; private set; }

    private float timer;

    public void SimulateEntity()
    {
        timer += Time.deltaTime;

        bool shouldBeOpen = (timer % interval) < openDuration;

        if (shouldBeOpen != isOpen)
        {
            isOpen = shouldBeOpen;

            if (cellDoor != null)
                cellDoor.SetActive(!isOpen);

            Debug.Log(isOpen ? "Puerta de celda abierta" :
                               "Puerta de celda cerrada");
        }
    }

    public bool CanEscape()
    {
        return isOpen;
    }

    public void ReturnPrisoner()
    {
        if (prisoner == null) return;

        prisoner.transform.position =
            transform.position + Vector3.right * 0.8f;

        prisoner.ReturnToCell();
    }
}