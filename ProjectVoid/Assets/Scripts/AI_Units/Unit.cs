using UnityEngine;
using System.Collections;

[RequireComponent (typeof(CapsuleCollider))]
[RequireComponent (typeof(NavMeshAgent))]
[RequireComponent (typeof(Rigidbody))]
[RequireComponent (typeof(UnitMovement))]
[RequireComponent (typeof(UnitStats))]
[RequireComponent (typeof(UnitAnimationController))]

public class Unit : MonoBehaviour {

    protected CapsuleCollider capsCollider;
    protected NavMeshAgent agent;
    protected Rigidbody rigidBody;
    protected Rigidbody rRagdoll;
    [HideInInspector] public UnitMovement movement;
    [HideInInspector] public UnitStats stats;
    [HideInInspector] public UnitAnimationController animationController;
    protected float fDistanceFromPlayer;                                        //Stores how far the enemy is from player at all times
    protected Transform playerTransform;                                        //Allows child scripts to reference player transform without needing to access the static instance for it


    private void Awake ()                                                       //Makes sure it gets the referrence for all controllers before they Start()
    {
        agent = GetComponent<NavMeshAgent>();
        movement = GetComponent<UnitMovement>();
        stats = GetComponent<UnitStats>();
        rigidBody = GetComponent<Rigidbody>();
        animationController = GetComponent<UnitAnimationController>();

        //Find the ragdoll in the child ignoring the rigidBody from the object itself
        Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rbs)
        {
            if (rb != rigidBody)
            {
                rRagdoll = rb;
            }
        }
	}

    protected virtual void Start()
    {
        //It's important to do this after Player's Awake has been runned.
        playerTransform = Player.instance.transform;
    }
	
	protected virtual void Update () 
    {
        fDistanceFromPlayer = Vector3.Distance (transform.position, Player.instance.transform.position);

        //Counts down the stun duration. There is no problem if the duration is a negative number.
        //So always counting down is more optimized than encapsulating it in a conditional statement.
        StunRecover();
        Think();
	}

    protected virtual void FixedUpdate()
    {
        
    }

    public void EnableRagdoll()
    {
        agent.enabled = false;
        animationController.SetAnimatorActive(false);
        capsCollider.enabled = false;
        rRagdoll.isKinematic = false;
    }

    /// <summary>
    /// Gets the distance from player.
    /// </summary>
    /// <returns>The distance from player.</returns>
    public float GetDistanceFromPlayer()
    {
        return fDistanceFromPlayer;
    }

    /// <summary>
    /// Gets the NavMeshAgent.
    /// </summary>
    /// <returns>The agent.</returns>
    public NavMeshAgent GetAgent()
    {
        return agent;
    }

    protected virtual void Think()
    {
        //THINK CODE GOES HERE    
    }

    /// <summary>
    /// Takes the damage.
    /// </summary>
    /// <param name="damage">Damage.</param>
    /// <param name="forceDirection">Force direction. Direction from where the attack came from to the enemy.</param>
    /// <param name="breakBlockStance">If set to <c>true</c> break block stance.</param>
    /// <param name="stopCounter">If set to <c>true</c> stop counter.</param>
    /// <param name="isHoming">If set to <c>true</c> is homing.</param>
    public virtual void TakeDamage(float damage, Vector3 forceDirection, bool breakBlockStance = false, bool stopCounter = false, 
                                   bool isHoming = false)
    {
        //TAKE DAMAGE CODE
    }

    /// <summary>
    /// Knockback the enemy.
    /// </summary>
    /// <param name="dir">Dir. Direction that the enemy will be pushed to.</param>
    /// <param name="fForceDiscount">Force discount. Reduces the knockback force.</param>
    /// <param name="bIgnoreBlock">If set to <c>true</c> b ignore block.</param>
    public virtual void Knockback(Vector3 dir, float fForceDiscount = 0f, bool bIgnoreBlock = false)
    {
        //KNOCK BACK CODE
    }

    /// <summary>
    /// Stun the enemy.
    /// </summary>
    /// <param name="stunDuration">Stun duration.</param>
    public virtual void Stun(float stunDuration)
    {
        stats.SetStunned(true, stunDuration);
    }

    //Counts down the stun duration.
    protected void StunRecover()
    {
        stats.AddStunDuration(-Time.deltaTime);
    }

    /// <summary>
    /// Player's position with offset.
    /// </summary>
    /// <returns>The position with offset.</returns>
    /// <param name="offset">Offset.</param>
    public Vector3 PlayerPositionWithOffset(float offset)
    {
        //how far away from the player the enemy will be before stop following him and switching to an attack stance
        return playerTransform.position - ((playerTransform.position - transform.position).normalized * offset);
    }

    /// <summary>
    /// Looks for player.
    /// </summary>
    /// <returns><c>true</c>, if for player was found, <c>false</c> otherwise.</returns>
    public bool LookForPlayer()
    {

        Vector3 enemyToPlayer = playerTransform.position - transform.position;
        RaycastHit hit;

        if (GetDistanceFromPlayer() < stats.GetSightRange())
        {
            #if UNITY_EDITOR
            Debug.DrawRay(transform.position, enemyToPlayer * stats.GetSightRange(), Color.red);
            #endif

            if (Physics.Raycast(transform.position, enemyToPlayer, out hit, stats.GetSightRange(), stats.GetIgnoreLayerInSightCheck()))
            {
                if (hit.collider.tag == "Player")
                {
                    //check if player is in front of the enemy, then raycast to see if there is line of sight to the player.
                    if (Vector3.Dot(enemyToPlayer.normalized, transform.forward.normalized) > 0.1f && hit.collider.tag == "Player")
                    {
                        //PLAYER HAS BEEN SPOTTED!
                        return true;
                    }
                }

            }
        }
        //the enemy can hear in a 360 degree around him so it will find the player without having to check for line of sight
        if (GetDistanceFromPlayer() < stats.GetListeningRange())
        {
            return true;
        }
        return false;
    }
}
