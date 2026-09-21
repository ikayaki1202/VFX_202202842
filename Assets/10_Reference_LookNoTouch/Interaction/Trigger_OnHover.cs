using UnityEngine;

public class Trigger_OnHover : MonoBehaviour, IHoverable
{
    public GameObjectEvent HoverEnter;
    public GameObjectEvent HoverExit;

    public void OnHoverEnter(GameObject sender) => HoverEnter?.Invoke(sender);
    public void OnHoverExit(GameObject sender) => HoverExit?.Invoke(sender);
}