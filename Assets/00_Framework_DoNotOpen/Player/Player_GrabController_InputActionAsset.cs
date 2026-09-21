using UnityEngine;
using UnityEngine.InputSystem;

public class Player_GrabController_InputActionAsset : MonoBehaviour, IPlayerGrabInput
{
    private const string DistanceGrabActionName = "DistanceGrab";
    private const string DistancePullActionName = "DistancePull";
    private const string MouseReleaseActionName = "MouseRelease";
    private const string TouchGrabActionName = "TouchGrab";
    private const string DropActionName = "Drop";
    private const string AddToInventoryActionName = "AddToInventory";
    private const string SwitchFireModeActionName = "SwitchFireMode";

    private Player_Config playerConfig;

    private InputAction distanceGrabAction;
    private InputAction distancePullAction;
    private InputAction mouseReleaseAction;
    private InputAction touchGrabAction;
    private InputAction dropAction;
    private InputAction addToInventoryAction;
    private InputAction switchFireModeAction;

    public bool DistanceGrabPressed => distanceGrabAction?.WasPressedThisFrame() ?? false;
    public bool DistancePullPressed => distancePullAction?.WasPressedThisFrame() ?? false;
    public bool DistanceGrabReleased => mouseReleaseAction?.WasReleasedThisFrame() ?? false;
    public bool DistancePullReleased => mouseReleaseAction?.WasReleasedThisFrame() ?? false;
    public bool TouchGrabPressed => touchGrabAction?.IsPressed() ?? false;
    public bool DropPressed => dropAction?.WasPressedThisFrame() ?? false;
    public bool AddToInventoryPressed => addToInventoryAction?.WasPressedThisFrame() ?? false;
    public bool SwitchFireModePressed => switchFireModeAction?.WasPressedThisFrame() ?? false;

    private void Awake()
    {
        playerConfig = GetComponentInParent<Player_Config>();
    }

    private void Start()
    {
        if (playerConfig.InputActions == null)
        {
            Debug.LogError($"[{nameof(Player_GrabController_InputActionAsset)}] Player_Config에 InputActions가 연결되지 않았습니다.");
            return;
        }

        var map = playerConfig.InputActions.FindActionMap(playerConfig.ActionMapName);
        if (map == null)
        {
            Debug.LogError($"[{nameof(Player_GrabController_InputActionAsset)}] Action Map '{playerConfig.ActionMapName}'을 찾을 수 없습니다.");
            return;
        }

        distanceGrabAction = FindActionOrWarn(map, DistanceGrabActionName);
        distancePullAction = FindActionOrWarn(map, DistancePullActionName);
        mouseReleaseAction = FindActionOrWarn(map, MouseReleaseActionName);
        touchGrabAction = FindActionOrWarn(map, TouchGrabActionName);
        dropAction = FindActionOrWarn(map, DropActionName);
        addToInventoryAction = FindActionOrWarn(map, AddToInventoryActionName);
        switchFireModeAction = FindActionOrWarn(map, SwitchFireModeActionName);
    }

    private InputAction FindActionOrWarn(InputActionMap map, string actionName)
    {
        var action = map.FindAction(actionName);
        if (action == null)
            Debug.LogWarning($"[{nameof(Player_GrabController_InputActionAsset)}] '{actionName}' 액션을 찾을 수 없습니다.");
        return action;
    }

    private void OnEnable() => playerConfig?.InputActions?.Enable();
    private void OnDisable() => playerConfig?.InputActions?.Disable();
}