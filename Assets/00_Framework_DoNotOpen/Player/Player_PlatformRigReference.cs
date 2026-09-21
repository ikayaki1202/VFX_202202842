using UnityEngine;
public class Player_PlatformRigReferences : MonoBehaviour
{
    public PlatformType CurrentPlatform; // 이제 Enums.cs의 PlatformType(Desktop, XR) 사용
    [Header("이 릭의 실제 오브젝트들")]
    public Transform MainCamera;
    public Transform PointingHand;
    public Transform TeleportHand;
    public Transform ItemHolder;
    public Transform GrabHolder;
}