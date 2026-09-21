using UnityEngine;

public partial class Player_GrabBehavior : MonoBehaviour
{
    private enum GrabSource { None, Telekinesis, Pull, Touch, Quickslot }

    private IPlayerGrabInput input;
    private IPlayerPointInput pointInput;
    private Player_Config playerConfig;
    private Player_State playerState;
    private CharacterController playerCharacterController;
    private Player_Inventory inventory;        
    private SO_ItemData equippedItemData;      

    [Header("Distance Grab / Distance Pull (레이캐스트 공용)")]
    public LayerMask GrabbableLayer = ~0;

    private GrabSource currentSource = GrabSource.None;

    private IPlayerQuickSlotInput quickSlotInput;

    private void Awake()
    {
        input = GetComponent<IPlayerGrabInput>();
        pointInput = GetComponent<IPlayerPointInput>();
        playerConfig = GetComponentInParent<Player_Config>();
        playerState = GetComponentInParent<Player_State>();
        playerCharacterController = GetComponentInParent<CharacterController>();
        inventory = GetComponentInParent<Player_Inventory>();  // ← 이 줄 추가

        if (pointInput == null)
            Debug.LogWarning($"[{nameof(Player_GrabBehavior)}] IPlayerPointInput을 찾을 수 없어 DistanceGrab/DistancePull이 동작하지 않습니다.");

        if (playerState != null)
            playerState.OnInteractionStateChanged += HandleInteractionStateChanged;
    }

    private void Start()
    {
        InitHandVelocityTracking();
        InitVisuals();
        InitTouch();
    }

    private void OnDestroy()
    {
        TeardownTouch();

        if (playerState != null)
            playerState.OnInteractionStateChanged -= HandleInteractionStateChanged;
    }

    private void Update()
    {
        TrackHandVelocity();

        // 들고 있던 오브젝트가 외부 요인(소비/파괴 등)으로 사라진 경우, 상태를 강제로 정리
        if (playerState.HandlingObject == null && currentSource != GrabSource.None)
        {
            ClearHeldState();
        }

        if (input == null) return;

        HandleTelekinesisInput();
        HandlePullInput();
        HandleTouchInput();

        if (input.DropPressed)
        {
            ForceReleaseCurrent();
        }

        // Pull/Touch로 손에 쥐고 있는 동안, 눌림/뗌 순간을 대상(무기)에게 그대로 전달한다.
        // 매프레임 반복 여부는 대상 스스로 판단한다 (예: UseBehavior_Firearm의 Auto 연사).
        //bool isHoldingSomething = (currentSource == GrabSource.Pull || currentSource == GrabSource.Touch)
        //                           && playerState.HandlingObject != null;
        bool isHoldingSomething = (currentSource == GrabSource.Pull || currentSource == GrabSource.Touch || currentSource == GrabSource.Quickslot)
                           && playerState.HandlingObject != null;

        if (isHoldingSomething && pointInput != null
            && playerState.HandlingObject.TryGetComponent<IInteractable>(out var usable))
        {
            if (pointInput.Pressed) usable.OnInteractionStart(gameObject);
            if (pointInput.Released) usable.OnInteractionEnd(gameObject);
        }

        if (input.AddToInventoryPressed && playerState.HandlingObject != null)
        {
            TryLootHeldObject();
        }

        if (quickSlotInput != null)
        {
            int idx = quickSlotInput.QuickSlotPressedIndex;
            if (idx >= 0 && idx < inventory.Items.Count)
            {
                Equip(inventory.Items[idx].ItemData);
            }
        }
    }

    private void ForceReleaseCurrent()
    {
        switch (currentSource)
        {
            case GrabSource.Telekinesis:
                EndTelekinesis();
                break;
            case GrabSource.Pull:
            case GrabSource.Touch:
                if (isFlyingToHand) AbortFlight();
                else DoDrop();
                break;
            case GrabSource.Quickslot:
                inventory?.RemoveItem(equippedItemData, 1);
                equippedItemData = null;
                DoDrop();
                break;
        }
    }

    private void HandleInteractionStateChanged(PlayerInteractionState newState)
    {
        // Grabing/Looting은 이 시스템 자신이 만드는 상태라 건드리지 않고, Menu일 때만 멈춘다.
        enabled = newState != PlayerInteractionState.Menu;
    }
}