using UnityEngine;
using UnityEngine.InputSystem;

public class Player_PointController_InputActionAsset : MonoBehaviour, IPlayerPointInput
{
    private const string PointActionName = "Point";

    private Player_Config playerConfig;
    private InputAction pointAction;

    public bool Pressed => pointAction?.WasPressedThisFrame() ?? false;
    public bool IsPressing => pointAction?.IsPressed() ?? false;
    public bool Released => pointAction?.WasReleasedThisFrame() ?? false;

    private void Awake()
    {
        playerConfig = GetComponentInParent<Player_Config>();
    }

    private void Start()
    {
        if (playerConfig.InputActions == null)
        {
            Debug.LogError($"[{nameof(Player_PointController_InputActionAsset)}] Player_Config에 InputActions가 연결되지 않았습니다.");
            return;
        }

        var map = playerConfig.InputActions.FindActionMap(playerConfig.ActionMapName);
        if (map == null)
        {
            Debug.LogError($"[{nameof(Player_PointController_InputActionAsset)}] Action Map '{playerConfig.ActionMapName}'을 찾을 수 없습니다.");
            return;
        }

        pointAction = map.FindAction(PointActionName);
        if (pointAction == null)
            Debug.LogWarning($"[{nameof(Player_PointController_InputActionAsset)}] '{PointActionName}' 액션을 찾을 수 없습니다.");
    }

    private void OnEnable() => playerConfig?.InputActions?.Enable();
    private void OnDisable() => playerConfig?.InputActions?.Disable();
}