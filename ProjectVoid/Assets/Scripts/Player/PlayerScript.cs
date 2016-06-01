using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Player))]
public class PlayerScript : MonoBehaviour {

    /*
     * This script is meant to be the parent of any player controller script.
     * This way all of them will have access to player, which then have access
     * to all other controllers.
     * i.e. player.stats.GetJumpSpeed();
     *      player.groundCheck.IsGrounded();
     *      player.rigidBody;
     *      player.animationController.SetJump(false);
     * 
     * Also every child of this script will require the Player component.
     * **Player component is hidden from the Inspector as it has no use there.
    */

    //references Player script which can access all controllers
    protected Player player;

    //this creates the enum to be used by all childs. The variable is handled by PlayerStats.
    public enum PlayerState
    {
        FREE,
        STUNNED,
        DEAD
    }

    //Makes sure this runs before all Start functions
    private void Awake()
    {
        player = GetComponent<Player>();
    }
}
