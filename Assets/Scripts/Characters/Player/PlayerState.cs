using UnityEngine;

public class PlayerState : BaseState
{
    protected PlayerController playerController;
    protected ColliderState colliderState;
    public bool isJumping;
    public bool isCrouching;

    public PlayerState(PlayerController pc)
    {
        playerController = pc;
        colliderState = pc.GetComponent<ColliderState>();
    }
    public override void Enter() { }
    public override void Update()
    {
        // 모든 상태에서 조건에 의해 전환 가능한 상태들

        //#점프 가능 조건
        if (playerController.isJumpInput)
        {
            // 일반 점프 가능 조건
            if (colliderState.isGrounded)
            {
                isJumping = true;

                playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.Jump]);
            }
            // 코요테 점프 가능 조건
            else if (colliderState.coyoteTimeCounter > 0f && !isJumping &&
                playerController.stateMachine.CurState != playerController.stateMachine.stateDic[PlayerStateTypes.Jump])
            {
                isJumping = true;

                playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.Jump]);
            }
            // 더블 점프 가능 조건
            /*else if (isJumping && 2단 점프 가능)
            {
                playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.DoubleJump]);
            }*/
        }

        //#방향키 아래(웅크리기, 앉아 걷기, 지면 슬라이딩) 조건들
        // 떨어지는 상태에서 지면에 닿자마자 상태 변환하면 안되기에 예외처리
        // Fall 상태의 FixedUpdate에서 플레이어의 발을 지면에 스냅하는 로직이 있기 때문.
        if (Mathf.Sign(playerController.InputDir.y) == -1 && colliderState.isGrounded && playerController.stateMachine.CurState != playerController.stateMachine.stateDic[PlayerStateTypes.Fall])
        {
            if (playerController.finalHorizontalVelocity.magnitude > playerController.GetSlidingSpeedMin())
            {
                playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.GroundSlide]);
            }
            else if (playerController.finalHorizontalVelocity.magnitude < 0.1f)
            {
                playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.CrouchIdle]);
            }
            else
            {
                playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.CrouchMove]);
            }
        }

        //#낙하 조건
        if (!colliderState.isGrounded &&
            playerController.stateMachine.CurState != playerController.stateMachine.stateDic[PlayerStateTypes.Jump])
        {
            playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.Fall]);
        }
    }

    public override void Exit() { }
}

public class Player_Idle : PlayerState
{
    public Player_Idle(PlayerController pc) : base(pc)
    {

    }

    public override void Enter()
    {
        playerController.animator.Play(playerController.IDLE_HASH);
    }

    public override void Update()
    {
        base.Update();
        if (Mathf.Abs(playerController.InputDir.x) > 0.1f)
        {
            playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.Walk]);
        }

        /*if (Mathf.Sign(playerController.InputDir.y) == -1)
        {
            playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.CrouchIdle]);
        }*/
    }

    public override void FixedUpdate()
    {
        // Velocity 초기화 용도
        playerController.SimulateFinalVelocity();
    }
    public override void Exit() { }
}

public class Player_Walk : PlayerState
{
    public Player_Walk(PlayerController pc) : base(pc)
    {

    }
    public override void Enter()
    {
        playerController.animator.Play(playerController.WALK_HASH);
    }
    public override void Update()
    {
        base.Update();

        if (playerController.isSprintInput && colliderState.isGrounded)
        {
            playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.Sprint]);
        }

        // Idle 전이.
        if (Mathf.Abs(playerController.InputDir.x) < 0.1f) // 조건 추가 필요
        {
            playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.Idle]);
        }

        // 왼쪽 오른쪽 방향 전환.
        playerController.SetSpriteDir();
    }
    public override void FixedUpdate()
    {
        playerController.SimulateFinalVelocity();
    }
    public override void Exit() { }
}

public class Player_Sprint : PlayerState
{
    public Player_Sprint(PlayerController pc) : base(pc)
    {

    }
    public override void Enter()
    {
        playerController.animator.Play(playerController.SPRINT_HASH);
    }
    public override void Update()
    {
        base.Update();

        if (!playerController.isSprintInput && colliderState.isGrounded)
        {
            playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.Walk]);
        }

        if (Mathf.Abs(playerController.InputDir.x) < 0.1f)
        {
            playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.Idle]);
        }

        // 왼쪽 오른쪽 방향 전환.
        playerController.SetSpriteDir();
    }
    public override void FixedUpdate()
    {
        playerController.SimulateFinalVelocity();
    }
    public override void Exit() { }

}

public class Player_Jump : PlayerState
{
    public Player_Jump(PlayerController pc) : base(pc)
    {

    }

    public override void Enter()
    {
        playerController.animator.Play(playerController.JUMP_HASH);
        playerController.SetJumpVelocity();
        colliderState.SetColliderToQuaternion(Quaternion.identity);
        colliderState.isGrounded = false;
        colliderState.isGroundCheckWait = true;
    }

    public override void Update()
    {
        base.Update();

        if (playerController.finalVerticalVelocity.y <= 0)
            playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.Fall]);

        // 왼쪽 오른쪽 방향 전환.
        playerController.SetSpriteDir();
    }

    public override void FixedUpdate()
    {
        // 중력 계속 적용
        playerController.ApplyGravity();
        playerController.ApplyJumpCut();

        // 점프 테스트용. 삭제필요
        playerController.SimulateFinalVelocity();
    }
}

