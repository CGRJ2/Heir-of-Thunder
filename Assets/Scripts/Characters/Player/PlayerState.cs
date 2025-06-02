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
        // ��� ���¿��� ���ǿ� ���� ��ȯ ������ ���µ�

        //#���� ���� ����
        if (playerController.isJumpInput)
        {
            // �Ϲ� ���� ���� ����
            if (colliderState.isGrounded)
            {
                isJumping = true;

                playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.Jump]);
            }
            // �ڿ��� ���� ���� ����
            else if (colliderState.coyoteTimeCounter > 0f && !isJumping &&
                playerController.stateMachine.CurState != playerController.stateMachine.stateDic[PlayerStateTypes.Jump])
            {
                isJumping = true;

                playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.Jump]);
            }
            // ���� ���� ���� ����
            /*else if (isJumping && 2�� ���� ����)
            {
                playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.DoubleJump]);
            }*/
        }

        //#����Ű �Ʒ�(��ũ����, �ɾ� �ȱ�, ���� �����̵�) ���ǵ�
        // �������� ���¿��� ���鿡 ���ڸ��� ���� ��ȯ�ϸ� �ȵǱ⿡ ����ó��
        // Fall ������ FixedUpdate���� �÷��̾��� ���� ���鿡 �����ϴ� ������ �ֱ� ����.
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

        //#���� ����
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
        // Velocity �ʱ�ȭ �뵵
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

        if (!playerController.isSprintInput && colliderState.isGrounded)
        {
            playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.Walk]);
        }

        if (Mathf.Abs(playerController.InputDir.x) < 0.1f)
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

        // ���� ������ ���� ��ȯ.
        playerController.SetSpriteDir();

        if (groundLanded)
        {
            // �÷��� ����
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
        // �ݶ��̴� ���� : isGrounded�� false���
        if (!colliderState.isGrounded)
        {
            // �߷� ��� ����
            playerController.ApplyGravity();

            playerController.SimulateFinalVelocity();
        }
        // �ݶ��̴� ���� : isGrounded�� true���
        else
        {
            // �÷��� ����
            groundLanded = true;

            // �ٴڿ� ����. ���� �ӵ� 0���� ����
            playerController.finalVerticalVelocity = Vector2.zero;

            // ���� Ground ������Ʈ�� Y������ ��ġ ����
            // ���⸦ �� ������ ���������
            // Todo: �ٴ� �������Ϳ� �÷��̾� ���߱�, �ٴڿ� ���缭 �ݶ��̴� ����&��ġ ���߱�
            //playerController.SetGroundPosY(colliderState.groundHitColids[0].transform.parent.position.y);
            colliderState.SetColliderToQuaternion(colliderState.groundHitColids[0].transform.rotation);
            // ���� �̵� ���� -> �ٴ� ������ �´� �̵� ���ͷ� ��ȯ
            playerController.finalHorizontalVelocity = playerController.finalHorizontalVelocity.magnitude * playerController.playerCollider.transform.right * Mathf.Sign(playerController.finalHorizontalVelocity.x);
            playerController.transform.position = colliderState.groundHitPos;

            // ���� ��� �Է� �� �ٽ� ����, �Է¾��� �� Idle���·� ��ȯ
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