using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatPlayerHand : MonoBehaviour {

    public CombatPlayerCard SelectedPlayerCard;
    public GameObject Hand;
    public Button ShortRestButton;

    private PlayerController playerController;

    public List<CombatPlayerCardButton> myCardsInHand = new List<CombatPlayerCardButton>();

    public CombatPlayerCardButton selectedCardLinkedButton;
    public CombatPlayerCard getSelectedCard()
    {
        return SelectedPlayerCard;
    }

    public int GetTotalCardsInHand()
    {
        return myCardsInHand.Count;
    }


    public void SelectCard(CombatPlayerCard card)
    {
        if (card == null) { Debug.LogWarning("Null card was chosen make sure button is wired properly"); }
        SelectedPlayerCard = card;
        CombatPlayerCardButton[] cardButtons = GetComponentsInChildren<CombatPlayerCardButton>();
        foreach (CombatPlayerCardButton button in cardButtons)
        {
            if (button.getMyCard() != SelectedPlayerCard) { button.Unhighlight(); }
            else { selectedCardLinkedButton = button; }
        }
        ShowPotential(SelectedPlayerCard);
        playerController.SelectCard();
    }

    public void ShowPotential(CombatPlayerCard card)
    {
        Character myCharacter = playerController.SelectPlayerCharacter;
        Action[] actions = card.CardAbility.Actions;
        FindObjectOfType<MyActionBoard>().showActions(actions, myCharacter);
    }

    public void HidePotential()
    {
        FindObjectOfType<MyActionBoard>().hideActions();
    }

    public void HideHand()
    {
        Hand.SetActive(false);
        HidePotential();
        FindObjectOfType<MyActionBoard>().HidePanel();
    }

    public void ShowHand()
    {
        Hand.SetActive(true);
        FindObjectOfType<MyActionBoard>().ShowPanel();
    }

    public void discardSelectedCard()
    {
        selectedCardLinkedButton.Unhighlight();
        if (!selectedCardLinkedButton.basicAttack) {
            if (SelectedPlayerCard.LostAbilityUsed) {
                selectedCardLinkedButton.LoseCard();
            }
            else
            {
                ShortRestButton.interactable = true;
                selectedCardLinkedButton.DiscardCard();
            }
            myCardsInHand.Remove(selectedCardLinkedButton);
        }
        SelectedPlayerCard.transform.SetParent(selectedCardLinkedButton.transform);
        SelectedPlayerCard = null;
    }

    public void ShortRest()
    {
        ShortRestButton.interactable = false;
        CombatPlayerCardButton[] cardButtons = GetComponentsInChildren<CombatPlayerCardButton>();
        List<CombatPlayerCardButton> discardedCards = new List<CombatPlayerCardButton>();
        foreach (CombatPlayerCardButton cardButton in cardButtons)
        {
            if (cardButton.Discarded) { discardedCards.Add(cardButton); }
        }

        int randomCardIndex = Random.Range(0, discardedCards.Count);
        CombatPlayerCardButton cardToLose = discardedCards[randomCardIndex];
        cardToLose.LoseCard();
        discardedCards.Remove(cardToLose);
        foreach (CombatPlayerCardButton cardButton in discardedCards)
        {
            cardButton.putBackInHand();
            myCardsInHand.Add(cardButton);
        }
        playerController.SetHandSize(myCardsInHand.Count - 1);
    }

    //Change this to better code for this functunallity
    public bool LoseRandomCard()
    {
        List<CombatPlayerCardButton> cardsInHand = new List<CombatPlayerCardButton>();
        foreach (CombatPlayerCardButton cardButton in myCardsInHand)
        {
            if (!cardButton.Discarded && !cardButton.Lost  && !cardButton.basicAttack && selectedCardLinkedButton != cardButton) { cardsInHand.Add(cardButton); }
        }
        if (cardsInHand.Count <= 0) { return false; }

        int randomCardIndex = Random.Range(0, cardsInHand.Count);
        CombatPlayerCardButton cardToLose = cardsInHand[randomCardIndex];
        cardToLose.LoseCard();
        myCardsInHand.Remove(cardToLose);
        return true;
    }

    public void showSelectedCard()
    {
        FindObjectOfType<MyActionBoard>().ShowPanel();
        ShowPotential(SelectedPlayerCard);
        Vector3 currentScale = SelectedPlayerCard.transform.localScale;
        PlayArea showArea = FindObjectOfType<PlayArea>();
        SelectedPlayerCard.transform.SetParent(showArea.transform);
        SelectedPlayerCard.transform.localPosition = Vector3.zero;
        SelectedPlayerCard.transform.localScale = currentScale;
        SelectedPlayerCard.gameObject.SetActive(true);
    }

    public void HideSelectedCard()
    {
        HidePotential();
        FindObjectOfType<MyActionBoard>().HidePanel();
        SelectedPlayerCard.CardAbility.UnHighlightAbility();
        SelectedPlayerCard.gameObject.SetActive(false);
    }

    // Use this for initialization
    void Start () {
        playerController = FindObjectOfType<PlayerController>();
      //  myCardsInHand.AddRange(GetComponentsInChildren<CombatPlayerCardButton>());
   //     playerController.SetUpCharacterHand(myCardsInHand.Count - 1);
    }

    public int CheckCardsInHand()
    {
        myCardsInHand.AddRange(GetComponentsInChildren<CombatPlayerCardButton>());
        return (myCardsInHand.Count - 1);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
