using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class ColliderState : MonoBehaviour
{
    // 이건 상태 전환에 유연할 필요없으니까, 그냥 enum타입으로 그때그때 맞는 상태를 업데이트하도록 만들자

    // 45도 기준 Wall vs Ground
    [SerializeField] LayerMask layerMask;
    [SerializeField] float groundRayRadius;
    public Collider[] groundHitColids;
    public bool isGroundCheckWait;
    public bool isGrounded;
    Vector3 groundNormal = Vector3.zero;


    public bool isWallSide;
    public bool isEdge; // isGrounded일 때

    //법선벡터가 45도 이상/이하로 기준 나뉨

    public void FixedUpdate()
    {
        GroundCheck();
    }

    public void GroundCheck()
    {
        if (isGroundCheckWait) return;

        groundHitColids = Physics.OverlapSphere(transform.position, groundRayRadius, layerMask);
        if (groundHitColids.Length > 0)
        {
            isGrounded = true;
        }

        // 목표 : 현재 접지한 가장 가까운 콜라이더의 충돌 지점에서의 법선 벡터 반환
        foreach (var col in groundHitColids)
        {
            Vector3 point = col.ClosestPoint(transform.position);
            Debug.DrawLine(point, point + col.transform.up * 10f, Color.red);
            groundNormal = col.transform.up;
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, groundRayRadius);

    }
    
}

