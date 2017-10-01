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

		if (Input.GetKeyDown (KeyCode.Space)) {
			player.OnJumpInputDown ();
		}
		if (Input.GetKeyUp (KeyCode.Space)) {
			player.OnJumpInputUp ();
		}
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            player.startSprint();
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            player.endSprint();
        }
        if (Input.GetKeyDown(KeyCode.C)) {
            player.PerformDodge();
        }
    }
}
