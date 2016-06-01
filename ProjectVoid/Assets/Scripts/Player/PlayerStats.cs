using UnityEngine;
using System.Collections;

public class PlayerStats : PlayerScript
{

    [SerializeField]private float fJumpSpeed = 15f;
    [SerializeField]private float fRunSpeed = 15f;
    [SerializeField]private float fStationaryTurnSpeed = 180f;
    [SerializeField]private float fMovingTurnSpeed = 360;

    [SerializeField]private float fMaxMagic = 100f;
    private float fCurrentMagic;

    private PlayerState eState;
    private bool bEnableMovement;

    private void Start()
    {
        bEnableMovement = true;
        fCurrentMagic = fMaxMagic;
    }

    private void Update()
    {
        if (eState == PlayerState.DEAD || eState == PlayerState.STUNNED)
        {
            bEnableMovement = false;
        }
    }

    /// <summary>
    /// Gets the jump speed.
    /// </summary>
    /// <returns>The jump speed.</returns>
    public float GetJumpSpeed()
    {
        return fJumpSpeed;
    }

    /// <summary>
    /// Gets the run speed.
    /// </summary>
    /// <returns>The run speed.</returns>
    public float GetRunSpeed()
    {
        return fRunSpeed;
    }

    /// <summary>
    /// Gets the stationary turn speed.
    /// </summary>
    /// <returns>The stationary turn speed.</returns>
    public float GetStationaryTurnSpeed()
    {
        return fStationaryTurnSpeed;
    }

    /// <summary>
    /// Gets the moving turn speed.
    /// </summary>
    /// <returns>The moving turn speed.</returns>
    public float GetMovingTurnSpeed()
    {
        return fMovingTurnSpeed;
    }

    /// <summary>
    /// Determines whether this instance can move.
    /// </summary>
    /// <returns><c>true</c> if this instance can move; otherwise, <c>false</c>.</returns>
    public bool CanMove()
    {
        return bEnableMovement;
    }

    /// <summary>
    /// Sets the move.
    /// </summary>
    /// <param name="enableMovement">If set to <c>true</c> enable movement.</param>
    public void SetMove(bool enableMovement)
    {
        bEnableMovement = enableMovement;
    }

    /// <summary>
    /// Gets the player state.
    /// </summary>
    /// <returns>The state.</returns>
    public PlayerState GetState()
    {
        return eState;
    }

    /// <summary>
    /// Determines whether this instance is dead. This is often called by other instances, like enemies.
    /// </summary>
    /// <returns><c>true</c> if this instance is dead; otherwise, <c>false</c>.</returns>
    public bool IsDead()
    {
        return (eState == PlayerState.DEAD);
    }

    /// <summary>
    /// Gets the current magic.
    /// </summary>
    /// <returns>The current magic.</returns>
    public float GetCurrentMagic()
    {
        return fCurrentMagic;
    }

    /// <summary>
    /// Adds the magic.
    /// </summary>
    /// <param name="amount">Amount. This can be negative.</param>
    public void AddMagic(float amount)
    {
        fCurrentMagic += amount;
    }
}
