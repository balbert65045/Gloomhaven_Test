using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardBuilder : MonoBehaviour {

    public Sprite AttackSprite;
    public Sprite MoveSprite;
    public Sprite RangeSprite;
    public Sprite HealSprite;
    public Sprite ArmorSprite;
    public Sprite SneakSprite;
    public Sprite DurationSprite;
    public Sprite ScoutSprite;


    public GameObject CombatCardPrefab;
    public GameObject OutOfCombatCardPrefab;
    public GameObject AbilityLinePrefab;

    public string CardName;
    public CardType TypeToBuild;
    public PlayerCharacterType characterForCard;
    public int Initiative;

    public bool Lost = false;
    public Action[] Actions;

    int StartingPos = 0;

    public void BuildCard()
    {
        switch (TypeToBuild)
        {
            case CardType.Combat:
                BuildCombatCard();
                break;
            case CardType.OutOfCombat:
                BuildOutOfCombatCard();
                break;
        }
    }


    void BuildCombatCard()
    {
        GameObject Card = Instantiate(CombatCardPrefab, this.transform);
        Card.name = CardName;
        CombatPlayerCard CombatCard = Card.GetComponent<CombatPlayerCard>();
        CombatCard.myType = characterForCard;
        CombatCard.NameText.text = CardName;
        CombatCard.Initiative = Initiative;
        CombatCard.InitiativeText.text = Initiative.ToString();

        CardAbility Ability = Card.GetComponentInChildren<CardAbility>();
        Ability.LostAbility = Lost;
        Ability.Actions = Actions;

        StartingPos = (CombatCard.AbilityLinePositions.Length - Actions.Length) / 2;
        for (int i = 0; i < Actions.Length; i++)
        {
            if (StartingPos >= CombatCard.AbilityLinePositions.Length) { return; }
            BuildActionLine(i, CombatCard);
            StartingPos++;
        }

    }

    void BuildActionLine(int index, CombatPlayerCard CombatCard)
    {
        GameObject actionLineObj = Instantiate(AbilityLinePrefab, CombatCard.AbilityLinePositions[StartingPos].transform);
        actionLineObj.transform.localPosition = Vector3.zero;
        ActionLine actionLine = actionLineObj.GetComponent<ActionLine>();
        switch (Actions[index].thisActionType)
        {
            case ActionType.Attack:
                actionLine.MyActionType = BuffType.Strength;
                actionLine.AbilityType.text = "Attack";
                actionLine.AbilityImage.sprite = AttackSprite;
                actionLine.AbilityAmount.text = Actions[index].thisAOE.Damage.ToString();
                SetRange(actionLine, Actions[index].Range, 1);
                break;
            case ActionType.Movement:
                actionLine.MyActionType = BuffType.Agility;
                actionLine.AbilityType.text = "Move";
                actionLine.AbilityImage.sprite = MoveSprite;
                actionLine.AbilityAmount.text = Actions[index].Range.ToString();
                break;
        }
    }

    void SetRange(ActionLine line, int Range, int threshhold)
    {
        if (Range > threshhold)
        {
            line.RangeAbilityType.gameObject.SetActive(true);
            line.RangeAbilityImage.gameObject.SetActive(true);
            line.RangeAbilityAmount.gameObject.SetActive(true);
            line.RangeAbilityAmount.text = Range.ToString();
        }
    }

    void BuildOutOfCombatCard()
    {

    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
