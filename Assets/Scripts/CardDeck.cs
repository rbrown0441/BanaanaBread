using Assets.Scripts;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CardDeck : MonoBehaviour
{
    public Deck Deck { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        Deck.AddCard(new AttackCard()
        {
            Name = "Sword"
        });

        Deck.AddCard(new SupportCard()
        {
            Name = "Shield"
        });

        Deck.AddCard(new ConsumableCard()
        {
            Name = "Nethercoin"
        });

        //foreach card in deck instantiate gameobject
    }

    // Update is called once per frame
    void Update()
    {
        Deck.RemoveExpiredCards();
    }
}

public class Deck
{
    public List<ICard> Cards = new List<ICard>();

    public void AddCard(ICard card) => Cards.Add(card);
    public void RemoveCard(ICard card) => Cards.Remove(card);
    public void RemoveExpiredCards() => Cards.RemoveAll(x => x.Uses < 1);


    public List<AttackCard> GetAttackCards() => Cards.OfType<AttackCard>().ToList();
    public List<SupportCard> GetSupportCards() => Cards.OfType<SupportCard>().ToList();
    public List<ConsumableCard> GetConsumableCards() => Cards.OfType<ConsumableCard>().ToList();

}