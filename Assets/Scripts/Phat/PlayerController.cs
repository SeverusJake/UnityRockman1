using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Component
    public SpriteRenderer SR;
    public Animator Anim;
    public Rigidbody2D RB;
    public BoxCollider2D BodyCollider;
    public BoxCollider2D GroundCollider;
    public BoxCollider2D HeadCollider;
    public string CurrentAnimation;

    //Physics
    public const float movementSpd = 81f;
    public const float jumpForce = 315f;
    public const float gravityForce = 15f;


    //Checking
    public bool isGrounded;
    public bool isContactWall;
    public bool isContactCeil;
    public bool isShoot;
    public bool isSliding;
    public LayerMask groundMask;


    //Animations States
    const string IDLE = "rockman_idle";
    const string SLIDE = "rockman_slide";
    const string JUMP = "rockman_air";
    const string STEP = "rockman_step";
    const string RUN = "rockman_run";
    const string IDLE_SHOOT = "rockman_idle_shoot";
    const string RUN_SHOOT = "rockman_run_shoot";
    const string JUMP_SHOOT = "rockman_air_shoot";

    //Timer
    int stepTimer = 0;
    int slidingTimer = 0;
    void Awake()
    {
        SR = GetComponent<SpriteRenderer>();
        Anim = GetComponent<Animator>();
        RB = GetComponent<Rigidbody2D>();
        BodyCollider = GetComponent<BoxCollider2D>();
        Application.targetFrameRate = 60;
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Get last frame velocity
        Vector2 velocity = RB.velocity;

        //Get input
        int inputX = Input.GetKey(KeyCode.RightArrow).GetHashCode() - Input.GetKey(KeyCode.LeftArrow).GetHashCode();
        float movementSpdModifier = movementSpd;

        //Flip player
        if (inputX != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(inputX);
            transform.localScale = scale;
        }

        if (Input.GetKeyDown(KeyCode.X) && !isShoot && !isSliding)
        {
            isShoot = true;
            switch (CurrentAnimation)
            {
                case IDLE:
                    ChangeAnimationState(IDLE_SHOOT);
                    break;

                case RUN:
                    ChangeAnimationState(RUN_SHOOT, Anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
                    break;

                case STEP:
                    ChangeAnimationState(RUN_SHOOT);
                    break;

                case JUMP:
                    ChangeAnimationState(JUMP_SHOOT);
                    break;
            }
            StartCoroutine(FinishShooting(0.4f));
        }

        //Logic
        if (isGrounded)
        {
            if (!isSliding)
            {
                //f you press jump key => add a jump speed to player
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    if (Input.GetKey(KeyCode.DownArrow))
                    {
                        isSliding = true;
                        isShoot = false;
                        StopCoroutine("FinishShooting");
                        ChangeAnimationState(SLIDE);
                    }
                    else
                    {
                        velocity.y = jumpForce;
                        if (!isShoot) ChangeAnimationState(JUMP);//Play jump animation
                        else ChangeAnimationState(JUMP_SHOOT);//Play jump animation
                    }
                }
                else
                if (inputX != 0 && !isContactWall)
                {
                    stepTimer++; //when holding right or left, set a timer 7 frame
                    if (stepTimer >= 7)
                    {
                        if (!isShoot) ChangeAnimationState(RUN);  //play run animation
                        else ChangeAnimationState(RUN_SHOOT); //Play run shoot animation
                    }
                    else
                    {
                        movementSpdModifier = 20f;
                        if (!isShoot) ChangeAnimationState(STEP); //play side step animation
                        else ChangeAnimationState(RUN_SHOOT); //Play run shoot animation
                    }
                }
                else
                {
                    stepTimer = 0;
                    if (!isShoot) ChangeAnimationState(IDLE); //Play idle animation
                    else ChangeAnimationState(IDLE_SHOOT); //Play idle shoot animation
                }
            }
            else
            {
                slidingTimer++;
                movementSpdModifier = movementSpd * 2;
                inputX = (int)Mathf.Sign(transform.localScale.x);
                if (slidingTimer >= 30 && !isContactCeil)
                {
                    isSliding = false;
                    slidingTimer = 0;
                    if (inputX != 0 && !isContactWall)
                    {
                        stepTimer = 7;
                        movementSpdModifier = movementSpd;
                    }
                }
                else
                if (Input.GetKeyDown(KeyCode.Z) && !isContactCeil)
                {
                    isSliding = false;
                    velocity.y = jumpForce;
                    ChangeAnimationState(JUMP);//Play jump animation                    
                }
            }
        }
        else
        {
            isSliding = false;
            slidingTimer = 0;
            //If you release the jump key => then player will fall down
            if ((!Input.GetKey(KeyCode.Z) || isContactCeil) && velocity.y >= 0)
            {
                velocity.y = 0;
            }
            if (!isShoot) ChangeAnimationState(JUMP);//Play jump animation
            else ChangeAnimationState(JUMP_SHOOT); //Play jump shoot animation
        }

        //Calculate horizontal velocity
        velocity.x = inputX * movementSpdModifier;

        //Gravity => the vertical speed is decreased per frame
        velocity.y = Mathf.Clamp(velocity.y - gravityForce, -jumpForce, jumpForce);

        //Apply velocity
        RB.velocity = velocity;
    }

    void FixedUpdate()
    {
        //Check if player hit ground
        isGrounded = Physics2D.BoxCast(GroundCollider.bounds.center, GroundCollider.bounds.size, 0, Vector2.down, 0.5f, groundMask);

        //Check if player hit ceil
        isContactCeil = Physics2D.BoxCast(HeadCollider.bounds.center, HeadCollider.bounds.size, 0, Vector2.up, 0.5f, groundMask);

        //Check if player hit wall
        isContactWall = Physics2D.BoxCast(BodyCollider.bounds.center, BodyCollider.bounds.size, 0, Vector2.right * transform.localScale.x, 0.5f, groundMask);
    }

    public void ChangeAnimationState(string newState, float time = 0)
    {
        if (CurrentAnimation == newState) return;

        Anim.Play(newState, -1, time);

        CurrentAnimation = newState;
    }

    IEnumerator FinishShooting(float time)
    {
        yield return new WaitForSeconds(time);
        switch (CurrentAnimation)
        {
            case IDLE_SHOOT:
                ChangeAnimationState(IDLE);
                break;

            case RUN_SHOOT:
                ChangeAnimationState(RUN, Anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
                break;

            case JUMP_SHOOT:
                ChangeAnimationState(JUMP);
                break;
        }
        isShoot = false;
    }
}
