using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// InputActionAsset는 Player_Config에 한 번만 연결해두면, 이 스크립트는 그걸 가져다 쓴다.
/// 맵/액션 이름은 거의 안 바뀌는 값이라 인스펙터 노출 없이 상수로 고정했다.
/// 참고: 필드별 개별 연결 방식은 Reference/Player_MoveController_PC_PropertyExample.cs 에 있다.
/// </summary>
public class Player_MoveController_InputActionAsset : MonoBehaviour, IPlayerMoveInput
{
    private const string MoveActionName = "Move";
    private const string SprintActionName = "Sprint";
    private const string JumpActionName = "Jump";
    private const string SnapTurnActionName = "SnapTurn";

    private Player_Config playerConfig;
    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction jumpAction;
    private InputAction snapTurnAction;

    public Vector2 MoveInput => moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
    public bool SprintInput => sprintAction?.IsPressed() ?? false;
    public bool JumpInput => jumpAction?.WasPressedThisFrame() ?? false;
    public float SnapTurnInput => snapTurnAction?.ReadValue<float>() ?? 0f;

    private void Awake()
    {
        playerConfig = GetComponentInParent<Player_Config>();

        if (playerConfig.InputActions == null)
        {
            Debug.LogError($"[{nameof(Player_MoveController_InputActionAsset)}] Player_Config에 InputActions가 연결되지 않았습니다.");
            return;
        }

        var map = playerConfig.InputActions.FindActionMap(playerConfig.ActionMapName);
        if (map == null)
        {
            Debug.LogError($"[{nameof(Player_MoveController_InputActionAsset)}] Action Map '{playerConfig.ActionMapName}'을 찾을 수 없습니다.");
            return;
        }

        moveAction = FindActionOrWarn(map, MoveActionName);
        sprintAction = FindActionOrWarn(map, SprintActionName);
        jumpAction = FindActionOrWarn(map, JumpActionName);
        snapTurnAction = FindActionOrWarn(map, SnapTurnActionName);
    }

    private InputAction FindActionOrWarn(InputActionMap map, string actionName)
    {
        var action = map.FindAction(actionName);
        if (action == null)
            Debug.LogWarning($"[{nameof(Player_MoveController_InputActionAsset)}] '{actionName}' 액션을 찾을 수 없습니다.");
        return action;
    }

    private void OnEnable() => playerConfig?.InputActions?.Enable();
    private void OnDisable() => playerConfig?.InputActions?.Disable();
}