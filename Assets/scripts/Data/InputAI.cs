using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MagicBattles
{
    public class InputAI : PlayerManager
    {
        
        private AbililtyInputManager _abililtyInputManager;
        protected override void Initialization()
        {
            base.Initialization();
        }

        public void IsAI()
        {
            Debug.Log("IsAI");
            
            _abililtyInputManager = GetComponent<AbililtyInputManager>();
            _abililtyInputManager.InitializingButton(true);
            StartCoroutine(SelectAbility());
        }
        
        private int SelectEnamy()
        {
            
            List<Player> enemies = new List<Player>();
            enemies = GameMeneger.Instance.Players;
            for (int i = enemies.Count-1; i>=0; i--)
            {
                if (m_Player.ID == enemies[i].ID)
                {
                    enemies.RemoveAt(i);
                }
            }
            int index = Random.Range(0, enemies.Count);
            
            return enemies[index].ID;
        }

        private IEnumerator SelectAbility()
        {
            while (!m_isFinished)
            {
                while (!m_Player.MyTurn)
                {
                    yield return null;
                }
                
                yield return new WaitForSeconds(0.4f);
                List<InputAbilityStats> inputAbilities = new List<InputAbilityStats>();
                foreach (var ability in _abililtyInputManager.Abilites)
                {
                    if(!ability.IsActive)
                        inputAbilities.Add(ability);
                }
                
                byte selectIDAbility = (byte)Random.Range(0, inputAbilities.Count);
                
                byte id_ability = inputAbilities[selectIDAbility].Ability_ID;
                m_Player.IDEnamy = SelectEnamy();
                Debug.Log(id_ability);
                m_Player.GetMessegToServer(id_ability);
            }
        }
        
        
        
    }
}