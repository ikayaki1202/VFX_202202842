public interface IPlayerGrabInput
{
    bool DistanceGrabPressed { get; }   // 염력 시작 (마우스+G. 키를 놓아도 염력 유지. HandlingObject에 할당)

    bool DistanceGrabReleased { get; }   // 염력 해제 (마우스 Release. HandlingObject에 null 할당)
    bool DistancePullPressed { get; }   // 당겨오기 시작 (마우스+U. 키를 놓아도 유지. HandlingObject에 할당)

    bool DistancePullReleased { get; }   // 당겨오기 해제 (마우스 Release. HandlingObject에 null 할당)

    bool TouchGrabPressed { get; }      // GrabZone 충돌 + G 단독

    bool DropPressed { get; }           // 모든 놓기/취소를 통합 처리 (X)
    bool SwitchFireModePressed { get; } // 단발, 자동 전환

    bool AddToInventoryPressed { get; }
}