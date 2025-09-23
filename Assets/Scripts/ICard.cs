using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public interface ICard
    {
        [SerializeField]
        public string Name { get; set; }
        public CardType Type { get; set; }
        public string Description { get; set; }
        //change of card dropping from enemy
        public double DropRate { get; set; }
        //the deck has a total cost limit, card can be different costs
        //cannot add a card if cost higher than remaining total deck cost
        public int Cost { get; set; }
        //when card is out of uses or on cooldown, set Active = false
        public bool Active { get; set; }
        public TimeSpan CooldownTime { get; set; }
        public DateTime LastUseTime { get; set; }
        public int RemainingCooldown { get; set; }
        //number of times card can be used before it is removed from deck
        public int Uses { get; set; }

        public bool CanUse()
        {
            return Active ? DateTime.Now >= LastUseTime + CooldownTime : false;
        }

        public void UseCard()
        {
            if (CanUse())
            {
                //Use();  // Execute the specific card's play logic
                LastUseTime = DateTime.Now;  // Update last played time
            }
            else
            {
                RemainingCooldown = ((LastUseTime + CooldownTime) - DateTime.Now).Seconds;
            }
        }
        public void Use();
    }

    public enum CardType
    {
        Attack,
        Support,
        Consumable
    }

    public enum SupportType
    {
        Shield,
        TripleJump,
    }

    public enum ResourceType
    {
        Nethercoin,
        HealthFruit,
        MagicCrystal,
        ExtraLife
    }
}

