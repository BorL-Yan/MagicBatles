using System;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Exception = System.Exception;

namespace MagicBattles.Server
{
    using csm = Clien_ServerMassage;
    public class ServerController : MonoBehaviour
    {
        [SerializeField] private string m_ip = "127.0.0.1";
        [SerializeField] private int m_port = 8080;

        [SerializeField] private int MaxClientRoom = 2;
        
        private int id_start = 0;

        TcpListener server = null;
        Thread thread;
        // Clients
        [SerializeField] private Dictionary<ClientStats, TcpClient> clients = new Dictionary<ClientStats, TcpClient>();
        
        // Client Turn
        private Queue<int> _clientsTurn = new Queue<int>();
        
        // Client Command
        private Dictionary<byte, IClientCommand> _commands;
        
        private void Start()
        {
            thread = new Thread(new ThreadStart(SetupServer));
            thread.Start();
            InitializeCommands();
        }
        
        private async void SetupServer()
        {
            try
            {
                IPAddress localAddr = IPAddress.Parse(m_ip);
                server = new TcpListener(localAddr, m_port);
                server.Start();

                while (true)
                {
                    //Debug.Log("Waiting for connection...");
                    TcpClient client = await server.AcceptTcpClientAsync();
                    //Debug.Log("Connected!");

                    ClientStats data = new ClientStats(++id_start);
                    clients.Add(data, client);

                    _ = Task.Run(() => HandleClient(client));
                    //_ = HandleClient(data, client);
                    //_ = Task.Run(() => SendMessageToClient(id, data));

                    InitialClientData initialClientData = null;
                    initialClientData = new InitialClientData(id_start, 100);
                    
                    byte[] message = initialClientData.Encode();
                    _ = SendMessageToClient(initialClientData.ID, message);

                    _clientsTurn.Enqueue(id_start);
                    if (_clientsTurn.Count == MaxClientRoom)
                    {
                        Debug.Log($"StartSinqronize of Clietn Turn");
                        _ = GetAllClient_Turn();
                    }
                }
            }
            catch (SocketException e)
            {
                Debug.LogWarning("SocketException: " + e.Message);
            }
            finally
            {
                server.Stop();
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            try
            {
                byte[] buffer = new byte[64];
                
                while (true)
                {
                    int length = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (length == 0)
                    {
                        return;
                    }
                    
                    byte[] message = new byte[length];
                    Array.Copy(buffer, message, length);
                    
                    ReadClientMessage(message);
                }

            }
            catch (SocketException e)
            {
                Debug.LogWarning("SocketException: " + e.Message);
            }
            finally
            {
                stream.Close();
                client.Close();
                Debug.Log("Client disconnected.");
            }
        }

        private async void ReadClientMessage(byte[] data)
        {
            if (_commands.TryGetValue(data[0], out var command))
            {
                command.Execute(data, this);
            }
            await LastColculation();
        }
        
        public void AttackEnamy(UseAbility useAbility)
        {
            ClientStats enamyUnit = GetClientData(useAbility.Enamy_ID);
            ClientStats myUnit = GetClientData(useAbility.Player_ID);
            
            switch (useAbility.Abality)
            {
                case (byte)Ability_Name.Atack:
                {
                    Attack _attack = new Attack();
                    enamyUnit.Health -= _attack.Damage;
                    break;
                }
                case (byte)Ability_Name.Barrier:
                {
                    myUnit.SetAbility(true, new Barrier());
                    myUnit.SetAbility(false, new Barrier());
                    break;
                }
                case (byte)Ability_Name.Regeneration:
                {
                    myUnit.SetAbility(true, new Regeneration());
                    myUnit.SetAbility(false, new Regeneration());
                    break;
                }
                case (byte)Ability_Name.FireBol:
                {
                    FireBol _fireBol = new FireBol();
                    enamyUnit.Health -= _fireBol.Damage;
                    enamyUnit.SetAbility(true, new FireBol());
                    myUnit.SetAbility(false, new FireBol());
                    break;
                }
                case (byte)Ability_Name.Cleaning:
                {
                    myUnit.CliningAbality();
                    myUnit.SetAbility(false, new Cleaning());
                    break;
                }
            }

        }

        private async Task LastColculation()
        {
            GetAllClient_Helath_Amount();
            await Task.Delay(100);
            GetAllClient_Ability_Amount();
            foreach (var client in clients)
            {
                client.Key.LastColculation();
                client.Key.EmployAbility();
            }
            GetAllClient_Helath_Amount();
            GetAllClient_Turn();
        }

        private async Task GetAllClient_Ability_Amount()
        {
            try
            {
                foreach (var client in clients)
                {
                    
                    List<byte[]> byte_abality= new List<byte[]>();
                    
                    foreach (var item in client.Key.ActiveAbilites)
                    {
                        if (item is DurationHandler reloading)
                        {
                            Ability_Name abilityName = item.GetName();
                            
                            // Debug.Log($" Ability Name: {abilityName}, " +
                            //           $"\n Ability finished: {reloading.AbilityFinished}, Duration finished: {reloading.DurationFinished}," +
                            //           $"\n Reloading: {reloading.Reloading}, Duration: {reloading.Duration}");
                            //
                            byte[] addToList = {(byte)abilityName, reloading.Reloading};
                            byte_abality.Add(addToList);
                        }
                    }
                    
                    ClietnAbilityAmount abilityAmount = new ClietnAbilityAmount(byte_abality);
                    
                    byte[] data = abilityAmount.Encode();
                    _ = SendMessageToClient(client.Key.ID, data);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Exeption: {e.Message}");
            }
        }
        
        private async Task GetAllClient_Helath_Amount()
        {
            try
            {
                foreach (var client in clients)
                {
                    ClientHealthAmount clientStatus = new ClientHealthAmount(client.Key.Health);
                
                    byte[] data = clientStatus.Encode();
                    _ = SendMessageToClient(client.Key.ID, data);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Exeption: {e.Message}");
            }
        }

        private async Task GetAllClient_Turn()
        {
            try
            {
                int clientIDTurn = GetTurnClient();

                
                ClientTurn clientTurn = new ClientTurn();
                foreach (var client in clients)
                {
                    clientTurn.MyTurn = false;
                    if (client.Key.ID == clientIDTurn)
                    {
                        clientTurn.MyTurn = true;
                        //client.Key.MyTurn = true;
                    }

                    byte[] data = clientTurn.Encode();
                    Debug.Log($"Send Turn: {client.Key.ID}");
                    _ = SendMessageToClient(client.Key.ID, data);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Exeption: {e.Message}");
                throw;
            }
            
        }

        private int GetTurnClient()
        {
            int clientID = _clientsTurn.Dequeue();
            _clientsTurn.Enqueue(clientID);
            return clientID;
        }
        
        public async Task SendMessageToClient(int id, byte[] messageToCLient)
        {
            TcpClient client = GetClient(id);
            NetworkStream stream = client.GetStream();
            try
            {
                await stream.WriteAsync(messageToCLient);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Exeption: {e.Message}");
            }
        }

        private TcpClient GetClient(int id)
        {
            foreach (var item in clients)
            {
                if (item.Key.ID == id)
                {
                    return item.Value;
                }
            }

            return null;
        }

        private ClientStats GetClientData(int id)
        {
            foreach (var item in clients)
            {
                if (item.Key.ID == id)
                {
                    return item.Key;
                }
            }

            return null;
        }

        private void InitializeCommands()
        {
            _commands = new Dictionary<byte, IClientCommand>
            {
                { (byte)csm.UseAbility, new UseAbilityCommand() },
            };
        }
        
        
        private void OnApplicationQuit()
        {
            foreach (var item in clients)
                item.Value.Close();

            server.Stop();
            thread.Abort();
        }
    }
}
