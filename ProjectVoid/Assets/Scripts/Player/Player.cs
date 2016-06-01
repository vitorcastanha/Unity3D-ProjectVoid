using UnityEngine;
using System.Collections;

[RequireComponent (typeof(PlayerGroundCheck))]
[RequireComponent (typeof(PlayerStats))]
[RequireComponent (typeof(PlayerAnimationController))]
[RequireComponent (typeof(PlayerMovement))]
[RequireComponent (typeof(Rigidbody))]
[RequireComponent (typeof(CapsuleCollider))]
[RequireComponent (typeof(PlayerCombat))]

public class Player : MonoBehaviour {

    /*
     * This script is required by all player controller scripts.
     * It connects all controllers.
     * 
     * i.e. player.stats.GetJumpSpeed();
     *      player.groundCheck.IsGrounded();
     *      player.rigidBody;
     *      player.animationController.SetJump(false);
    */

    [HideInInspector] public PlayerGroundCheck groundCheck;
    [HideInInspector] public PlayerStats stats;
    [HideInInspector] public PlayerAnimationController animationController;
    [HideInInspector] public PlayerMovement movement;
    [HideInInspector] public Rigidbody rigidBody;
    [HideInInspector] public CapsuleCollider capsCollider;
    [HideInInspector] public PlayerCombat combat;
    public static Player instance;

    private void OnEnbled()
    {
        //This script should not be displayed in Inspector to avoid confusion to the designer.
        this.hideFlags = HideFlags.HideInInspector;
    }

    //Makes sure it gets the referrence for all controllers before they Start()
    private void Awake()
    {
        groundCheck = GetComponent<PlayerGroundCheck>();
        stats = GetComponent<PlayerStats>();
        animationController = GetComponent<PlayerAnimationController>();
        movement = GetComponent<PlayerMovement>();
        rigidBody = GetComponent<Rigidbody>();
        capsCollider = GetComponent<CapsuleCollider>();
        combat = GetComponent<PlayerCombat>();
        instance = this;
    }
        
}
