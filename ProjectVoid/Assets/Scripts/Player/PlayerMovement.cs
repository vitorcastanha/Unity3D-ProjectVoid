using UnityEngine;
using System.Collections;

public class PlayerMovement : PlayerScript
{

    [SerializeField][Range (0, 1)] private float fAirControl = 0.5f;
    private Vector3 v3Move;
    private float fTurnAmount;
    private float fForwardAmount;

    private void Start()
    {
        
    }

    private void Update()
    {
        //JUMP
        if (Input.GetButtonDown("Jump") && player.groundCheck.IsGrounded())
        {
            if (!player.rigidBody.isKinematic)
            {
                player.rigidBody.velocity += Vector3.up * player.stats.GetJumpSpeed();
            }

            //Delays jump animation to avoid conflict with groundCheck (See SetJumpingTrue declaration)
            if (!IsInvoking("SetJumpingTrue"))
            {
                Invoke("SetJumpingTrue", 0.1f);
            }
        }
    }

    private void FixedUpdate()
    {
        MovementController();
    }

    //This function exists so we can delay the animation adjustment to jump in order to avoid
    //conflict with the groundCheck logic, giving the gameObject time to properly leave ground.
    //When groundCheck sees that the player is grounded, it will stop jump animation.
    private void SetJumpingTrue()
    {
        player.animationController.SetJumping(true);
    }

    /// <summary>
    /// Implements the player inputs regarding movement.
    /// </summary>
    private void MovementController()
    {

        if (player.groundCheck.IsGrounded())
        {
            player.animationController.SetJumping(false);
        }

        //from here on player always needs to be able to move.
        if (!player.stats.CanMove())
        {
            return;
        }

        //if movement is enabled
        Movement();

        //Check if air attack is being used in order to do the dive movement
        //CheckForAirAttack();

        if (!player.rigidBody.isKinematic)
        {
            player.rigidBody.velocity = new Vector3(v3Move.x, player.rigidBody.velocity.y, v3Move.z);
        }

    }
        
    /// <summary>
    /// Moves this instance according to inputs and camera position.
    /// </summary>
    private void Movement ()
    {
        float fVerticalMovement = Input.GetAxis("Vertical");
        float fHorizontalMovement = Input.GetAxis("Horizontal");
        bool input = (fVerticalMovement != 0 && fHorizontalMovement != 0) ? true : false;

        //get camera direction
        Vector3 camForward = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;

        //walk relative to camera direction
        Vector3 move = fVerticalMovement * camForward + fHorizontalMovement * Camera.main.transform.right;
        move *= player.stats.GetRunSpeed();

        #if UNITY_EDITOR
        CheckAirControlValue();
        #endif

        if (!player.groundCheck.IsGrounded())
        {
            float momentum = 1 - fAirControl;
            v3Move = (v3Move * momentum) + (move * fAirControl);
        }
        else
        {
            v3Move = move;
        }


        //magnitude ignores vector direction
        player.animationController.SetMovementSpeed(v3Move.magnitude);


        ConvertMoveInput(input);
        ApplyExtraTurnRotation ();
    }

    #if UNITY_EDITOR
    private void CheckAirControlValue()
    {
        //fAirControl refers to the % of control the player has in the air. 1 = 100%
        if (fAirControl > 1)
        {
            fAirControl = 1;
        }
        else if (fAirControl < 0)
        {
            fAirControl = 0;
        }
    }
    #endif

    // converts the relative move vector into local turn & fwd values
    private void ConvertMoveInput(bool input)
    {
        // convert the world relative moveInput vector into a local-relative
        // turn amount and forward amount required to head in the desired
        // direction. 
        Vector3 localMove = transform.InverseTransformDirection(v3Move);
        fTurnAmount = Mathf.Atan2(localMove.x, localMove.z);

        //fix bug where character will snap to certain rotation.
        if (fTurnAmount == Mathf.PI && !input)
            fTurnAmount = 0f;

        fForwardAmount = localMove.z;
    }

    private void ApplyExtraTurnRotation()
    {
        // help the character turn faster (this is in addition to root rotation in the animation)
        float turnSpeed = Mathf.Lerp(player.stats.GetStationaryTurnSpeed(), player.stats.GetMovingTurnSpeed(),
            fForwardAmount);
        transform.Rotate(0, fTurnAmount*turnSpeed*Time.deltaTime, 0);
    }

}
