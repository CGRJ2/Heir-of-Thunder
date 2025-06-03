using UnityEngine;

public class ColliderState : MonoBehaviour
{
    // �̰� ���� ��ȯ�� ������ �ʿ�����ϱ�, �׳� enumŸ������ �׶��׶� �´� ���¸� ������Ʈ�ϵ��� ������

    // 45�� ���� Wall vs Ground
    [SerializeField] Collider colliderSelf;
    [SerializeField] LayerMask layerMask;
    [SerializeField] float groundRayRadius;
    [SerializeField] float coyoteTime;
    public float coyoteTimeCounter;
    public Collider[] groundHitColids;
    public Collider closestGroundCollider;
    public bool isGroundCheckWait;
    public bool isGrounded;
    public Vector3 groundNormal = Vector3.zero;
    public Vector3 groundHitPos;


    public bool isWallSide;
    public bool isEdge; // isGrounded�� ��

    //�������Ͱ� 45�� �̻�/���Ϸ� ���� ����

    private void Start()
    {
        //Application.targetFrameRate = 60;
    }

    public void Update()
    {
        //GroundCheck();
    }

    public void FixedUpdate()
    {
        GroundCheck();
    }

    public void GroundCheck()
    {
        groundHitColids = Physics.OverlapSphere(transform.position, groundRayRadius, layerMask);
        if (groundHitColids.Length > 0)
        {
            isGrounded = true;
            coyoteTimeCounter = coyoteTime;

            // ��ǥ : ���� ������ ���� ����� �ݶ��̴��� �浹 ���������� ���� ���� ��ȯ
            foreach (var col in groundHitColids)
            {
                Vector3 point = col.ClosestPoint(transform.position);
                Debug.DrawLine(point, point + col.transform.up * 10f, Color.red);
                groundHitPos = point;
                groundNormal = col.transform.up;
                closestGroundCollider = col;
            }
        }
        else
        {
            isGrounded = false;
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }

        /*groundHits = Physics.SphereCastAll(transform.position + transform.up * 0.1f, groundRayRadius, Vector3.down, 0.5f, layerMask);

        if (groundHits.Length > 0)
        {
            isGrounded = true;
            coyoteTimeCounter = coyoteTime;

            // ��ǥ : ���� ������ ���� ����� �ݶ��̴��� �浹 ���������� ���� ���� ��ȯ
            float closestDistance = float.MaxValue;

            foreach (var hit in groundHits)
            {
                // �ٴ�
                if (Vector3.Dot(hit.normal, Vector3.up) > 0.5f)
                {
                    Debug.Log(hit.distance);
                    if (hit.distance > 0 && hit.distance < closestDistance)
                    {
                        closestDistance = hit.distance;
                        groundHitPos = hit.point;
                        groundNormal = hit.normal;
                        closestGroundCollider = hit.collider;
                    }
                }
                // ��
                else
                {
                    Debug.LogWarning("õ�� �浹 ����");
                    continue;
                }
            }
            if (closestDistance == float.MaxValue)
            {
                Debug.Log(groundHitPos);
            }

            Debug.DrawLine(groundHitPos, groundHitPos + groundNormal * 10, Color.red);

        }
        else
        {
            isGrounded = false;
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }*/
    }




    public void SetColliderToQuaternion(Quaternion quaternion)
    {
        colliderSelf.gameObject.transform.rotation = quaternion;
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, groundRayRadius);

    }

}

