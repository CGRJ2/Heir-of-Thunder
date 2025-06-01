using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class ColliderState : MonoBehaviour
{
    // �̰� ���� ��ȯ�� ������ �ʿ�����ϱ�, �׳� enumŸ������ �׶��׶� �´� ���¸� ������Ʈ�ϵ��� ������

    // 45�� ���� Wall vs Ground
    [SerializeField] LayerMask layerMask;
    [SerializeField] float groundRayRadius;
    public Collider[] groundHitColids;
    public bool isGroundCheckWait;
    public bool isGrounded;
    Vector3 groundNormal = Vector3.zero;


    public bool isWallSide;
    public bool isEdge; // isGrounded�� ��

    //�������Ͱ� 45�� �̻�/���Ϸ� ���� ����

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

        // ��ǥ : ���� ������ ���� ����� �ݶ��̴��� �浹 ���������� ���� ���� ��ȯ
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

