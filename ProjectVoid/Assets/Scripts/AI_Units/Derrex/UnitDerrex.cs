using UnityEngine;
using System.Collections;

public class UnitDerrex : Unit {

    private const float fRagdollInitSpeed = 170f;

    private enum EnemyState{chasing, patrolling, fleeing, charging, dead, attacking}; 
    private EnemyState eState;

    [System.Serializable]
    public class DerrexCombat
    {
        public GameObject goBullet;                         //Projectile attack particle
        public Transform goBulletSpawnPoint;
        public GameObject goCleanUpParticle;                //Spawns when destroying enemy corpse.
        public float fCleanUpTime = 3f;                     //How long before cleaning up the corpse from scene.

        public float fRateOfFire = 1.5f;
        [HideInInspector] public float fRateOfFireAux;      //Counter for the rate of fire.
    }
    [SerializeField] private DerrexCombat derrexCombat;

    private bool bFleeingAux;                               //Returns if this instance is currently fleeing.

    protected override void Start()
    {
        base.Start();
        //Initializes max life value
        stats.SetCurrentLife(stats.GetMaxLife());

        agent.speed = stats.GetWalkingSpeed();
        agent.angularSpeed = stats.GetTurnSpeed();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        EnemyStateController();     //Controls the enemy state based on players actions

        animationController.SetMovementSpeed(agent.velocity.magnitude);
    }

    public override void TakeDamage(float damage, Vector3 forceDirection, bool breakBlockStance = false, bool stopCounter = false, bool isHoming = false)
    {
        base.TakeDamage(damage, forceDirection, breakBlockStance, stopCounter, isHoming);
        //fEngagementClock = 0;

        if (stats.GetCurrentLife() > 0) {
            animationController.ForcePlayHurt();
            GameObject clone;
            /*clone = Instantiate(goEnemyBlood, transform.position, transform.rotation) as GameObject;
            clone.transform.LookAt(Player.instance.transform.position);*/
        }

        stats.AddLife(-damage);

        if (stats.GetCurrentLife() <= 0) 
        {
            Die(forceDirection);
            return;
        }
    }

    //Controls the enemy state based on players actions
    void EnemyStateController()
    {
        if (eState == EnemyState.dead)
            return;

        FindState();            //Find which state the enemy is based on player distance.
        ProcessState();         //Set actions based on current state.
    }

    //Find which state the enemy is based on player distance.
    private void FindState()
    {
        NavMeshPath path = new NavMeshPath ();
        bool validatePath = NavMesh.CalculatePath (transform.position, PlayerPositionWithOffset(2f), -1, path);

        //enemy is stuck charging it's attack
        if (eState == EnemyState.charging)
        {
            return;
        }
        //too far from player, can see player, can reach player
        if (fDistanceFromPlayer > (stats.GetEngagementDistance()) && LookForPlayer () && validatePath) 
        {
            eState = EnemyState.chasing;
        }
        //not too far, can see player, doesnt have to reach player but must be in range
        else if (LookForPlayer () && fDistanceFromPlayer < (stats.GetEngagementDistance())) 
        {
            eState = EnemyState.attacking;
        }
        else
        {
            eState = EnemyState.patrolling;
        }
    }

    //Set actions based on current state.
    private void ProcessState()
    {
        switch (eState)
        {
            case EnemyState.charging:
                transform.LookAt(new Vector3(playerTransform.position.x, transform.position.y, playerTransform.transform.position.z));
                break;
            case EnemyState.patrolling:
                agent.speed = stats.GetWalkingSpeed();
                break;

            case EnemyState.attacking:
                transform.LookAt(new Vector3(playerTransform.position.x, transform.position.y, playerTransform.transform.position.z));
                Attack();
                break;

            default: //used when chacing, feeling
                agent.speed = stats.GetRunSpeed();
                break;
        }
    }

    protected void Attack(){
        if (Player.instance.stats.IsDead()) 
        {
            return;
        }

        derrexCombat.fRateOfFireAux += Time.deltaTime;
        RaycastHit rhit;
        Physics.Raycast (transform.position, playerTransform.position - transform.position, out rhit, stats.GetEngagementDistance());
        if (!rhit.collider)
            return;

        if (derrexCombat.fRateOfFire < derrexCombat.fRateOfFireAux && !stats.IsStunned() && rhit.collider.tag == "Player" || rhit.collider.tag == "boat") 
        {
            animationController.ForcePlayAttack();
            agent.Stop();
            eState = EnemyState.charging;
            derrexCombat.fRateOfFireAux = 0;
        }
    }

    public void DoAttack()
    {
        //GameObject clone = Instantiate (derrexCombat.goBullet, derrexCombat.goBulletSpawnPoint.position, transform.rotation) as GameObject;
        //clone.GetComponent<HomingProjectile>().SetProjectileTarget(tPlayer);
        Debug.Log(this.name + " has attacked! Pew!", this);
        eState = EnemyState.attacking;
    }

    private void Die(Vector3 dir)
    {
        //anim.Play ("WalkTree");
        if (derrexCombat.goBulletSpawnPoint) 
        {
            Destroy (derrexCombat.goBulletSpawnPoint.gameObject);
        }

        rigidBody.isKinematic = true;
        EnableRagdoll();
        rRagdoll.velocity = dir.normalized * fRagdollInitSpeed;
        eState = EnemyState.dead;
        agent.enabled = false;
        agent.speed = 0;
        agent.angularSpeed = 0;

        //Allows us to set a delay to DestroyEnemy function
        if (!IsInvoking("DestroyEnemy"))
            Invoke ("DestroyEnemy", derrexCombat.fCleanUpTime);
    }

    private void DestroyEnemy()
    {
        Instantiate (derrexCombat.goCleanUpParticle, rRagdoll.position, Quaternion.identity);
        Destroy(gameObject);
    }

    protected override void Think(){

        base.Think ();

        if (eState == EnemyState.chasing || eState == EnemyState.attacking) 
        {
            movement.FollowPlayer();
        }
        else if (eState == EnemyState.patrolling) 
        {
            if (movement.IsPathAvailable()) 
            {
                movement.FollowPath();
            }
            else
            {
                movement.Wander();
            }
        }
        else if (eState == EnemyState.fleeing) 
        {
            Flee();
        }


    }

    private void Flee(){

        if (!bFleeingAux) 
        {
            agent.SetDestination(PlayerPositionWithOffset(stats.GetEngagementDistance()));
            bFleeingAux = true;
        }

        if (Vector3.Distance(transform.position, PlayerPositionWithOffset(stats.GetEngagementDistance())) <= 2f) 
        {
            bFleeingAux = false;
        }

    }
}
