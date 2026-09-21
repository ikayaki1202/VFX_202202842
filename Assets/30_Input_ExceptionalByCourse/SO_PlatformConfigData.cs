using UnityEngine;

[CreateAssetMenu(fileName = "SO_PlatformConfigData", menuName = "Scriptable Objects/SO_PlatformConfigData")]
public class SO_PlatformConfigData : ScriptableObject
{
    [Header("Platform Settings")]
    public PlatformType CurrentPlatform;
    public string ConfigName;

    [Header("Core Values")]
    public float Gravity = -9.81f;
    public float PlayerRotSpeed = 0.1f;

    [Header("Move Settings")]
    public float WalkSpeed = 1.5f;
    public float RunSpeed = 3.5f;
    public float Friction = 0.9f;
    public float ClimbSpeed = 1.2f;
    public float ClimbOffset = 0.1f;
    public float JumpHeight = 2.0f;

    [Header("Mantle (절벽 꼭대기 올라서기)")]
    public float MantleCheckDistance = 0.6f;   
    public float MantleMaxLedgeDrop = 1.5f;    
    public float MantleDuration = 0.25f;       

    [Header("XR Only - SnapTurn Settings")]
    public bool canSnapTurn = true;
    public float snapTurnThreshold = 0.5f;

    [Header("UI / Canvas")]
    public RenderMode canvasRenderMode = RenderMode.ScreenSpaceCamera;
    public float canvasPlaneDistance = 1f;
    public Vector3 canvasWorldScale = Vector3.one * 0.001f;

    [Header("Point Settings")]
    public float PointMaxDistance = 20f;

    [Header("Grab Settings")]
    public float GrabMaxDistance = 10f;
    public float TelekinesisFollowSpeed = 15f;
    public float TelekinesisRotationSpeed = 360f;
    public float PullFlySpeed = 10f;
    public float PullArrivalThreshold = 0.05f;
    public float MaxThrowSpeed = 15f;

    [Header("Aim Visuals (Point/Grab 공용)")]
    public float AimLineWidth = 0.01f;

    [Header("투척 방식")]
    public bool UseAimedThrow = false;   // Desktop=true, XR=false로 각 애셋에서 설정
    public float AimedThrowSpeed = 12f;  // 조준 던지기 시 고정 속도
}