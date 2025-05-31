using UnityEngine;

public class EnemyState : BaseState
{
    protected CharacterControllerBase characterController; // 이거 EnemyController로 변경
    public EnemyState(CharacterControllerBase cc)
    {
        characterController = cc;
    }
    public override void Enter() { }
    public override void Update()
    {
        /*if (player.isJumped && player.isGrounded)
            player.stateMachine.ChangeState(player.stateMachine.stateDic[EState.Jump]);*/
    }
    public override void Exit() { }

}

/*public class Idle : EnemyState
{
    public Idle(CharacterControllerBase cc) : base(cc)
    {

    }

    public override void Enter()
    {
        characterController.animator.Play(characterController.IDLE_HASH);
    }

    public override void Update()
    {
        base.Update();
        // 이거는 플레이어, 몬스터, NPC 나눠서 해야할듯
        // 이동을 시작하면 상태를 이동으로 바꿈
        *//*if (Mathf.Abs(characterController.FinalVelocity.x) > 0.1f && characterController.FinalVelocity.y < 0.1f)
        {
            characterController.stateMachine.ChangeState(playerController.stateMachine.stateDic[EState.Walk]);
        }*//*
    }
    public override void Exit() { }
}

public class Move : EnemyState
{
    public Move(PlayerController pc) : base(pc)
    {

    }
    public override void Enter()
    {
        characterController.animator.Play(characterController.WALK_HASH);
    }
    public override void Update()
    {
        base.Update();
        // Idle 전이.
        *//*if (Mathf.Abs(playerController.inputX) < 0.1f)
        {
            playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[EState.Idle]);
        }
        // 왼쪽 오른쪽 방향 전환.
        if (playerController.inputX < 0)
        {
            playerController.spriteRenderer.flipX = true;
            playerController.cinemachine.m_TrackedObjectOffset = new Vector3(-10, 2, 0);
        }
        else
        {
            playerController.spriteRenderer.flipX = false;
            playerController.cinemachine.m_TrackedObjectOffset = new Vector3(10, 2, 0);
        }*//*
    }
    public override void FixedUpdate()
    {
        playerController.rigid.velocity = new Vector2(playerController.inputX * playerController.moveSpeed, playerController.rigid.velocity.y);
    }
    public override void Exit() { }
}

public class Jump : EnemyState
{
    public Player_Jump(PlayerController pc) : base(pc)
    {
        HasPhysics = true;
    }

    public override void Enter()
    {
        playerController.animator.Play(playerController.JUMP_HASH);
        playerController.rigid.AddForce(Vector2.up * playerController.jumpSpeed, ForceMode2D.Impulse);
        playerController.isGrounded = false;
        playerController.isJumped = false;
    }

    public override void Update()
    {

        if (playerController.isGrounded)
            playerController.stateMachine.ChangeState(playerController.stateMachine.stateDic[EState.Idle]);

        if (playerController.inputX < 0)
        {
            playerController.spriteRenderer.flipX = true;
            playerController.cinemachine.m_TrackedObjectOffset = new Vector3(-10, 2, 0);
        }
        else
        {
            playerController.spriteRenderer.flipX = false;
            playerController.cinemachine.m_TrackedObjectOffset = new Vector3(10, 2, 0);
        }
    }

    public override void FixedUpdate()
    {
        playerController.rigid.velocity = new Vector2(playerController.inputX * playerController.moveSpeed, playerController.rigid.velocity.y);
    }
}*/


