﻿using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Player))]
public class PlayerInput : MonoBehaviour {

    Player player;

    void Start () {
        player = GetComponent<Player> ();
    }

    void Update () {
        Vector2 directionalInput = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
        player.SetDirectionalInput (directionalInput);

        if (Input.GetButtonDown("Jump")) {
            player.OnJumpInputDown ();
        }
        else if (Input.GetButtonUp("Jump")) {
            player.OnJumpInputUp ();
        }

        player.SetIsTryingToSprint(Input.GetButton("Sprint"));

        if (Input.GetButtonDown("Dodge")) {
            player.PerformDodge();
        }

        //DEBUG FUNCTION
        if (Input.GetKeyDown(KeyCode.E)) {
            player.addExperience(100);
        }
    }
}