public class Player_Fall : PlayerState
{
    private bool groundLanded = false;

    public Player_Fall(PlayerController pc) : base(pc)
    {

    }

    public override void Enter()
    {
        playerController.animator.Play(playerController.FALL_HASH);
        colliderState.SetColliderToQuaternion(Quaternion.identity);
        colliderState.isGroundCheckWait = false;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        // 왼쪽 오른쪽 방향 전환.
        playerController.SetSpriteDir();

        if (groundLanded)
        {
            // 플래그 리셋
            groundLanded = false;

            if (playerController.isJumpInput)
                playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.Jump]);
            else if (Mathf.Abs(playerController.InputDir.x) > 0.1f)
            {
                playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.Walk]);
                isJumping = false;
            }
            else
            {
                playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.Idle]);
                isJumping = false;
            }
        }
    }

    public override void FixedUpdate()
    {
        // 콜라이더 상태 : isGrounded가 false라면
        if (!colliderState.isGrounded)
        {
            // 중력 계속 적용
            playerController.ApplyGravity();

            playerController.SimulateFinalVelocity();
        }
        // 콜라이더 상태 : isGrounded가 true라면
        else
        {
            // 플래그 설정
            groundLanded = true;

            // 바닥에 닿음. 수직 속도 0으로 설정
            playerController.finalVerticalVelocity = Vector2.zero;

            // 현재 Ground 오브젝트의 Y값으로 위치 설정
            // 여기를 점 점으로 맞춰줘야함
            // Todo: 바닥 법선벡터에 플레이어 맞추기, 바닥에 맞춰서 콜라이더 각도&위치 맞추기
            //playerController.SetGroundPosY(colliderState.groundHitColids[0].transform.parent.position.y);
            colliderState.SetColliderToQuaternion(colliderState.groundHitColids[0].transform.rotation);
            // 기존 이동 벡터 -> 바닥 각도에 맞는 이동 벡터로 변환
            playerController.finalHorizontalVelocity = playerController.finalHorizontalVelocity.magnitude * playerController.playerCollider.transform.right * Mathf.Sign(playerController.finalHorizontalVelocity.x);
            playerController.transform.position = colliderState.groundHitPos;

            // 점프 계속 입력 시 다시 점프, 입력없을 시 Idle상태로 전환
        }
    }
}

public class Player_CrouchIdle : PlayerState
{
    public Player_CrouchIdle(PlayerController pc) : base(pc)
    {

    }

    public override void Enter()
    {
        playerController.animator.Play(playerController.CrouchIdle_HASH);
    }


    public override void Update()
    {
        base.Update();

        if (Mathf.Sign(playerController.InputDir.y) != -1)
        {
            playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.Idle]);
        }

        else if (Mathf.Abs(playerController.InputDir.x) > 0.1f)
        {
            playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.CrouchMove]);
        }
    }

    public override void FixedUpdate()
    {
        playerController.SimulateFinalVelocity();
    }
}

public class Player_CrouchMove : PlayerState
{
    public Player_CrouchMove(PlayerController pc) : base(pc)
    {

    }

    public override void Enter()
    {
        playerController.animator.Play(playerController.CrouchMove_HASH);
    }

    public override void Update()
    {
        base.Update();

        if (Mathf.Sign(playerController.InputDir.y) != -1 && Mathf.Abs(playerController.InputDir.x) < 0.1f)
        {
            playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.Idle]);
        }

        else if (Mathf.Sign(playerController.InputDir.y) == -1 && Mathf.Abs(playerController.InputDir.x) < 0.1f)
        {
            playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.CrouchIdle]);
        }

        else if (Mathf.Sign(playerController.InputDir.y) != -1 && Mathf.Abs(playerController.InputDir.x) >= 0.1f) 
        {
            playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.Walk]);
        }

        playerController.SetSpriteDir();
    }

    public override void FixedUpdate()
    {
        playerController.SimulateFinalVelocity();
    }
}

public class Player_GroundSlide : PlayerState
{
    public Player_GroundSlide(PlayerController pc) : base(pc)
    {

    }

    public override void Enter()
    {
        playerController.animator.Play(playerController.GroundSlide_HASH);
    }


    public override void Update()
    {
        base.Update();

        if (Mathf.Sign(playerController.InputDir.y) != -1 && Mathf.Abs(playerController.InputDir.x) < 0.1f)
        {
            playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.Idle]);
        }

        else if (Mathf.Sign(playerController.InputDir.y) == -1 && Mathf.Abs(playerController.InputDir.x) < 0.1f)
        {
            playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.CrouchIdle]);
        }

        else if (Mathf.Sign(playerController.InputDir.y) != -1 && Mathf.Abs(playerController.InputDir.x) >= 0.1f)
        {
            playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.Walk]);
        }
    }

    public override void FixedUpdate()
    {
        playerController.SimulateFinalVelocity();
    }
}
public enum PlayerStateTypes
{
    Idle, Walk, Sprint, Brake, Jump, Fall, CrouchIdle, CrouchMove, GroundSlide, WallSlide, Attack01, Attack02, Attack03
}