using UnityEngine;
using UnityEngine.UI;
using TMPro; // 추가

public class UI_InventorySlot : MonoBehaviour
{
    [Header("UI 연결")]
    public Image IconImage;
    public TMP_Text QuantityText; // Text → TMP_Text로 변경
    public Button UseButton;
    public Button DropButton;

    private SO_ItemData itemData;
    private UI_InventoryController controller;

    public void Setup(ItemStack stack, UI_InventoryController owner)
    {
        itemData = stack.ItemData;
        controller = owner;

        if (IconImage != null) IconImage.sprite = itemData.Icon;
        if (QuantityText != null)
            QuantityText.text = stack.Quantity > 1 ? stack.Quantity.ToString() : "";

        UseButton?.onClick.RemoveAllListeners();
        UseButton?.onClick.AddListener(() => controller.UseItem(itemData));

        DropButton?.onClick.RemoveAllListeners();
        DropButton?.onClick.AddListener(() => controller.DropItem(itemData));
    }
}