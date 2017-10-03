using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Chris
public class Stamina : MonoBehaviour {

    private float currentStamina;
    private float maxStamina;
    public bool inStaminaPenalty;
    public float staminaThreshold = 1.0f;
    public float staminaDampner = 0.5f;

    public Stamina(int current, int max) {
        currentStamina = current;
        maxStamina = max;
    }

    // Use this for initialization
    void Start() {
        inStaminaPenalty = false;
    }

    // Update is called once per frame
    void Update() {

    }

    public float GetCurrentStamina() {
        return currentStamina;
    }

    public void SetCurrentStamina(float current) {
        if (currentStamina >= 0) {
            currentStamina = current;
        } else {
            currentStamina = 0;
        }
    }

    public float GetMaxStamina() {
        return maxStamina;
    }

    public void SetMaxStamina(float max) {
        if (maxStamina >= 0) {
            maxStamina = max;
        } else {
            maxStamina = 0;
        }
    }

    public void SetCurrentStaminaToMax() {
        currentStamina = maxStamina;
    }

    public void DecreaseCurrentStamina(float deduction) {
        currentStamina -= deduction;
        if (currentStamina < 0) {
            currentStamina = 0;
            inStaminaPenalty = true;
        }
    }

    public void IncreaseCurrentStamina(float addition) {
        currentStamina += addition;
        if (currentStamina > maxStamina) {
            currentStamina = maxStamina;
        }
    }
}