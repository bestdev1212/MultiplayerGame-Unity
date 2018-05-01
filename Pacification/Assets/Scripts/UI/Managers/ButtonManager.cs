﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour {

    public GameObject leftPannel;
    public GameObject rightPannel;
    public GameObject activePlayer;
    public ControlsManager controls;

    public bool cheatmode = false;

    private Client client;

    void Start()
    {
        controls = FindObjectOfType<ControlsManager>();
        client = FindObjectOfType<Client>();
    }

    public void EndTurnButton()
    {
        client.Send("CEND|" +  client.clientName);
        client.player.canPlay = false;
        EndTurn();
    }

    public void TakeTurn()
    {
        client.player.canPlay = true; ;
        activePlayer.SetActive(true);
    }

    public void EndTurn()
    {
        activePlayer.SetActive(false);
    }

    public void DeconnectionButton()
    {
        Server server = FindObjectOfType<Server>();
        if(server != null)
        {
            server.server.Stop();
            Destroy(server.gameObject);
        }

        client = FindObjectOfType<Client>();
        if(client != null)
        {
            Destroy(client.gameObject);
        }
        SceneManager.LoadScene("Menu");

        Destroy(GameObject.Find("GameManager"));
    }
}
