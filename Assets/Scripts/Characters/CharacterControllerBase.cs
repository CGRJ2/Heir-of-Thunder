using UnityEngine;
namespace CJM
{
    public abstract class CharacterControllerBase : MonoBehaviour
    {
        [SerializeField] protected ColliderController colliderController;
        
        // 콜라이더 상태 처리




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




}
