using UnityEngine;

public class PlayerState : BaseState
{
    protected PlayerController playerController;
    protected ColliderState colliderState;

    public PlayerState(PlayerController pc)
    {
        playerController = pc;
        colliderState = pc.GetComponent<ColliderState>();
    }
    public override void Enter() { }
    public override void Update()
    {
        if (playerController.isJumpInput && colliderState.isGrounded)
            playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.Jump]);
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
        if (Mathf.Abs(playerController.InputDir.x) > 0.1f) // 여기 조건문 추가 필요. 점프 아닐 때, 그라운드 이동일 때.
        {
            playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.Walk]);
        }
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

        // 왼쪽 오른쪽 방향 전환.
        playerController.SetSpriteDir();
    }
    public override void FixedUpdate()
    {

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
        colliderState.isGrounded = false;
        colliderState.isGroundCheckWait = true;


    }

    public override void Update()
    {

        if (playerController.finalVelocity.y <= 0)
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
    public Player_Fall(PlayerController pc) : base(pc)
    {

    }

    public override void Enter()
    {
        playerController.animator.Play(playerController.FALL_HASH);
        colliderState.isGroundCheckWait = false;
    }

    public override void Update()
    {
        // 왼쪽 오른쪽 방향 전환.
        playerController.SetSpriteDir();
    }

    public override void FixedUpdate()
    {
        if (!colliderState.isGrounded)
        {
            // 중력 계속 적용
            playerController.ApplyGravity();

            playerController.SimulateFinalVelocity();
        }
        else
        {
            // 점프 종료, 입력이 있으면 이동or달리기 상태로 전환, 없으면 Idle로 전환
            playerController.finalVelocity.y = 0;

            // 현재 Ground 오브젝트의 Y값으로 위치 설정
            playerController.SetGroundPosY(colliderState.groundHitColids[0].transform.parent.position.y);

            // 점프 계속 입력 시 다시 점프, 입력없을 시 Idle상태로 전환
            if (playerController.isJumpInput)
                playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.Jump]);
            else
                playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.Idle]);
        }
    }
}


public enum PlayerStateTypes
{
    Idle, Walk, Jump, Fall, Attack01, Attack02, Attack03
}