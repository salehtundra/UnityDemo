using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Chris
public class Health : MonoBehaviour {

    private float currentHealth;
    private float maxHealth;
    public bool isDead;

    public Health(int current, int max) {
        currentHealth = current;
        maxHealth = max;
    }

	// Use this for initialization
	void Start () {
        isDead = false;
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public float GetCurrentHealth() {
        return currentHealth;
    }

    public void SetCurrentHealth(float current) {
        if (currentHealth >= 0) {
            currentHealth = current;
        } else {
            currentHealth = 0;
        }
    }

    public float GetMaxHealth() {
        return maxHealth;
    }

    public void SetMaxHealth(float max) {
        if (maxHealth >= 0) {
            maxHealth = max;
        } else {
            maxHealth = 0;
        }
    }

    public void SetCurrentHealthToMax() {
        currentHealth = maxHealth;
    }

    public void DecreaseCurrentHealht(float deduction) {
        currentHealth -= deduction;
        if (currentHealth < 0) {
            currentHealth = 0;
            isDead = true;
        }
    }

    public void IncreaseCurrentHealht(float addition) {
        currentHealth += addition;
        if (currentHealth > maxHealth) {
            currentHealth = maxHealth;
        }
    }
}
