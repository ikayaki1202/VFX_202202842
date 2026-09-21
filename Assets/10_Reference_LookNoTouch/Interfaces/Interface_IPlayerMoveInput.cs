using UnityEngine;

// 기본적인 물리 이동 관련
public interface IPlayerMoveInput
{
    Vector2 MoveInput { get; }
    bool SprintInput { get; }
    bool JumpInput { get; }

    float SnapTurnInput { get; }
}