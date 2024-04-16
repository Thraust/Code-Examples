using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleSystem : MonoBehaviour
{

    #region Singleton

    public static BattleSystem instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of BattleSystem found");
            return;
        }
        instance = this;
    }

    #endregion

    private State state;
    private enum State
    {
        WaitingForPlayer,
        Busy,
    }

    // Get player and enemy objects
    [SerializeField] private Transform playerTransform = null;
    [SerializeField] private Transform enemyTransform = null;
    private PlayerCharacter pc;
    private EnemyCharacter ec;

    //public Transform pcHealthBarTransform;
    public HealthSystem pcHealthSystem;
    public Slider pcHealthBar;

    //public Transform ecHealthBarTransform;
    private HealthSystem ecHealthSystem;
    public Slider ecHealthBar;
    [SerializeField] private Button fightbtn = null;
    Vector3 enemyPos = new Vector3(0f, -160f, 0f);

    public GameObject defeatScreen;
    public GameObject victoryScreen;
    public GameObject gameCompleteScreen;
    public GameObject confirmationScreen;
    public GameObject exitScreen;
    public Text levelText;
    public Text enemyName;
    public int gameLevel = 1;
    public int dungeonLevel = 1;
    public int subLevel = 1;
    private int[] classLevel;
    private int adCount = 0;
    private int incomingDmg = 0;
    private bool playerAttackedFirst = false;
    private float width;

    [SerializeField] private PopUpText damagePopUp = null;

    // Start is called before the first frame update
    void Start()
    {
        // Setup Player Character health system and health bar
        pc = playerTransform.GetComponent<PlayerCharacter>();
        pcHealthSystem = gameObject.AddComponent<HealthSystem>().HealthSystemInstance(Mathf.RoundToInt(pc.MaxHP()));
        //pcHealthSystem.HealthSystemInstance(Mathf.RoundToInt(pc.MaxHP()));

        //pcHealthBar = Instantiate(pcHealthBar, new Vector3(240, 750, 60), Quaternion.identity);
        pcHealthBar = Instantiate(pcHealthBar, Vector3.zero, Quaternion.identity);

        pcHealthBar.transform.SetParent(FindObjectOfType<Canvas>().transform);
        pcHealthBar.transform.SetSiblingIndex(0);

        pcHealthBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f,-50f);
        pcHealthBar.GetComponent<RectTransform>().sizeDelta = new Vector2(160,70);

        // Setup Enemy Character health system and health bar
        ec = enemyTransform.GetComponent<EnemyCharacter>();
        ecHealthSystem = gameObject.AddComponent<HealthSystem>().HealthSystemInstance(Mathf.RoundToInt(ec.MaxHP()));

        ecHealthBar = Instantiate(ecHealthBar, ec.transform.position + enemyPos, Quaternion.identity);
        ecHealthBar.transform.SetParent(FindObjectOfType<Canvas>().transform);
        ecHealthBar.transform.SetSiblingIndex(1);

        
        ecHealthBar.GetComponent<RectTransform>().anchorMin = new Vector2(0f,.5f);
        ecHealthBar.GetComponent<RectTransform>().anchorMax = new Vector2(0f,.5f);

        //ecHealthBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f,-500f);
        //ecHealthBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(width/2,-85f);

        enemyName.text = ec.name;
        adCount = PlayerPrefs.GetInt("AdCount");

        CombatMusic.instance.StartCombat();

        // Get Class Records Data
        if (ES3.KeyExists("ClassLevel"))
        {
            classLevel = ES3.Load<int[]>("ClassLevel");
        }
        else
        {
            classLevel = new int[] { 0, 0, 0, 0, 0 };
            ES3.Save<int[]>("ClassLevel", classLevel);
        }
    }

    void Update()
    {
        // Because I'm bad at setting 
        width = FindAnyObjectByType<Canvas>().GetComponent<RectTransform>().sizeDelta.x;
        ecHealthBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(width/2,-85f);
        //Debug.Log(FindAnyObjectByType<Canvas>().GetComponent<RectTransform>().sizeDelta.x);
    }

    public void FightButton()
    {
        // disable fight button for the duration of the fight
        fightbtn.interactable = false;

        if (pc.GetComponentInChildren<EquipSystem>().currentEquipment[0].name == "BerserkersAxe")
        {
            while (pc.GetComponentInChildren<EquipSystem>().currentEquipment[0].name == "BerserkersAxe")
            {
                StartCoroutine(Fight());
                fightbtn.interactable = false;
            }
        }
        else
        {
            StartCoroutine(Fight());
        }
    }

    // Main combat function
    IEnumerator Fight()
    {
        // Pause after button press, before fight starts
        yield return new WaitForSeconds(.5f);

        // Using agility to pick which side goes first
        if ((pc.GetComponent<CharacterAttributes>().agility + Random.Range(1,pc.GetComponent<CharacterAttributes>().agility)) >= ec.GetComponent<EnemyAttributes>().agility + Random.Range(1, ec.GetComponent<EnemyAttributes>().agility))
        {
            playerAttackedFirst = true;
            ecHealthSystem.Damage(AttackEnemy());
            ecHealthBar.value = ecHealthSystem.GetHealthPercent();
            yield return new WaitForSeconds(1);

            if (!CheckConditions())
                yield break;

            pcHealthSystem.Damage(AttackPlayer());
            pcHealthBar.value = pcHealthSystem.GetHealthPercent();
            yield return new WaitForSeconds(1);

            if (!CheckConditions())
                yield break;
        }
        else
        {
            playerAttackedFirst = false;
            pcHealthSystem.Damage(AttackPlayer());
            pcHealthBar.value = pcHealthSystem.GetHealthPercent();
            yield return new WaitForSeconds(1);

            if (!CheckConditions())
                yield break;

            ecHealthSystem.Damage(AttackEnemy());
            ecHealthBar.value = ecHealthSystem.GetHealthPercent();
            yield return new WaitForSeconds(1);

            if (!CheckConditions())
                yield break;
        }

        // Re-enable fight button
        if (CheckConditions())
            fightbtn.interactable = true;
    }

    private int AttackPlayer()
    {
        // Calculate damage on player
        int dmg = 0;
        switch (ec.GetComponent<EnemyAttributes>().type)
        {
            case AttributeType.STR:
                dmg = (Random.Range(ec.GetComponent<EnemyAttributes>().strength, (int)(ec.GetComponent<EnemyAttributes>().strength * 1.5f)) + Random.Range(1, ec.GetComponent<EnemyAttributes>().strength) / 2) - pc.GetComponent<CharacterAttributes>().defense;
                break;
            case AttributeType.INT:
                dmg = (ec.GetComponent<EnemyAttributes>().intelligence + (int)(ec.GetComponent<EnemyAttributes>().intelligence * Random.Range(.1f,1f))) - pc.GetComponent<CharacterAttributes>().spirit;
                break;
            case AttributeType.LCK:
                int def = 0;
                if (pc.GetComponent<CharacterAttributes>().defense > pc.GetComponent<CharacterAttributes>().spirit)
                {
                    def = pc.GetComponent<CharacterAttributes>().defense;
                }
                else
                {
                    def = pc.GetComponent<CharacterAttributes>().spirit;
                }
                dmg = Random.Range(1, ec.GetComponent<EnemyAttributes>().luck * 3) - (def + Random.Range(0, pc.GetComponent<CharacterAttributes>().luck));
                break;
            default:
                break;
        }
        Debug.Log("Enemy does " + dmg + " points of damage!");

        // Sound effects
        SFX.instance.PlayerDamage(ec.GetComponent<EnemyAttributes>().type, dmg);

        // Show damage numbers
        DamagePop(pcHealthBar.transform, dmg,ec.GetComponent<EnemyAttributes>().type.ToString());
        incomingDmg += dmg;

        // Withstand Ability
        if (pc.attributes.ability == CharacterAbility.Withstand && ClassAbilityActive() == true)
        {
            if (pcHealthSystem.GetHealthPercent() > .4f && pcHealthSystem.GetHealthAmount() - dmg <= 0)
            {
                dmg = pcHealthSystem.GetHealthAmount() - 1;
            }
        }

        return dmg;
    }

    // Enemy attack function
    private int AttackEnemy()
    {
        // Calculate damage on enemy
        int dmg = 0;
        dmg = EquipSystem.instance.currentEquipment[0].WeaponDamage(pc, ec);
        print("Player does " + dmg + " points of damage!");

        // Play sound effects
        SFX.instance.PlayerAttack(EquipSystem.instance.currentEquipment[0].type, EquipSystem.instance.currentEquipment[0].ability, dmg);

        // Show damage numbers 
        DamagePop(ecHealthBar.transform, dmg, EquipSystem.instance.currentEquipment[0].type.ToString());

        // If there are any special abilities
        SpecialAttack(EquipSystem.instance.currentEquipment[0].ability);
        return dmg;
    }

    private void DamagePop(Transform t, int dmg, string type)
    {
        PopUpText dmgPop = Instantiate(damagePopUp, t.position, Quaternion.identity, t.transform);
        if (dmg < 0)
            dmg = 0;
        dmgPop.animator.GetComponent<Text>().text = dmg.ToString();
        switch (type)
        {
            case "STR":
                dmgPop.animator.GetComponent<Text>().color = new Color((float)0.572549, 0, 0);
                break;
            case "INT":
                dmgPop.animator.GetComponent<Text>().color = new Color(0, 0, (float)0.572549);
                break;
            case "LCK":
                dmgPop.animator.GetComponent<Text>().color = new Color(1, 1, 0);
                break;
            default:
                break;
        }
    }
    
    private void SpecialAttack(Ability ability)
    {
        int eStat = 0;
        switch (ability)
        {
            case Ability.Overwhelm:
                eStat = ec.GetComponent<EnemyAttributes>().strength;
                ec.GetComponent<EnemyAttributes>().strength -= (int)(eStat * .05);
                eStat = ec.GetComponent<EnemyAttributes>().intelligence;
                ec.GetComponent<EnemyAttributes>().intelligence -= (int)(eStat * .05);
                break;
            case Ability.Break:
                float randBreak = Random.Range(.01f, .1f);
                eStat = ec.GetComponent<EnemyAttributes>().strength;
                ec.GetComponent<EnemyAttributes>().strength -= (int)(eStat * randBreak);
                break;
            case Ability.Ruin:
                eStat = ec.GetComponent<EnemyAttributes>().defense;
                ec.GetComponent<EnemyAttributes>().defense -= (int)(eStat * .05);
                break;
            case Ability.Bane:
                eStat = ec.GetComponent<EnemyAttributes>().spirit;
                ec.GetComponent<EnemyAttributes>().spirit -= (int)(eStat * .05);
                break;
            default:
                break;
        }
    }

    private bool CheckConditions()
    {
        if (pcHealthSystem.GetHealthAmount() <= 0)
        {
            CheckAd();
            defeatScreen.SetActive(true);
            CombatMusic.instance.PlayGameOver();
            return false;
        }
        else if(ecHealthSystem.GetHealthAmount() <= 0)
        {
            CheckAd();
            pc.GetComponentInChildren<EquipSystem>().currentEquipment[0].IncrementKillCount(pc.GetComponentInChildren<EquipSystem>().currentEquipment[0].ability);

            if (gameLevel == 45)
            {
                gameLevel++;
                BattleRecord();
                gameCompleteScreen.SetActive(true);
            }
            else
            {
                victoryScreen.SetActive(true);
            }

            CombatMusic.instance.PlayVictory();
            return false;
        }

        // Rage Ability
        if (pc.attributes.ability == CharacterAbility.Rage && ClassAbilityActive() == true)
        {
            Debug.Log("Rage Activated");
            pc.EquipItems();
            Inventory.instance.UpdateStatsUI();
        }

        return true;
    }

    private void CheckAd()
    {
        if (adCount > 6)
        {
            // ads.PlayAd();
            // adCount = 0;
            // PlayerPrefs.SetInt("AdCount", adCount);
        }
        else
        {
            // adCount++;
            // PlayerPrefs.SetInt("AdCount", adCount);
        }
    }

    public void SetNextLevel()
    {
        GetNewEnemy();
        ResetPlayer();
        victoryScreen.SetActive(false);
        fightbtn.interactable = true;
        //Background.instance.SetBackground(gameLevel);
        CombatMusic.instance.StartCombat();
    }

    private void ResetPlayer()
    {
        int recover;

        if (pc.attributes.ability == CharacterAbility.Appriase && ClassAbilityActive() == true)
        {
            recover = 10;
        }
        else
        {
            recover = 0;
        }

        float recoverPercent = (Random.Range(10, 25) + recover) / 100f;
        recover = 0;

        Debug.Log("Recovery Percent: " + recoverPercent * 100 + "%");
        recover = (int)(pcHealthSystem.GetMaxHealth() * recoverPercent);
        
        pcHealthSystem.Heal(recover);
        pcHealthBar.value = pcHealthSystem.GetHealthPercent();
        Debug.Log("Player recovers " + recover + " health");
    }

    private void GetNewEnemy()
    {
        if (subLevel < 10)
        {
            gameLevel++;
            subLevel++;
            levelText.text = "LEVEL: " + dungeonLevel.ToString() + " - " + subLevel.ToString();
        }
        else
        {
            subLevel = 1;
            dungeonLevel++;
            gameLevel++;
            levelText.text = "LEVEL: " + dungeonLevel.ToString() + " - " + subLevel.ToString();
        }
        
        // Selects and sets new enemy attributes
        ec.NewEnemy(gameLevel);
        enemyName.text = ec.enemyType.ToString().Replace("_"," ");
        ec.GetComponent<SpriteRenderer>().sprite = ec.GetComponent<EnemySprites>().GetSprite(ec.enemyType);
        ec.GetComponent<Image>().sprite = ec.GetComponent<SpriteRenderer>().sprite;

        // Set enemy health
        ecHealthSystem.SetHealthAmount((int)ec.MaxHP());
        ecHealthSystem.SetMaxHealth((int)ec.MaxHP());
        
        // Reset enemy health bar
        ecHealthBar.value = ecHealthSystem.GetHealthPercent();

        // Reset incoming damage for the level
        incomingDmg = 0;

        // Reset special counters
        if (pc.GetComponentInChildren<EquipSystem>().currentEquipment[0].ability.ToString() == "Vindicate")
        {
            pc.GetComponentInChildren<EquipSystem>().currentEquipment[0].ResetRoundCount();
        }

        BattleRecord();
    }

    private void BattleRecord()
    {
        int recordLevel = gameLevel - 1;

        // Set Records
        switch (pc.characterType)
        {
            case CharacterType.Warrior:
                if (classLevel[0] < recordLevel)
                {
                    classLevel[0] = recordLevel;
                    ES3.Save<int[]>("ClassLevel", classLevel);
                }
                break;
            case CharacterType.Barbarian:
                if (classLevel[1] < recordLevel)
                {
                    classLevel[1] = recordLevel;
                    ES3.Save<int[]>("ClassLevel", classLevel);
                }
                break;
            case CharacterType.Mage:
                if (classLevel[2] < recordLevel)
                {
                    classLevel[2] = recordLevel;
                    ES3.Save<int[]>("ClassLevel", classLevel);
                }
                break;
            case CharacterType.Wizard:
                if (classLevel[3] < recordLevel)
                {
                    classLevel[3] = recordLevel;
                    ES3.Save<int[]>("ClassLevel", classLevel);
                }
                break;
            case CharacterType.Rogue:
                if (classLevel[4] < recordLevel)
                {
                    classLevel[4] = recordLevel;
                    ES3.Save<int[]>("ClassLevel", classLevel);
                }
                break;
        }
    }

    public int GetTotalDamage()
    {
        return incomingDmg;
    }

    public void QuitButton()
    {
        confirmationScreen.SetActive(true);
    }

    public void ExitButton()
    {
        exitScreen.SetActive(true);
    }

    public void UpdatePlayerAttribute(int modifier, StatType stat)
    {
        string attr = null;
        switch (stat)
        {
            case StatType.VIT:
                pc.attributes.vitality += modifier;
                attr = "Vitality";
                break;
            case StatType.STR:
                pc.attributes.strength += modifier;
                attr = "Strength";
                break;
            case StatType.DEF:
                pc.attributes.defense += modifier;
                attr = "Defense";
                break;
            case StatType.INT:
                pc.attributes.intelligence += modifier;
                attr = "Intelligence";
                break;
            case StatType.SPR:
                pc.attributes.spirit += modifier;
                attr = "Spirit";
                break;
            case StatType.AGL:
                pc.attributes.agility += modifier;
                attr = "Agility";
                break;
            case StatType.LCK:
                pc.attributes.luck += modifier;
                attr = "Luck";
                break;
            default:
                break;
        }

        pc.attributes.ModifyStat(attr, modifier);
    }

    public PlayerCharacter GetPlayer()
    {
        return pc;
    }

    public bool GetAttackOrder()
    {
        return playerAttackedFirst;
    }

    public int GetPlayerHealth()
    {
        int hp = pcHealthSystem.GetHealthAmount();
        return hp;
    }

    public int GetEnemyHealth()
    {
        int hp = ecHealthSystem.GetHealthAmount();
        return hp;
    }

    public void StartingHealth()
    {
        pcHealthSystem.SetHealthAmount(pcHealthSystem.GetMaxHealth());
        Inventory.instance.UpdateStatsUI();
    }

    public void ResetPlayerHealthBar()
    {
        instance.pcHealthBar.value = instance.pcHealthSystem.GetHealthPercent();
    }

    public bool ClassAbilityActive()
    {
        string equippedAbility = pc.GetComponentInChildren<EquipSystem>().currentEquipment[0].ability.ToString();
        int count=0;

        switch (pc.attributes.ability)
        {
            case CharacterAbility.Rage:
                string[] maceAbilities = { "Whack", "Pound", "Smash", "Clobber", "Pummel", "Impact", "Obliterate", "Break", "Ruin", "Devestate"};

                foreach (string i in maceAbilities)
                {
                    if (i == equippedAbility)
                        count++;
                }

                if (count != 0)
                    return true;
                else
                    return false;
            case CharacterAbility.Withstand:
                string[] swordAbilities = { "Cut", "Slash", "Judge", "Flay", "Sunder", "Black_Out", "Slice", "Vindicate", "Cascade", "Last_Rites" };

                foreach (string i in swordAbilities)
                {
                    if (i == equippedAbility)
                        count++;
                }

                if (count != 0)
                    return true;
                else
                    return false;
            case CharacterAbility.Restore:
                string[] staffAbilities = { "Force", "Arc", "Cold_Snap", "Arcane_Surge", "Summon_Earth", "Tempest", "Burst", "Fracture", "Eclipse", "Harvest", "Overwhelm" };

                foreach (string i in staffAbilities)
                {
                    if (i == equippedAbility)
                        count++;
                }

                if (count != 0)
                    return true;
                else
                    return false;
            case CharacterAbility.Enhance:
                string[] bookAbilities = { "Paper_Cut", "Well_Read", "Purify", "Conflagrate", "Quake", "Hope", "Sacrifice", "Anguish", "Bane", "Bloodlust" };

                foreach (string i in bookAbilities)
                {
                    if (i == equippedAbility)
                        count++;
                }

                if (count != 0)
                    return true;
                else
                    return false;
            case CharacterAbility.Appriase:
                string[] rogueAbilities = { "Prick", "Stab", "Double_Stab", "Shred", "Lacerate", "Pierce", "Parry", "Incinerate", "Karma", "Necrotizing_Touch" };

                foreach (string i in rogueAbilities)
                {
                    if (i == equippedAbility)
                        count++;
                }

                if (count != 0)
                    return true;
                else
                    return false;
            default:
                return false;
        }
    }
}
