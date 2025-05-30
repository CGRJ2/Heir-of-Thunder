using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : CJM.CharacterControllerBase
{
    [SerializeField] float moveSpeedLimit = 0.16f;
    [SerializeField] float moveAccelation = 10f;
    [SerializeField] float dragAccelation = 6f;
    [SerializeField] float customGravity;

    private Rigidbody rb;

    Vector2 inputDir = Vector2.zero;
    [SerializeField] Vector2 finalVelocity;
    [SerializeField] Vector2 currentVelocity;
    [SerializeField] Vector2 sideMoveForce;


    //Vector2 moveDir;
    //Vector2 aimDir;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnMove(InputValue value)
    {
        inputDir = value.Get<Vector2>();
    }

    void SetDirections()
    {
        //aimDir = ((transform.right * inputMovement.x) + (transform.up * inputMovement.y)).normalized;
        //moveDir = (transform.right * inputMovement.x).normalized;
    }

    void Move()
    {
        //ClampInputVelocity(inputDir, )
        //rb.MovePosition(transform.position + (Vector3)GetFinalVelocity());
        transform.position += (Vector3)GetFinalVelocity();
    }

    private void FixedUpdate()
    {
        Move();
    }

    public Vector2 GetFinalVelocity()
    {
        // �ִ� �ӵ��� ���� && �Է°� ��������� ���� ��
        if (Mathf.Abs(currentVelocity.x) >= moveSpeedLimit && Mathf.Sign(currentVelocity.x) == Mathf.Sign(inputDir.x))
        {
            // Ⱦ �ӵ� limit ����
            currentVelocity.x = moveSpeedLimit * Mathf.Sign(currentVelocity.x);

            // ���ӷ� 0���� ����
            sideMoveForce = Vector2.zero;
        }
        // �� �ܿ� ��Ȳ������ ����or����
        else
        {
            sideMoveForce = (Vector2)(transform.right * inputDir.x).normalized * moveAccelation / 10f;
        }

        // Ⱦ �̵� �Է°��� ���ٸ� => ������ ����
        if (inputDir.x == 0)
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

    // �ִ� �ӵ� ����
    public Vector2 ClampInputVelocity(Vector2 inputDir, Vector2 velocity, float limit)
    {
        float dot = Vector2.Dot(inputDir.normalized, velocity);
        if (dot > limit)
        {
            inputDir = Vector2.zero;
        }
        return inputDir;
    }

    // ������ (�ݴ���� �Է� �� �ӵ� ���̱�)
    /*public Vector2 GetStoppingForce(Vector2 velocity, float stoppingForce)
    {
        Vector2 inputDir = GetDirectedInputMovement();
        float dot = Vector2.Dot(inputDir, velocity);

        if (dot > 0 || (!applyStoppingWhenBraking && inputDir.magnitude >= 0.05f))
            return Vector2.zero;

        Vector2 stopDir = -GetWalkDirection(velocity); // ���� �̵� �ݴ� ����
        Vector2 maxStopChange = stopDir * stoppingForce * dt;
        Vector2 velInDir = | Dot(velocity, stopDir) | *stopDir;

        return (velInDir > maxStopChange) ? maxStopChange : velInDir;
    }*/

    // �⺻ ������, �Է��� ������ �ڵ� ����
    /*public Vector2 GetFriction(Vector2 velocity, Vector2 currentForce, float frictionConstant)
    {
        if (IsGrounded())
        {
            Vector2 dir = -GetWalkDirection(velocity); // ���� �̵� �ݴ� ����
            Vector2 maxFrictionChange = dir * frictionConstant * Time.fixedDeltaTime;

            Vector2 velInDir = | Dot(velocity, dir) | *dir;

            return (velInDir.magnitude > maxFrictionChange.magnitude) ? maxFrictionChange : velInDir;
        }

        return Vector2.zero;
    }

    public Vector2 GetDirectedInputMovement()
    {
        Vector2 input =
        Vector2 dir = GetWalkDirection(input);
    }*/

    /*public Vector2 GetGravity()
    {
        Vector2 fGravity = Vector2.down * customGravity;
        if (m_ApplyGravityIntoGroundNormal)
        {
            fGravity = -m_ControlledCollider.GetGroundedInfo().GetNormal() * customGravity;
        }
        if (m_ControlledCollider.IsGrounded() && !m_ApplyGravityOnGround)
        {
            fGravity = Vector2.zero;
        }
        return fGravity;
    }*/
}
