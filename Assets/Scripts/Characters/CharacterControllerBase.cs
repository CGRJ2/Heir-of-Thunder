using UnityEngine;
public abstract class CharacterControllerBase : MonoBehaviour
{
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    

    

    // ��������Ʈ ���� ó��
    public string GetCurrentSpriteState()
    {

        return GetCurrentSpriteStateForDefault();
    }

    protected virtual string GetCurrentSpriteStateForDefault()
    {
        return "";
    }
}




