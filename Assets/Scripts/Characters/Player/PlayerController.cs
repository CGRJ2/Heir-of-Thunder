using Cinemachine;
using SmallScaleInteractive._2DCharacter;
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
    [SerializeField] float dragAccelation = 6f;
    [SerializeField] float customGravity;

    Rigidbody rb;

    public Vector2 InputDir { get; private set; }
    public Vector2 finalVelocity;
    public Vector2 currentVelocity;

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
        stateMachine.stateDic.Add(PlayerStateTypes.Jump, new Player_Jump(this));
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
            isJumpInput = true;
        else if (context.canceled)
            isJumpInput = false;
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
    public void Move()
    {
        transform.position += (Vector3)GetFinalVelocity();
    }

    public Vector2 GetFinalVelocity()
    {
        Vector2 sideMoveForce;
        // �ִ� �ӵ��� ���� && �Է°� ��������� ���� ��
        if (Mathf.Abs(currentVelocity.x) >= moveSpeedLimit && Mathf.Sign(currentVelocity.x) == Mathf.Sign(InputDir.x))
        {
            // Ⱦ �ӵ� limit ����
            currentVelocity.x = moveSpeedLimit * Mathf.Sign(currentVelocity.x);

            // ���ӷ� 0���� ����
            sideMoveForce = Vector2.zero;
        }
        // �� �ܿ� ��Ȳ������ ����or����
        else
        {
            sideMoveForce = (Vector2)(transform.right * InputDir.x).normalized * moveAccelation / 10f;
        }

        // Ⱦ �̵� �Է°��� ���ٸ� => ������ ����
        if (InputDir.x == 0)
        {
            // ���� �ӵ� �̻��� �� ����
            if (Mathf.Abs(currentVelocity.x) > 0.01f)
            {
                sideMoveForce = -(Vector2)(transform.right * currentVelocity.x).normalized * dragAccelation / 10f;
            }
            // ���� �ӵ� ���϶�� finalVelocity = zero�� ��ȯ�� ����
            else
            {
                // Ⱦ �̵��� zero�� �ǰ� �����.
                finalVelocity = Vector2.zero;
                currentVelocity = Vector2.zero;
                return Vector2.zero;
            }
        }
        
        finalVelocity = currentVelocity + sideMoveForce * Time.fixedDeltaTime;
        currentVelocity = finalVelocity;

        return finalVelocity;
    }

    public void Jump()
    {
        transform.position += (Vector3)GetFinalVelocity();
    }

}
