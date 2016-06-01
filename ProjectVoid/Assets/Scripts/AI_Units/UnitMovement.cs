using UnityEngine;
using System.Collections;

public class UnitMovement : UnitScript
{

    private int iFrameCount;

    [System.Serializable]
    public class Pathing
    {
        public UnitPath path;
        public Transform tPathnode;
        public float fWanderDistance = 10f;     //Distance moved from last node every wander cycle
        public float fWanderTimeMin = 5f;       //Min time before moving again
        public float fWanderTimeMax = 10f;      //Max time before moving again
        public bool tether;                     //Enables teather between enemy and current path node.
    }
    [SerializeField]
    protected Pathing pathing;

    private Vector3 spawnPoint;
    private float fWanderDuration;              //Auxiliar that counts time between wandering cycles.

    private void Start()
    {
        spawnPoint = transform.position;
    }

    public virtual void FollowPlayer()
    {
        if (unit.GetDistanceFromPlayer() > unit.stats.GetEngagementDistance())
        {
            unit.GetAgent().Resume();

            if (unit.LookForPlayer())
            {
                iFrameCount = 0;
                if (unit.GetAgent().enabled)
                    unit.GetAgent().SetDestination(unit.PlayerPositionWithOffset(unit.stats.GetEngagementDistance()));

            }
            else
            {
                iFrameCount++;
                //Skip one frame before re-thinkin
            }
            if (iFrameCount > 1)
            {
                //CODE TO THINK AGAIN
            }
        }
        else
        {
            if (unit.GetAgent().enabled)
                unit.GetAgent().Stop();
        }
    }




    public void FollowPath()
    {
        unit.GetAgent().speed = unit.stats.GetWalkingSpeed();

        if (!pathing.tPathnode)
        {
            pathing.tPathnode = pathing.path.GetClosest(transform.position);
        }

        if (pathing.tPathnode)
        {
            //POTENTIAL OPTIMIZATION ISSUE
            if (unit.GetAgent().enabled)
                unit.GetAgent().SetDestination(pathing.tPathnode.position);

            if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(pathing.tPathnode.position.x, 0, pathing.tPathnode.position.z)) > 2f)
            {
                return;
            }

            if (pathing.path)
            {
                pathing.tPathnode = pathing.path.GetNext(pathing.tPathnode);
            }
            else
            {
                pathing.tPathnode = null;
            }

        }

    }


    public void Wander()
    {
        unit.GetAgent().speed = unit.stats.GetWalkingSpeed();

        fWanderDuration -= Time.deltaTime;

        if (fWanderDuration <= 0f) 
        {
            Vector3 wanderDest = new Vector3(Random.Range(-pathing.fWanderDistance, pathing.fWanderDistance), 0f, Random.Range(-pathing.fWanderDistance, pathing.fWanderDistance));

            if (pathing.tether) 
            {
                wanderDest += spawnPoint;
            }
            else
            {
                wanderDest += transform.position;
            }

            if (unit.GetAgent().enabled)
                unit.GetAgent().SetDestination(wanderDest);

            fWanderDuration = Random.Range(pathing.fWanderTimeMin, pathing.fWanderTimeMax);
        }
    }

    public bool IsPathAvailable()
    {
        return (pathing.path || pathing.tPathnode);
    }
}
