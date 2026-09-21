using UnityEngine;

/// <summary>
/// Desktop 전용 마우스룩. DesktopRig 안의 카메라 오브젝트에 부착한다.
/// XR일 때는 DesktopRig 자체가 Player_Config에 의해 비활성화되므로,
/// 이 스크립트가 플랫폼을 직접 체크할 필요가 없다.
/// </summary>
public class Player_LookBehavior : MonoBehaviour
{
    [Header("Vertical Clamp")]
    public float VerticalClampAngle = 80f;

    private IPlayerLookInput input;
    private Player_Config playerConfig;
    private Transform cameraTransform;
    private Transform bodyTransform; // 좌우 회전은 Player 루트가 담당                                     
    public Transform PointingHand;

    private float pitch;

    private void Awake()
    {
        input = GetComponent<IPlayerLookInput>();
        playerConfig = GetComponentInParent<Player_Config>();
    }

    private void Start()
    {
        if (input == null)
        {
            Debug.LogWarning($"[{gameObject.name}] IPlayerLookInput 컴포넌트를 찾을 수 없습니다.");
        }

        cameraTransform = playerConfig.PlayerCamera.parent; // 카메라의 부모 = Head
        bodyTransform = playerConfig.transform; // Player 루트 Transform

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        if (input == null) return;
        HandleLook();
        SyncPointingHand();
    }

    private void HandleLook()
    {
        float sensitivity = playerConfig.PlatformConfigData.PlayerRotSpeed;
        Vector2 lookInput = input.LookInput * sensitivity;

        pitch -= lookInput.y;
        pitch = Mathf.Clamp(pitch, -VerticalClampAngle, VerticalClampAngle);

        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f); // 카메라 자신 - 상하
        bodyTransform.Rotate(Vector3.up * lookInput.x);            // Player 루트 - 좌우
    }

    // Player_LookBehavior의 SyncPointingHand()
    private void SyncPointingHand()
    {
        PointingHand.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}