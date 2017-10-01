using UnityEngine;
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

        // Clint - channged  
        if (Input.GetButtonDown("Jump")) {
			player.OnJumpInputDown ();
		}
		else if (Input.GetButtonUp("Jump")) {
			player.OnJumpInputUp ();
		}

		if (Input.GetButtonDown("Sprint")) {
            player.startSprint();
        }
		else if (Input.GetButtonUp("Sprint")) {
            player.endSprint();
        }

		if (Input.GetButtonDown("Dodge")) {
            player.PerformDodge();
        }
    }
}
