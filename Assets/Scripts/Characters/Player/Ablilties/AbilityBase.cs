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
                Debug.LogError("ĳ���� ��Ʈ�ѷ��� �߰ߵ��� ����!");
                return;
            }
            controllerBase = characterControllerBase;
        }
    }
}
