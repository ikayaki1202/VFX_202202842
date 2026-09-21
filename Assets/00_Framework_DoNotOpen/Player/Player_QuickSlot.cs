using UnityEngine;
using UnityEngine.InputSystem;

public class Player_QuickSlotController : MonoBehaviour, IPlayerQuickSlotInput
{
    [SerializeField] private InputActionProperty[] slotActions; // 인스펙터에서 9개 등록 (1~9키 각각)

    public int QuickSlotPressedIndex
    {
        get
        {
            for (int i = 0; i < slotActions.Length; i++)
            {
                if (slotActions[i].action != null && slotActions[i].action.WasPressedThisFrame())
                    return i;
            }
            return -1;
        }
    }

    private void OnEnable()
    {
        foreach (var slot in slotActions) slot.action?.Enable();
    }

    private void OnDisable()
    {
        foreach (var slot in slotActions) slot.action?.Disable();
    }
}