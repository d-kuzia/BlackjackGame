using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardScript : MonoBehaviour
{
    public int value = 0;

    // Returns the value of the card
    public int GetValueOfCard() => value;

    // Sets the value of the card
    public void SetValue(int newValue) => value = newValue;

    // Sets the sprite of the card
    public void SetSprite(Sprite newSprite) => gameObject.GetComponent<SpriteRenderer>().sprite = newSprite;

    // Resets the card to the back sprite and default value
    public void ResetCard()
    {
        Sprite back = GameObject.Find("Deck").GetComponent<DeckScript>().GetCardBack();
        gameObject.GetComponent<SpriteRenderer>().sprite = back;
        value = 0;
    }

}
