using UnityEngine;

public class PlayerState : BaseState
{
    protected PlayerController pc;
    protected ColliderState colliderState;
    public bool isJumping;
    public bool isCrouching;

    public PlayerState(PlayerController pc)
    {
        this.pc = pc;
        colliderState = pc.GetComponent<ColliderState>();
    }
    public override void Enter() { }
    public override void Update()
    {
        // 모든 상태에서 조건에 의해 전환 가능한 상태들

        //#점프 가능 조건
        if (pc.isJumpInput)
        {
            // 일반 점프 가능 조건
            if (colliderState.isGrounded)
            {
                isJumping = true;

                pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.Jump]);
            }
            // 코요테 점프 가능 조건
            else if (colliderState.coyoteTimeCounter > 0f && !isJumping &&
                pc.stateMachine.CurState != pc.stateMachine.stateDic[PlayerStateTypes.Jump])
            {
                isJumping = true;

                pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.Jump]);
            }
            // 더블 점프 가능 조건
            /*else if (isJumping && 2단 점프 가능[어빌리티 매니저에서 더블점프 능력이 활성화 되었다면])
            {
                playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.DoubleJump]);
            }*/
        }

        //#방향키 아래(웅크리기, 앉아 걷기, 지면 슬라이딩) 조건들
        // 떨어지는 상태에서 지면에 닿자마자 상태 변환하면 안되기에 예외처리
        // Fall 상태의 FixedUpdate에서 플레이어의 발을 지면에 스냅하는 로직이 있기 때문.
        if (Mathf.Sign(pc.InputDir.y) == -1 && colliderState.isGrounded && pc.stateMachine.CurState != pc.stateMachine.stateDic[PlayerStateTypes.Fall])
        {
            colliderState.CrouchCollidToggle(true);
            if (pc.finalHorizontalVelocity.magnitude > pc.GetSlidingSpeedMin())
            {
                pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.GroundSlide]);
            }
            else if (pc.finalHorizontalVelocity.magnitude < 0.1f)
            {
                pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.CrouchIdle]);
            }
            else
            {
                pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.CrouchMove]);
            }
        }
        else
        {
            colliderState.CrouchCollidToggle(false);
        }

        //#낙하 조건
        if (!colliderState.isGrounded &&
            pc.stateMachine.CurState != pc.stateMachine.stateDic[PlayerStateTypes.Jump])
        {
            pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.Fall]);
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
        pc.animator.Play(pc.IDLE_HASH);
    }

    public override void Update()
    {
        base.Update();
        if (Mathf.Abs(pc.InputDir.x) > 0.1f)
        {
            pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.Walk]);
        }

        /*if (Mathf.Sign(playerController.InputDir.y) == -1)
        {
            playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.CrouchIdle]);
        }*/
    }

    public override void FixedUpdate()
    {
        // Velocity 초기화 용도
        pc.SimulateFinalVelocity();
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
        pc.animator.Play(pc.WALK_HASH);
    }
    public override void Update()
    {
        base.Update();

        if (pc.isSprintInput && colliderState.isGrounded)
        {
            pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.Sprint]);
        }

        // Idle 전이.
        if (Mathf.Abs(pc.InputDir.x) < 0.1f) // 조건 추가 필요
        {
            pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.Idle]);
        }

        // 왼쪽 오른쪽 방향 전환.
        pc.SetSpriteDir();
    }
    public override void FixedUpdate()
    {
        pc.SimulateFinalVelocity();
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
        pc.animator.Play(pc.SPRINT_HASH);
    }
    public override void Update()
    {
        base.Update();

        if (!pc.isSprintInput && colliderState.isGrounded)
        {
            pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.Walk]);
        }

        if (Mathf.Abs(pc.InputDir.x) < 0.1f)
        {
            pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.Idle]);
        }

        // 왼쪽 오른쪽 방향 전환.
        pc.SetSpriteDir();
    }
    public override void FixedUpdate()
    {
        pc.SimulateFinalVelocity();
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
        pc.animator.Play(pc.JUMP_HASH);
        pc.SetJumpVelocity();
        colliderState.SetColliderToQuaternion(Quaternion.identity);
        colliderState.isGrounded = false;
        colliderState.isGroundCheckWait = true;
    }

    public override void Update()
    {
        base.Update();

        if (pc.finalVerticalVelocity.y <= 0)
            pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.Fall]);

        // 왼쪽 오른쪽 방향 전환.
        pc.SetSpriteDir();
    }

    public override void FixedUpdate()
    {
        // 중력 계속 적용
        pc.ApplyGravity();
        pc.ApplyJumpCut();

        // 점프 테스트용. 삭제필요
        pc.SimulateFinalVelocity();
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
        pc.animator.Play(pc.FALL_HASH);
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
        pc.SetSpriteDir();

        if (groundLanded)
        {
            // 플래그 리셋
            groundLanded = false;

            // 점프 계속 입력 시 다시 점프
            if (pc.isJumpInput)
                pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.Jump]);
            // 이동 계속 입력 시 Walk상태로 전환
            else if (Mathf.Abs(pc.InputDir.x) > 0.1f)
            {
                pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.Walk]);
                isJumping = false;
            }
            // 입력 없을 시 Idle상태로 전환
            else
            {
                pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.Idle]);
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
            pc.ApplyGravity();

            pc.SimulateFinalVelocity();
        }
        // 콜라이더 상태 : isGrounded가 true라면
        else
        {
            // 플래그 설정
            groundLanded = true;

            // 바닥에 닿음. 수직 속도 0으로 설정
            pc.finalVerticalVelocity = Vector2.zero;

            // 플레이어 콜라이더를 현재 Ground 오브젝트의 접점 위치에 맞추기 & 바닥 법선벡터 각도로 맞추기
            colliderState.SetColliderToQuaternion(colliderState.closestGroundCollider.transform.rotation);
            pc.transform.position = colliderState.groundHitPos;
            
            // 기존 이동 벡터 -> 바닥 각도에 맞는 이동 벡터로 변환
            pc.finalHorizontalVelocity = pc.finalHorizontalVelocity.magnitude * pc.playerCollider.transform.right * Mathf.Sign(pc.finalHorizontalVelocity.x);

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
        pc.animator.Play(pc.CrouchIdle_HASH);
    }


    public override void Update()
    {
        base.Update();

        if (Mathf.Sign(pc.InputDir.y) != -1)
        {
            pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.Idle]);
        }

        else if (Mathf.Abs(pc.InputDir.x) > 0.1f)
        {
            pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.CrouchMove]);
        }
    }

    public override void FixedUpdate()
    {
        pc.SimulateFinalVelocity();
    }
}

