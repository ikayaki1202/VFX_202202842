using UnityEngine;
using UnityEngine.InputSystem;

public class Player_UIController : MonoBehaviour, IPlayerUIInput
{
    [SerializeField] private InputActionProperty toggleInventoryAction;

    public bool ToggleInventoryPressed =>
        toggleInventoryAction.action != null && toggleInventoryAction.action.WasPressedThisFrame();

    private void OnEnable() => toggleInventoryAction.action?.Enable();
    private void OnDisable() => toggleInventoryAction.action?.Disable();
}