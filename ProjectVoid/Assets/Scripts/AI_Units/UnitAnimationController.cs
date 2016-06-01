using UnityEngine;
using System.Collections;

public class UnitAnimationController : UnitScript {

    private Animator anim;

	private void Start () 
    {
        anim = GetComponentInChildren<Animator>();
	}
	
    private void Update () 
    {
	
	}

    /// <summary>
    /// Sets the animator active.
    /// </summary>
    /// <param name="enable">If set to <c>true</c> enable.</param>
    public void SetAnimatorActive(bool enable)
    {
        anim.enabled = enable;
    }

    /// <summary>
    /// Sets the animation speed.
    /// </summary>
    /// <param name="newSpeed">New speed.</param>
    public void SetAnimationSpeed(float newSpeed)
    {
        anim.speed = newSpeed;
    }

    /// <summary>
    /// Forces to play hurt animation.
    /// </summary>
    public void ForcePlayHurt()
    {
        anim.Play("Hurt", -1, 0f);
    }

    /// <summary>
    /// Sets the stun.
    /// </summary>
    /// <param name="isStunned">If set to <c>true</c> is stunned.</param>
    public void SetStun(bool isStunned)
    {
        anim.SetBool("stunned", isStunned);
    }

    /// <summary>
    /// Sets the movement speed.
    /// </summary>
    /// <param name="moveVectorMagnitude">Move vector magnitude.</param>
    public void SetMovementSpeed(float moveVectorMagnitude)
    {
        anim.SetFloat ("speed", moveVectorMagnitude);
    }

    public void ForcePlayAttack()
    {
        anim.Play("Attack");
    }
}
