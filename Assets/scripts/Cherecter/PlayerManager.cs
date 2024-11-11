using System;
using UnityEngine;

namespace MagicBattles
{
    public class PlayerManager : MonoBehaviour
    {
        protected Player m_Player;
        protected ClientController ClientController;
        protected HealtAmplitude _healAmplitude;
        
        protected bool m_isFinished;
        
        private void Awake()
        {
            Initialization();
        }

        protected virtual void Initialization()
        {
            m_Player = GetComponent<Player>();
            ClientController = GetComponent<ClientController>();
            _healAmplitude = GetComponent<HealtAmplitude>();
        }
    }
}