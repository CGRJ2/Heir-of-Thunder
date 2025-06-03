using UnityEngine;

public class ColliderState : MonoBehaviour
{
    // 이건 상태 전환에 유연할 필요없으니까, 그냥 enum타입으로 그때그때 맞는 상태를 업데이트하도록 만들자

    // 45도 기준 Wall vs Ground
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
    public bool isEdge; // isGrounded일 때

    //법선벡터가 45도 이상/이하로 기준 나뉨

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

            // 목표 : 현재 접지한 가장 가까운 콜라이더의 충돌 지점에서의 법선 벡터 반환
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

            // 목표 : 현재 접지한 가장 가까운 콜라이더의 충돌 지점에서의 법선 벡터 반환
            float closestDistance = float.MaxValue;

            foreach (var hit in groundHits)
            {
                // 바닥
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
                // 벽
                else
                {
                    Debug.LogWarning("천장 충돌 무시");
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

