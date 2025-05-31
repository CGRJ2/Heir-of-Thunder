using UnityEngine;
public abstract class CharacterControllerBase : MonoBehaviour
{
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    

    

    // 스프라이트 상태 처리
    public string GetCurrentSpriteState()
    {

        return GetCurrentSpriteStateForDefault();
    }

    protected virtual string GetCurrentSpriteStateForDefault()
    {
        return "";
    }
}




