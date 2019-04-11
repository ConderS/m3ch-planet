﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetGravity : MonoBehaviour
{
    //Game Variables
    GameController GC;
    TurnController TC;
    Rigidbody[] RB;
    PlayerController[] PCs;

    // Start is called before the first frame update
    void Start()
    {
        GC = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        TC = GC.GetComponent<TurnController>();
    }

    public void Init(int InitSeed)
    {
        PCs = TC.GetPlayers();
        RB = new Rigidbody[PCs.Length];
        for (int i = 0; i < PCs.Length; i++)
        {
            RB[i] = PCs[i].transform.GetChild(0).gameObject.GetComponent<Rigidbody>();
        }
        Random.seed = InitSeed;
        foreach (PlayerController Player in PCs)
        {
            Player.transform.parent = transform;
            //Randomly place the player somewhere
            Player.transform.position = new Vector3(
                Random.Range(4f, 10f),
                Random.Range(4f, 10f),
                Random.Range(4f, 10f));
            //Place the player on the surface of the planet
            Vector3 n = Player.transform.position - transform.position;
            n = n.normalized * (transform.localScale.x + 0.8f);
            Player.transform.position = transform.position + n;
            //Align Player's rotation to planet's normal
            RaycastHit hit;
            if(Physics.Raycast(
                Player.transform.position, //Shoot a ray from player's position
                -n,  //in the direction from the player to the planet
                out hit, //store the ray hit in hit
                Mathf.Infinity, //no limit to distance
                1<<9) //only detect objects in layer 9 (Planet)
                )
            {
                print(hit.transform.position);
                Player.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //TODO handle when a player disconnects or leaves the room
        foreach(Rigidbody rb in RB)
        {
            Vector3 force = transform.position - rb.transform.position;
            force = force.normalized * 3f;
            rb.AddForce(force);
        }
    }
}
