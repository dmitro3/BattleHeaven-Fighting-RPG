using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DailyGift : MonoBehaviour
{
    // UI
    GameObject itemsContainer;
    GameObject confirmGiftCanvas;
    GameObject giftCollectedButton;
    List<GameObject> giftItems = new List<GameObject>();

    // Manager
    MainMenu mainMenu;

    // variables
    string lastButtonClicked = "";

    private void Awake()
    {
        itemsContainer = GameObject.Find("Group_Reward");
        confirmGiftCanvas = GameObject.Find("Canvas_Gift_Collected");
        giftCollectedButton = GameObject.Find("Button_BackToDailyGift");
        mainMenu = GameObject.Find("MainMenuManager").GetComponent<MainMenu>(); // notifications system

        // on enable
        GetDailyItems();
        confirmGiftCanvas.SetActive(false);
        giftCollectedButton.GetComponent<Button>().onClick.AddListener(() => GoToMainMenu());

        // ResetWeek();
        DisableInteraction();
        LoadUI();
        EnableNextReward();
    }

    /* Day items structure
     * 
     * - 1-6 rewards GO container
     *      - DayX
     *          - VFX
     *          - UI
     *          - UI
     *          - Reward value
     *          - Reward collected UI
     *          - Focus UI (item is collectable)
     *              - GO container
     *              - Text
     * - 7 reward
     */

    private void LoadUI()
    {
        // 0 - false
        // 1 - true
        float flag;
        string day;

        for (int i = 0; i < giftItems.Count; i++)
        {
            day = "DAY" + (i + 1);
            flag = PlayerPrefs.GetFloat(day);

            if(flag == 1)
                DisableButtonOnRewardCollected(day);
        }
    }

    private void SaveDay(string day)
    {
        PlayerPrefs.SetFloat(day, 1);
        PlayerPrefs.Save();
    }

    private void GetDailyItems()
    {
        for(int i = 1; i <= 7; i++)
            giftItems.Add(GameObject.Find("Day" + i));
    }

    private void DisableInteraction()
    {
        for (int i = 0; i < giftItems.Count; i++)
            giftItems[i].GetComponent<Button>().interactable = false;
    }

    public void ResetWeek()
    {
        for(int i = 0; i < giftItems.Count; i++)
            PlayerPrefs.SetFloat("DAY" + (i + 1), 0);

        PlayerPrefs.Save();
    }

    private Dictionary<string, string> GetRewardType(string day)
    {
        day = day.ToUpper();

        return new Dictionary<string, string>
        {
            { 
              DailyGiftDB.gifts[(DailyGiftDB.Days)Enum.Parse(typeof(DailyGiftDB.Days), day)]["reward"],
              DailyGiftDB.gifts[(DailyGiftDB.Days)Enum.Parse(typeof(DailyGiftDB.Days), day)]["value"]
            }
        };
    }

    private void GiveReward(Dictionary<string, string> reward)
    {
        if (reward.ContainsKey("gold"))
        {
            CurrencyHandler.instance.AddGold(int.Parse(reward["gold"]));
            Debug.Log(int.Parse(reward["gold"]));
        }
        if (reward.ContainsKey("gems"))
        {
            CurrencyHandler.instance.AddGems(int.Parse(reward["gems"]));
            Debug.Log(int.Parse(reward["gems"]));
        }
        if (reward.ContainsKey("chest"))
        {
            // give chest here
        }

        // show confirm button + manage UI
        confirmGiftCanvas.SetActive(true);
    }

    public void GiveRewardButton()
    {
        lastButtonClicked = EventSystem.current.currentSelectedGameObject.name.ToUpper();
        GiveReward(GetRewardType(lastButtonClicked));
        DisableButtonOnRewardCollected(lastButtonClicked);
        SaveDay(lastButtonClicked);
        mainMenu.DisableDailyGiftNotification();
    }

    public void DisableButtonOnRewardCollected(string day)
    {
        for(int i = 0; i < giftItems.Count; i++)
        {
            if(giftItems[i].name.ToUpper() == day)
            {
                giftItems[i].transform.GetChild(4).gameObject.SetActive(true);
                giftItems[i].transform.GetChild(5).gameObject.SetActive(false);
                giftItems[i].GetComponent<Button>().interactable = false;
            }
        }
    }

    public void EnableCollectableGift()
    {
        giftItems[0].transform.GetChild(5).gameObject.SetActive(true);
        giftItems[0].GetComponent<Button>().interactable = true;
    }
    public void EnableNextReward()
    {
        for (int i = 0; i < giftItems.Count; i++)
        {
            if (!giftItems[i].transform.GetChild(4).gameObject.activeSelf)
            {
                giftItems[i].transform.GetChild(4).gameObject.SetActive(false);
                giftItems[i].transform.GetChild(5).gameObject.SetActive(true);
                giftItems[i].GetComponent<Button>().interactable = true;

                return;
            }
        }
    }

    public void DisableDailyGiftNotification()
    {
        mainMenu.DisableDailyGiftNotification();
    }

    public void EnableDailyGiftNotification()
    {
        mainMenu.EnableDailyGiftNotification();
    }

    public void GoToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.MainMenu.ToString());
    }
}
