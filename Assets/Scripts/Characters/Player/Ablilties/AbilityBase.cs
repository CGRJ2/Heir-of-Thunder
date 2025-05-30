using UnityEngine;
namespace CJM
{
    public class AbilityBase : ScriptableObject
    {
        [SerializeField] int priority;
        [SerializeField] string abilityName;
        protected bool isLocked;
        protected bool isActive;

        protected CharacterControllerBase controllerBase;


        public virtual void InitAbility(CharacterControllerBase characterControllerBase)
        {
            if (characterControllerBase == null)
            {
                Debug.LogError("캐릭터 컨트롤러가 발견되지 않음!");
                return;
            }
            controllerBase = characterControllerBase;
        }
    }
}
