using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Controller2D))]
public class Player : MonoBehaviour {

    float staminaThreshold = 1.0f;
    float staminaDampner = 0.5f;

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
    float currentWallJumpTime = 0;
    float wallJumpDuration = .1f;

    //Stamina bar
    public float stamina, maxStamina = 5;
    bool inStaminaPenalty = false;
    Rect staminaRect;
    Texture2D staminaTexture;

    //Health bar
    public float health, maxHealth = 5;
    Rect healthRect;
    Texture2D healthTexture;

    //Experience and Experience bar
    float initalExperienceToLevel = 500;
    float experienceNeededRate = 1.25f;
    public int currentExperience = 0;
    public int currentLevel = 1;
    int hardcapLevel = 99;
    int previousLevelExpNeeded = 0;
    int currentLevelExpNeeded;
    Rect expRect;
    Texture2D expTexture;

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

    private void Awake() {
    }

    void Start() {
        controller = GetComponent<Controller2D> ();
        stamina = maxStamina;
        health = maxHealth;

        ResetExperienceForNextLevel();

        healthRect = new Rect(Screen.width * 0.1F, Screen.height * 0.15F, Screen.width * 0.4F, Screen.height * 0.02F);
        healthTexture = new Texture2D(1, 1);
        healthTexture.SetPixel(1, 1, Color.red);
        healthTexture.Apply();

        debugRect = new Rect(Screen.width * 0.1F, Screen.height * 0.175F, Screen.width / 3, Screen.height * 0.02F);
        debugTexture = new Texture2D(1, 1);
        debugTexture.SetPixel(1, 1, Color.yellow);
        debugTexture.Apply();

        staminaRect = new Rect(Screen.width * 0.1F, Screen.height * 0.2F, Screen.width * 0.3F, Screen.height * 0.02F);
        staminaTexture = new Texture2D(1, 1);
        staminaTexture.SetPixel(1, 1, Color.green);
        staminaTexture.Apply();

        expRect = new Rect(Screen.width * 0.1F, Screen.height * 0.225F, Screen.width * 0.3F, Screen.height * 0.01F);
        expTexture = new Texture2D(1, 1);
        expTexture.SetPixel(1, 1, Color.cyan);
        expTexture.Apply();

        gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpHeight);
    }
 
    void Update() {
        CalculateFacingDirection();
        HandleWallSliding();
        HandleDodging();
        HandleSprinting();
        HandleWallJumping();
        CalculateVelocity ();
        rechargeStamina();

        controller.Move (velocity * Time.deltaTime, directionalInput);

        if (controller.collisions.above || controller.collisions.below) {
            if (controller.collisions.slidingDownMaxSlope) {
                velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
            } else {
                velocity.y = 0;
            }
        }
    }

    private void LateUpdate() {
    }

    public void SetDirectionalInput (Vector2 input) {
        directionalInput = input;
    }

    public void OnJumpInputDown() {
        if (wallSliding && !inStaminaPenalty) {
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
            inStaminaPenalty = true;
            staminaTexture.SetPixel(1, 1, Color.white);
            staminaTexture.Apply();
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
    }

    //Chris
    public void addExperience(int expPoints) {
        currentExperience += expPoints;
        if (currentExperience >= currentLevelExpNeeded && currentLevel <= hardcapLevel) {
            currentLevel++;
            ResetExperienceForNextLevel();
        }
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
        isSprinting = isTryingToSprint && stamina > 0 && !inStaminaPenalty;
        if ( isSprinting && directionalInput.x != 0) { 
            drainStamina(Time.deltaTime);
        }
    }

    void rechargeStamina() {
        if (inStaminaPenalty && stamina >= staminaThreshold) {
            inStaminaPenalty = false;
            staminaTexture.SetPixel(1, 1, Color.green);
            staminaTexture.Apply();
        }
        if (stamina < maxStamina) {
            if (inStaminaPenalty) {
                stamina += Time.deltaTime * staminaDampner;
            } else if (velocity.x == 0 || !isTryingToSprint) {
                stamina += Time.deltaTime;
            }
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

    void ResetExperienceForNextLevel() {
        previousLevelExpNeeded = (currentLevel - 1 == 0) ? 0 : currentLevelExpNeeded;
        currentLevelExpNeeded = Mathf.RoundToInt(Mathf.Pow(currentLevel, experienceNeededRate) * initalExperienceToLevel);
    }

    public void PerformDodge() {
        if (controller.collisions.below && !inStaminaPenalty) {
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
        float healthRatio = health / maxHealth;
        float healthRectWidth = healthRatio * Screen.width / 3;
        healthRect.width = healthRectWidth;
        GUI.DrawTexture(healthRect, healthTexture);

        float debugRatio = timeToWallUnstick / wallStickTime;
        float debugRectWidth = debugRatio * Screen.width / 3;
        debugRect.width = debugRectWidth;
        GUI.DrawTexture(debugRect, debugTexture);

        float staminaRatio = stamina / maxStamina;
        float staminaRectWidth = staminaRatio * Screen.width / 3;
        staminaRect.width = staminaRectWidth;
        GUI.DrawTexture(staminaRect, staminaTexture);

        float expRatio = (float)(currentExperience - previousLevelExpNeeded) / (float)(currentLevelExpNeeded - previousLevelExpNeeded);
        float expRectWidth = expRatio * Screen.width / 3;
        expRect.width = expRectWidth;
        GUI.DrawTexture(expRect, expTexture);
    }
}
