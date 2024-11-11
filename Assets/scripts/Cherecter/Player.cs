using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicBattles
{
    public class Player : PlayerManager, IUnitData
    {
        
        [SerializeField] private int m_id;
        public int ID
        {
            get => m_id;
            private set => m_id = value;
        }
        
        [SerializeField] private int m_health;
        public int Health { get => m_health; set => m_health = value; }

        [SerializeField] private bool m_myTurn;

        public bool MyTurn
        {
            get => m_myTurn;
            set
            {
                m_myTurn = value;
                if (value)
                {
                    _abilityManager.EnabledButtons();
                }
                else
                {
                    _abilityManager.DisableButtons();
                }
                
            }
        }

        [SerializeField] private List<byte[]> _activeAbilities = new List<byte[]>();

        public List<byte[]> Ability
        {
            set
            {
                _activeAbilities.Clear();
                _activeAbilities.AddRange(value);
            }
        }
        
        
        [SerializeField]
        private int _idEnamy;
        public int IDEnamy
        {
            get => _idEnamy;
            set
            {
                _idEnamy = (ID == value) ? 0 : value;
            }
        }
        
        private AbililtyInputManager _abilityManager;
        protected override void Initialization()
        {
            base.Initialization();
            _abilityManager = GetComponent<AbililtyInputManager>();
        }

        public void GetMessegToServer( byte id_attack )
        {
            if (IDEnamy == 0)
            {
                Debug.Log("Enamy is not Selected");
                return;
            }
            
            UseAbility useAbility = new UseAbility(ID, IDEnamy, id_attack);
            
            ClientController.GetMessageToServer(useAbility.Encode());
            
            MyTurn = false;
        }

        public void PlayerInitializationStats(UnitStats stats)
        {
            ID = stats.ID;
            Health = stats.Health;
            _healAmplitude.UpdateHealth(stats.Health);
        }

        public void SetPlayerInfo(ClientHealthAmount status)
        {
            Health = status.Health;
            _healAmplitude.UpdateHealth(Health);
        }

        public void SetAbility( List<byte[]> abilites)
        {
            _activeAbilities = abilites;
            foreach (var ability in abilites)
            {
               // Debug.Log($"Send Ability ID: {ability[0]}, Reloading: {ability[1]}");
                _abilityManager.AbilityStats(ability[0], ability[1]);
            }
        }
        
    }
}