using UnityEngine;

/// <summary>
/// 실제 이동/점프/등반 로직. 튜닝값은 전부 Player_Config.PlatformConfigData(SO)에서 읽어온다 -
/// 자체 필드로 값을 들고 있지 않으므로 Desktop/XR 애셋을 갈아끼우면 자동으로 값이 달라진다.
/// 상태 전이는 Player_State에 기록해서 다른 시스템(애니메이션, UI 등)이 이벤트로 반응할 수 있게 한다.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class Player_MoveBehavior : MonoBehaviour
{
    private CharacterController character;
    private IPlayerMoveInput input;
    private Player_Config playerConfig;
    private Player_State playerState;

    private Vector3 moveVelocity;
    private ClimbType currentClimbType = ClimbType.None;
    private bool snapReady = true; // 스틱이 중앙으로 돌아왔는지(연속 스냅턴 방지용 히스테리시스)

    private bool isMantling;

    void Awake()
    {
        character = GetComponent<CharacterController>();
        input = GetComponent<IPlayerMoveInput>();
        playerConfig = GetComponentInParent<Player_Config>();
        playerState = GetComponentInParent<Player_State>();
    }

    private void Start()
    {
        playerState.SetPlayerState(PlayerState.Idle);
        currentClimbType = ClimbType.None;
    }

    void Update()
    {
        if (input == null || character == null || !character.enabled) return;

        var state = playerState.CurrentPlayerState;

        if (state != PlayerState.Teleport)
        {
            HandleSnapTurn();
            HandleMoveState(state);
        }

        if (!isMantling) // 추가: 맨틀 진행/시작 중엔 CharacterController.Move 자체를 건너뜀
            character.Move(moveVelocity * Time.deltaTime);
    }
    // void Update()
    // {
    //     if (input == null || character == null || !character.enabled) return;

    //     var state = playerState.CurrentPlayerState;

    //     if (state != PlayerState.Teleport)
    //     {
    //         HandleSnapTurn();
    //         HandleMoveState(state);
    //     }

    //     character.Move(moveVelocity * Time.deltaTime);
    // }

    void HandleMoveState(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle:
            case PlayerState.Walk:
            case PlayerState.Sprint:
            case PlayerState.Crouch:
                MoveGround();
                break;
            case PlayerState.Jump:
            case PlayerState.Fall:
                MoveAir();
                break;
            case PlayerState.Climb:
                MoveVertical();
                break;
        }
    }

    void MoveGround()
    {
        var config = playerConfig.PlatformConfigData;

        Vector2 moveInput = input.MoveInput;
        Vector3 dirInput = playerConfig.PlayerCamera.forward * moveInput.y
                          + playerConfig.PlayerCamera.right * moveInput.x;
        dirInput.y = 0f;

        bool isMoving = dirInput.magnitude > 0.1f;
        float speed = input.SprintInput ? config.RunSpeed : config.WalkSpeed;

        if (isMoving)
        {
            moveVelocity.x = dirInput.normalized.x * speed;
            moveVelocity.z = dirInput.normalized.z * speed;
        }
        else
        {
            moveVelocity.x *= config.Friction;
            moveVelocity.z *= config.Friction;
        }

        if (!isMoving)
            playerState.SetPlayerState(PlayerState.Idle);
        else if (input.SprintInput)
            playerState.SetPlayerState(PlayerState.Sprint);
        else
            playerState.SetPlayerState(PlayerState.Walk);

        if (input.JumpInput && character.isGrounded)
        {
            moveVelocity.y = Mathf.Sqrt(config.JumpHeight * -2f * config.Gravity);
            playerState.SetPlayerState(PlayerState.Jump);
        }
        else
        {
            moveVelocity.y = -2f; // 접지 안정화
        }

        if (!character.isGrounded && moveVelocity.y < 0f)
        {
            playerState.SetPlayerState(PlayerState.Fall);
        }
    }

    void MoveAir()
    {
        var config = playerConfig.PlatformConfigData;

        Vector2 moveInput = input.MoveInput;
        Vector3 dirInput = transform.forward * moveInput.y + transform.right * moveInput.x;
        moveVelocity.x = dirInput.x * config.WalkSpeed;
        moveVelocity.z = dirInput.z * config.WalkSpeed;
        moveVelocity.y += config.Gravity * Time.deltaTime;

        if (playerState.CurrentPlayerState == PlayerState.Jump && moveVelocity.y <= 0f)
        {
            playerState.SetPlayerState(PlayerState.Fall);
        }

        if (character.isGrounded)
        {
            playerState.SetPlayerState(PlayerState.Idle);
        }
    }

    //void MoveVertical()
    //{
    //    var config = playerConfig.PlatformConfigData;

    //    Vector2 moveInput = input.MoveInput;
    //    moveVelocity = Vector3.zero;

    //    if (currentClimbType == ClimbType.Ladder)
    //    {
    //        moveVelocity.y = moveInput.y * config.ClimbSpeed;
    //    }
    //    else if (currentClimbType == ClimbType.Cliff)
    //    {
    //        moveVelocity = transform.right * moveInput.x * config.ClimbSpeed + Vector3.up * moveInput.y * config.ClimbSpeed;
    //        moveVelocity += transform.forward * config.ClimbOffset;
    //    }

    //    // 벽에서 뛰어내리기 - 반대 방향 + 위쪽으로 튕겨나가듯
    //    if (input.JumpInput)
    //    {
    //        Vector3 jumpDir = (-transform.forward + Vector3.up).normalized;
    //        moveVelocity = jumpDir * Mathf.Sqrt(config.JumpHeight * -2f * config.Gravity);

    //        currentClimbType = ClimbType.None;
    //        playerState.SetPlayerState(PlayerState.Fall);
    //    }
    //}
    void MoveVertical()
    {
        var config = playerConfig.PlatformConfigData;

        Vector2 moveInput = input.MoveInput;
        moveVelocity = Vector3.zero;

        if (isMantling) return; // 맨틀 진행 중엔 일반 등반 이동 무시

        if (currentClimbType == ClimbType.Ladder)
        {
            moveVelocity.y = moveInput.y * config.ClimbSpeed;
        }
        else if (currentClimbType == ClimbType.Cliff)
        {
            // 위로 오르려는 입력이 있을 때만 맨틀 가능 여부 확인
            if (moveInput.y > 0.1f && TryMantle())
                return;

            moveVelocity = transform.right * moveInput.x * config.ClimbSpeed + Vector3.up * moveInput.y * config.ClimbSpeed;
            moveVelocity += transform.forward * config.ClimbOffset;
        }

        if (input.JumpInput)
        {
            Vector3 jumpDir = (-transform.forward + Vector3.up).normalized;
            moveVelocity = jumpDir * Mathf.Sqrt(config.JumpHeight * -2f * config.Gravity);

            currentClimbType = ClimbType.None;
            playerState.SetPlayerState(PlayerState.Fall);
        }
    }

    private bool TryMantle()
    {
        var config = playerConfig.PlatformConfigData;

        Vector3 headOrigin = transform.position + Vector3.up * character.height;

        // 1. 머리 위쪽 방향이 막혀있으면 아직 꼭대기가 아님 - 계속 등반
        if (Physics.Raycast(headOrigin, transform.forward, config.MantleCheckDistance))
            return false;

        // 2. 뚫린 지점에서 바로 아래로 디딜 바닥이 있는지 확인
        Vector3 downOrigin = headOrigin + transform.forward * config.MantleCheckDistance + Vector3.up * 0.1f;
        if (!Physics.Raycast(downOrigin, Vector3.down, out RaycastHit hit, character.height + config.MantleMaxLedgeDrop))
            return false;

        StartCoroutine(MantleTo(hit.point));
        return true;
    }

    private System.Collections.IEnumerator MantleTo(Vector3 ledgePoint)
    {
        isMantling = true;
        currentClimbType = ClimbType.None;
        playerState.SetPlayerState(PlayerState.Teleport); // Update()가 이 상태일 때 일반 이동 로직을 건너뜀

        Vector3 startPos = transform.position;
        Vector3 targetPos = ledgePoint + Vector3.up * (character.height * 0.5f + 0.05f);

        character.enabled = false; // 일반 충돌 판정 우회

        float elapsed = 0f;
        var config = playerConfig.PlatformConfigData;
        while (elapsed < config.MantleDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / config.MantleDuration);
            yield return null;
        }
        transform.position = targetPos;

        character.enabled = true;
        isMantling = false;
        moveVelocity = Vector3.zero;
        playerState.SetPlayerState(PlayerState.Idle);
    }

    void HandleSnapTurn()
    {
        var config = playerConfig.PlatformConfigData;
        if (!config.canSnapTurn) return;

        float turnInput = input.SnapTurnInput;

        if (Mathf.Abs(turnInput) > config.snapTurnThreshold && snapReady)
        {
            float turnAngle = turnInput > 0 ? 45f : -45f;
            transform.Rotate(Vector3.up, turnAngle);
            snapReady = false;
        }
        else if (Mathf.Abs(turnInput) < 0.2f)
        {
            snapReady = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ClimbableLadder"))
        {
            currentClimbType = ClimbType.Ladder;
            playerState.SetPlayerState(PlayerState.Climb);
        }
        if (other.CompareTag("ClimbableCliff"))
        {
            currentClimbType = ClimbType.Cliff;
            playerState.SetPlayerState(PlayerState.Climb);
        }
    }

    //void OnTriggerExit(Collider other)
    //{
    //    if (other.CompareTag("ClimbableLadder") || other.CompareTag("ClimbableCliff"))
    //    {
    //        currentClimbType = ClimbType.None;
    //        playerState.SetPlayerState(PlayerState.Fall);
    //    }
    //}
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ClimbableLadder") || other.CompareTag("ClimbableCliff"))
        {
            currentClimbType = ClimbType.None;

            // 이미 상판에 착지했다면 Fall이 아니라 Idle로 — 그렇지 않으면
            // MoveAir()의 중력이 즉시 끌어내려 트리거 경계에서 오르내림이 반복됨
            playerState.SetPlayerState(character.isGrounded ? PlayerState.Idle : PlayerState.Fall);
        }
    }
}