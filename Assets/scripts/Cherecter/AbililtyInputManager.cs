using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MagicBattles
{
    public class AbililtyInputManager : MonoBehaviour
    {
        [SerializeField] private List<Button> m_abilityButtons;
        
        [SerializeField] private List<InputAbilityStats> m_abilites = new List<InputAbilityStats>();

        public List<InputAbilityStats> Abilites => m_abilites;

        private InputPlayer _input;
        
        public void InitializingButton(bool isAI)
        {
            if (!isAI) _input=GetComponent<InputPlayer>();
            
            for (int i = 0; i < m_abilityButtons.Count; i++)
            {
                Button button = m_abilityButtons[i];
                
                InputAbilityStats abilityStats = new InputAbilityStats();
                AbilityUse abilityUse = button.GetComponent<AbilityUse>();
                abilityStats.CoolDown = abilityUse.CoolDown;
                abilityStats.Ability_ID = abilityUse.Ability_ID;
                abilityStats.AbilityIcon = abilityUse.AbilityIcon;
                abilityStats.AbilityReloadingText = abilityUse.AbilityReloadingText;
                m_abilites.Add(abilityStats);
                
                if (!isAI)
                {
                    byte abilityID = abilityUse.Ability_ID;
                    button.onClick.AddListener(() => _input.HandleAbilitySelected(abilityID));
                }
                else
                {
                    button.interactable = false;
                }
                
            }
        }

        public void DisableButtons()
        {
            foreach (var button in m_abilityButtons)
            {
                button.enabled = false;
            }
        }

        public void EnabledButtons()
        {
            foreach (var button in m_abilityButtons)
            {
                button.enabled = true;
            }
        }

        public void AbilityStats(byte id_ability, byte reloading)
        {
            InputAbilityStats abilityStats = GetAbilityStats(id_ability);

            if (reloading == 0)
            {
                abilityStats.IsActive = false;
                abilityStats.ToContinue = true;
                return;
            }
            
            if (abilityStats != null )
            {
                if (!abilityStats.IsActive && reloading > 0)
                {
                    abilityStats.IsActive = true;
                    abilityStats.Reloading = reloading;
                    int buttonIndex = m_abilites.FindIndex(item => item.Ability_ID == id_ability);
                    if (buttonIndex >= 0)
                    {
                        Button button = m_abilityButtons[buttonIndex];
                        StartCoroutine(abilityStats.CooldownButton(button));
                    }
                }
                else
                {
                    abilityStats.ToContinue = true;
                }
            }
            
        }
        


        private InputAbilityStats GetAbilityStats(byte id_ability)
        {
            foreach (var item in m_abilites)
            {
                if(id_ability == item.Ability_ID) return item;
            }
            return null;
        }
    }
    
    
}