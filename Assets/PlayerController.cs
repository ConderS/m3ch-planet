﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

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
    [SyncVar]
    string PlayerName;
    GameObject PlayerMesh;

    //Other Game variables
    GameController GC;
    AssetManager AM;

    //Debug Variables
    ARDebugger d;

    //PlayerUI
    public GameObject PlayerNameText;
    GameObject PlayerUICanvas;
    public GameObject HealthBar;
    public GameObject EnergyBar;
    Slider HealthBarSlider;
    Slider EnergyBarSlider;
    public TextMeshProUGUI ReadyText;
    
    private void Awake()
    {
        currentHealth = maxHealth;
        currentEnergy = 0;
        //Init Player UI variables
        HealthBarSlider = HealthBar.GetComponent<Slider>();
        EnergyBarSlider = EnergyBar.GetComponent<Slider>();
    }

    private void Start()
    {
        GC = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        d = GC.GetComponent<ARDebugger>();
        AM = GC.GetComponent<AssetManager>();
        prevReady = false;
        MyTurn = false;
        PlayerUICanvas = PlayerNameText.transform.parent.gameObject;
        PlayerUICanvas.GetComponent<Canvas>().worldCamera = Camera.main;
        PlayerMesh = transform.GetChild(0).gameObject;
    }

    public void InitPlayerName(string name)
    {
        if (name == "") PlayerName = "Bob";
        else PlayerName = name;
        PlayerNameText.GetComponent<TextMeshProUGUI>().text = PlayerName;
    }

    public void TakeDamage(int _amt)
    {
        currentHealth -= _amt;
    }

    public void Update()
    {
        if (GC.GetGameHappening())
        {

        }
        else
        {
            CheckReady();
            PlayerNameText.GetComponent<TextMeshProUGUI>().text = PlayerName;
        }

        PlayerUICanvas.transform.LookAt(Camera.main.transform);
        HealthBarSlider.value = (float)currentHealth / 100;
        EnergyBarSlider.value = (float)currentEnergy / 100;
    }

    [Command]
    public void CmdSetReady(bool ready)
    {
        _ready = ready;
        d.LogPersist(gameObject.name + " is " + ready);
        if (GC.AreAllPlayersReady())
            CmdStartGame();
    }

    public bool GetReady()
    {
        return _ready;
    }

    void CheckReady()
    {
        if (_ready != prevReady)
        {
            //User just updated ready
            ReadyText.text = _ready ? "Ready" : "Not Ready";
            ReadyText.color = _ready ? Color.green : Color.red;
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
        Planet.transform.parent = AssetManager.Instance.Get("GroundImageTarget").transform;
        ReadyText.gameObject.SetActive(false);
        PlayerMesh.GetComponent<Rigidbody>().useGravity = false;
        GC.StartGame();
        Planet.GetComponent<PlanetGravity>().Init(4);
    }
}
