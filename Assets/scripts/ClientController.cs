using UnityEngine;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace MagicBattles
{
    using CSM = Clien_ServerMassage;
    public class ClientController : PlayerManager
    {

        [SerializeField] private string serverIP = "127.0.0.1"; // Set this to your server's IP address.
        [SerializeField] private int serverPort = 5468; // Set this to your server's port.


        private TcpClient client;
        private NetworkStream stream;
        private Thread clientReceiveThread;

        
        private ConcurrentQueue<byte[]> serverMessage = new ConcurrentQueue<byte[]>();

        private Dictionary<byte, IServerCommand> _commands;
        
        protected override void Initialization() => base.Initialization();
        
        private void InitializeCommands()
        {
            _commands = new Dictionary<byte, IServerCommand>
            {
                { (byte)CSM.InitialClientData, new InitializeDataCommand() },
                { (byte)CSM.ClientHelath, new HealthAmountCommand() },
                { (byte)CSM.ClientAbility, new AbilityAmountCommand() },
                { (byte)CSM.MyTurn, new MyTurnCommand() }
            };
        }
        
        void Start()
        {
            Invoke(nameof(ConnectToServer), 0.1f);
            InitializeCommands();
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
                if (_commands.TryGetValue(message[0], out var command))
                {
                    command.Execute(message, this, m_Player);
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
            try
            {
                if (client == null || !client.Connected)
                {
                    Debug.LogError("Client not connected to server.");
                    return;
                }
                
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