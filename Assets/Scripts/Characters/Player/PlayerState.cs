using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : BaseState
{
    protected PlayerController playerController;
    protected ColliderState colliderState;

    public PlayerState(PlayerController pc)
    {
        playerController = pc;
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
        playerController.Move();
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
        playerController.Jump();
    }

   /* public override void Update()
    {

        if (playerController.isGrounded)
            playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[EState.Idle]);

        // ���� ������ ���� ��ȯ.
        playerController.SetSpriteDir();
    }

    public override void FixedUpdate()
    {
        playerController.rigid.velocity = new Vector2(playerController.inputX * playerController.moveSpeed, playerController.rigid.velocity.y);
    }*/
}
public class Player_Fall : PlayerState
{
    /*public Player_Fall(PlayerController pc) : base(pc)
    {

    }

    public override void Enter()
    {
        playerController.animator.Play(playerController.FALL_HASH);
        playerController.rigid.AddForce(Vector2.up * playerController.jumpSpeed, ForceMode2D.Impulse);
        playerController.isGrounded = false;
        playerController.isJumped = false;
    }

    public override void Update()
    {

        if (playerController.isGrounded)
            playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[EState.Idle]);

        // ���� ������ ���� ��ȯ.
        playerController.SetSpriteDir();
    }

    public override void FixedUpdate()
    {
        playerController.rigid.velocity = new Vector2(playerController.inputX * playerController.moveSpeed, playerController.rigid.velocity.y);
    }*/
}


public enum PlayerStateTypes
{
    Idle, Walk, Jump, Attack01, Attack02, Attack03
}