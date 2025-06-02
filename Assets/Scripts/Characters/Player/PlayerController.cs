using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : CharacterControllerBase
{
    public bool isSprintInput;
    public bool isJumpInput;
    private InputAction sprintAction;
    private InputAction jumpAction;


    [HideInInspector] public StateMachine<PlayerStateTypes> stateMachine;
    [HideInInspector] public CinemachineFramingTransposer cinemachineFrame;
    [SerializeField] private CinemachineVirtualCamera virtualCam;
    public float cineFrameOffsetX;
    public float cineFrameOffsetY;




    [SerializeField] float moveSpeedLimit = 0.16f;
    [SerializeField] float moveAccelation = 10f;
    [SerializeField] float sprintSpeedLimit = 0.16f;
    [SerializeField] float sprintAccelation = 10f;
    [SerializeField] float dragAccelation = 6f;
    [SerializeField] float customGravity = -10f;
    [SerializeField] float jumpVelocity = 15f;
    [SerializeField] float jumpCutVelocity = 5f;
    [SerializeField] float fallingVelocityLimit = 5f;
    public bool isJumpCut;

    Rigidbody rb;

    public Vector2 InputDir { get; private set; }
    public Vector2 finalVelocity;
    //public Vector2 totalVerticalVelocity;

    public readonly int IDLE_HASH = Animator.StringToHash("Idle");
    public readonly int WALK_HASH = Animator.StringToHash("Walk");
    public readonly int SPRINT_HASH = Animator.StringToHash("Sprint");
    public readonly int DASH_HASH = Animator.StringToHash("Dash");
    public readonly int JUMP_HASH = Animator.StringToHash("Jump");
    public readonly int FALL_HASH = Animator.StringToHash("Fall");

    // ������������ �ݶ��̴� ���µ��� ��������
    // �ٴڿ� ����
    // ���߿� ��
    // �ٴ�+���� ����
    // ������ ����
    // 45�� �̻��� ������ �ٴڿ� ����(�����̵�)
    // 90�� ���� ���� ����-> ���� ���·� ����
    // �ٴڿ� ���� ���¿��� �ٴ� �𼭸��� ���� => ��û�̴� ���
    // ���߿� �� + ���� ���� + �𼭸��� ���� => �� �𼭸� ���

    private void StateMachineInit()
    {
        stateMachine = new StateMachine<PlayerStateTypes>();
        stateMachine.stateDic.Add(PlayerStateTypes.Idle, new Player_Idle(this));
        stateMachine.stateDic.Add(PlayerStateTypes.Walk, new Player_Walk(this));
        stateMachine.stateDic.Add(PlayerStateTypes.Sprint, new Player_Sprint(this));
        stateMachine.stateDic.Add(PlayerStateTypes.Jump, new Player_Jump(this));
        stateMachine.stateDic.Add(PlayerStateTypes.Fall, new Player_Fall(this));
        stateMachine.CurState = stateMachine.stateDic[PlayerStateTypes.Idle];
    }

    private void InputActionsInit()
    {
        // �÷��̾� ���� ��
        var playerControlMap = GetComponent<PlayerInput>().actions.FindActionMap("PlayerActions");

        // �޸��� �׼�
        sprintAction = playerControlMap.FindAction("Sprint");
        sprintAction.Enable();
        sprintAction.performed += HandleSprint;
        sprintAction.canceled += HandleSprint;

        // ���� �׼�
        jumpAction = playerControlMap.FindAction("Jump");
        jumpAction.Enable();
        jumpAction.performed += HandleJump;
        jumpAction.canceled += HandleJump;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        cinemachineFrame = virtualCam.GetCinemachineComponent<CinemachineFramingTransposer>();
        StateMachineInit();
        InputActionsInit();
    }

    #region Ʈ������ Input ... �̵�, ����, ��ų, ��ȣ�ۿ�, ������ ���
    public void OnMove(InputValue value)
    {
        InputDir = value.Get<Vector2>();
    }

    public void OnAttack(InputValue value)
    {

    }
    #endregion

    #region ���� ���� Input ... �޸���, ����, ��¡
    public void HandleSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
            isSprintInput = true;
        else if (context.canceled)
            isSprintInput = false;
    }

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

    #endregion

    private void OnDestroy()
    {
        sprintAction.performed -= HandleSprint;
        sprintAction.canceled -= HandleSprint;
        jumpAction.performed -= HandleJump;
        jumpAction.canceled -= HandleJump;
    }

    public void SetSpriteDir()
    {
        if (InputDir.x < 0)
        {
            spriteRenderer.flipX = true;
            cinemachineFrame.m_TrackedObjectOffset = new Vector3(-10, 2, 0);
        }
        else if (InputDir.x > 0)
        {
            spriteRenderer.flipX = false;
            cinemachineFrame.m_TrackedObjectOffset = new Vector3(10, 2, 0);
        }
    }


    private void Update()
    {
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

        finalVelocity += newTotalVelocity * Time.fixedDeltaTime;

        return finalVelocity;
    }

    public Vector2 GetGroundSideVelocity()
    {
        Vector2 currentVelocity = finalVelocity;
        Vector2 sideMoveForce;
        // Ⱦ�̵� �Է°��� ���� �� => Ⱦ�̵� ó��
        if (InputDir.x != 0)
        {
            if (!isSprintInput)
            {
                // �ִ� �ӵ��� ���� && �Է°� ��������� ���� �� 
                if (Mathf.Abs(currentVelocity.x) >= moveSpeedLimit && Mathf.Sign(currentVelocity.x) == Mathf.Sign(InputDir.x))
                {
                    // Ⱦ �ӵ� limit ����
                    finalVelocity.x = moveSpeedLimit * Mathf.Sign(currentVelocity.x);

                    // ���ӷ� 0���� ����
                    sideMoveForce = Vector2.zero;
                }
                // �� �ܿ� ��Ȳ������ ����or����
                else
                {
                    sideMoveForce = (Vector2.right * InputDir.x).normalized * moveAccelation / 10f;
                }
            }
            else
            {
                // �ִ� �ӵ��� ���� && �Է°� ��������� ���� �� 
                if (Mathf.Abs(currentVelocity.x) >= sprintSpeedLimit && Mathf.Sign(currentVelocity.x) == Mathf.Sign(InputDir.x))
                {
                    // Ⱦ �ӵ� limit ����
                    finalVelocity.x = sprintSpeedLimit * Mathf.Sign(currentVelocity.x);

                    // ���ӷ� 0���� ����
                    sideMoveForce = Vector2.zero;
                }
                // �� �ܿ� ��Ȳ������ ����or����
                else
                {
                    sideMoveForce = (Vector2.right * InputDir.x).normalized * sprintAccelation / 10f;
                }
            }
            return sideMoveForce;
        }
        else return Vector2.zero;
    }

    public Vector2 GetGroundDrag()
    {
        // Ⱦ �̵� �Է°��� ���ٸ� => ������ ����
        if (InputDir.x == 0)
        {
            Vector2 currentVelocity = finalVelocity;
            Vector2 dragForce;

            // ���� �ӵ� �̻��� �� ����
            if (Mathf.Abs(currentVelocity.x) > 0.03f)
            {
                dragForce = -(Vector2.right * currentVelocity.x).normalized * dragAccelation / 10f;
                return dragForce;
            }
            // ���� �ӵ� ���϶�� finalVelocity = zero�� ��ȯ�� ����
            else
            {
                // Ⱦ �̵� �ӵ� 0���� ���� �� zero ��ȯ
                finalVelocity.x = 0;
                return Vector2.zero;
            }
        }
        else return Vector2.zero;
    }

    public void SetJumpVelocity()
    {
        finalVelocity.y = jumpVelocity * Time.fixedDeltaTime;
    }

    public void ApplyGravity()
    {
        if (finalVelocity.y > -fallingVelocityLimit)
        {
            finalVelocity.y -= customGravity * Time.fixedDeltaTime;
        }
        else
        {
            finalVelocity.y = -fallingVelocityLimit;
        }
    }

    // ���� ������ ���� ������ �����ؾ��ҵ�?
    public void ApplyJumpCut()
    {
        if (isJumpCut)
        {
            if (finalVelocity.y > -fallingVelocityLimit)
                finalVelocity.y -= jumpCutVelocity * Time.fixedDeltaTime;
        }
    }



}
