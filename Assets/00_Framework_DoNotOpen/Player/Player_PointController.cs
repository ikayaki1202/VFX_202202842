using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 필드별로 액션을 개별 연결하는 방식. 참고: Asset 하나만 연결하는 방식은
/// Player_PointController_InputActionAsset 에 있다.
/// </summary>
public class Player_PointController : MonoBehaviour, IPlayerPointInput
{
    [Header("Point Properties")]
    public InputActionProperty PointAction;

    public bool Pressed => PointAction.action?.WasPressedThisFrame() ?? false;
    public bool IsPressing => PointAction.action?.IsPressed() ?? false;
    public bool Released => PointAction.action?.WasReleasedThisFrame() ?? false;

    private void OnEnable() => PointAction.action?.Enable();
    private void OnDisable() => PointAction.action?.Disable();
}