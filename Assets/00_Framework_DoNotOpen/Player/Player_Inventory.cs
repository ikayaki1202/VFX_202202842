using System;
using System.Collections.Generic;
using UnityEngine;

public class Player_Inventory : MonoBehaviour
{
    public int MaxSlots = 99;

    [field: SerializeField] public List<ItemStack> Items { get; private set; } = new();

    public event Action OnInventoryChanged;

    /// <returns>실제로 추가된 개수 (슬롯이 부족하면 요청한 수량보다 적을 수 있다)</returns>
    public int AddItem(SO_ItemData item, int quantity = 1)
    {
        if (item == null || quantity <= 0) return 0;

        int remaining = quantity;
        int added = 0;

        if (item.IsStackable)
        {
            int idx = Items.FindIndex(s => s.ItemData == item && s.Quantity < item.MaxStack);
            if (idx >= 0)
            {
                var stack = Items[idx];
                int space = item.MaxStack - stack.Quantity;
                int fillAmount = Mathf.Min(space, remaining);

                stack.Quantity += fillAmount;
                Items[idx] = stack;

                remaining -= fillAmount;
                added += fillAmount;
            }
        }

        while (remaining > 0)
        {
            if (Items.Count >= MaxSlots)
            {
                Debug.LogWarning($"[Player_Inventory] 인벤토리가 가득 찼습니다 (MaxSlots: {MaxSlots}). {remaining}개를 넣지 못했습니다.");
                break;
            }

            int stackAmount = item.IsStackable ? Mathf.Min(item.MaxStack, remaining) : 1;
            Items.Add(new ItemStack { ItemData = item, Quantity = stackAmount });

            remaining -= stackAmount;
            added += stackAmount;
        }

        if (added > 0) OnInventoryChanged?.Invoke();
        return added;
    }

    public bool RemoveItem(SO_ItemData item, int quantity = 1)
    {
        int idx = Items.FindIndex(s => s.ItemData == item);
        if (idx < 0 || Items[idx].Quantity < quantity) return false;

        var stack = Items[idx];
        stack.Quantity -= quantity;
        if (stack.Quantity <= 0) Items.RemoveAt(idx);
        else Items[idx] = stack;

        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool HasItem(SO_ItemData item, int quantity = 1)
    {
        int idx = Items.FindIndex(s => s.ItemData == item);
        return idx >= 0 && Items[idx].Quantity >= quantity;
    }
}