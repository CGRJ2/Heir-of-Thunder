using System.Collections.Generic;
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

    public float wallMinHeight; // 벽으로 판정될 최소 높이
    public float sideRayDistance;
    public RaycastHit[] rightWallHits;
    public RaycastHit[] leftWallHits;
    public Collider closestLeftWallCollider;
    public Collider closestRightWallCollider;
    public Vector3 leftWallHitPos;
    public Vector3 rightWallHitPos;
    public Vector3 enterDir;

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
        WallCheck();
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
            closestGroundCollider = null;
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

        /*// 최종 hit 확정 후, 정확하게 위치 맞추기
        if (closestDistance != float.MaxValue && closestDistance > 0)
        {
            transform.position += dir * closestDistance;
        }*/
    }

    public void WallCheck()
    {
        if (colliderSelf is CapsuleCollider capCol)
        {
            Vector3 centerDown = transform.position + colliderSelf.transform.up * wallMinHeight + colliderSelf.transform.up * capCol.radius;
            Vector3 centerTop = transform.position + colliderSelf.transform.up * capCol.height - colliderSelf.transform.up * capCol.radius;

            Vector3 colliderRight = colliderSelf.transform.right;

            closestRightWallCollider = GetWallCollider(centerDown, centerTop, capCol.radius, colliderRight, out rightWallHitPos);
            closestLeftWallCollider = GetWallCollider(centerDown, centerTop, capCol.radius, -colliderRight, out leftWallHitPos);
        }
    }

    public Collider GetWallCollider(Vector3 centerDown, Vector3 centerTop, float radius, Vector3 dir, out Vector3 hitPos)
    {
        RaycastHit[] raycastHits = Physics.CapsuleCastAll(centerDown, centerTop, radius, dir, sideRayDistance, layerMask);
        Collider closestWallCollider = null;
        Vector2 correctSidePos = centerDown + dir * sideRayDistance;

        hitPos = Vector3.zero;

        // 가장 접촉 지점 -> 가장 낮은 접촉 지점
        if (raycastHits.Length > 0)
        {
            float lowestY = float.MaxValue;
            float closestDistance = float.MaxValue;
            List<RaycastHit> closestHits = new List<RaycastHit>();

            // 캡슐 콜라이더와 가장 가까운 거리 확인
            foreach (var hit in raycastHits)
            {
                if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                }
            }

            // 가까운 점들 리스트로 받기
            foreach (var hit in raycastHits)
            {
                if (hit.distance == closestDistance)
                {
                    closestHits.Add(hit);
                }
            }

            // 가까운 점들 중 Y값 가장 낮은 점으로 콜라이더 결정
            foreach (var hit in closestHits)
            {
                if (hit.collider == closestGroundCollider) continue;

                if (hit.point.y < lowestY)
                {
                    lowestY = hit.point.y;
                    closestWallCollider = hit.collider;
                    hitPos = hit.point;
                    
                    
                    if (closestDistance > 0)
                    {
                        enterDir = hit.point - transform.position;
                    }
                }
            }

            // 최종 hit 확정 후, 정확하게 위치 맞추기
            if (closestDistance != float.MaxValue && Mathf.Sign(enterDir.x) == Mathf.Sign(GetComponent<PlayerController>().InputDir.x))
            {
                transform.position += dir * closestDistance;
            }

        }
        

            return closestWallCollider;
    }

    public void SetColliderToQuaternion(Quaternion quaternion)
    {
        colliderSelf.gameObject.transform.rotation = quaternion;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, groundRayRadius);

        if (colliderSelf is CapsuleCollider capCol)
        {
            // 캡슐 시작점과 끝점 계산
            Vector3 centerDown = transform.position + colliderSelf.transform.up * wallMinHeight + colliderSelf.transform.up * capCol.radius;
            Vector3 centerTop = transform.position + colliderSelf.transform.up * capCol.height - colliderSelf.transform.up * capCol.radius;

            // 오른쪽 방향 기즈모
            Gizmos.color = Color.red;
            DrawCapsuleCastGizmo(centerDown, centerTop, capCol.radius, transform.right, sideRayDistance);

            // 왼쪽 방향 기즈모
            Gizmos.color = Color.blue;
            DrawCapsuleCastGizmo(centerDown, centerTop, capCol.radius, -transform.right, sideRayDistance);
        }
    }

    private void DrawCapsuleCastGizmo(Vector3 point1, Vector3 point2, float radius, Vector3 direction, float distance)
    {
        // 시작 캡슐 위치
        DrawCapsule(point1, point2, radius);

        // 끝 캡슐 위치 (방향으로 distance만큼 이동)
        Vector3 end1 = point1 + direction.normalized * distance;
        Vector3 end2 = point2 + direction.normalized * distance;

        DrawCapsule(end1, end2, radius);

        // 연결 선 (양 끝)
        Gizmos.DrawLine(point1 + Vector3.forward * radius, end1 + Vector3.forward * radius);
        Gizmos.DrawLine(point1 - Vector3.forward * radius, end1 - Vector3.forward * radius);
    }

    private void DrawCapsule(Vector3 p1, Vector3 p2, float radius)
    {
        // 간단한 캡슐 대체 표현: 구체 두 개 + 선
        Gizmos.DrawWireSphere(p1, radius);
        Gizmos.DrawWireSphere(p2, radius);
        Gizmos.DrawLine(p1 + Vector3.forward * radius, p2 + Vector3.forward * radius);
        Gizmos.DrawLine(p1 - Vector3.forward * radius, p2 - Vector3.forward * radius);
        Gizmos.DrawLine(p1 + Vector3.right * radius, p2 + Vector3.right * radius);
        Gizmos.DrawLine(p1 - Vector3.right * radius, p2 - Vector3.right * radius);
    }
}

