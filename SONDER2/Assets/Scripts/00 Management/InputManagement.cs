using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

public class InputManagement : MonoBehaviourReferenced {

    [SerializeField] private int playerID = 0;
    [SerializeField] private Player player;

    private void Start() {
        player = ReInput.players.GetPlayer("SYSTEM");
    }

    public float GetInput(string key) {
        return player.GetAxis(key);
    }

    public bool GetInputBool(string key) {
        return player.GetAxis(key) != 0;
    }

    public bool GetInputButton(string key) {
        return player.GetButton(key);
    }
}

public static class Inputs {
    public static string gear0 = "Gear0";
    public static string gear1 = "Gear1";
    public static string gear2 = "Gear2";
    public static string gear3 = "Gear3";
    public static string gear4 = "Gear4";
    public static string gear5 = "Gear5";
    public static string clutch = "Clutch";
    public static string up = "Up";
    public static string left = "Left";
    public static string down = "Down";
    public static string right = "Right";
    public static string flash = "Flash";
    public static string enter = "Enter";
    public static string esc = "Escape";

    public static string[] gears = { gear0, gear1, gear2, gear3, gear4, gear5 };
    public static string[] directions = { up, left, down, right };
}

