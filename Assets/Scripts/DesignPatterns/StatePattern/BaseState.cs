using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseState
{
    // 상태가 시작될때.
    public abstract void Enter();
    
    // 해당 상태에서 동작을 담당
    public abstract void Update();
    // 사용하지 않는 상태들도 있기때문에 가상함수로 선언.
    public virtual void FixedUpdate() { }
    
    // 상태가 끝날때.
    public abstract void Exit();
}


