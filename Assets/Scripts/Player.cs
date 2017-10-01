﻿using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

	public float maxJumpHeight = 4;
	public float minJumpHeight = 1;
	public float timeToJumpApex = .4f;
	public float accelerationTimeAirborne = .2f;
	public float accelerationTimeGrounded = .1f;
    public float moveSpeed = 6;

    //Stamina bar
    public float stamina, maxStamina = 5;
    Rect staminaRect;
    Texture2D staminaTexture;

    //Health bar
    public float health, maxHealth = 5;
    Rect healthRect;
    Texture2D healthTexture;

    //Run logic
    public float runSpeed = 12;
    bool isRunning;
    bool isTryingToRun;

    //Dodge logic
    float dodgeSpeed = 20;
    float currentDodgeTime = 0;
    float dodgeDuration = 0.20f;
    float accelerationTimeDodge = .0001f;
    bool isDodging;
    bool isFacingLeft;

    //Dodge bar debug
    Rect debugRect;
    Texture2D debugTexture;

    public Vector2 wallJumpClimb;
	public Vector2 wallJumpOff;
	public Vector2 wallLeap;

	public float wallSlideSpeedMax = 3;
	public float wallStickTime = .25f;
	float timeToWallUnstick;

	float gravity;
	float maxJumpVelocity;
	float minJumpVelocity;
	Vector3 velocity;
	float velocityXSmoothing;

	Controller2D controller;

	Vector2 directionalInput;
	bool wallSliding;
    public bool wallJumping = false;
	int wallDirX;

	void Start() {
		controller = GetComponent<Controller2D> ();
        stamina = maxStamina;
        health = maxHealth;

        staminaRect = new Rect(Screen.width * 0.1F, Screen.height * 0.2F, Screen.width / 3, Screen.height / 50);
        staminaTexture = new Texture2D(1, 1);
        staminaTexture.SetPixel(1, 1, Color.green);
        staminaTexture.Apply();

        healthRect = new Rect(Screen.width * 0.1F, Screen.height * 0.15F, Screen.width / 3, Screen.height / 50);
        healthTexture = new Texture2D(1, 1);
        healthTexture.SetPixel(1, 1, Color.red);
        healthTexture.Apply();

        debugRect = new Rect(Screen.width * 0.1F, Screen.height * 0.175F, Screen.width / 3, Screen.height / 50);
        debugTexture = new Texture2D(1, 1);
        debugTexture.SetPixel(1, 1, Color.cyan);
        debugTexture.Apply();

        gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpHeight);
	}

	void Update() {
        HandleWallSliding();
        HandleDodging();
        HandleRunning();
        CalculateVelocity ();

        isFacingLeft = (velocity.x > 0) ? false : true;

        controller.Move (velocity * Time.deltaTime, directionalInput);

		if (controller.collisions.above || controller.collisions.below) {
            wallJumping = false;
			if (controller.collisions.slidingDownMaxSlope) {
				velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
			} else {
				velocity.y = 0;
			}
		}
	}

	public void SetDirectionalInput (Vector2 input) {
        if (!isDodging) {
            directionalInput = input;
        }
	}

	public void OnJumpInputDown() {
		if (wallSliding) {
			if (wallDirX == directionalInput.x) {
				velocity.x = -wallDirX * wallJumpClimb.x;
				velocity.y = wallJumpClimb.y;
			}
			else if (directionalInput.x == 0) {
				velocity.x = -wallDirX * wallJumpOff.x;
				velocity.y = wallJumpOff.y;
			}
			else {
				velocity.x = -wallDirX * wallLeap.x;
				velocity.y = wallLeap.y;
			}
            drainStamina(1);
            wallJumping = true;
        }
		if (controller.collisions.below) {
			if (controller.collisions.slidingDownMaxSlope) {
				if (directionalInput.x != -Mathf.Sign (controller.collisions.slopeNormal.x)) { // not jumping against max slope
					velocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
                    velocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;
				}
			} else {
				velocity.y = maxJumpVelocity;
			}
        }

	}

    public void drainStamina(float deduction) {
        stamina -= deduction;
        if (stamina < 0) {
            stamina = 0;
        }
    }


	public void OnJumpInputUp() {
		if (velocity.y > minJumpVelocity) {
			velocity.y = minJumpVelocity;
		}
	}
		

	void HandleWallSliding() {
		wallDirX = (controller.collisions.left) ? -1 : 1;
		wallSliding = false;
		if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0) {
			wallSliding = true;

			if (velocity.y < -wallSlideSpeedMax) {
				velocity.y = -wallSlideSpeedMax;
			}

			if (timeToWallUnstick > 0) {
				velocityXSmoothing = 0;
				velocity.x = 0;

				if (directionalInput.x != wallDirX && directionalInput.x != 0) {
					timeToWallUnstick -= Time.deltaTime;
				}
				else {
					timeToWallUnstick = wallStickTime;
				}
			}
			else {
				timeToWallUnstick = wallStickTime;
			}

		}

	}

    void HandleRunning() {
        if (isRunning) {
            drainStamina(Time.deltaTime);
            if (stamina == 0) {
                isRunning = false;
            }
        } else if (stamina < maxStamina && !isTryingToRun) {
            stamina += Time.deltaTime;
        }
    }

    void HandleDodging() {
        if (isDodging) {
            currentDodgeTime += Time.deltaTime;
            Debug.Log(Time.deltaTime);
            if (currentDodgeTime > dodgeDuration) {
                isDodging = false;
                currentDodgeTime = 0;
            }
        }
    }

    public void startSprint ()
    {
        if (stamina > 0) {
            isRunning = true;
            isTryingToRun = true;
        }
    }

    public void endSprint ()
    {
        isRunning = false;
        isTryingToRun = false;
    }

    void CalculateVelocity() {
        float targetVelocityX;
        float currentAcceleration;
        if (isDodging && !wallJumping) {
            targetVelocityX = ((isFacingLeft) ? -1 : 1) * dodgeSpeed;
            currentAcceleration = accelerationTimeDodge;
        } else {
            if (isRunning && !wallJumping) {
                targetVelocityX = directionalInput.x * runSpeed;
            } else {
                targetVelocityX = directionalInput.x * moveSpeed;
            }
            currentAcceleration = (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne;
        }

        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, currentAcceleration);
        velocity.y += gravity * Time.deltaTime;
	}

    void OnGUI() {
        float staminaRatio = stamina / maxStamina;
        float staminaRectWidth = staminaRatio * Screen.width / 3;
        staminaRect.width = staminaRectWidth;
        GUI.DrawTexture(staminaRect, staminaTexture);

        float healthRatio = health / maxHealth;
        float healthRectWidth = healthRatio * Screen.width / 3;
        healthRect.width = healthRectWidth;
        GUI.DrawTexture(healthRect, healthTexture);

        float debugRatio = (dodgeDuration - currentDodgeTime) / dodgeDuration;
        Debug.Log("debugRatio = " + debugRatio);
        float debugRectWidth = debugRatio * Screen.width / 3;
        debugRect.width = debugRectWidth;
        GUI.DrawTexture(debugRect, debugTexture);
    }

    public void PerformDodge() {
        if (controller.collisions.below) {
            isDodging = true;
            drainStamina(1);
        }
    }
}
