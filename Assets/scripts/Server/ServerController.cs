using System;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.PlayerLoop;
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
            else
            {
                Debug.Log($"Not a Command!!!: {data[0]}");
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
                    myUnit.SetAbility( new Barrier(), true);
                    myUnit.SetAbility( new Barrier(), false);
                    break;
                }
                case (byte)Ability_Name.Regeneration:
                {
                    myUnit.SetAbility( new Regeneration(), true);
                    myUnit.SetAbility( new Regeneration(), false);
                    break;
                }
                case (byte)Ability_Name.FireBol:
                {
                    FireBol _fireBol = new FireBol();
                    enamyUnit.Health -= _fireBol.Damage;
                    enamyUnit.SetAbility( new FireBol(), true);
                    myUnit.SetAbility( new FireBol(), false);
                    break;
                }
                case (byte)Ability_Name.Cleaning:
                {
                    myUnit.CliningAbality();
                    myUnit.SetAbility( new Cleaning(), false);
                    break;
                }
            }

        }

        private async Task LastColculation()
        {
            await GetAllClient_Helath_Amount();
            await Task.Delay(100);
            await GetAllClient_Ability_Amount();
            foreach (var client in clients)
            {
                client.Key.LastColculation();
                client.Key.EmployAbility();
            }
            await GetAllClient_Helath_Amount();
            await GetAllClient_Turn();
        }

        private async Task WhoIsTheWinner()
        {
            bool thereIsAWiner = false;
            foreach (var client in clients)
            {
                thereIsAWiner = client.Key.isDead;
            }

            if (!thereIsAWiner) return;
            
            foreach (var client in clients)
            {
                await SendMessageToClient(client.Key.ID,new WinOrLose(client.Key.isDead).Encode());
            }
        }
        
        private async Task GetAllClient_Ability_Amount()
        {
            try
            {
                foreach (var client in clients)
                {
                    List<byte[]> ability = client.Key.ActiveAbilites
                        .Where(item => item is DurationHandler)
                        .Select(item =>
                        {
                            var reloading = item as DurationHandler;
                            Ability_Name abilityName = item.GetName();
                            return new byte[] { (byte)abilityName, reloading.Reloading };
                        })
                        .ToList();
                    await SendMessageToClient(client.Key.ID, new ClietnAbilityAmount(ability).Encode());
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
                    await SendMessageToClient(client.Key.ID, new ClientHealthAmount(client.Key.Health).Encode());
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
                foreach (var client in clients)
                {
                    ClientTurn clientTurn = new ClientTurn();
                    if (client.Key.ID == clientIDTurn) clientTurn.MyTurn = true;

                    await SendMessageToClient(client.Key.ID, clientTurn.Encode());
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Exeption: {e.Message}");
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
            if (client == null)
            {
                Debug.LogWarning($"Client {id} not found.");
                return;
            }
            
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
