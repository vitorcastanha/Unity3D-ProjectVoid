using UnityEngine;
using System.Collections;

public class PlayerAnimationController : PlayerScript {

    private Animator anim;
    private AnimatorStateInfo info;
    public enum StateInfo
    {
        SHIELD_ATTACK,
        SWING_01,
        SWING_02,
        COMBO_FINISHER,
        AIR_ATTACK,
        SPECIAL_ATTACK,
        DASH_ATTACK,
        CHARGE,
        JUMP,
        LANDING,
        MOVEMENT_TREE,
        HURT_01,
        HURT_02,
        HURT_03
    }

    private const float bounceSpeed = 3f;

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
    /// Sets the jumping animation state.
    /// </summary>
    /// <param name="bJumping">If set to <c>true</c> if jumping.</param>
    public void SetJumping(bool jumping)
    {
        anim.SetBool("bJumping", jumping);
    }

    /// <summary>
    /// Sets the movement speed.
    /// </summary>
    /// <param name="moveVectorMagnitude">Move vector magnitude.</param>
    public void SetMovementSpeed(float moveVectorMagnitude)
    {
        anim.SetFloat ("fMovementSpeed", moveVectorMagnitude);
    }

    /// <summary>
    /// Sets the shield block state.
    /// </summary>
    /// <param name="shieldBlock">If set to <c>true</c> shield block.</param>
    public void SetShieldBlock(bool shieldBlock)
    {
        anim.SetBool("bShieldActive", shieldBlock);
    }

    /// <summary>
    /// Sets the animation trigger for swing01.
    /// </summary>
    public void SetSwing01()
    {
        anim.SetTrigger("tAttack1");
    }

    /// <summary>
    /// Forces to play the swing01 animation.
    /// </summary>
    public void ForcePlaySwing01()
    {
        anim.Play("UlfarAttack01", -1, 0f);
    }

    /// <summary>
    /// Sets the animation trigger for swing02.
    /// </summary>
    public void SetSwing02()
    {
        anim.SetTrigger("tAttack2");
    }

    /// <summary>
    /// Sets the animation trigger for combo finisher.
    /// </summary>
    public void SetComboFinisher()
    {
        anim.SetTrigger("tAttack3");
    }

    /// <summary>
    /// Sets the animation trigger for special attack.
    /// </summary>
    public void SetSpecialAttack()
    {
        anim.SetTrigger("tAttackStomp");
    }

    /// <summary>
    /// Sets the animation trigger for dash attack.
    /// </summary>
    public void SetDashAttack()
    {
        anim.SetTrigger("tDashAttack");
    }

    /// <summary>
    /// Sets the air attack.
    /// </summary>
    public void SetAirAttack()
    {
        anim.SetTrigger("tAttackAir");
    }

    public void SetPickUpSword(bool pickingUpSword)
    {
        anim.SetBool("bAmPickingTheSword", pickingUpSword);
    }

    /// <summary>
    /// Does the bounce.
    /// </summary>
    public void DoBounce()
    {
        anim.SetTrigger("tStomping"); //trigger stomping animation to blend from "Land"
        anim.speed = bounceSpeed; //increase speed of land to adjust for the action urgency
        anim.Play("Land"); //play land animation when the player hits the enemy head
    }

    /// <summary>
    /// Compares the state of the animation.
    /// </summary>
    /// <returns><c>true</c>, if animation state matched, <c>false</c> otherwise.</returns>
    /// <param name="state">State.</param>
    public bool CompareAnimationState(StateInfo state)
    {
        AnimatorStateInfo currentState = anim.GetCurrentAnimatorStateInfo (0); //tracks animation state

        switch (state)
        {
            case StateInfo.SHIELD_ATTACK:
                if ((currentState.fullPathHash == Animator.StringToHash("Base Layer.Shield") ||
                    currentState.fullPathHash == Animator.StringToHash("Base Layer.Shield Move Tree") ||
                    currentState.fullPathHash == Animator.StringToHash("Base Layer.UlfarShieldBounce"))) 
                {
                    return true;
                }
                break;

            case StateInfo.SWING_01:
                if (currentState.fullPathHash == Animator.StringToHash("Base Layer.UlfarAttack01"))
                    return true;

                break;

            case StateInfo.SWING_02:
                if (currentState.fullPathHash == Animator.StringToHash("Base Layer.UlfarAttack02"))
                    return true;

                break;
            case StateInfo.COMBO_FINISHER:
                if (currentState.fullPathHash == Animator.StringToHash("Base Layer.Finisher"))
                    return true;

                break;

            case StateInfo.CHARGE:
                if(currentState.fullPathHash == Animator.StringToHash("Base Layer.ChargeAttack"))
                    return true;

                break;

            case StateInfo.SPECIAL_ATTACK:
                if (currentState.fullPathHash == Animator.StringToHash("Base Layer.UlfarStomp"))
                    return true;

                break;

            case StateInfo.DASH_ATTACK:
                if (currentState.fullPathHash == Animator.StringToHash("Base Layer.DashAttack")
                    || currentState.fullPathHash == Animator.StringToHash("Base Layer.DashLand"))
                {
                    return true;
                }
                break;

            case StateInfo.AIR_ATTACK:
                if (currentState.fullPathHash == Animator.StringToHash("Base Layer.AirAttack")
                    || currentState.fullPathHash == Animator.StringToHash("Base Layer.AirAttack2"))
                {
                    return true;
                }
                break;

            case StateInfo.JUMP:
                if (currentState.fullPathHash == Animator.StringToHash("Base Layer.UlfarJump"))
                    return true;

                break;

            case StateInfo.LANDING:
                if (currentState.fullPathHash == Animator.StringToHash("Base Layer.Land"))
                    return true;

                break;

            case StateInfo.HURT_01:
                if (currentState.fullPathHash == Animator.StringToHash("Base Layer.Hurt01"))
                    return true;

                break;

            case StateInfo.HURT_02:
                if (currentState.fullPathHash == Animator.StringToHash("Base Layer.Hurt02"))
                    return true;

                break;

            case StateInfo.HURT_03:
                if (currentState.fullPathHash == Animator.StringToHash("Base Layer.Hurt03"))
                    return true;

                break;

            case StateInfo.MOVEMENT_TREE:
                if (currentState.fullPathHash == Animator.StringToHash("Base Layer.Move Tree"))
                    return true;

                break;

            default:
                break;
        }
        return false;   
    }
        
}
