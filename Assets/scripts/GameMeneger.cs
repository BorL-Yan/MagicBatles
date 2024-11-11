using System.Collections.Generic;
using MagicBattles;
using UnityEngine;

public class GameMeneger : MonoBehaviour
{
    public static GameMeneger Instance;
    public Camera mainCamera;

    [SerializeField] private List<Player> m_unity = new List<Player>();
    private List<Player> _players = new List<Player>();
    public List<Player> Players { get { return _players; } }

    [SerializeField] private List<Transform> m_spavnPosition = new List<Transform>();
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            mainCamera = Instantiate(mainCamera);
        }
        else if (Instance != this)
        {
            Debug.LogWarning("Instance already exists, destroying object!");
            Destroy(this);
        }
            
    }

    public void StartGame(int option)
    {
        Debug.Log($"Start Optin Game - {option}");
        switch (option)
        {
            case 0:
            {
                if (m_unity != null) {
                    InitializationPlayer();
                }

                if (m_unity != null) {
                    InitializationAI();
                }
                break;
            }
            case 1:
            {
                if (m_unity != null) {
                    InitializationPlayer();
                }
                if (m_unity != null) {
                    InitializationPlayer();
                }
                break;
            }
            case 2:
            {
                if (m_unity != null) {
                    InitializationAI();
                }
                if (m_unity != null) {
                    InitializationAI();
                }
                break;
            }
        }

        void InitializationPlayer()
        {
                
            var unitObject = Instantiate(m_unity[0], m_spavnPosition[0].position, Quaternion.identity);
            unitObject.GetComponent<InputPlayer>().IsUser();
                        
            Destroy(unitObject.GetComponent<InputAI>());
                        
            m_unity.RemoveAt(0);
            m_spavnPosition.RemoveAt(0);
            Players.Add(unitObject);
        }

        void InitializationAI()
        {
            //  IS DESTROY LINIE
            m_unity[0].GetComponent<EnamySelected>().enabled = false;
                
            var unitObject = Instantiate(m_unity[0],m_spavnPosition[0].position, Quaternion.identity);
            unitObject.GetComponent<InputAI>().IsAI();
                        
            Destroy(unitObject.GetComponent<EnamySelected>());
            Destroy(unitObject.GetComponent<InputPlayer>());
                        
                
            m_unity.RemoveAt(0);
            m_spavnPosition.RemoveAt(0);
            Players.Add(unitObject);
        }

    }
}