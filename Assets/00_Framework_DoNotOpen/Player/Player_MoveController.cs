using UnityEngine;
using UnityEngine.InputSystem;

public class Player_MoveController : MonoBehaviour, IPlayerMoveInput
{
    [Header("Move Properties")]
    public InputActionProperty MoveAction;
    public InputActionProperty SprintAction;
    public InputActionProperty JumpAction;
    public InputActionProperty SnapTurnAction;

    public Vector2 MoveInput => MoveAction.action?.ReadValue<Vector2>() ?? Vector2.zero;
    public bool SprintInput => SprintAction.action?.IsPressed() ?? false;
    public bool JumpInput => JumpAction.action?.WasPressedThisFrame() ?? false;
    public float SnapTurnInput => SnapTurnAction.action?.ReadValue<float>() ?? 0f;

    private void OnEnable()
    {
        MoveAction.action?.Enable();
        SprintAction.action?.Enable();
        JumpAction.action?.Enable();
        SnapTurnAction.action?.Enable();
    }

    private void OnDisable()
    {
        MoveAction.action?.Disable();
        SprintAction.action?.Disable();
        JumpAction.action?.Disable();
        SnapTurnAction.action?.Disable();
    }
}