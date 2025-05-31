using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderState : MonoBehaviour
{
    // 이건 상태 전환에 유연할 필요없으니까, 그냥 enum타입으로 그때그때 맞는 상태를 업데이트하도록 만들자

    // 45도 기준 Wall vs Ground

    public bool isGrounded;
    public bool isWallSide;
    public bool isEdge; // isGrounded일 때

    //법선벡터가 45도 이상/이하로 기준 나뉨
}

