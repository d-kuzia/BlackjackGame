using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Buttons for game actions
    public Button dealButton;
    public Button hitButton;
    public Button standButton;
    public Button betButton;
    public Button allInButton;
    public Button doubleButton;

    // Player, dealer and bot scripts
    public PlayerScript playerScript;
    public PlayerScript dealerScript;
    public PlayerScript gtoScript;

    // Text fields for displaying scores, bets, and cash information
    public Text scoreText;
    public Text dealerScoreText;
    public Text betsText;
    public Text botBetsText;
    public Text cashText;
    public Text botCashText;
    public Text mainText;
    public Text botMainText;
    public Text botScoreText;

    // Game objects
    public GameObject hideCard;
    public GameObject rulesCanvas;

    // Sound effects
    public AudioSource cardHitSound;
    public AudioSource betSound;

    // Slider and tracks for music
    public Slider musicVolumeSlider;
    public AudioSource[] musicTracks;
    private int currentTrackIndex = 0;

    // Betting pots for player and bot
    private int pot = 0;
    private int botPot = 0;

    // Flags for doubling status
    private bool playerDoubled = false;
    private bool botDoubled = false;

    // Decision matrix for bot's decisions based on hand values
    private readonly int[,] decisionMatrix = new int[17, 10]
    {
        // Сума бота = 4
        {1, 1, 1, 1, 1, 1, 1, 1, 1, 1}, 
        // Сума бота = 5
        {1, 1, 1, 1, 1, 1, 1, 1, 1, 1}, 
        // Сума бота = 6
        {1, 1, 1, 1, 1, 1, 1, 1, 1, 1}, 
        // Сума бота = 7
        {1, 1, 1, 1, 1, 1, 1, 1, 1, 1}, 
        // Сума бота = 8
        {1, 1, 1, 1, 1, 1, 1, 1, 1, 1}, 
        // Сума бота = 9
        {1, 2, 2, 2, 2, 1, 1, 1, 1, 1}, 
        // Сума бота = 10
        {2, 2, 2, 2, 2, 2, 2, 2, 1, 1}, 
        // Сума бота = 11
        {2, 2, 2, 2, 2, 2, 2, 2, 2, 2}, 
        // Сума бота = 12
        {1, 1, 0, 0, 0, 1, 1, 1, 1, 1}, 
        // Сума бота = 13
        {0, 0, 0, 0, 0, 1, 1, 1, 1, 1}, 
        // Сума бота = 14
        {0, 0, 0, 0, 0, 1, 1, 1, 1, 1}, 
        // Сума бота = 15
        {0, 0, 0, 0, 0, 1, 1, 1, 1, 1}, 
        // Сума бота = 16
        {0, 0, 0, 0, 0, 1, 1, 1, 1, 1}, 
        // Сума бота = 17
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        // Сума бота = 18
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, 
        // Сума бота = 19
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, 
        // Сума бота = 20
        {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
    };

    // Initialize the game and set up event listeners
    private void Start()
    {
        dealButton.onClick.AddListener(DealClicked);
        hitButton.onClick.AddListener(() => HitClicked(playerScript));
        standButton.onClick.AddListener(StandClicked);
        betButton.onClick.AddListener(BetClicked);
        allInButton.onClick.AddListener(AllInClicked);
        doubleButton.onClick.AddListener(() => DoubleClicked(playerScript));

        musicVolumeSlider.onValueChanged.AddListener(ChangeMusicVolume);
        musicVolumeSlider.value = musicTracks[currentTrackIndex].volume;
        hitButton.gameObject.SetActive(false);
        standButton.gameObject.SetActive(false);
        betButton.gameObject.SetActive(false);
        allInButton.gameObject.SetActive(false);
        doubleButton.gameObject.SetActive(false);
        rulesCanvas.SetActive(true);

        foreach (AudioSource track in musicTracks)
        {
            track.Stop();
        }

        PlayNextTrack();
    }

    // Update is called once per frame, used to play the next track if the current one has finished
    private void Update()
    {
        if (!musicTracks[currentTrackIndex].isPlaying)
        {
            PlayNextTrack();
        }
    }

    // Called when the Deal button is clicked, starts a new round
    private void DealClicked()
    {
        InitializeRound();

        playerScript.StartHand();
        dealerScript.StartHand();
        gtoScript.StartHand();

        UpdateScoreTexts();

        hideCard.GetComponent<Renderer>().enabled = true;

        ToggleButtons(true);

        SetInitialBets();

        CheckForResetMoney();
    }

    // Initializes the round by resetting hands and hiding UI elements
    private void InitializeRound()
    {
        playerScript.ResetHand();
        dealerScript.ResetHand();
        gtoScript.ResetHand();

        playerDoubled = false;
        botDoubled = false;


        rulesCanvas.SetActive(false);
        mainText.gameObject.SetActive(false);
        botMainText.gameObject.SetActive(false);
        dealerScoreText.gameObject.SetActive(false);

        GameObject.Find("Deck").GetComponent<DeckScript>().Shuffle();
    }

    // Updates the score texts for player, dealer, and bot
    private void UpdateScoreTexts()
    {
        scoreText.text = "Hand: " + playerScript.handValue.ToString();
        dealerScoreText.text = "Dealer's Hand: " + dealerScript.handValue.ToString();
        botScoreText.text = "Bot's Hand: " + gtoScript.handValue.ToString();
    }

    // Toggles the visibility of game control buttons
    private void ToggleButtons(bool isActive)
    {
        dealButton.gameObject.SetActive(!isActive);
        hitButton.gameObject.SetActive(isActive);
        standButton.gameObject.SetActive(isActive);
        betButton.gameObject.SetActive(isActive);
        allInButton.gameObject.SetActive(isActive);
        doubleButton.gameObject.SetActive(isActive);
    }

    // Sets the initial bets for player and bot
    private void SetInitialBets()
    {
        pot = 25;
        betsText.text = "Bet: $" + pot.ToString();
        playerScript.AdjustMoney(-pot);

        int randomMultiplier = Random.Range(1, 9);
        int randomBet = randomMultiplier * 25;
        botPot = randomBet;
        botBetsText.text = "Bet: $" + botPot.ToString();
        gtoScript.AdjustMoney(-botPot);
    }

    // Checks if the player or bot needs to reset their money
    private void CheckForResetMoney()
    {
        if (playerScript.GetMoney() <= 0)
        {
            playerScript.AdjustMoney(1000);
        }
        else if (gtoScript.GetMoney() <= 0)
        {
            gtoScript.AdjustMoney(1000);
        }

        cashText.text = "$" + playerScript.GetMoney().ToString();
        botCashText.text = "$" + gtoScript.GetMoney().ToString();
    }

    // Bot's turn logic based on decision matrix
    private void BotTurn()
    {
        bool needMoreCards = true;

        while (needMoreCards)
        {
            int botSum = gtoScript.handValue;

            if (botSum > 20)
            {
                needMoreCards = false;
                return;
            }

            int dealerCardValue = dealerScript.hand[1].GetComponent<CardScript>().GetValueOfCard();
            int botDecision = decisionMatrix[botSum - 4, dealerCardValue - 2];

            switch (botDecision)
            {
                case 0: // Stand
                    needMoreCards = false;
                    break;
                case 1: // Hit
                    HitClicked(gtoScript);
                    break;
                case 2: // Double
                    DoubleClicked(gtoScript);
                    needMoreCards = false;
                    break;
                default:
                    needMoreCards = false;
                    break;
            }
        }
    }

    // Called when the Stand button is clicked
    private void StandClicked()
    {
        HitDealer();
        BotTurn();

        RoundOver();
    }

    // Called when the Hit button is clicked or Bot hits
    private void HitClicked(PlayerScript player)
    {
        doubleButton.gameObject.SetActive(false);
        betButton.gameObject.SetActive(false);
        allInButton.gameObject.SetActive(false);

        if (player.cardIndex <= 7)
        {
            player.GetCard();
            cardHitSound.Play();

            UpdatePlayerScoreText(player);

            if (player != gtoScript && player.handValue > 20)
            {
                BotTurn();
                RoundOver();
            }
        }
    }

    // Updates the score text for the player or bot
    private void UpdatePlayerScoreText(PlayerScript player)
    {
        if (player == gtoScript)
        {
            botScoreText.text = "Bot's Hand: " + player.handValue.ToString();
        }
        else
        {
            scoreText.text = "Hand: " + player.handValue.ToString();
        }
    }

    // Called when the Double button is clicked
    private void DoubleClicked(PlayerScript player)
    {
        betButton.gameObject.SetActive(false);
        allInButton.gameObject.SetActive(false);

        player.GetCard();

        UpdatePlayerScoreText(player);

        if (player != gtoScript)
        {
            player.AdjustMoney(-pot);
            playerDoubled = true;

            cashText.text = "$" + player.GetMoney().ToString();
            betsText.text = "Bet: $" + (pot * 2).ToString();

            HitDealer();
            BotTurn();

            RoundOver();
        }
        else
        {
            player.AdjustMoney(-botPot);
            botDoubled = true;

            botCashText.text = "$" + player.GetMoney().ToString();
            botBetsText.text = "Bet: $" + (botPot * 2).ToString();
        }
    }

    // Dealer's turn logic to hit cards
    private void HitDealer()
    {
        while (dealerScript.handValue < 16 && dealerScript.cardIndex <= 7)
        {
            dealerScript.GetCard();
            dealerScoreText.text = "Dealer's hand: " + dealerScript.handValue.ToString();
        }
    }

    // Called when the Bet button is clicked
    private void BetClicked()
    {
        int betAmount = 25;

        if (playerScript.GetMoney() < betAmount)
        {
            mainText.text = "You can't bet";
            return;
        }

        betSound.Play();

        playerScript.AdjustMoney(-betAmount);

        cashText.text = "$" + playerScript.GetMoney().ToString();
        pot += betAmount;
        betsText.text = "Bet: $" + pot.ToString();

        if (playerScript.GetMoney() < pot)
        {
            doubleButton.gameObject.SetActive(false);
        }
    }

    // Called when the All-In button is clicked
    private void AllInClicked()
    {
        int allInAmount = playerScript.GetMoney();

        if (allInAmount <= 0)
        {
            mainText.text = "You have lost your money";
            return;
        }

        betSound.Play();

        pot += allInAmount;
        playerScript.AdjustMoney(-allInAmount);

        cashText.text = "$" + playerScript.GetMoney().ToString();
        betsText.text = "Bet: $" + pot.ToString();

        doubleButton.gameObject.SetActive(false);
    }

    // Game over
    // Ends the round and determines the payouts
    private void RoundOver()
    {
        bool playerBust = playerScript.handValue > 21;
        bool dealerBust = dealerScript.handValue > 21;
        bool botBust = gtoScript.handValue > 21;

        int playerPayout = DeterminePayout(playerScript, playerBust, dealerBust, pot, playerDoubled, mainText);
        playerScript.AdjustMoney(playerPayout);

        int botPayout = DeterminePayout(gtoScript, botBust, dealerBust, botPot, botDoubled, botMainText);
        gtoScript.AdjustMoney(botPayout);

        EndRound();
    }

    // Determines the payout for a given player
    private int DeterminePayout(PlayerScript player, bool playerBust, bool dealerBust, int pots, bool doubled, Text text)
    {
        int playerPayout = 0;

        if (playerBust && dealerBust)
        {
            text.text = "All Bust. Bets returned";
            playerPayout = pots;
        }
        else if (playerBust || (!dealerBust && dealerScript.handValue > player.handValue))
        {
            text.text = player == gtoScript ? "Dealer wins against Bot" : "Dealer wins";
        }
        else if (dealerBust || player.handValue > dealerScript.handValue)
        {
            text.text = player == gtoScript ? "Bot wins" : "You win";
            playerPayout = pots * (doubled ? 4 : 2);
        }
        else if (player.handValue == dealerScript.handValue)
        {
            text.text = player == gtoScript ? "Push for Bot. Bets returned" : "Push. Bets returned";
            playerPayout = pots;
        }

        return playerPayout;
    }

    // Ends the round, toggles UI elements, and updates cash texts
    private void EndRound()
    {
        ToggleButtons(false);
        dealButton.gameObject.SetActive(true);
        mainText.gameObject.SetActive(true);
        botMainText.gameObject.SetActive(true);
        dealerScoreText.gameObject.SetActive(true);

        hideCard.GetComponent<Renderer>().enabled = false;
        cashText.text = "$" + playerScript.GetMoney().ToString();
        botCashText.text = "$" + gtoScript.GetMoney().ToString();
    }

    // Changes the volume of all music tracks
    private void ChangeMusicVolume(float value)
    {
        foreach (AudioSource track in musicTracks)
        {
            track.volume = value;
        }
    }

    // Plays the next music track
    private void PlayNextTrack()
    {
        if (currentTrackIndex < musicTracks.Length)
        {
            musicTracks[currentTrackIndex].Stop();
        }

        currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Length;

        musicTracks[currentTrackIndex].Play();
    }

}
