using System;
using MagicBattles;
using TMPro;
using UnityEngine;

namespace MagicBattles
{


    public class EnamySelected : PlayerManager
    {
         private Camera _mainCamera;

        //[SerializeField] private LayerMask _layerMask;
        //private Player _player;
        
        protected override void Initialization() => base.Initialization();
        
        public Transform Point;
        private Transform point;
        
        private void Start()
        {
            point = Instantiate(Point).transform;
            _mainCamera = GameMeneger.Instance.mainCamera;
        }

        private void Update()
        {
            
            if (Input.GetMouseButtonDown(0))
            {
                 Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
                 if (Physics.Raycast(ray, out RaycastHit hit, 100f))
                 {
                     point.position = hit.point; 
                     if (hit.collider.tag == "Player")
                     {
                         point.position = hit.point;
                         m_Player.IDEnamy = hit.collider.gameObject.GetComponent<Player>().ID;
                     }
                 }
            }
        }
    }
}