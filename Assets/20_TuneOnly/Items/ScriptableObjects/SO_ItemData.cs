using UnityEngine;

[CreateAssetMenu(fileName = "SO_ItemData", menuName = "Scriptable Objects/SO_ItemData")]
public class SO_ItemData : ScriptableObject
{
    [Header("Identity")]
    public string ItemId;
    public string ItemName;
    [TextArea] public string Description;
    public Sprite Icon;

    [Header("Classification")]
    public ItemCategory Category;
    public ItemType Type;

    [Header("Stacking")]
    public bool IsStackable = false;
    public int MaxStack = 1;

    [Header("Equip Link")]
    public GameObject WorldPrefab;

    [Header("Consumable 전용 (Type=Consumable일 때만 사용)")]
    public float ConsumeEffectAmount;  

    private void OnValidate()
    {
        if (!IsStackable) MaxStack = 1;
        if (string.IsNullOrEmpty(ItemId)) ItemId = name;
    }
}