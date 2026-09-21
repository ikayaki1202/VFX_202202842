using System;
using UnityEngine;

public class Player_State : MonoBehaviour
{
    // Action Event
    public event Action<PlayerState> OnPlayerStateChanged;
    public event Action<PlayerInteractionState> OnInteractionStateChanged;
    public event Action<GameObject> OnHandlingObjectChanged;


    [Header("Current States")]
    [field: SerializeField] public PlayerState CurrentPlayerState { get; private set; } = PlayerState.Idle;
    [field: SerializeField] public PlayerInteractionState CurrentInteractionState { get; private set; } = PlayerInteractionState.Idle;

    [Header("Interaction Data")]
    [field: SerializeField] public GameObject HandlingObject { get; private set; }

    public void SetPlayerState(PlayerState newState)
    {
        if (CurrentPlayerState == newState) return;
        CurrentPlayerState = newState;
        Debug.Log($"<color=yellow>[PlaterState]</color> <b>{newState}</b>");
        OnPlayerStateChanged?.Invoke(newState);
    }

    public void SetInteractionState(PlayerInteractionState newState)
    {
        if (CurrentInteractionState == newState) return;
        CurrentInteractionState = newState;
        Debug.Log($"<color=yellow>[InteractionState]</color> <b>{CurrentInteractionState}</b>");
        OnInteractionStateChanged?.Invoke(newState);
    }

    public void SetHandlingObject(GameObject newObject)
    {
        if (HandlingObject == newObject) return;
        HandlingObject = newObject;
        Debug.Log($"<color=yellow>[HandlingObject]</color> <b>{(newObject != null ? newObject.name : "None")}</b>");
        OnHandlingObjectChanged?.Invoke(newObject);
    }
}