using UnityEngine;
using UnityEngine.InputSystem;

public class Player_GrabController : MonoBehaviour, IPlayerGrabInput
{
    [Header("Start Actions")]
    public InputActionProperty DistanceGrabAction; // G 단독
    public InputActionProperty DistancePullAction;  // U 단독

    [Header("Release Action (공용 - 마우스 왼쪽)")]
    public InputActionProperty MouseReleaseAction;

    [Header("기타")]
    public InputActionProperty TouchGrabAction; // G 단독 (IsPressed)
    public InputActionProperty DropAction;       // X
    public InputActionProperty AddToInventoryAction; // F
    public InputActionProperty SwitchFireModeAction; // B 단독

    public bool DistanceGrabPressed => DistanceGrabAction.action?.WasPressedThisFrame() ?? false;
    public bool DistancePullPressed => DistancePullAction.action?.WasPressedThisFrame() ?? false;
    public bool DistanceGrabReleased => MouseReleaseAction.action?.WasReleasedThisFrame() ?? false;
    public bool DistancePullReleased => MouseReleaseAction.action?.WasReleasedThisFrame() ?? false;
    public bool TouchGrabPressed => TouchGrabAction.action?.IsPressed() ?? false;
    public bool DropPressed => DropAction.action?.WasPressedThisFrame() ?? false;
    public bool AddToInventoryPressed => AddToInventoryAction.action?.WasPressedThisFrame() ?? false;
    public bool SwitchFireModePressed => SwitchFireModeAction.action?.WasPressedThisFrame() ?? false;

    private void OnEnable()
    {
        DistanceGrabAction.action?.Enable();
        DistancePullAction.action?.Enable();
        MouseReleaseAction.action?.Enable();
        TouchGrabAction.action?.Enable();
        DropAction.action?.Enable();
        AddToInventoryAction.action?.Enable();
        SwitchFireModeAction.action?.Enable();
    }

    private void OnDisable()
    {
        DistanceGrabAction.action?.Disable();
        DistancePullAction.action?.Disable();
        MouseReleaseAction.action?.Disable();
        TouchGrabAction.action?.Disable();
        DropAction.action?.Disable();
        AddToInventoryAction.action?.Disable();
        SwitchFireModeAction.action?.Disable();
    }
}