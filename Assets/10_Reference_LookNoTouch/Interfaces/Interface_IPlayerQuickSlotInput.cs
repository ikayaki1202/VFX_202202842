public interface IPlayerQuickSlotInput
{
    int QuickSlotPressedIndex { get; } // 이번 프레임에 눌린 슬롯(0~8), 없으면 -1
}