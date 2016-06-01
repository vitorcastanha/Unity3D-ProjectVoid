using UnityEngine;
using System.Collections;

public class UnitStats : UnitScript {

    [System.Serializable]
    public class Movement
    {
        public float fWalkSpeed = 2f;
        public float fRunSpeed = 8f;
        public float fTurnSpeed = 180f;
    }
    [SerializeField] private Movement movement;

    [System.Serializable]
    public class Detection
    {
        public float fSightRange;
        public float fListeningRange;
        public float fAlertRange = 30f;
        public LayerMask lmIgnoreLayerInSightCheck;     //ignores the target layers when checking for line of sight with the player
                                                        //(i.e. Enemies should not block line of sight of other enemies)
    }
    [SerializeField] private Detection detection;

    [System.Serializable]
    public class Combat
    {
        public float fEngagementDistance = 50f;         //distance where the enemy will stop chacing the player and get ready to fight
        [HideInInspector]public bool bIsStunned;
        [HideInInspector]public float fStunDuration;
        [HideInInspector]public float fStunTimerAux;    //auxiliar to count down the stun duration

        public float fMaxLife;
        [HideInInspector]public float fCurrentLife;
    }
    [SerializeField] private Combat combat;

    /// <summary>
    /// Gets the walking speed.
    /// </summary>
    /// <returns>The walking speed.</returns>
    public float GetWalkingSpeed()
    {
        return movement.fWalkSpeed;
    }

    /// <summary>
    /// Gets the run speed.
    /// </summary>
    /// <returns>The run speed.</returns>
    public float GetRunSpeed()
    {
        return movement.fRunSpeed;
    }

    /// <summary>
    /// Gets the turn speed.
    /// </summary>
    /// <returns>The turn speed.</returns>
    public float GetTurnSpeed()
    {
        return movement.fTurnSpeed;
    }

    /// <summary>
    /// Gets the engagement distance.
    /// </summary>
    /// <returns>The engagement distance.</returns>
    public float GetEngagementDistance()
    {
        return combat.fEngagementDistance;
    }

    /// <summary>
    /// Gets the sight range.
    /// </summary>
    /// <returns>The sight range.</returns>
    public float GetSightRange()
    {
        return detection.fSightRange;
    }

    /// <summary>
    /// Gets the listening range.
    /// </summary>
    /// <returns>The listening range.</returns>
    public float GetListeningRange()
    {
        return detection.fListeningRange;
    }

    public LayerMask GetIgnoreLayerInSightCheck()
    {
        return detection.lmIgnoreLayerInSightCheck;
    }

    /// <summary>
    /// Determines whether this instance is stunned.
    /// </summary>
    /// <returns><c>true</c> if this instance is stunned; otherwise, <c>false</c>.</returns>
    public bool IsStunned()
    {
        return combat.bIsStunned;
    }

    /// <summary>
    /// Sets the stunned state.
    /// </summary>
    /// <param name="isStunned">If set to <c>true</c> is stunned.</param>
    public void SetStunned(bool isStunned, float duration)
    {
        combat.bIsStunned = isStunned;
        combat.fStunDuration = duration;
        combat.fStunTimerAux = duration;
    }

    /// <summary>
    /// Adds to the duration of the stun.
    /// </summary>
    /// <param name="extraTime">Extra time. *This value can be negative</param>
    public void AddStunDuration(float extraTime)
    {
        combat.fStunTimerAux += extraTime;              //This value can be negative.
        if (combat.fStunTimerAux <= 0f) 
        {
            combat.bIsStunned = false;
        }
    }

    /// <summary>
    /// Adds to the enemy's life. This ammount can be a negative number.
    /// </summary>
    /// <param name="lifeAmmount">Life ammount.</param>
    public void AddLife(float lifeAmmount)
    {
        combat.fCurrentLife += lifeAmmount;
        if (combat.fCurrentLife > combat.fMaxLife)
        {
            combat.fCurrentLife = combat.fMaxLife;
        }
    }

    /// <summary>
    /// Gets the current life.
    /// </summary>
    /// <returns>The current life.</returns>
    public float GetCurrentLife()
    {
        return combat.fCurrentLife;
    }

    /// <summary>
    /// Sets the current life.
    /// </summary>
    /// <param name="newValue">New value.</param>
    public void SetCurrentLife(float newValue)
    {
        combat.fCurrentLife = newValue;
    }

    /// <summary>
    /// Gets the max life.
    /// </summary>
    /// <returns>The max life.</returns>
    public float GetMaxLife()
    {
        return combat.fMaxLife;
    }
}
