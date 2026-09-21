using UnityEngine;

public class Connector_Once : MonoBehaviour
{
    public GameObjectEvent Fired;

    private bool hasFired = false;

    public void Receive(GameObject sender)
    {
        if (hasFired) return;
        hasFired = true;
        Fired?.Invoke(sender);
    }

    public void Reset()
    {
        hasFired = false;
    }
}