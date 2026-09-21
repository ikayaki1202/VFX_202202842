using UnityEngine;

public interface IUsable
{
    void OnUsePressed(GameObject user);
    void OnUseReleased(GameObject user);
}