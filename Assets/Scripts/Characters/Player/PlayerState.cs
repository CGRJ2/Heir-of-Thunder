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
        if (Mathf.Abs(playerController.InputDir.x) > 0.1f) // ���� ���ǹ� �߰� �ʿ�. ���� �ƴ� ��, �׶��� �̵��� ��.
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

        // Idle ����.
        if (Mathf.Abs(playerController.InputDir.x) < 0.1f) // ���� �߰� �ʿ�
        {
            playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.Idle]);
        }

        // ���� ������ ���� ��ȯ.
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

        // ���� ������ ���� ��ȯ.
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

        // ���� ������ ���� ��ȯ.
        playerController.SetSpriteDir();
    }

    public override void FixedUpdate()
    {
        // �߷� ��� ����
        playerController.ApplyGravity();

        playerController.ApplyJumpCut();


        // ���� �׽�Ʈ��. �����ʿ�
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
        // ���� ������ ���� ��ȯ.
        playerController.SetSpriteDir();
    }

    public override void FixedUpdate()
    {
        if (!colliderState.isGrounded)
        {
            // �߷� ��� ����
            playerController.ApplyGravity();

            playerController.SimulateFinalVelocity();
        }
        else
        {
            // ���� ����, �Է��� ������ �̵�or�޸��� ���·� ��ȯ, ������ Idle�� ��ȯ
            playerController.finalVelocity.y = 0;

            // ���� Ground ������Ʈ�� Y������ ��ġ ����
            playerController.SetGroundPosY(colliderState.groundHitColids[0].transform.parent.position.y);

            // ���� ��� �Է� �� �ٽ� ����, �Է¾��� �� Idle���·� ��ȯ
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