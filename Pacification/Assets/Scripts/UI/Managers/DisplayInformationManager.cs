﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayInformationManager : MonoBehaviour {

    public Text nbRound;

    public Text money;
    public Text science;
    public Text food;
    public Text wood;
    public Text horse;
    public Text iron;
    public Text gold;
    public Text diamond;

    public GameObject townResources;
    public Text productivity;
    public Text population;
    public Text happiness;


    public Transform downPanel;
    public Transform upEditPanel;
    public Transform upGamePanel;

    public GameObject editorLeftPanel;
    public GameObject editorRightPanel;
    public GameObject loadingPanel;
    public GameObject rightGamePanel;



    //UPGRADE
    //////////////////////
    public GameObject upgradePannel;
    public GameObject upgradePannelClose;

    public Text regularLvl;
    public Text regularScience;
    public Text regularRes1;
    public Text regularRes2;
    public Text regularRes3;
    public GameObject regularUpgrade;

    public Text rangedLvl;
    public Text rangedScience;
    public Text rangedRes1;
    public Text rangedRes2;
    public Text rangedRes3;
    public GameObject rangedUpgrade;

    public Text heavyLvl;
    public Text heavyScience;
    public Text heavyRes1;
    public Text heavyRes2;
    public Text heavyRes3;
    public GameObject heavyUpgrade;
    //////////////////////

    public Player player;

    void Start()
    {
        InitiateResources();

        if(GameManager.Instance.gamemode == GameManager.Gamemode.EDITOR)
        {
            foreach(Transform t in downPanel)
                t.gameObject.SetActive(false);

            foreach(Transform t in upGamePanel)
                t.gameObject.SetActive(false);

            editorLeftPanel.SetActive(true);
            editorRightPanel.SetActive(true);
            rightGamePanel.SetActive(false);

            Shader.EnableKeyword("HEX_MAP_EDITOR");
        }
        else
        {
            foreach(Transform t in upEditPanel)
                t.gameObject.SetActive(false);

            editorLeftPanel.SetActive(false);
            editorRightPanel.SetActive(false);
            rightGamePanel.SetActive(true);

            Shader.DisableKeyword("HEX_MAP_EDITOR");
        }
    }

    private void Update()
    {
        if(player != null && player.canPlay)
            DisplayResources();
    }

    public void UpdateRoundDisplay(int rounds)
    {
        nbRound.text = "" + rounds;
    }

    public void DisplayResources()
    {
        money.text = "" + player.money;
        science.text = "" + player.science;
        food.text = "" + player.resources[5];
        wood.text = "" + player.resources[4];
        horse.text = "" + player.resources[3];
        iron.text = "" + player.resources[0];
        gold.text = "" + player.resources[1];
        diamond.text = "" + player.resources[2];
    }

    public void DisplayTownResources(string productivityC, string populationC, string happinessC)
    {
        productivity.text = productivityC;
        population.text = populationC;
        happiness.text = happinessC;
        townResources.SetActive(true);
    }

    public void HideTownResources()
    {
        townResources.SetActive(false);
    }

    void InitiateResources()
    {
        money.text = "0";
        science.text = "0";
        food.text = "0";
        wood.text = "0";
        horse.text = "0";
        iron.text = "0";
        gold.text = "0";
        diamond.text = "0";

        townResources.SetActive(false);
        productivity.text = "0";
        population.text = "0";
        happiness.text = "0";
}

    public void KillLoading()
    {
        loadingPanel.SetActive(false);
    }

    public void LevelUp(string type)
    {
        player.LevelUp(Unit.StrToType(type));
    }

    //Upgrade pannel
    public void OpenUpgradePannel()
    {
        upgradePannelClose.SetActive(false);
        UpdateUpgradePannel();
        upgradePannel.SetActive(true);
    }
    public void CloseUpgradePannel()
    {
        upgradePannel.SetActive(false);
        upgradePannelClose.SetActive(true);
    }
    public void UpdateUpgradePannel()
    {
        regularLvl.text = "" + player.GetUnitLevel(Unit.UnitType.REGULAR) + "/20";
        regularScience.text = "";
        regularRes1.text = ""; //Food
        regularRes2.text = "";//Iron
        regularRes3.text = "";//Horse
        regularUpgrade.SetActive(player.science < 0); //Change the 0 (condition must be false in order to buy)

        rangedLvl.text = "" + player.GetUnitLevel(Unit.UnitType.RANGED) + "/20";
        rangedScience.text = "";
        rangedRes1.text = "";//Wood
        rangedRes2.text = "";//Iron
        rangedRes3.text = "";//Gold
        rangedUpgrade.SetActive(player.science < 0); //Change the 0 (condition must be false in order to buy)

        heavyLvl.text = "" + player.GetUnitLevel(Unit.UnitType.HEAVY) + "/20";
        heavyScience.text = "";
        heavyRes1.text = ""; //Wood
        heavyRes2.text = "";//Iron
        heavyRes3.text = "";//Diams
        heavyUpgrade.SetActive(player.science < 0); //Change the 0 (condition must be false in order to buy)
    }

}
