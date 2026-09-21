using UnityEngine;

public class Connector_Counter : MonoBehaviour
{
    public int RequiredCount = 3;
    public GameObjectEvent Fired;

    private int currentCount = 0;

    public void Receive(GameObject sender)
    {
        currentCount++;
        if (currentCount >= RequiredCount)
        {
            Fired?.Invoke(sender);
        }
    }

    public void ResetCount()
    {
        currentCount = 0;
    }
}