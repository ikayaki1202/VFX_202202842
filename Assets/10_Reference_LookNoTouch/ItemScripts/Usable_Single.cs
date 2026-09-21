using UnityEngine;

public class Usable_Single : MonoBehaviour, IUsable
{
    [Header("이펙트 연결")]
    public GameObjectEvent OnUsed;

    public void OnUsePressed(GameObject user)
    {
        OnUsed?.Invoke(user);
        Debug.Log($"[Usable_Single] OnUsePressed!");
    }

    public void OnUseReleased(GameObject user) { }
}