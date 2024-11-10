using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MagicBattles
{
    public class AbilityUse : MonoBehaviour, IAbilityUseData
    {
        [SerializeField] private Ability_Name m_abilityName;
        public byte Ability_ID => (byte)m_abilityName;
        
        [SerializeField] private Image m_abilityIcon;
        public Image AbilityIcon => m_abilityIcon;
        
        [SerializeField] private TextMeshProUGUI m_abilityReloadingText;
        public TextMeshProUGUI AbilityReloadingText { 
            get => m_abilityReloadingText;
            set => m_abilityReloadingText = value; 
        }

        [SerializeField] private GameObject m_coolDown;
        public GameObject CoolDown
        {
            get => m_coolDown;
            set => m_coolDown = value;
        }
    }

    
}