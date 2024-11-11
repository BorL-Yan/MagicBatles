
//using TMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MagicBattles
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager instance;
        
        [SerializeField] private GameObject m_startManu;
        [SerializeField] private TMP_Dropdown m_selectOptoin;

        private void Start()
        {
            m_startManu.SetActive(true);
        }


        public void SelectOption()
        {
            m_startManu.SetActive(false);
            GameMeneger.Instance.StartGame(m_selectOptoin.value);
        }
    }
}