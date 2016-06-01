using UnityEngine;
using System.Collections;

public class PlayerGroundCheck : PlayerScript
{
    //Tells us which layers are meant to be checked when looking for a ground.
    [SerializeField] private LayerMask playerCanStepOn;

    private void Start()
    {
        
    }

    public bool IsGrounded()
    {
        RaycastHit hit;
        //Do a capsule cast to get a more precise result
        Physics.CapsuleCast(transform.position, transform.position - new Vector3(0f, 0.1f, 0f), player.capsCollider.radius, Vector3.down, out hit, 1f, playerCanStepOn);

        TryBounce(hit);

        if (hit.collider)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void TryBounce(RaycastHit hit)
    {
        //If the player jumps on an enemy head, stuns the enemy and bounce the player off
        if (hit.collider && hit.collider.tag == "Enemy")
        {
            if (!player.rigidBody.isKinematic)
            {
                player.rigidBody.velocity = new Vector3(player.rigidBody.velocity.x, player.stats.GetJumpSpeed() * 0.6f, player.rigidBody.velocity.z); //bounce player
            }
            //hit.collider.GetComponentInParent<Unit>().Stun(2f); //stun enemies
            player.animationController.DoBounce();
        }
    }
}
