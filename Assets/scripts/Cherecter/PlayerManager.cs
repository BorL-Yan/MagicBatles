using System;
using UnityEngine;

namespace MagicBattles
{
    public class PlayerManager : MonoBehaviour
    {
        protected Player m_Player;
        protected Client _client;
        protected HealtAmplitude _healAmplitude;
        
        protected bool m_isFinished;
        
        private void Awake()
        {
            Initialization();
        }

        protected virtual void Initialization()
        {
            m_Player = GetComponent<Player>();
            _client = GetComponent<Client>();
            _healAmplitude = GetComponent<HealtAmplitude>();
        }
    }
}