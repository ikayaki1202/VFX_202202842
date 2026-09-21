using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// InputActionAsset는 Player_Config에 한 번만 연결해두면, 이 스크립트는 그걸 가져다 쓴다.
/// 참고: 필드별 개별 연결 방식은 Player_LookController 에 있다.
/// </summary>
public class Player_LookController_InputActionAsset : MonoBehaviour, IPlayerLookInput
{
    private const string LookActionName = "Look";

    private Player_Config playerConfig;
    private InputAction lookAction;

    public Vector2 LookInput => lookAction?.ReadValue<Vector2>() ?? Vector2.zero;

    private void Awake()
    {
        playerConfig = GetComponentInParent<Player_Config>();

        if (playerConfig.InputActions == null)
        {
            Debug.LogError($"[{nameof(Player_LookController_InputActionAsset)}] Player_Config에 InputActions가 연결되지 않았습니다.");
            return;
        }

    }

    void Start()
    {
        var map = playerConfig.InputActions.FindActionMap(playerConfig.ActionMapName);
        if (map == null)
        {
            Debug.LogError($"[{nameof(Player_LookController_InputActionAsset)}] Action Map '{playerConfig.ActionMapName}'을 찾을 수 없습니다.");
            return;
        }

        lookAction = map.FindAction(LookActionName);
        if (lookAction == null)
            Debug.LogWarning($"[{nameof(Player_LookController_InputActionAsset)}] '{LookActionName}' 액션을 찾을 수 없습니다.");

    }

    private void OnEnable() => playerConfig?.InputActions?.Enable();
    private void OnDisable() => playerConfig?.InputActions?.Disable();
}