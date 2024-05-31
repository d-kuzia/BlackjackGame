using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public CardScript cardScript;
    public DeckScript deckScript;

    public int handValue = 0;
    private int money = 1000;

    // Array to hold player's hand cards
    public GameObject[] hand;
    public int cardIndex = 0;

    // List to manage aces in hand for adjusting their value
    List<CardScript> aceList = new();

    // Starts the hand with two cards
    public void StartHand()
    {
        GetCard();
        GetCard();
    }

    // Deals a card to the player
    public int GetCard()
    {
        int cardValue = deckScript.DealCard(hand[cardIndex].GetComponent<CardScript>());
        hand[cardIndex].GetComponent<Renderer>().enabled = true;
        handValue += cardValue;

        if (cardValue == 1)
        {
            aceList.Add(hand[cardIndex].GetComponent<CardScript>());
        }
        AceCheck();
        cardIndex++;

        return handValue;
    }

    // Checks and adjusts the value of aces in hand
    public void AceCheck()
    {
        foreach (CardScript ace in aceList)
        {
            if (handValue + 10 < 22 && ace.GetValueOfCard() == 1)
            {
                ace.SetValue(11);
                handValue += 10;
            }
            else if (handValue > 21 && ace.GetValueOfCard() == 11)
            {
                ace.SetValue(1);
                handValue -= 10;
            }
        }
    }

    // Adjusts the player's money
    public void AdjustMoney(int amount)
    {
        money += amount;
    }

    // Returns the player's current money
    public int GetMoney()
    {
        return money;
    }

    // Resets the player's hand for a new round
    public void ResetHand()
    {
        for (int i = 0; i < hand.Length; i++)
        {
            hand[i].GetComponent<CardScript>().ResetCard();
            hand[i].GetComponent<Renderer>().enabled = false;
        }
        cardIndex = 0;
        handValue = 0;
        aceList = new List<CardScript>();
    }

}
