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
        // ��� ���¿��� ���ǿ� ���� ��ȯ ������ ���µ�

        //#���� ���� ����
        if (pc.isJumpInput)
        {
            // �Ϲ� ���� ���� ����
            if (colliderState.isGrounded)
            {
                isJumping = true;

                pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.Jump]);
            }
            // �ڿ��� ���� ���� ����
            else if (colliderState.coyoteTimeCounter > 0f && !isJumping &&
                pc.stateMachine.CurState != pc.stateMachine.stateDic[PlayerStateTypes.Jump])
            {
                isJumping = true;

                pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.Jump]);
            }
            // ���� ���� ���� ����
            /*else if (isJumping && 2�� ���� ����[�����Ƽ �Ŵ������� �������� �ɷ��� Ȱ��ȭ �Ǿ��ٸ�])
            {
                playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[PlayerStateTypes.DoubleJump]);
            }*/
        }

        //#����Ű �Ʒ�(��ũ����, �ɾ� �ȱ�, ���� �����̵�) ���ǵ�
        // �������� ���¿��� ���鿡 ���ڸ��� ���� ��ȯ�ϸ� �ȵǱ⿡ ����ó��
        // Fall ������ FixedUpdate���� �÷��̾��� ���� ���鿡 �����ϴ� ������ �ֱ� ����.
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

        //#���� ����
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
        // Velocity �ʱ�ȭ �뵵
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

        // Idle ����.
        if (Mathf.Abs(pc.InputDir.x) < 0.1f) // ���� �߰� �ʿ�
        {
            pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.Idle]);
        }

        // ���� ������ ���� ��ȯ.
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

        // ���� ������ ���� ��ȯ.
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

        // ���� ������ ���� ��ȯ.
        pc.SetSpriteDir();
    }

    public override void FixedUpdate()
    {
        // �߷� ��� ����
        pc.ApplyGravity();
        pc.ApplyJumpCut();

        // ���� �׽�Ʈ��. �����ʿ�
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

        // ���� ������ ���� ��ȯ.
        pc.SetSpriteDir();

        if (groundLanded)
        {
            // �÷��� ����
            groundLanded = false;

            // ���� ��� �Է� �� �ٽ� ����
            if (pc.isJumpInput)
                pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.Jump]);
            // �̵� ��� �Է� �� Walk���·� ��ȯ
            else if (Mathf.Abs(pc.InputDir.x) > 0.1f)
            {
                pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.Walk]);
                isJumping = false;
            }
            // �Է� ���� �� Idle���·� ��ȯ
            else
            {
                pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.Idle]);
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
            pc.ApplyGravity();

            pc.SimulateFinalVelocity();
        }
        // �ݶ��̴� ���� : isGrounded�� true���
        else
        {
            // �÷��� ����
            groundLanded = true;

            // �ٴڿ� ����. ���� �ӵ� 0���� ����
            pc.finalVerticalVelocity = Vector2.zero;

            // �÷��̾� �ݶ��̴��� ���� Ground ������Ʈ�� ���� ��ġ�� ���߱� & �ٴ� �������� ������ ���߱�
            colliderState.SetColliderToQuaternion(colliderState.closestGroundCollider.transform.rotation);
            pc.transform.position = colliderState.groundHitPos;
            
            // ���� �̵� ���� -> �ٴ� ������ �´� �̵� ���ͷ� ��ȯ
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

        // Idle ����.
        if (chainTime < 0f)
        {
            pc.stateMachine.ChangeState(pc.stateMachine.stateDic[PlayerStateTypes.Idle]);
        }

        // ���� ������ ���� ��ȯ.
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