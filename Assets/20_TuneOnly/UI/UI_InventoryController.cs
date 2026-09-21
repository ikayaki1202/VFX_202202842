using UnityEngine;

public class UI_InventoryController : MonoBehaviour
{
    [Header("연결")]
    public GameObject InventoryPanel;
    public Transform SlotContainer;
    public UI_InventorySlot SlotPrefab;
    public Transform DropSpawnPoint; // 비우면 자기 위치 기준 앞쪽으로 드롭

    private Player_Inventory inventory;
    private Player_State playerState;
    private IPlayerUIInput uiInput;

    private bool isOpen;
    private PlayerInteractionState stateBeforeMenu;

    private Player_GrabBehavior grabBehavior;

    private void Awake()
    {
        inventory = GetComponentInParent<Player_Inventory>();
        playerState = GetComponentInParent<Player_State>();
        uiInput = GetComponentInParent<IPlayerUIInput>();
        grabBehavior = GetComponentInParent<Player_GrabBehavior>();

        if (inventory != null)
            inventory.OnInventoryChanged += RefreshSlots;
    }

    private void OnDestroy()
    {
        if (inventory != null)
            inventory.OnInventoryChanged -= RefreshSlots;
    }

    private void Start()
    {
        if (InventoryPanel != null) InventoryPanel.SetActive(false);
    }

    private void Update()
    {
        if (uiInput != null && uiInput.ToggleInventoryPressed)
        {
            if (isOpen) Close();
            else Open();
        }
    }

    private void Open()
    {
        isOpen = true;
        stateBeforeMenu = playerState.CurrentInteractionState; // 열기 전 상태 기억 (예: Grabing 중이었으면 그걸로 복귀)
        playerState.SetInteractionState(PlayerInteractionState.Menu);

        if (InventoryPanel != null) InventoryPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;

        RefreshSlots();
    }

    private void Close()
    {
        isOpen = false;

        // 인벤토리 안에서 장착이 일어났다면(HandlingObject가 채워짐), Idle이 아니라 Grabing으로 복원해야 함
        PlayerInteractionState restoreState = playerState.HandlingObject != null
            ? PlayerInteractionState.Grabing
            : stateBeforeMenu;

        playerState.SetInteractionState(restoreState);

        if (InventoryPanel != null) InventoryPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
    }

    private void RefreshSlots()
    {
        if (SlotContainer == null || SlotPrefab == null) return;

        foreach (Transform child in SlotContainer)
            Destroy(child.gameObject);

        foreach (ItemStack stack in inventory.Items)
        {
            UI_InventorySlot slot = Instantiate(SlotPrefab, SlotContainer);
            slot.Setup(stack, this);
        }
    }

    public void UseItem(SO_ItemData item)
    {
        switch (item.Type)
        {
            case ItemType.Equippable:
                grabBehavior.Equip(item);
                Close();
                break;

            case ItemType.Consumable:
                if (inventory.RemoveItem(item, 1))
                {
                    // TODO: 체력 시스템(IDamageable) 생기면 item.ConsumeEffectAmount 적용
                    Debug.Log($"[UI_InventoryController] {item.ItemName} 소비, 효과량: {item.ConsumeEffectAmount}");
                }
                Close();
                break;

            case ItemType.Collectable:
                Debug.Log($"[UI_InventoryController] {item.ItemName}은(는) 수집품이라 사용할 수 없습니다.");
                break;
        }
    }

    public void DropItem(SO_ItemData item)
    {
        // 지금 손에 쥐고 있는 게 바로 이 아이템이면, 새로 만들지 말고 실제로 들고 있는 걸 내려놓는다
        if (grabBehavior != null && grabBehavior.IsCurrentlyEquipped(item))
        {
            grabBehavior.DropEquippedFromInventory();
            Close();
            return;
        }

        if (!inventory.RemoveItem(item, 1)) return;

        if (item.WorldPrefab != null)
        {
            Vector3 spawnPos = DropSpawnPoint != null
                ? DropSpawnPoint.position
                : transform.position + transform.forward * 1.5f;

            GameObject instance = Instantiate(item.WorldPrefab, spawnPos, Quaternion.identity);

            if (instance.TryGetComponent(out Rigidbody rb))
            {
                rb.isKinematic = false;
                rb.linearVelocity = Vector3.zero;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }
        }
    }
}