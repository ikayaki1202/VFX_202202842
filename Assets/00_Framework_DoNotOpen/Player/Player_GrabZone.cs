using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Player_GrabZone : MonoBehaviour
{
    public event Action<Collider> TouchEntered;
    public event Action<Collider> TouchExited;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (!col.isTrigger)
            Debug.LogWarning($"[{nameof(Player_GrabZone)}] Collider의 Is Trigger가 꺼져 있습니다. 켜주세요.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<IGrabbable>(out _)) return;
        TouchEntered?.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<IGrabbable>(out _)) return;
        TouchExited?.Invoke(other);
    }
}