public class Player_CrouchMove : PlayerState
{
    public Player_CrouchMove(PlayerController pc) : base(pc)
    {

    }

    public override void Enter()
    {
        pc.animator.Play(pc.CrouchMove_HASH);
    }

    public override void Update()
    {
        base.Update();

        if (Mathf.Sign(pc.InputDir.y) != -1 && Mathf.Abs(pc.InputDir.x) < 0.1f)
        {
            pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.Idle]);
        }

        else if (Mathf.Sign(pc.InputDir.y) == -1 && Mathf.Abs(pc.InputDir.x) < 0.1f)
        {
            pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.CrouchIdle]);
        }

        else if (Mathf.Sign(pc.InputDir.y) != -1 && Mathf.Abs(pc.InputDir.x) >= 0.1f) 
        {
            pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.Walk]);
        }

        pc.SetSpriteDir();
    }

    public override void FixedUpdate()
    {
        pc.SimulateFinalVelocity();
    }
}

public class Player_GroundSlide : PlayerState
{
    public Player_GroundSlide(PlayerController pc) : base(pc)
    {

    }

    public override void Enter()
    {
        pc.animator.Play(pc.GroundSlide_HASH);
    }


    public override void Update()
    {
        base.Update();

        if (Mathf.Sign(pc.InputDir.y) != -1 && Mathf.Abs(pc.InputDir.x) < 0.1f)
        {
            pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.Idle]);
        }

        else if (Mathf.Sign(pc.InputDir.y) == -1 && Mathf.Abs(pc.InputDir.x) < 0.1f)
        {
            pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.CrouchIdle]);
        }

        else if (Mathf.Sign(pc.InputDir.y) != -1 && Mathf.Abs(pc.InputDir.x) >= 0.1f)
        {
            pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.Walk]);
        }
    }

    public override void FixedUpdate()
    {
        pc.SimulateFinalVelocity();
    }
}

public class Player_Attack : PlayerState
{
    private float coolTime;
    private float chainTime;
    private int index;
    public Player_Attack(PlayerController pc) : base(pc)
    {

    }
    public override void Enter()
    {
        pc.finalHorizontalVelocity = Vector2.zero;

        index = 0;
        AttackPlayByIndex();
    }
    public override void Update()
    {
        base.Update();

        coolTime -= Time.deltaTime;
        chainTime -= Time.deltaTime;

        // Idle 전이.
        if (chainTime < 0f)
        {
            pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.Idle]);
        }

        // 왼쪽 오른쪽 방향 전환.
        pc.SetSpriteDir();
    }
    public override void FixedUpdate()
    {

    }
    public override void Exit() { }

    public void AttackPlayByIndex()
    {
        if (coolTime > 0) return;

        switch (index)
        {
            case 0:
                pc.animator.Play(pc.Attack01_HASH);
                break;
            case 1:
                pc.animator.Play(pc.Attack02_HASH);
                break;
            case 2:
                pc.animator.Play(pc.Attack03_HASH);
                break;
        }

        pc.transform.position += Mathf.Sign(pc.AttackDir.x) * pc.playerCollider.transform.right * 1f;

        coolTime = pc.attackCoolTime;
        chainTime = pc.attackChainTime;
        index += 1;
        if (index > 2) index = 0;
    }
}


public enum PlayerStateTypes
{
    Idle, Walk, Sprint, Brake, Jump, Fall, CrouchIdle, CrouchMove, GroundSlide, WallSlide, Attack
}