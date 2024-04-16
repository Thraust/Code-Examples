using ES3Types;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Victory : MonoBehaviour
{
    public CombatMusic combatMusic;
    public Button nextLevel;
    public Transform rewardsPanel;
    RewardSlot[] slots;
    Rewards inventory;
    int selectedItemSlot;
    public Text itemName;
    public Text noxText;

    private GameObject fade;

    public void Start()
    {
        //combatMusic.StopCombat();
        //combatMusic.PlayVictory();
        slots = rewardsPanel.GetComponentsInChildren<RewardSlot>();
        inventory = Rewards.instance;
        itemName.enabled = false;

        fade = GameObject.FindGameObjectWithTag("BlackFade");
    }

    public void Update()
    {
        #region Nox
        int lvl = BattleSystem.instance.gameLevel;
        if (lvl > 0 && lvl < 10)
        {
            noxText.text = "Nox: 25";
        }

        if (lvl >= 10 && lvl < 20)
        {
            noxText.text = "Nox: 50";
        }

        if (lvl >= 20 && lvl < 30)
        {
            noxText.text = "Nox: 75";
        }

        if (lvl >= 30 && lvl < 40)
        {
            noxText.text = "Nox: 100";
        }

        if (lvl >= 40)
        {
            noxText.text = "Nox: 200";
        }
        #endregion
    }

    public void NextLevel()
    {
        StartCoroutine(StartNextLevel());
    }

    IEnumerator StartNextLevel()
    {
        yield return new WaitForSeconds(1.5f);

        int lvl = BattleSystem.instance.gameLevel;
        Debug.Log("Next Level is Pressed");
        slots[selectedItemSlot].AddReward();
        BattleSystem.instance.SetNextLevel();
        unHighlight(Rewards.instance.items[selectedItemSlot]);
        itemName.enabled = false;
        Rewards.instance.GetComponentInChildren<RewardsUI>().RewardItems(lvl);
        nextLevel.interactable = false;
        #region Nox
        if (lvl > 0 && lvl < 10)
        {
                PlayerPrefs.SetInt("Nox", PlayerPrefs.GetInt("Nox") + 25);
        }

        if (lvl >= 10 && lvl < 20)
        {
            PlayerPrefs.SetInt("Nox", PlayerPrefs.GetInt("Nox") + 50);
        }

        if (lvl >= 20 && lvl < 30)
        {
            PlayerPrefs.SetInt("Nox", PlayerPrefs.GetInt("Nox") + 75);
        }

        if (lvl >= 30 && lvl < 40)
        {
            PlayerPrefs.SetInt("Nox", PlayerPrefs.GetInt("Nox") + 100);
        }

        if (lvl >= 40)
        {
            PlayerPrefs.SetInt("Nox", PlayerPrefs.GetInt("Nox") + 200);
        }
        #endregion

        fade.GetComponent<BlackTransition>().FadeIn();
    }

    public void highlightItem(Item item)
    {
        Debug.Log("Highlight Selected");
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inventory.items.Count)
            {
                // Inventory Highlight
                if (inventory.items[i] == item)
                {
                    slots[i].GetComponentsInChildren<Image>()[2].enabled = true;
                    itemName.enabled = true;
                    itemName.text = item.itemName;
                    selectedItemSlot = i;
                }
                else
                {
                    slots[i].GetComponentsInChildren<Image>()[2].enabled = false;
                }
            }
        }
    }

    public void unHighlight(Item item)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (inventory.items[i] == item)
            {
                slots[i].GetComponentsInChildren<Image>()[2].enabled = false;
            }
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (inventory.items[i] == item)
            {
                slots[i].GetComponentsInChildren<Image>()[2].enabled = false;
            }
        }
    }

}
