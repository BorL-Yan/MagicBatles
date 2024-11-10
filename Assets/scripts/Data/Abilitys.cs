
using UnityEngine;

namespace MagicBattles{
    
    public interface IAbility
    {
        Ability_Name GetName();
    }

    public interface IDamageable
    {
        int Damage { get; }
    }

    public interface IBlockDemage
    {
        int BlockDemage { get; }
    }

    public interface IRegenerable
    {
        int Regenerat { get; }
    }

    public interface ILongTime_Interaction
    {
        int Interaction { get; }
    }
    
    public abstract class DurationHandler
    {
        public abstract byte Duration { get; protected set; }
        public bool DurationFinished { get; protected set; } = false;
        public abstract byte Reloading { get; protected set; }
        public bool AbilityFinished { get; protected set; } = false;
        public virtual void Employ()
        {
            if(AbilityFinished && DurationFinished) return;
            
            if (!DurationFinished )
            {
                if(Duration > 0) Duration--;
                
                if(Duration == 0) DurationFinished = true;
            }

            if (!AbilityFinished )
            {
                if(Reloading > 0) Reloading--;
                
                if(Reloading == 0) AbilityFinished = true;
            }
        }
    }


    public class Attack : IDamageable, IAbility
    {
        public Ability_Name GetName() => Ability_Name.Atack;
        
        public int Damage { get; } = 8;
    }

    public class Barrier : DurationHandler, IBlockDemage, IAbility
    {
        public Ability_Name GetName() => Ability_Name.Barrier;
        
        public int BlockDemage { get; } = 5;
        public override byte Duration { get; protected set; } = 2;
        public override byte Reloading { get; protected set; } = 5;
        
        public Barrier()
        {
            AbilityFinished = false;
        }
    }

    public class Regeneration : DurationHandler, IAbility, ILongTime_Interaction
    {
        public Ability_Name GetName() => Ability_Name.Regeneration;
       
        public int Interaction { get; } = 2;
        public override byte Duration { get; protected set; } = 5;
        public override byte Reloading { get; protected set; } = 5;
        
        public Regeneration()
        {
            AbilityFinished = false;
        }
    }

    public class FireBol : DurationHandler, IDamageable, IAbility, ILongTime_Interaction
    {
        public Ability_Name GetName() => Ability_Name.FireBol;
        public int Damage { get; } = 5;
        
        public int Interaction { get; } = -1;

        public override byte Duration { get; protected set; } = 5;
        public override byte Reloading { get; protected set; } = 6;
        
        public FireBol()
        {
            AbilityFinished = false;
        }
    }

    public class Cleaning : DurationHandler, IAbility
    {
        public Ability_Name GetName() => Ability_Name.Cleaning;
        public override byte Duration { get; protected set; } = 0;
        public override byte Reloading { get; protected set; } = 5;
        public Cleaning()
        {
            AbilityFinished = false;
        }
        
    }
}