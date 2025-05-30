using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderStateCheck : MonoBehaviour
{
    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    [Header("Wall Check")]
    public Transform wallCheckPoint;
    public float wallCheckDistance = 0.2f;

    [Header("Ledge Check")]
    public Transform ledgeCheckPoint;
    public float ledgeCheckDistance = 0.2f;

    public bool IsGrounded { get; private set; }
    public bool IsTouchingWall { get; private set; }
    public bool IsTouchingLedge { get; private set; }

    void Update()
    {
        UpdateGroundCheck();
        UpdateWallCheck();
        UpdateLedgeCheck();
    }

    void UpdateGroundCheck()
    {
        Collider[] ground = Physics.OverlapSphere(groundCheckPoint.position, groundCheckRadius, groundLayer);
    }

    void UpdateWallCheck()
    {
        RaycastHit2D wallHit = Physics2D.Raycast(wallCheckPoint.position, transform.right, wallCheckDistance, groundLayer);
        IsTouchingWall = (wallHit.collider != null);
    }

    void UpdateLedgeCheck()
    {
        Vector2 origin = ledgeCheckPoint.position;
        Vector2 down = Vector2.down;

        RaycastHit2D ledgeHit = Physics2D.Raycast(origin, down, ledgeCheckDistance, groundLayer);
        IsTouchingLedge = ledgeHit.collider != null;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint)
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);

        if (wallCheckPoint)
            Gizmos.DrawLine(wallCheckPoint.position, wallCheckPoint.position + transform.right * wallCheckDistance);

        if (ledgeCheckPoint)
            Gizmos.DrawLine(ledgeCheckPoint.position, ledgeCheckPoint.position + Vector3.down * ledgeCheckDistance);
    }
}
