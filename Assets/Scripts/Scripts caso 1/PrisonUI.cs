using UnityEngine;

public class PrisonUI : MonoBehaviour
{
    public AgentEntity cell;

    public void ToggleCell()
    {
        if (cell != null) cell.ToggleCell();
    }
}
