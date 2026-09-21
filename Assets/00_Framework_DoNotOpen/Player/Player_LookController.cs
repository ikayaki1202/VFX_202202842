using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 필드별로 액션을 개별 연결하는 방식. 참고: Asset 하나만 연결하는 방식은
/// Player_LookController_InputActionAsset 에 있다.
/// </summary>
public class Player_LookController : MonoBehaviour, IPlayerLookInput
{
    [Header("Look Properties")]
    public InputActionProperty LookAction;

    public Vector2 LookInput => LookAction.action?.ReadValue<Vector2>() ?? Vector2.zero;

    private void OnEnable() => LookAction.action?.Enable();
    private void OnDisable() => LookAction.action?.Disable();
}