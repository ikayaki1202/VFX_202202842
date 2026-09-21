using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플랫폼 감지, Rig 활성화, 그리고 그로부터 확정되는 참조들(카메라, 손 위치 등)을 담당.
/// Awake에서 한 번 결정된 뒤로는 게임 내내 값이 바뀌지 않는 "설정" 성격의 데이터.
/// 계속 바뀌는 상태(MoveState, InteractionState 등)는 Player_State가 별도로 담당한다.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class Player_Config : MonoBehaviour
{
    [Header("수동 우선권 (Auto가 아니면 자동 감지 무시)")]
    public PlatformOverride platformOverride = PlatformOverride.Auto;

    [Header("Selected Platform")]
    public PlatformType CurrentPlatform;

    [Header("플랫폼 설정 에셋 (SO)")]
    public SO_PlatformConfigData PlatformConfigData_Desktop;
    public SO_PlatformConfigData PlatformConfigData_XR;

    [Header("Input Actions (모든 Controller가 공유할 수 있는 액션맵)")]
    public InputActionAsset InputActions;
    public string ActionMapName { get; private set; }


    // 외부에서는 읽기만 가능 - 전부 Player_Config 내부에서만 값이 채워짐
    public SO_PlatformConfigData PlatformConfigData { get; private set; }
    public Transform PlayerCamera { get; private set; }
    public Transform PointingHand { get; private set; }
    public Transform TeleportHand { get; private set; }
    public Transform GrabHolder { get; private set; }
    public Transform ItemHolder { get; private set; }

    void Awake()
    {
        SetPlatform();
        InitializeRig();
    }

    private void SetPlatform()
    {
        switch (platformOverride)
        {
            case PlatformOverride.ForceDesktop:
                CurrentPlatform = PlatformType.Desktop;
                break;

            case PlatformOverride.ForceXR:
                CurrentPlatform = PlatformType.XR;
                break;

            case PlatformOverride.Auto:
            default:
#if UNITY_ANDROID && !UNITY_EDITOR
    CurrentPlatform = PlatformType.XR;
#else
                CurrentPlatform = UnityEngine.XR.XRSettings.isDeviceActive ? PlatformType.XR : PlatformType.Desktop;
#endif
                break;
        }

        PlatformConfigData = (CurrentPlatform == PlatformType.Desktop) ? PlatformConfigData_Desktop : PlatformConfigData_XR;
        Debug.Log($"<color=white>[Platform]</color> <b>{CurrentPlatform} 환경 활성화</b> (Override: {platformOverride})");

        ActionMapName = (CurrentPlatform == PlatformType.Desktop) ? "Desktop" : "XR"; 

    }

    private void InitializeRig()
    {
        if (PlatformConfigData == null)
        {
            Debug.LogError("[Player_Config] 활성화된 PlatformConfig 데이터가 없습니다! 인스펙터를 확인하세요.");
            return;
        }

        Player_PlatformRigReferences[] allRigs = GetComponentsInChildren<Player_PlatformRigReferences>(true);

        foreach (var rig in allRigs)
        {
            if (rig.CurrentPlatform == PlatformConfigData.CurrentPlatform)
            {
                rig.gameObject.SetActive(true);
                PlayerCamera = rig.MainCamera;
                PointingHand = rig.PointingHand;
                TeleportHand = rig.TeleportHand;
                GrabHolder = rig.GrabHolder;
                ItemHolder = rig.ItemHolder;
            }
            else
            {
                rig.gameObject.SetActive(false);
            }
        }
    }
}