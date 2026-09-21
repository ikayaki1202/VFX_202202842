using UnityEngine;

public class Trigger_OnInteract : MonoBehaviour, IInteractable
{
    public GameObjectEvent InteractionStarted;      // 기존 필드, 이름/의미 그대로 유지 (누른 순간)
    public GameObjectEvent InteractionEnded; // 신규, 필요한 경우에만 인스펙터에서 연결

    public void OnInteractionStart(GameObject sender)
    {
        InteractionStarted?.Invoke(sender);
    }

    public void OnInteractionEnd(GameObject sender)
    {
        InteractionEnded?.Invoke(sender);
    }
}