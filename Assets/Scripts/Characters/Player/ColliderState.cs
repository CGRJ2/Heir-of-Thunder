using System.Collections.Generic;
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

    public float wallMinHeight; // ������ ������ �ּ� ����
    public float sideRayDistance;
    public RaycastHit[] rightWallHits;
    public RaycastHit[] leftWallHits;
    public Collider closestLeftWallCollider;
    public Collider closestRightWallCollider;
    public Vector3 leftWallHitPos;
    public Vector3 rightWallHitPos;
    public Vector3 enterDir;

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
        WallCheck();
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
            closestGroundCollider = null;
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

        /*// ���� hit Ȯ�� ��, ��Ȯ�ϰ� ��ġ ���߱�
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

        // ���� ���� ���� -> ���� ���� ���� ����
        if (raycastHits.Length > 0)
        {
            float lowestY = float.MaxValue;
            float closestDistance = float.MaxValue;
            List<RaycastHit> closestHits = new List<RaycastHit>();

            // ĸ�� �ݶ��̴��� ���� ����� �Ÿ� Ȯ��
            foreach (var hit in raycastHits)
            {
                if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                }
            }

            // ����� ���� ����Ʈ�� �ޱ�
            foreach (var hit in raycastHits)
            {
                if (hit.distance == closestDistance)
                {
                    closestHits.Add(hit);
                }
            }

            // ����� ���� �� Y�� ���� ���� ������ �ݶ��̴� ����
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

            // ���� hit Ȯ�� ��, ��Ȯ�ϰ� ��ġ ���߱�
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
            // ĸ�� �������� ���� ���
            Vector3 centerDown = transform.position + colliderSelf.transform.up * wallMinHeight + colliderSelf.transform.up * capCol.radius;
            Vector3 centerTop = transform.position + colliderSelf.transform.up * capCol.height - colliderSelf.transform.up * capCol.radius;

            // ������ ���� �����
            Gizmos.color = Color.red;
            DrawCapsuleCastGizmo(centerDown, centerTop, capCol.radius, transform.right, sideRayDistance);

            // ���� ���� �����
            Gizmos.color = Color.blue;
            DrawCapsuleCastGizmo(centerDown, centerTop, capCol.radius, -transform.right, sideRayDistance);
        }
    }

    private void DrawCapsuleCastGizmo(Vector3 point1, Vector3 point2, float radius, Vector3 direction, float distance)
    {
        // ���� ĸ�� ��ġ
        DrawCapsule(point1, point2, radius);

        // �� ĸ�� ��ġ (�������� distance��ŭ �̵�)
        Vector3 end1 = point1 + direction.normalized * distance;
        Vector3 end2 = point2 + direction.normalized * distance;

        DrawCapsule(end1, end2, radius);

        // ���� �� (�� ��)
        Gizmos.DrawLine(point1 + Vector3.forward * radius, end1 + Vector3.forward * radius);
        Gizmos.DrawLine(point1 - Vector3.forward * radius, end1 - Vector3.forward * radius);
    }

    private void DrawCapsule(Vector3 p1, Vector3 p2, float radius)
    {
        // ������ ĸ�� ��ü ǥ��: ��ü �� �� + ��
        Gizmos.DrawWireSphere(p1, radius);
        Gizmos.DrawWireSphere(p2, radius);
        Gizmos.DrawLine(p1 + Vector3.forward * radius, p2 + Vector3.forward * radius);
        Gizmos.DrawLine(p1 - Vector3.forward * radius, p2 - Vector3.forward * radius);
        Gizmos.DrawLine(p1 + Vector3.right * radius, p2 + Vector3.right * radius);
        Gizmos.DrawLine(p1 - Vector3.right * radius, p2 - Vector3.right * radius);
    }
}

