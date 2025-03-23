using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCard : MonoBehaviour, ICard
{
    public string Name { get ; set; }
    public CardType Type { get; set; }
    public string Description { get; set; }
    public double DropRate { get; set; }
    public int Cost { get; set; }
    public bool Active { get; set; }
    public TimeSpan CooldownTime { get; set; }
    public DateTime LastUseTime { get; set; }
    public int RemainingCooldown { get; set; }
    public int Uses { get; set; }


    //number of times weapon can be used before it goes on cooldown
    public int Durability { get; set; }
    public int AttackValue { get; set; }

    public void Use()
    {
        Durability -= 1;
        if (Durability < 1) Active = false;
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}