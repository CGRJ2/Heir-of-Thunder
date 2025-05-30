using UnityEngine;
namespace CJM
{
    public abstract class CharacterControllerBase : MonoBehaviour
    {
        [SerializeField] protected ColliderController colliderController;
        
        // �ݶ��̴� ���� ó��




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




}
