using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableCard : MonoBehaviour, ICard
{
    public string Name { get; set; }
    public CardType Type { get; set; }
    public string Description { get; set; }
    public double DropRate { get; set; }
    public int Cost { get; set; }
    public bool Active { get; set; }
    public TimeSpan CooldownTime { get; set; }
    public DateTime LastUseTime { get; set; }
    public int RemainingCooldown { get; set; }
    public int Uses { get; set; }

    //nethercoin, health fruit, magic crystal, etc
    public ResourceType ResourceType { get; set; }
    [SerializeField] public int healAmount;
    public void Use()
    {
        Uses -= 1;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    // To be used on separate health or crystals
    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.CompareTag("Player"))
        {
            CharacterScript player = other.GetComponentInParent<CharacterScript>();
            player.Heal(healAmount);
            Destroy(gameObject);
        }
    }
    
}