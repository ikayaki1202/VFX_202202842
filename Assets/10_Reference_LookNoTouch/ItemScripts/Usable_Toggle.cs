using UnityEngine;

public class Usable_Toggle : MonoBehaviour, IUsable
{
    [Header("대상")]
    public Light TargetLight;

    [Header("이펙트 연결")]
    public GameObjectEvent OnToggled;

    public void OnUsePressed(GameObject user)
    {
        if (TargetLight != null)
        {
            TargetLight.enabled = !TargetLight.enabled;
        }

        OnToggled?.Invoke(user);
    }

    public void OnUseReleased(GameObject user) { }
}