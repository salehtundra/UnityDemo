using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

    // Jumping
	public float maxJumpHeight = 4;
	public float minJumpHeight = 1;
	public float timeToJumpApex = .4f;
	public float accelerationTimeAirborne = .2f;
    bool isJumping = false;
    bool isTryingToJump = false;
    float maxJumpVelocity;
	float minJumpVelocity;

    // Sprint/Walking  
	public float accelerationTimeGrounded = .1f;
    public float moveSpeed = 6;
    public float sprintSpeed = 12;
    bool isSprinting;
    bool isTryingToSprint;   

    //Dodge logic
    float dodgeSpeed = 20;
    float currentDodgeTime = 0;
    float dodgeDuration = 0.20f;
    float accelerationTimeDodge = .0001f;
    bool isDodging;
    bool isFacingLeft;

        // Wall Jump
	public Vector2 wallLeap;
	public float wallSlideSpeedMax = 0;
	float wallStickTime = .1f;
	float timeToWallUnstick;
    bool wallSliding;
    public bool wallJumping = false;
	int wallDirX;
    //---------------------
    float currentWallJumpTime = 0;
    float wallJumpDuration = .1f;

    //Stamina bar
    public float stamina, maxStamina = 5;
    Rect staminaRect;
    Texture2D staminaTexture;

    //Health bar
    public float health, maxHealth = 5;
    Rect healthRect;
    Texture2D healthTexture;

    // Debug Bar 
    Rect debugRect;
    Texture2D debugTexture;

    // Misc
    float gravity;
	Vector3 velocity;
	float velocityXSmoothing;

	Controller2D controller;
	Vector2 directionalInput;


    public void SetIsTryingToSprint(bool sprintValue) {
        isTryingToSprint = sprintValue;
    }

    void Start() {
		controller = GetComponent<Controller2D> ();
        stamina = maxStamina;
        health = maxHealth;

        staminaRect = new Rect(Screen.width * 0.1F, Screen.height * 0.2F, Screen.width * 0.3F, Screen.height * 0.02F);
        staminaTexture = new Texture2D(1, 1);
        staminaTexture.SetPixel(1, 1, Color.green);
        staminaTexture.Apply();

        healthRect = new Rect(Screen.width * 0.1F, Screen.height * 0.15F, Screen.width * 0.4F, Screen.height * 0.02F);
        healthTexture = new Texture2D(1, 1);
        healthTexture.SetPixel(1, 1, Color.red);
        healthTexture.Apply();

        debugRect = new Rect(Screen.width * 0.1F, Screen.height * 0.175F, Screen.width / 3, Screen.height * 0.02F);
        debugTexture = new Texture2D(1, 1);
        debugTexture.SetPixel(1, 1, Color.cyan);
        debugTexture.Apply();

        gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
		maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpHeight);
	}
 
	void Update() {
        // Clint - abstracted into a function (with a shitty name) and moved to 
        // the beginning of update
        CalculateFacingDirection();
        HandleWallSliding();
        HandleDodging();
        HandleSprinting();
        HandleWallJumping();
        CalculateVelocity ();

        controller.Move (velocity * Time.deltaTime, directionalInput);

		if (controller.collisions.above || controller.collisions.below) {
            //wallJumping = false;
			if (controller.collisions.slidingDownMaxSlope) {
				velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
			} else {
				velocity.y = 0;
			}
		}
	}

	public void SetDirectionalInput (Vector2 input) {
        // float deadZone = 0.3F;

        // Clint - Not necassary anymore - we don't use directional input when we are dodging
        // if (!isDodging) {
        directionalInput = input;
            // if (directionalInput.x < deadZone  && directionalInput.x > -deadZone) {
            //     directionalInput.x = 0F;
            // }
            // if (directionalInput.y < deadZone && directionalInput.y > -deadZone) {
            //     directionalInput.y = 0F;
            //}

        // }
	}

	public void OnJumpInputDown() {
		if (wallSliding) {
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

    public void OnJumpInputUp() {
        if (velocity.y > minJumpVelocity) {
            velocity.y = minJumpVelocity;
        }
    }

    public void drainStamina(float deduction) {
        stamina -= deduction;
        if (stamina < 0) {
            stamina = 0;
        }
    }
		
    // Clint 
    void CalculateFacingDirection() {

        // isFacingLeft = (velocity.x > 0) ? false : true;

        if (directionalInput.x > 0) {
             isFacingLeft = false;
        } else if (directionalInput.x < 0) {
             isFacingLeft = true;
        }
        Debug.Log("Is Facing Left: " + isFacingLeft);
        Debug.Log("Directionl Inp: " + directionalInput.x);
    }
   
	void HandleWallSliding() {
        wallSliding = false;
        if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0) {
            wallDirX = (controller.collisions.left) ? -1 : 1;
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

    void HandleSprinting() {
        isSprinting = isTryingToSprint && stamina > 0;
        if (velocity.x != 0 && isSprinting) {
            drainStamina(Time.deltaTime);
        } else if (stamina < maxStamina && (velocity.x == 0 || !isTryingToSprint)) {
            stamina += Time.deltaTime;
        }
    }

    void HandleDodging() {
        if (isDodging) {
            currentDodgeTime += Time.deltaTime;
            if (currentDodgeTime > dodgeDuration) {
                isDodging = false;
                currentDodgeTime = 0;
            }
        }
    }

    void HandleWallJumping() {
        if(currentWallJumpTime > wallJumpDuration) {
            wallJumping = false;
            currentWallJumpTime = 0f;

        } else if (wallJumping) {
            currentWallJumpTime += Time.deltaTime;
        }

    }

    public void PerformDodge() {
        if (controller.collisions.below) {
            isDodging = true;
            drainStamina(1);
        }
    }

    void CalculateVelocity() {
        float targetVelocityX;
        float currentAcceleration;
        if (wallJumping) {
            velocity.x = -wallDirX * wallLeap.x;
            velocity.y = wallLeap.y;
        } else {
            if (isDodging && !wallJumping) {
                targetVelocityX = ((isFacingLeft) ? -1 : 1) * dodgeSpeed;
                currentAcceleration = accelerationTimeDodge;
            } else if (isSprinting && !wallJumping) {
                targetVelocityX = directionalInput.x * sprintSpeed;
                currentAcceleration = (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne;
            } else {
                targetVelocityX = directionalInput.x * moveSpeed;
                currentAcceleration = (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne;
            }

            if ((wallSliding && timeToWallUnstick <= 0) || !wallSliding) {
                velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, currentAcceleration);
            }
            velocity.y += gravity * Time.deltaTime;
        }
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

        float debugRatio = timeToWallUnstick / wallStickTime;
        float debugRectWidth = debugRatio * Screen.width / 3;
        debugRect.width = debugRectWidth;
        GUI.DrawTexture(debugRect, debugTexture);
    }
}
