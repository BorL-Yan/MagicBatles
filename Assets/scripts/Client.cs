using UnityEngine;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace MagicBattles
{
    using CSM = Clien_ServerMassage;
    public class Client : PlayerManager
    {

        [SerializeField] private string serverIP = "127.0.0.1"; // Set this to your server's IP address.
        [SerializeField] private int serverPort = 5468; // Set this to your server's port.


        private TcpClient client;
        private NetworkStream stream;
        private Thread clientReceiveThread;

        
        private ConcurrentQueue<byte[]> serverMessage = new ConcurrentQueue<byte[]>();
        
        protected override void Initialization() => base.Initialization();
        
        void Start()
        {
            Invoke(nameof(ConnectToServer), 0.1f);
        }

        void Update()
        {
            if (serverMessage.Count > 0)
            {
                if (serverMessage.TryDequeue(out byte[] message))
                {
                    _ = ReadServerMessage(message);
                }
            }
        }

        public void ConnectToServer()
        {
            try
            {
                client = new TcpClient(serverIP, serverPort);
                
                //Debug.Log("Connected to server.");

                clientReceiveThread = new Thread(new ThreadStart(ListenForData));
                clientReceiveThread.IsBackground = true;
                clientReceiveThread.Start();
                
            }
            catch (SocketException e)
            {
                Debug.LogError("SocketException: " + e.ToString());
            }
        }

        private async void ListenForData()
        {
            stream = client.GetStream();
            if (stream == null)
            {
                Debug.LogError("Stream is null. Client may not be connected.");
                return;
            }
            try
            {
                byte[] messageFromServer = new byte[64];
                
                while (client.Connected)
                {
                    
                    int length = await stream.ReadAsync(messageFromServer, 0, messageFromServer.Length);
                    if (length == 0) 
                        return;
                    
                    byte[] message = new byte[length];
                    Array.Copy(messageFromServer, message, length);
                    
                    if (message[0] == (byte)CSM.MyTurn)
                    {
                        Debug.Log($"Clinet ID - {m_Player.ID}: Turn - {message[1]}");
                    }
                    
                    serverMessage.Enqueue(message);
                }
            }
            catch (SocketException socketException)
            {
                Debug.LogError("Socket exception: " + socketException);
            }
            finally
            {
                Debug.Log("Disconnected from server.");
                client?.Close();
                stream?.Close();
            }
        }
        
        
        private async Task ReadServerMessage(byte[] message)
        {
            await Task.CompletedTask;
            try
            {
                switch (message[0])
                {
                    case ((byte)CSM.InitialClientData):
                    {

                        InitialClientData initialClientData = new InitialClientData();
                        initialClientData.Decode(message);
                        UnitStats stats = new UnitStats(initialClientData.ID, initialClientData.Health);
                        m_Player.PlayerInitializationStats(stats);

                        break;
                    }
                    case (byte)CSM.ClientHelath:
                    {
                        
                        ClientHealthAmount clientHealth = new ClientHealthAmount();
                        clientHealth.Decode(message);
                        //m_Player.Health = clientHealth.Health;
                        m_Player.SetPlayerInfo(clientHealth);

                        break;
                    }
                    case (byte)CSM.ClientAbility:
                    {
                        ClietnAbilityAmount clinetAbility = new ClietnAbilityAmount();

                        clinetAbility.Decode(message);

                        m_Player.SetAbility(clinetAbility.Ability);

                        break;
                    }
                    case (byte)CSM.MyTurn:
                    {
                        ClientTurn clientTurn = new ClientTurn();
                        clientTurn.Decode(message);
                        m_Player.MyTurn = clientTurn.MyTurn;
                        // Debug.Log($"ID: {m_Player.ID}, Turn for attack: {m_Player.MyTurn}");

                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Read Server message: {e.Message}");
                throw;
            }

        }

        private async Task SendMessageToServer(byte[] data)
        {
            
            //Debug.Log(this.gameObject.name);
            try
            {
                if (client == null || !client.Connected)
                {
                    Debug.LogError("Client not connected to server.");
                    return;
                }
                //Debug.Log("Sent message to server: " + data);
                await stream.WriteAsync(data, 0, data.Length);

            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
                throw;
            }
        }

        public void GetMessageToServer(byte[] data)
        {
            _ = SendMessageToServer(data);
        }

        void OnApplicationQuit()
        {
            if (stream != null)
                stream.Close();
            if (client != null)
                client.Close();
            if (clientReceiveThread != null)
                clientReceiveThread.Abort();
        }

    }
}