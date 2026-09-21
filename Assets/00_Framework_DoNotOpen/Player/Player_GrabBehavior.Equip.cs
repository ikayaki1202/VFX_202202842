using UnityEngine;

public partial class Player_GrabBehavior
{
    public void Equip(SO_ItemData item)
    {
        UnequipCurrent();

        if (item == null || item.WorldPrefab == null) return;

        GameObject instance = Instantiate(item.WorldPrefab, playerConfig.GrabHolder);

        if (!instance.TryGetComponent<IGrabbable>(out var grabbable))
        {
            Debug.LogWarning($"[{nameof(Player_GrabBehavior)}] {item.ItemName}의 WorldPrefab에 IGrabbable이 없어 장착할 수 없습니다.");
            Destroy(instance);
            return;
        }

        instance.transform.localPosition = grabbable.HoldOffset;
        instance.transform.localRotation = Quaternion.identity;

        if (instance.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
            heldRigidbody = rb;
        }
        if (instance.TryGetComponent(out Collider col))
        {
            heldCollider = col;
            if (playerCharacterController != null)
                Physics.IgnoreCollision(col, playerCharacterController, true);
        }

        heldGrabbable = grabbable;
        heldTransform = instance.transform;
        currentSource = GrabSource.Quickslot;
        equippedItemData = item;

        grabbable.OnGrab(gameObject);

        if (grabbable is Grabbable_Handheld handheld)
            handheld.Arm();

        playerState.SetHandlingObject(instance);
        playerState.SetInteractionState(PlayerInteractionState.Grabing);
    }

    private void UnequipCurrent()
    {
        if (playerState.HandlingObject == null) return;

        if (currentSource == GrabSource.Quickslot)
        {
            // 인벤토리에서 빼지 않는다 — "보유"는 유지, 손에서만 내려놓는 것 (스왑)
            heldGrabbable?.OnRelease(gameObject);
            Destroy(playerState.HandlingObject);
            ClearHeldState();
        }
        else
        {
            // 기존 물리 그랩(Pull/Touch)은 원래 방식대로 세상에 떨어뜨림
            DoDrop();
        }
    }

    public bool IsCurrentlyEquipped(SO_ItemData item)
    {
        return currentSource == GrabSource.Quickslot && equippedItemData == item;
    }

    public void DropEquippedFromInventory()
    {
        if (currentSource != GrabSource.Quickslot) return;

        inventory.RemoveItem(equippedItemData, 1);
        equippedItemData = null;
        DoDrop();
    }
}