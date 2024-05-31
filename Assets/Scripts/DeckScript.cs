using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckScript : MonoBehaviour
{
    public Sprite[] cardSprites;
    readonly int[] cardValues = new int[53];
    int currentIndex = 0;

    // Initialize the deck
    void Start()
    {
        GetCardValues();
    }

    // Sets card values based on sprite index
    void GetCardValues()
    {
        int num = 0;
        for (int i = 0; i < cardSprites.Length; i++)
        {
            num = i;
            num %= 13;
            if (num > 10 || num == 0)
            {
                num = 10;
            }
            cardValues[i] = num++;
        }
    }

    // Shuffles the deck
    public void Shuffle()
    {
        for (int i = cardSprites.Length - 1; i > 0; i--)
        {
            int j = Random.Range(1, i + 1);

            (cardSprites[j], cardSprites[i]) = (cardSprites[i], cardSprites[j]);
            (cardValues[j], cardValues[i]) = (cardValues[i], cardValues[j]);
        }
        currentIndex = 1;
    }

    // Deals a card from the deck
    public int DealCard(CardScript cardScript)
    {
        cardScript.SetSprite(cardSprites[currentIndex]);
        cardScript.SetValue(cardValues[currentIndex]);
        currentIndex++;

        return cardScript.GetValueOfCard();
    }

    // Returns the back of the card sprite
    public Sprite GetCardBack()
    {
        return cardSprites[0];
    }

}
