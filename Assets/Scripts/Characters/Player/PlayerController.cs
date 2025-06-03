using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : CharacterControllerBase
{
    public bool isSprintInput;
    public bool isJumpInput;
    public bool isAttackInput;
    public float attackCoolTime;
    public float attackChainTime;
    public bool isChargeInput;
    private InputAction sprintAction;
    private InputAction jumpAction;
    private InputAction attackAction;
    Coroutine attackRountine;
    private InputAction chargeAction;

    public Collider playerCollider;

    ColliderState colliderState;
    [HideInInspector] public StateMachine<PlayerStateTypes> stateMachine;
    [HideInInspector] public CinemachineFramingTransposer cinemachineFrame;
    [SerializeField] private CinemachineVirtualCamera virtualCam;
    [SerializeField] Vector2 cineFrameOffset;


    [SerializeField] float moveSpeedLimit = 0.16f;
    [SerializeField] float moveAccelation = 10f;
    [SerializeField] float sprintSpeedLimit = 0.16f;
    [SerializeField] float sprintAccelation = 10f;
    [SerializeField] float crouchSpeedLimit = 0.16f;
    [SerializeField] float dragAccelation = 6f;
    [SerializeField] float SlidingDragAccelation = 6f;
    [SerializeField] float SlidingSpeedMin = 0.3f;
    [SerializeField] float customGravity = -10f;
    [SerializeField] float jumpVelocity = 15f;
    [SerializeField] float jumpCutVelocity = 5f;
    [SerializeField] float fallingVelocityLimit = 5f;
    public bool isJumpCut;

    Rigidbody rb;

    public Vector2 InputDir { get; private set; }
    public Vector2 mouseWorldPos;
    public Vector2 finalHorizontalVelocity;
    public Vector2 finalVerticalVelocity;
    //public Vector2 totalVerticalVelocity;

    public readonly int IDLE_HASH = Animator.StringToHash("Idle");
    public readonly int WALK_HASH = Animator.StringToHash("Walk");
    public readonly int SPRINT_HASH = Animator.StringToHash("Sprint");
    public readonly int DASH_HASH = Animator.StringToHash("Dash");
    public readonly int JUMP_HASH = Animator.StringToHash("Jump");
    public readonly int FALL_HASH = Animator.StringToHash("Fall");
    public readonly int CrouchIdle_HASH = Animator.StringToHash("CrouchIdle");
    public readonly int CrouchMove_HASH = Animator.StringToHash("CrouchMove");
    public readonly int GroundSlide_HASH = Animator.StringToHash("GroundSlide");

    public readonly int Attack01_HASH = Animator.StringToHash("Attack01");
    public readonly int Attack02_HASH = Animator.StringToHash("Attack02");
    public readonly int Attack03_HASH = Animator.StringToHash("Attack03");


    public readonly int WallSlide_HASH = Animator.StringToHash("WallSlide");

    // 상태패턴으로 콜라이더 상태들을 정리하자
    // 바닥에 닿음
    // 공중에 뜸
    // 바닥+벽에 닿음
    // 벽에만 닿음
    // 45도 이상의 슬로프 바닥에 닿음(슬라이딩)
    // 90도 넘은 벽에 닿음-> 점프 상태로 변경
    // 바닥에 닿음 상태에서 바닥 모서리에 닿음 => 휘청이는 모션
    // 공중에 뜸 + 벽에 닿음 + 모서리에 닿음 => 벽 모서리 잡기

    private void StateMachineInit()
    {
        stateMachine = new StateMachine<PlayerStateTypes>();
        stateMachine.stateDic.Add(PlayerStateTypes.Idle, new Player_Idle(this));
        stateMachine.stateDic.Add(PlayerStateTypes.Walk, new Player_Walk(this));
        stateMachine.stateDic.Add(PlayerStateTypes.Sprint, new Player_Sprint(this));
        stateMachine.stateDic.Add(PlayerStateTypes.Jump, new Player_Jump(this));
        stateMachine.stateDic.Add(PlayerStateTypes.Fall, new Player_Fall(this));
        stateMachine.stateDic.Add(PlayerStateTypes.CrouchIdle, new Player_CrouchIdle(this));
        stateMachine.stateDic.Add(PlayerStateTypes.CrouchMove, new Player_CrouchMove(this));
        stateMachine.stateDic.Add(PlayerStateTypes.GroundSlide, new Player_GroundSlide(this));
        //stateMachine.stateDic.Add(PlayerStateTypes.WallSlide, new Player_WallSlide(this));

        stateMachine.stateDic.Add(PlayerStateTypes.Attack, new Player_Attack(this));

        stateMachine.CurState = stateMachine.stateDic[PlayerStateTypes.Idle];
    }

    private void InputActionsInit()
    {
        // 플레이어 조작 맵
        var playerControlMap = GetComponent<PlayerInput>().actions.FindActionMap("PlayerActions");

        // 달리기 액션
        sprintAction = playerControlMap.FindAction("Sprint");
        sprintAction.Enable();
        sprintAction.performed += HandleSprint;
        sprintAction.canceled += HandleSprint;

        // 점프 액션
        jumpAction = playerControlMap.FindAction("Jump");
        jumpAction.Enable();
        jumpAction.performed += HandleJump;
        jumpAction.canceled += HandleJump;

        // 공격 액션
        attackAction = playerControlMap.FindAction("Attack");
        attackAction.Enable();
        attackAction.started += HandleAttack;
        attackAction.canceled += HandleAttack;

        // 차지 액션
        chargeAction = playerControlMap.FindAction("Charge");
        chargeAction.Enable();
        chargeAction.started += HandleCharge;
        chargeAction.canceled += HandleCharge;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        cinemachineFrame = virtualCam.GetCinemachineComponent<CinemachineFramingTransposer>();
        colliderState = GetComponent<ColliderState>();
        StateMachineInit();
        InputActionsInit();
    }

    #region 트리거형 Input ... 이동, 공격, 스킬, 상호작용, 아이템 사용
    public void OnMove(InputValue value)
    {
        InputDir = value.Get<Vector2>();
    }



    /*public void OnAim(InputValue value)
    {
        mouseWorldPos = Camera.main.ScreenToWorldPoint(value.Get<Vector2>());

        Debug.Log(mouseWorldPos);
    }*/
    #endregion

    #region 지속 상태 Input ... 달리기, 점프, 차징

    // 달리기 키 토글
    public void HandleSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
            isSprintInput = true;
        else if (context.canceled)
            isSprintInput = false;
    }

    // 점프 키 토글
    public void HandleJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isJumpCut = false;
            isJumpInput = true;
        }
        if (context.canceled)
        {
            isJumpCut = true;
            isJumpInput = false;
        }
    }

    // 마우스 좌표 설정, Update에서 지속 갱신
    void SetMousePos()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0;
        this.mouseWorldPos = mouseWorldPos;
    }

    // 공격 키 토글
    public void HandleAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // 접지 상태 => 일반 공격
            if (colliderState.isGrounded)
            {
                if (stateMachine.CurState is Player_Attack attackState)
                {
                    attackState.AttackPlayByIndex();
                }
                else
                {
                    stateMachine.ChangeState(stateMachine.stateDic[PlayerStateTypes.Attack]);
                }

            }
            // 공중 상태 => 점프 공격
            /*if (isJumping)
            {

            }*/
        }
    }
    IEnumerator AttackRoutine()
    {
        isAttackInput = true;
        yield return new WaitForSeconds(0.3f);
        isAttackInput = false;
    }

    // 차지 키 토글
    public void HandleCharge(InputAction.CallbackContext context)
    {
        if (context.started)
            isChargeInput = true;
        else if (context.canceled)
            isChargeInput = false;
    }

    #endregion

    private void OnDestroy()
    {
        sprintAction.performed -= HandleSprint;
        sprintAction.canceled -= HandleSprint;
        jumpAction.performed -= HandleJump;
        jumpAction.canceled -= HandleJump;
        attackAction.started -= HandleAttack;
        attackAction.canceled -= HandleAttack;
        chargeAction.started -= HandleCharge;
        chargeAction.canceled -= HandleCharge;
    }

    public void SetSpriteDir()
    {
        if (InputDir.x < 0)
        {
            spriteRenderer.flipX = true;
            cinemachineFrame.m_TrackedObjectOffset = new Vector3(-cineFrameOffset.x, cineFrameOffset.y, 0);
        }
        else if (InputDir.x > 0)
        {
            spriteRenderer.flipX = false;
            cinemachineFrame.m_TrackedObjectOffset = new Vector3(cineFrameOffset.x, cineFrameOffset.y, 0);
        }
    }



    private void Update()
    {
        SetMousePos();
        stateMachine.Update();
    }
    private void FixedUpdate()
    {
        stateMachine.FixedUpdate();
    }
    public void SimulateFinalVelocity()
    {
        transform.position += GetFinalVelocity();
    }
    public void SetGroundPosY(float y)
    {
        transform.position = new Vector2(transform.position.x, y);
    }

    public Vector3 GetFinalVelocity()
    {
        Vector2 newTotalVelocity = GetGroundSideVelocity() + GetGroundDrag();

        finalHorizontalVelocity += newTotalVelocity * Time.fixedDeltaTime;

        return finalHorizontalVelocity + finalVerticalVelocity;
    }

    public Vector2 GetGroundSideVelocity()
    {
        Vector2 currentVelocity = finalHorizontalVelocity;
        Vector2 sideMoveForce;
        // 횡이동 입력값이 있을 때 => 횡이동 처리
        if (InputDir.x != 0)
        {
            // 달리기 상태
            if (stateMachine.CurState == stateMachine.stateDic[PlayerStateTypes.Sprint])
            {
                // 최대 속도에 도달 && 입력과 진행방향이 같을 때 
                if (Mathf.Abs(currentVelocity.x) >= sprintSpeedLimit && Mathf.Sign(currentVelocity.x) == Mathf.Sign(InputDir.x))
                {
                    // 횡 속도 limit 고정
                    finalHorizontalVelocity = playerCollider.transform.right * sprintSpeedLimit * Mathf.Sign(currentVelocity.x);

                    // 가속력 0으로 설정
                    sideMoveForce = Vector2.zero;
                }
                // 그 외에 상황에서는 가속or감속
                else
                {
                    sideMoveForce = (playerCollider.transform.right * InputDir.x).normalized * sprintAccelation / 10f;
                }
            }

            // 앉아 걷기 상태
            else if (stateMachine.CurState == stateMachine.stateDic[PlayerStateTypes.CrouchMove])
            {
                // 최대 속도에 도달 && 입력과 진행방향이 같을 때 
                if (Mathf.Abs(currentVelocity.x) >= crouchSpeedLimit && Mathf.Sign(currentVelocity.x) == Mathf.Sign(InputDir.x))
                {
                    // 횡 속도 limit 고정
                    finalHorizontalVelocity = playerCollider.transform.right * crouchSpeedLimit * Mathf.Sign(currentVelocity.x);

                    // 가속력 0으로 설정
                    sideMoveForce = Vector2.zero;
                }
                // 그 외에 상황에서는 가속or감속
                else
                {
                    sideMoveForce = (playerCollider.transform.right * InputDir.x).normalized * moveAccelation / 10f;
                }
            }

            // 지면 슬라이딩 상태
            else if (stateMachine.CurState == stateMachine.stateDic[PlayerStateTypes.GroundSlide])
            {
                // 가속력 0으로 설정
                sideMoveForce = Vector2.zero;
            }
            // 일반 이동 상태
            else
            {
                // 최대 속도에 도달 && 입력과 진행방향이 같을 때 
                if (Mathf.Abs(currentVelocity.x) >= moveSpeedLimit && Mathf.Sign(currentVelocity.x) == Mathf.Sign(InputDir.x))
                {
                    // 횡 속도 limit 고정
                    finalHorizontalVelocity = playerCollider.transform.right * moveSpeedLimit * Mathf.Sign(currentVelocity.x);

                    // 가속력 0으로 설정
                    sideMoveForce = Vector2.zero;
                }
                // 그 외에 상황에서는 가속or감속
                else
                {
                    sideMoveForce = (playerCollider.transform.right * InputDir.x).normalized * moveAccelation / 10f;
                }
            }
            return sideMoveForce;
        }
        else return Vector2.zero;
    }

    public Vector2 GetGroundDrag()
    {
        Vector2 currentVelocity = finalHorizontalVelocity;
        Vector2 dragForce;

        // 횡 이동 입력값이 없다면, 또는 슬라이딩 상태일 때 => 서서히 감속
        if (InputDir.x == 0)
        {
            // 일정 속도 이상일 때 감속
            if (Mathf.Abs(currentVelocity.x) > 0.03f)
            {
                dragForce = -(playerCollider.transform.right * currentVelocity.x).normalized * dragAccelation / 10f;
                return dragForce;
            }
            // 일정 속도 이하라면 finalVelocity = zero로 반환해 정지
            else
            {
                // 횡 이동 속도 0으로 설정 후 zero 반환
                finalHorizontalVelocity = Vector2.zero;
                return Vector2.zero;
            }
        }
        else if (stateMachine.CurState == stateMachine.stateDic[PlayerStateTypes.GroundSlide])
        {
            // 일정 속도 이상일 때 감속
            if (Mathf.Abs(currentVelocity.x) > 0.03f)
            {
                dragForce = -(playerCollider.transform.right * currentVelocity.x).normalized * SlidingDragAccelation / 10f;
                return dragForce;
            }
            // 일정 속도 이하라면 finalVelocity = zero로 반환해 정지
            else
            {
                // 횡 이동 속도 0으로 설정 후 zero 반환
                finalHorizontalVelocity = Vector2.zero;
                return Vector2.zero;
            }
        }
        else return Vector2.zero;
    }

    public void SetJumpVelocity()
    {
        finalVerticalVelocity.y = jumpVelocity * Time.fixedDeltaTime;
    }

    public void ApplyGravity()
    {
        if (finalVerticalVelocity.y > -fallingVelocityLimit)
        {
            finalVerticalVelocity.y -= customGravity * Time.fixedDeltaTime;
        }
        else
        {
            finalVerticalVelocity.y = -fallingVelocityLimit;
        }
    }

    // 낮은 점프는 점프 컷으로 구현해야할듯?
    public void ApplyJumpCut()
    {
        if (isJumpCut)
        {
            if (finalVerticalVelocity.y > -fallingVelocityLimit)
                finalVerticalVelocity.y -= jumpCutVelocity * Time.fixedDeltaTime;
        }
    }

    public float GetSlidingSpeedMin()
    {
        return SlidingSpeedMin;
    }

}
