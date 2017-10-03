using UnityEngine;
using System.Collections;

public class BumRushEnemeyMovementPattern : MonoBehaviour {

    Transform player;               // Reference to the player's position.
    Health playerHealth;      // Reference to the player's health.
    Health enemyHealth;        // Reference to this enemy's health.
    ////////NavMeshAgent nav;               // Reference to the nav mesh agent.


    void Awake() {
        // Set up the references.
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerHealth = GetComponent<Health>();
        enemyHealth = GetComponent<Health>();
        /////////nav = GetComponent<NavMeshAgent>();
    }


    void Update() {
        // If the enemy and the player have health left...
        if (enemyHealth.GetCurrentHealth() > 0 && playerHealth.GetCurrentHealth() > 0) {
            // ... set the destination of the nav mesh agent to the player.
            //////////nav.SetDestination(player.position);
        }
        // Otherwise...
        else {
            // ... disable the nav mesh agent.
            //////////nav.enabled = false;
        }
    }
}
