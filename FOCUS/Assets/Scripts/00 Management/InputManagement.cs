﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class InputManagement : MonoBehaviourReferenced {

    [SerializeField] private int playerID = 0;
    [SerializeField] private Player player;

    //private Dictionary<string, string> nameMaps = new Dictionary<string, string>()  {
    //                                                                                    {"Vertical", "Gas"},
    //                                                                                    {"Horizontal", "Steer"},
    //                                                                                    {"Jump", "Jump"},
    //                                                                                    {"Throttle", "Jump"},
    //                                                                                    {"Brake", "Jump"},
    //                                                                                    {"Drift", "Jump"},
    //                                                                                    {"Boost", "Jump"}
    //                                                                                };

    private void Start() {
        player = ReInput.players.GetPlayer(playerID); 
    }


    public float GetInput(string key) {
        Debug.Log(key);
        return player.GetAxis(key);
    }
}
