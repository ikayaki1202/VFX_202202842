using UnityEngine;
public interface IInteractable
{
    void OnInteractionStart(GameObject sender); // 누른 순간 1회
    void OnInteractionEnd(GameObject sender);   // 뗀 순간 1회
}