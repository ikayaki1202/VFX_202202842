using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Grabbable_Handheld : MonoBehaviour, IGrabbable, IInteractable
{
    [Header("Grab Settings")]
    [SerializeField] private Vector3 holdOffset;
    [SerializeField] private float throwForceMultiplier = 1f;
    [Header("작용 지점 (총구, 칼끝 등 - 비워두면 이 오브젝트 기준)")]
    [SerializeField] private Transform muzzlePoint;

    public Vector3 HoldOffset => holdOffset;
    public float ThrowForceMultiplier => throwForceMultiplier;
    public Transform MuzzlePoint => muzzlePoint != null ? muzzlePoint : transform;
    public bool IsPressing { get; private set; }

    private IUsable usable;
    private bool isArmed;

    private void Awake()
    {
        usable = GetComponent<IUsable>();
        if (usable == null)
            Debug.LogWarning($"[{nameof(Grabbable_Handheld)}] {name}에 IUsable 컴포넌트가 없어 Use 입력이 무시됩니다.");
    }

    public void OnGrab(GameObject sender)
    {
        isArmed = false;
        IsPressing = false;
        (usable as IUsableInit)?.OnEquipped(sender);
    }

    // 신규: 인벤토리/퀵슬롯 경유 장착 시, "첫 클릭 방지" 없이 바로 사용 가능하게 함
    public void Arm() => isArmed = true;

    public void OnRelease(GameObject sender)
    {
        if (IsPressing)
        {
            usable?.OnUseReleased(sender);
            IsPressing = false;
        }
        (usable as IUsableInit)?.OnUnequipped(sender);
        isArmed = false;
    }

    public void OnInteractionStart(GameObject sender)
    {
        if (!isArmed) return;
        IsPressing = true;
        usable?.OnUsePressed(sender);
    }

    public void OnInteractionEnd(GameObject sender)
    {
        if (!isArmed)
        {
            isArmed = true;
            return;
        }
        if (IsPressing)
        {
            IsPressing = false;
            usable?.OnUseReleased(sender);
        }
    }
}