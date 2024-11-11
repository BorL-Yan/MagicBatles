using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using MagicBattles.Server;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MagicBattles
{
    public interface IUnitData
    {
        int ID { get; }
        int Health { get; }
    }

    public class ClientStats : IUnitData
    {
        public int ID { get; private set; }
        
        private int _health;
        public int Health
        {
            get => _health;
            set
            {
                if(value < _health)
                {
                    foreach (var ability in InteractionAbilites)
                    {
                        if(ability is IBlockDemage block)
                        {
                            int _value = value + block.BlockDemage;
                            value = ( _health < _value ) ? _health : _value;
                        }
                    }
                }
                
                _health = (value>=100) ? 100 : value;
            }
        }
        private List<IAbility> _interactionAbilites = new List<IAbility>();
        public List<IAbility> InteractionAbilites
        {
            get => _interactionAbilites;
            set
            {
                if(value != null)
                    _interactionAbilites = value;
            }
        }
        private List<IAbility> activeAbilites = new List<IAbility>();

        public List<IAbility> ActiveAbilites
        {
            get => activeAbilites;
        }
        public ClientStats(int _id, int health = 100)
        {
            ID = _id;
            Health = health;
        }

        public void SetAbility(bool isInteraction, IAbility _ability)
        {
            if (isInteraction) {
                AddAbility(InteractionAbilites);
            }
            else {
               AddAbility(ActiveAbilites);
            }

            void AddAbility(List<IAbility> abilites)
            {
                foreach (var item in abilites)
                {
                    if (item.GetName() == _ability.GetName())
                    {
                        abilites.Remove(item);
                        abilites.Add(_ability);
                        return;
                    }
                }
                abilites.Add(_ability);
            }
        }
        
        public byte GetAbilityReloadingCount(Ability_Name name)
        {
            foreach (var item in ActiveAbilites)
            {
                if (item.GetName() == name)
                {
                    DurationHandler reloading = (DurationHandler)item;
                    return reloading.Reloading;
                }
            }
            return 0;
        }
        
        public void CliningAbality()
        {
            Debug.Log($"Clining Abality");

            InteractionAbilites.RemoveAll(ability => ability is IDamageable);

            //
            // var abilitesToRemuve = new List<IAbility>();
            // foreach (var ability in InteractionAbilites)
            // {
            //     if (ability is IDamageable)
            //     {
            //         Debug.Log($"Clining Ability: {ability.GetName()}");
            //         
            //         abilitesToRemuve.Add(ability);
            //     }
            // }
            //
            // foreach (var ability in abilitesToRemuve)
            // {
            //     InteractionAbilites.Remove(ability);
            // }
        }

        public void EmployAbility()
        {
            Employ(InteractionAbilites);
            Employ(ActiveAbilites);

            void Employ (List<IAbility> abilities)
            {
                List<IAbility> toRemove = new List<IAbility>();
                
                abilities.RemoveAll(ability => ability is DurationHandler item && item.AbilityFinished);
                
                foreach (var ability in abilities.OfType<DurationHandler>())
                    ability.Employ();
            }
        }
        

        public void LastColculation()
        {
            foreach (var ability in InteractionAbilites)
            {
                if (ability is ILongTime_Interaction interaction)
                {
                    Health += interaction.Interaction;
                }
            }
        }
    }

    public class UnitStats : IUnitData
    {
        public int ID { get; private set; }
        public int Health { get; set; }
        
        public UnitStats(int id, int health)
        {
            ID = id;
            Health = health;
        }
    }
    
    
#region Server_ClientMasage

    internal interface IEncode
    {
        byte[] Encode();
        void Decode(byte[] data);
    }

    public class InitialClientData : IEncode
    {
        public int ID { get; set; }
        public int Health { get; set; }
        
        public InitialClientData(int id = 0, int health = 0)
        {
            ID = id;
            Health = health;
        }

        public byte[] Encode()
        {
            byte type = (byte)Clien_ServerMassage.InitialClientData;
            byte[] byte_ID = BitConverter.GetBytes(this.ID);
            byte[] byte_Health = BitConverter.GetBytes(this.Health);
            
            byte[] result = new byte[1 + byte_ID.Length + byte_Health.Length];
            
            result[0] = type;
            Buffer.BlockCopy(byte_ID, 0, result, 1, byte_ID.Length);
            Buffer.BlockCopy(byte_Health, 0, result, byte_ID.Length + 1, byte_Health.Length);
            
            return result;
        }

        public void Decode(byte[] data)
        {
            ID = BitConverter.ToInt32(data, 1);
            Health = BitConverter.ToInt32(data, 5);
        }
        
    }
    public class UseAbility : IEncode
    {
        public int Player_ID { get; set; }
        public int Enamy_ID { get; set; }
        public byte Abality { get; set; }

        public UseAbility(int player_id = 0, int enamy_id = 0, byte abality = 0)
        {
            Player_ID = player_id;
            Enamy_ID = enamy_id;
            Abality = abality;
        }

        public byte[] Encode()
        {
            byte type = (byte)Clien_ServerMassage.UseAbility;
            
            byte[] byte_playerID = BitConverter.GetBytes(Player_ID);
            byte[] byte_enamyID = BitConverter.GetBytes(Enamy_ID);
            byte byte_Ability = Abality;
            
            byte[] result = new byte[byte_playerID.Length + byte_enamyID.Length + 2];
            
            result[0] = type;
            Buffer.BlockCopy(byte_playerID, 0, result, 1, byte_playerID.Length);
            Buffer.BlockCopy(byte_enamyID, 0, result, byte_playerID.Length + 1, byte_enamyID.Length);
            result[byte_playerID.Length + byte_enamyID.Length + 1] = byte_Ability;
            
            return result;
        }

        public void Decode(byte[] data)
        {
            Player_ID = BitConverter.ToInt32(data, 1);
            Enamy_ID = BitConverter.ToInt32(data, 5);
            Abality = data[9];
        }
    }
    public class ClientHealthAmount : IEncode
    {
        public int Health { get; set; }
    
        public ClientHealthAmount(int helath = 0)
        {
            Health = helath;
        }

        public byte[] Encode()
        {
            byte type = (byte)Clien_ServerMassage.ClientHelath;
            byte[] byte_health = BitConverter.GetBytes(Health);
            byte[] result = new byte[byte_health.Length +1];
            
            result[0] = type;
            Buffer.BlockCopy(byte_health, 0, result, 1, byte_health.Length);
            
            return result;
        }
        
        public void Decode(byte[] data)
        {
            Health = BitConverter.ToInt32(data, 1);
        }
    }
    
    public class ClietnAbilityAmount : IEncode
    {
        // inedx 0 - ID, index 1 - Reloading count
        public List<byte[]> Ability = new List<byte[]>();

        public ClietnAbilityAmount(List<byte[]> abilities = null)
        {
            Ability = abilities ?? new List<byte[]>();
        }
        
        public byte[] Encode()
        {
            byte type = (byte)Clien_ServerMassage.ClientAbility;

            if (Ability == null || Ability.Count == 0)
            {
                return new[] { type };
            }
            
            byte[] result = new byte[Ability.Count*2 + 1];
            result[0] = type;
            
            for (int i = 0; i < Ability.Count; i++)
            {
                result[i*2+1] = Ability[i][0];  
                result[i*2+2] = Ability[i][1];
            }
            
            
            
            return result;
        }

        public void Decode(byte[] data)
        {
            if ( data == null || (data.Length - 1) % 2 != 0)
            {
                Debug.LogWarning($"Data Lenght: {data?.Length}, Invalid data length for decoding.");
                return;
            }
            
            Ability.Clear();
            
            for (int i = 1; i < data.Length; i += 2)
            {
                byte id = data[i];
                byte reloading = data[i + 1];
                Ability.Add(new byte[] { id, reloading });
            }
        }
    }
    
    public class CanMakeChosen : IEncode
    {
        public int MyID { get; set; }
        public byte Abality{ get; set; }

        public CanMakeChosen(int id = 0, byte abality = 0)
        {
            MyID = id;
            Abality = abality;
        }

        public byte[] Encode()
        {
            byte type = (byte)Clien_ServerMassage.CanMakeChosenServer;
            byte[] byte_myID = BitConverter.GetBytes(MyID);
            byte byte_Ability = Abality;
            
            byte[] result = new byte[byte_myID.Length + 2];
            
            result[0] = type;
            Buffer.BlockCopy(byte_myID, 0, result, 1, byte_myID.Length);
            result[byte_myID.Length + 1] = byte_Ability;
            return result;
        }

        public void Decode(byte[] data)
        {
            MyID = BitConverter.ToInt32(data, 1);
            Abality = data[5];
        }
    }

    public class ClientTurn : IEncode
    {
        public bool MyTurn { get; set; }

        public ClientTurn(bool turn = false)
        {
            MyTurn = turn;
        }
        
        public byte[] Encode()
        {
            byte type = (byte)Clien_ServerMassage.MyTurn;
            byte myTurn = (byte)(MyTurn ? 1 : 0);
            return new byte[] { type, myTurn }; 
        }

        public void Decode(byte[] data)
        {
            if (data == null || data.Length % 2 != 0)
            {
                Debug.LogWarning($"Turn Data is not Corect: {data.Length}");
            }
            
            MyTurn = data[1] == 1 ? true : false;
        }
    }
    
#endregion
    
    public interface IAbilityUseData 
    {
        byte Ability_ID { get; }
        Image AbilityIcon { get; }
        GameObject CoolDown { get; }
        
        TextMeshProUGUI AbilityReloadingText{ get; set; }
    }    
    [Serializable]
    public class InputAbilityStats : IAbilityUseData
    {
        public byte Ability_ID { get; set; }
        public byte Reloading { get; set; }
        public bool ToContinue { get; set; }
        public bool IsActive { get; set; }
        public GameObject CoolDown { get; set; }
        public Image AbilityIcon { get; set; }
        public TextMeshProUGUI AbilityReloadingText{ get; set; }
        
        public IEnumerator CooldownButton(Button button)
        {
            button.interactable = false;
            CoolDown.SetActive(true);
            int reloading = (int)Reloading;
            
            while (reloading > 0 && IsActive)
            {
                AbilityReloadingText.text = reloading.ToString();
                AbilityIcon.fillAmount = (float)reloading / Reloading;
                ToContinue = false;
                --reloading;
                while (!ToContinue)
                {
                    yield return null;
                }
            }
            button.interactable = true;
            CoolDown.SetActive(false);
            IsActive = false;
        }
    }

    public interface IClientCommand
    {
        void Execute(byte[] data, ServerController server);
    }

    public class UseAbilityCommand : IClientCommand
    {
        public void Execute(byte[] data, ServerController server)
        {
            UseAbility useAbility = new UseAbility();
            useAbility.Decode(data);
            server.AttackEnamy(useAbility);
        }
    }
    
    public enum Ability_Name : byte
    {
        Atack = 1,
        Barrier,
        Regeneration,
        FireBol,
        Cleaning,
    }

    public enum Clien_ServerMassage : byte
    {
        InitialClientData = 1,
        UseAbility,
        ClientHelath,
        ClientAbility,
        MyTurn,
        CanMakeChosenServer,
        CanMakeChosenClient,
        WinorLus,
    }
}