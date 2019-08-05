using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour {

    public GameObject backGround;

    public GameObject HealthEncapsulation;
    public GameObject HealthChunk;
    public GameObject HealthValueEnclosment;
    public Sprite Health;
    public Sprite NoHealth;

    public TextMesh textMesh;

    public float Width = 26;
    public float Offset = 6.46f;

    public GameObject AttackSymbol;
    public TextMesh AttackValue;
    public TextMesh ModifierValue;

    public GameObject HealSymbol;
    public TextMesh HealValue;

    public GameObject ArmorSymbol;
    public TextMesh ArmorValue;
    public GameObject ArmorEncapsulation;
    public GameObject ArmorChunk;

    public GameObject HandAmountEncapsulator;
    public TextMesh HandAmount;

    public Sprite StrengthSprite;
    public Sprite AgilitySprite;
    public Sprite RangeSprite;
    public Sprite ArmorSprite;

    public GameObject Buff1;
    public SpriteRenderer Buff1Sprite;
    public GameObject Buff2;
    public SpriteRenderer Buff2Sprite;
    public GameObject Buff3;
    public SpriteRenderer Buff3Sprite;
    public GameObject Buff4;
    public SpriteRenderer Buff4Sprite;

    float AttackSymbolXStart;
    float AttackValueXStart;
    float HealSymbolXStart;
    float HealValueXStart;

    Camera cam;

    int MaxHealth;
    int CurrentHealth;
    int CurrentShield = 0;
    float sFactor;


    public List<GameObject> CurrentArmorPieces = new List<GameObject>();
    public List<GameObject> CurrentHealthPieces = new List<GameObject>();

    public void AddBuff(BuffType buff)
    {
        if (!Buff1.activeSelf)
        {
            setBuff(Buff1, Buff1Sprite, buff);
        }
        else if (!Buff2.activeSelf)
        {
            setBuff(Buff2, Buff2Sprite, buff);
        }
        else if (!Buff3.activeSelf)
        {
            setBuff(Buff3, Buff3Sprite, buff);
        }
        else if (!Buff4.activeSelf)
        {
            setBuff(Buff4, Buff4Sprite, buff);
        }
    }

    void setBuff(GameObject obj, SpriteRenderer SpriteRend, BuffType buffType)
    {
        obj.SetActive(true);
        switch (buffType)
        {
            case BuffType.Strength:
                SpriteRend.sprite = StrengthSprite;
                break;
            case BuffType.Agility:
                SpriteRend.sprite = AgilitySprite;
                break;
            case BuffType.Dexterity:
                SpriteRend.sprite = RangeSprite;
                break;
            case BuffType.Armor:
                SpriteRend.sprite = ArmorSprite;
                break;
        }
    }

    public void RemoveBuff(BuffType buffType)
    {
        switch (buffType)
        {
            case BuffType.Strength:
                removeBuff(StrengthSprite);
                break;
            case BuffType.Agility:
                removeBuff(AgilitySprite);
                break;
            case BuffType.Dexterity:
                removeBuff(RangeSprite);
                break;
            case BuffType.Armor:
                removeBuff(ArmorSprite);
                break;
        }
    }

    void removeBuff(Sprite sprite)
    {
        if (Buff1.activeSelf && Buff1Sprite.sprite == sprite) { Buff1.SetActive(false); }
        else if (Buff2.activeSelf && Buff2Sprite.sprite == sprite) { Buff2.SetActive(false); }
        else if (Buff3.activeSelf && Buff3Sprite.sprite == sprite) { Buff3.SetActive(false); }
        else if (Buff4.activeSelf && Buff4Sprite.sprite == sprite) { Buff4.SetActive(false); }
    }

    public void CreateHandSize(int handSize)
    {
        HandAmountEncapsulator.SetActive(true);
        HandAmount.text = handSize.ToString();
    }

    public void LoseCardInHand()
    {
        HandAmount.text = (int.Parse(HandAmount.text) - 1).ToString();
    }

    public void ResetHandSize(int handSize)
    {
        HandAmount.text = handSize.ToString();
    }

    public void CreateHealthBar(int maxHealth)
    {
        backGround.SetActive(true);
        HealthValueEnclosment.SetActive(true);
        MaxHealth = maxHealth;
        CurrentHealth = MaxHealth;
        textMesh.gameObject.SetActive(true);
        textMesh.text = maxHealth.ToString();
        sFactor = 5f / (float)maxHealth;
        for (int i = 0; i < maxHealth; i++)
        {
            CreateHealthPiece(i);
        }
    }

    void CreateHealthPiece(int index)
    {
        GameObject healthChunk = Instantiate(HealthChunk, HealthEncapsulation.transform);
        healthChunk.transform.localPosition = new Vector3((-Width / 2) + Offset * sFactor * index, 0, 0);
        healthChunk.transform.localScale = new Vector3(healthChunk.transform.localScale.x * sFactor, healthChunk.transform.localScale.y, healthChunk.transform.localScale.z);
        CurrentHealthPieces.Add(healthChunk);
    }

    public void RemoveShield(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            int index = CurrentShield - 1;
            if (index < 0) { break; }
            GameObject ArmorPiece = CurrentArmorPieces[index];
            Destroy(ArmorPiece);
            CurrentArmorPieces.Remove(ArmorPiece);
            CurrentShield--;
            ArmorValue.text = CurrentShield.ToString();
        }
        if (CurrentShield == 0)
        {
            ArmorSymbol.gameObject.SetActive(false);
            ArmorValue.gameObject.SetActive(false);
        }
    }

    public void AddShield(int shieldAmount)
    {

        IEnumerator AddArmorCoroutine = AddingArmor(shieldAmount);
        StartCoroutine(AddArmorCoroutine);
    }

    IEnumerator AddingArmor(int shieldAmount)
    {
        if (CurrentShield == 0)
        {
            ArmorSymbol.gameObject.SetActive(true);
            ArmorValue.gameObject.SetActive(true);
        }

        ArmorValue.text = (CurrentShield + shieldAmount).ToString();

        yield return new WaitForSeconds(.2f);
        for (int i = 0; i < shieldAmount; i++)
        {
            CreateArmorPiece(CurrentShield + i);
            yield return new WaitForSeconds(.2f);
        }
        CurrentShield += shieldAmount;
        GetComponentInParent<Character>().FinishedShielding();
    }

    void CreateArmorPiece(int index)
    {
        GameObject armorChunk = Instantiate(ArmorChunk, ArmorEncapsulation.transform);
        armorChunk.transform.localPosition = new Vector3((-Width / 2) + Offset * sFactor * index, 0, 0);
        armorChunk.transform.localScale = new Vector3(armorChunk.transform.localScale.x * sFactor, armorChunk.transform.localScale.y, armorChunk.transform.localScale.z);
        CurrentArmorPieces.Add(armorChunk);
    }

    public void DummyHealHealth()
    {
        AddHealth(3);
    }

    public void DummyLostHealth()
    {
       // IEnumerator LoseHealthCoroutine = LosingHealth(3, "+1", 4);
        //StartCoroutine(LoseHealthCoroutine);
    }

    public void AddHealth(int amount)
    {
        IEnumerator AddHealthCoroutine = AddingHealth(amount);
        StartCoroutine(AddHealthCoroutine);
    }

    IEnumerator AddingHealth(int healthAmount)
    {
        HealSymbol.SetActive(true);
        HealValue.gameObject.SetActive(true);
        HealValue.text = healthAmount.ToString();
        yield return (1f);
        float EndPosition = HealValue.transform.localPosition.x + 2.3f;
        while (HealValue.gameObject.transform.localPosition.x < EndPosition)
        {
            HealSymbol.transform.localPosition = new Vector3(HealSymbol.transform.localPosition.x + Time.deltaTime * 2f, HealSymbol.transform.localPosition.y, HealSymbol.transform.localPosition.z);
            HealValue.gameObject.transform.localPosition = new Vector3(HealValue.gameObject.transform.localPosition.x + Time.deltaTime * 2f, HealValue.gameObject.transform.localPosition.y, HealValue.gameObject.transform.localPosition.z);
            yield return new WaitForEndOfFrame();
        }

        for (int i = 0; i < healthAmount; i++)
        {
            int index = CurrentHealth;
            if (index > MaxHealth - 1) { break; }
            GameObject healthPiece = CurrentHealthPieces[index];
            healthPiece.GetComponent<SpriteRenderer>().sprite = Health;
            CurrentHealth++;
            //CreateHealthPiece(index);
            textMesh.text = (int.Parse(textMesh.text) + 1).ToString();
            yield return new WaitForSeconds(.2f);
        }
        HealSymbol.SetActive(false);
        HealValue.gameObject.SetActive(false);
        resetPositions();
        GetComponentInParent<Character>().FinishedHealing();
    }

    public void LoseCalculateDamage(int attack, string modifier, int totalHealthLoss)
    {
        IEnumerator CalculateDamagehCoroutine = CalculateDamage(attack, modifier, totalHealthLoss);
        StartCoroutine(CalculateDamagehCoroutine);
    }

    IEnumerator CalculateDamage(int attack, string modifier, int totalDamageIncomming)
    {
        AttackSymbol.SetActive(true);
        AttackValue.gameObject.SetActive(true);
        AttackValue.text = attack.ToString();

        yield return new WaitForSeconds(1f);
        ModifierValue.gameObject.SetActive(true);
        ModifierValue.text = modifier;
        if (modifier[0] == "+"[0]) { ModifierValue.color = Color.green; }
        else if (modifier[0] == "-"[0]) { ModifierValue.color = Color.red; }
        else if (modifier == "x0") { ModifierValue.color = Color.black; }
        else if (modifier == "x2") { ModifierValue.color = Color.yellow; }
        yield return new WaitForSeconds(.5f);
        float hitModifierPoint = ModifierValue.gameObject.transform.localPosition.x;
        while (AttackValue.gameObject.transform.localPosition.x < hitModifierPoint)
        {
            AttackSymbol.transform.localPosition = new Vector3(AttackSymbol.transform.localPosition.x + Time.deltaTime * 2f, AttackSymbol.transform.localPosition.y, AttackSymbol.transform.localPosition.z);
            AttackValue.gameObject.transform.localPosition = new Vector3(AttackValue.gameObject.transform.localPosition.x + Time.deltaTime * 2f, AttackValue.gameObject.transform.localPosition.y, AttackValue.gameObject.transform.localPosition.z);
            yield return new WaitForEndOfFrame();
        }
        ModifierValue.gameObject.SetActive(false);
        AttackValue.text = totalDamageIncomming.ToString();
        while (AttackValue.gameObject.transform.localPosition.x < hitModifierPoint + 1f)
        {
            AttackSymbol.transform.localPosition = new Vector3(AttackSymbol.transform.localPosition.x + Time.deltaTime * 2f, AttackSymbol.transform.localPosition.y, AttackSymbol.transform.localPosition.z);
            AttackValue.gameObject.transform.localPosition = new Vector3(AttackValue.gameObject.transform.localPosition.x + Time.deltaTime * 2f, AttackValue.gameObject.transform.localPosition.y, AttackValue.gameObject.transform.localPosition.z);
            yield return new WaitForEndOfFrame();
        }
        GetComponentInParent<Character>().LetAttackerAttack();
    }

    public void LoseHealth(int totalHealthLoss)
    {
        IEnumerator LoseHealthCoroutine = LosingHealth(totalHealthLoss);
        StartCoroutine(LoseHealthCoroutine);
    }

    IEnumerator LosingHealth(int totalDamageIncomming)
    {
        yield return new WaitForSeconds(.2f);
        int totalHealthLoss = totalDamageIncomming - CurrentShield;
        for (int i = 0; i < totalHealthLoss; i++)
        {
            int index = CurrentHealth - 1;
            if (index < 0) { break; }
            GameObject healthPiece = CurrentHealthPieces[index];
            healthPiece.GetComponent<SpriteRenderer>().sprite = NoHealth;
            CurrentHealth--;
            //CurrentHealthPieces.Remove(healthPiece);
            textMesh.text = (int.Parse(textMesh.text) - 1).ToString();
            yield return new WaitForSeconds(.2f);
        }
        AttackSymbol.SetActive(false);
        AttackValue.gameObject.SetActive(false);
        resetPositions();
        GetComponentInParent<Character>().finishedTakingDamage();
        yield return null;
    }

    void resetPositions()
    {
        AttackSymbol.transform.localPosition = new Vector3(AttackSymbolXStart, AttackSymbol.transform.localPosition.y, AttackSymbol.transform.localPosition.z);
        AttackValue.transform.localPosition = new Vector3(AttackValueXStart, AttackValue.transform.localPosition.y, AttackValue.transform.localPosition.z);
        HealSymbol.transform.localPosition = new Vector3(HealSymbolXStart, HealSymbol.transform.localPosition.y, HealSymbol.transform.localPosition.z);
        HealValue.transform.localPosition = new Vector3(HealValueXStart, HealValue.transform.localPosition.y, HealValue.transform.localPosition.z);

    }

	// Use this for initialization
	void Start () {
        cam = FindObjectOfType<Camera>();
        AttackSymbolXStart = AttackSymbol.transform.localPosition.x;
        AttackValueXStart = AttackValue.transform.localPosition.x;
        HealSymbolXStart = HealSymbol.transform.localPosition.x;
        HealValueXStart = HealValue.transform.localPosition.x;
    }
	
	// Update is called once per frame
	void Update () {
        transform.rotation = Quaternion.Euler(90 - cam.transform.localRotation.eulerAngles.x, cam.transform.localRotation.eulerAngles.y, 0);
	}
}
