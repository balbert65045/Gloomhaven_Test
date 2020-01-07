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

    void BuildOutOfCombatCard()
    {
        GameObject Card = Instantiate(OutOfCombatCardPrefab, this.transform);
        Card.name = CardName;
        OutOfCombatCard OutOfCombatCard = Card.GetComponent<OutOfCombatCard>();
        OutOfCombatCard.myType = characterForCard;
        OutOfCombatCard.NameText.text = CardName;

        CardAbility Ability = Card.GetComponentInChildren<CardAbility>();
        Ability.Actions = Actions;
        StartingPos = (OutOfCombatCard.AbilityLinePositions.Length - Actions.Length) / 2;
        for (int i = 0; i < Actions.Length; i++)
        {
            if (StartingPos >= OutOfCombatCard.AbilityLinePositions.Length) { return; }
            BuildActionLine(i, OutOfCombatCard);
            StartingPos++;
        }
    }

    void BuildActionLine(int index, Card Card)
    {
        GameObject actionLineObj = Instantiate(AbilityLinePrefab, Card.AbilityLinePositions[StartingPos].transform);
        actionLineObj.transform.localPosition = Vector3.zero;
        ActionLine actionLine = actionLineObj.GetComponent<ActionLine>();
        actionLine.actionIndex = index;
        switch (Actions[index].thisActionType)
        {
        case ActionType.Attack:
            actionLine.MyActionType = BuffType.Strength;
            actionLine.AbilityType.text = "Attack";
            actionLine.AbilityImage.sprite = AttackSprite;
            actionLine.AbilityAmount.text = Actions[index].thisAOE.Damage.ToString();
            SetRange(actionLine, Actions[index].Range, 1);
            SetDuration(actionLine, Actions[index].Duration);
            break;
        case ActionType.Movement:
            actionLine.MyActionType = BuffType.Agility;
            actionLine.AbilityType.text = "Move";
            actionLine.AbilityImage.sprite = MoveSprite;
            actionLine.AbilityAmount.text = Actions[index].Range.ToString();
            SetRange(actionLine, 0, 1);
            SetDuration(actionLine, Actions[index].Duration);
            break;
        case ActionType.Shield:
            actionLine.AbilityType.text = "Shield";
            actionLine.AbilityImage.sprite = ArmorSprite;
            actionLine.AbilityAmount.text = Actions[index].thisAOE.Damage.ToString();
            SetRange(actionLine, Actions[index].Range, 1);
            SetDuration(actionLine, Actions[index].Duration);
            break;
        case ActionType.Heal:
            actionLine.AbilityType.text = "Heal";
            actionLine.AbilityImage.sprite = HealSprite;
            actionLine.AbilityAmount.text = Actions[index].thisAOE.Damage.ToString();
            SetRange(actionLine, Actions[index].Range, 1);
            SetDuration(actionLine, Actions[index].Duration);
            break;
        case ActionType.BuffArmor:
            actionLine.AbilityType.text = "Buff";
            actionLine.AbilityImage.sprite = ArmorSprite;
            actionLine.AbilityAmount.text = Actions[index].thisAOE.Damage.ToString();
            SetRange(actionLine, Actions[index].Range, 1);
            SetDuration(actionLine, Actions[index].Duration);
            break;
        case ActionType.BuffAttack:
            actionLine.AbilityType.text = "Buff";
            actionLine.AbilityImage.sprite = AttackSprite;
            actionLine.AbilityAmount.text = Actions[index].thisAOE.Damage.ToString();
            SetRange(actionLine, Actions[index].Range, 1);
            SetDuration(actionLine, Actions[index].Duration);
            break;
        case ActionType.BuffMove:
            actionLine.AbilityType.text = "Buff";
            actionLine.AbilityImage.sprite = MoveSprite;
            actionLine.AbilityAmount.text = Actions[index].thisAOE.Damage.ToString();
            SetRange(actionLine, Actions[index].Range, 1);
            SetDuration(actionLine, Actions[index].Duration);
            break;
        case ActionType.BuffRange:
            actionLine.AbilityType.text = "Buff";
            actionLine.AbilityImage.sprite = RangeSprite;
            actionLine.AbilityAmount.text = Actions[index].thisAOE.Damage.ToString();
            SetRange(actionLine, Actions[index].Range, 1);
            SetDuration(actionLine, Actions[index].Duration);
            break;
        case ActionType.Stealth:
            actionLine.AbilityType.text = "Stealth";
            actionLine.AbilityImage.sprite = SneakSprite;
            actionLine.AbilityAmount.text = Actions[index].Duration.ToString();
            SetDuration(actionLine, Actions[index].Duration);
            break;
        case ActionType.Scout:
            actionLine.AbilityType.text = "Scout";
            actionLine.AbilityImage.sprite = ScoutSprite;
            actionLine.AbilityAmount.text = Actions[index].Range.ToString();
            SetDuration(actionLine, Actions[index].Duration);
            break;
        }
    }

    void SetDuration (ActionLine line, int Duration)
    {
        if (Duration > 0)
        {
            line.DurationAbilityAmount.gameObject.SetActive(true);
            line.DurationAbilityImage.gameObject.SetActive(true);
            line.DurationAbilityAmount.text = Duration.ToString();
        }
        else
        {
            line.DurationAbilityAmount.gameObject.SetActive(false);
            line.DurationAbilityImage.gameObject.SetActive(false);
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
        else
        {
            line.RangeAbilityType.gameObject.SetActive(false);
            line.RangeAbilityAmount.gameObject.SetActive(false);
            line.RangeAbilityImage.gameObject.SetActive(false);
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
