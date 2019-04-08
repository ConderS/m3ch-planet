﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerController : NetworkBehaviour
{
    //Warfare variables
    int maxHealth = 100;
    int maxEnergy = 100;
    [SyncVar]
    int currentHealth;
    [SyncVar]
    int currentEnergy;

    //Networking variables
    public bool _isLocalPlayer;
    [SyncVar]
    private bool _ready; //whether the Game server can start the game
    private bool prevReady;

    //Turn Variables
    bool MyTurn; //true if is current player's turn

    //PlayerVariables
    string PlayerName;

    //Other Game variables
    GameController GC;
    AssetManager AM;

    //Debug Variables
    Renderer[] Renderers;
    ARDebugger d;
    
    private void Awake()
    {
        currentHealth = maxHealth;
        currentEnergy = 0;
        Renderers = gameObject.GetComponentsInChildren<Renderer>();
    }

    private void Start()
    {
        GC = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        d = GC.GetComponent<ARDebugger>();
        AM = GC.GetComponent<AssetManager>();
        prevReady = false;
        MyTurn = false;
        PlayerName = PlayerPrefs.GetString("PlayerName");
        if (PlayerName == "") PlayerName = "Bob";
    }

    public void TakeDamage(int _amt)
    {
        currentHealth -= _amt;
    }

    public void Update()
    {
        CheckReady();
    }

    [Command]
    public void CmdSetReady(bool ready)
    {
        _ready = ready;
        if (GC.AreAllPlayersReady())
            CmdStartGame();
    }

    public bool GetReady()
    {
        return _ready;
    }

    void CheckReady()
    {
        d.Log(gameObject.name + " is " + _ready + " and " + prevReady);
        if (_ready != prevReady)
        {
            //User just updated ready
            foreach (Renderer r in Renderers)
            {
                r.material.color = _ready ? Color.red : Color.white;
            }
            prevReady = _ready;
        }
    }

    public void SetMyTurn(bool turn)
    {
        MyTurn = turn;
    }

    //Tells server to start game
    [Command]
    private void CmdStartGame()
    {
        GameObject Planet = Instantiate(AM.Get("Planet"));
        NetworkServer.Spawn(Planet);
        Planet.gameObject.name = "Planet " + Planet.GetComponent<NetworkIdentity>().netId.ToString();
        RpcStartGame(Planet);
    }

    //Tells every client to start game
    [ClientRpc]
    private void RpcStartGame(GameObject Planet)
    {
        Planet.transform.parent = AM.Get("GroundImageTarget").transform;
        GC.StartGame();
    }
}
