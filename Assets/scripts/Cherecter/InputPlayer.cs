using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MagicBattles
{
    public class InputPlayer : PlayerManager
    {
        private AbililtyInputManager _abililtyInputManager;
        
        protected override void Initialization()
        {
            base.Initialization();
        }

        public void IsUser()
        {
            _abililtyInputManager = GetComponent<AbililtyInputManager>();
            _abililtyInputManager.InitializingButton(false);
        }
        
        public void HandleAbilitySelected(byte id_ability)
        {
            Debug.Log($"Ability Selected: {id_ability}: My Turn: {m_Player.MyTurn}");
            if (m_Player.MyTurn)
            {
                m_Player.GetMessegToServer(id_ability);
            }
        }
        
        
    }

    
}